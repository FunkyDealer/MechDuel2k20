using MechDuelCommon;
using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Linq;

public class TCPClientController : MonoBehaviour
{
    public string IpAddress;
    public int Port;

    [HideInInspector]
    public Player player;

    public Text playerNameInputText;

    public Dictionary<Guid, GameObject> playersList;

    [SerializeField]
    private GameObject mainPlayerPrefab;    
    [SerializeField]
    GameObject enemyPlayerPrefab;
    [SerializeField]
    private List<GameObject> ConnectionUI;

    MainPlayer mainPlayer;

    [SerializeField]
    ShotsManager shotsManager;

    [SerializeField]
    GameManager gameManager;
    public GameManager GetGameManager => gameManager;

    [SerializeField]
    GameObject ScoreBoardObject;
    [HideInInspector]
    public ScoreBoard scoreBoard;

    void Awake()
    {
        gameManager.getTcpController(this);
        playersList = new Dictionary<Guid, GameObject>();
        player = new Player();
        player.GameState = GameState.Disconnected;
        player.TcpClient = new TcpClient();
    }

    void Start()
    {
        SpawnScoreBoard();
    }

    void OnDestroy()
    {
        if (player.TcpClient.Connected)
        Disconnect();
    }

    void Update()
    {
        if (player.TcpClient.Connected)
        {
            //Debug.Log(player.GameState.ToString());
            switch (player.GameState)
            {
                case GameState.Connecting:
                    // Debug.Log("connecting");
                    Connecting();
                    break;
                case GameState.Connected:
                    // Debug.Log("Connected");
                    Connected();
                    break;
                case GameState.Sync:
                    // Debug.Log("Sync");
                    Sync();
                    break;
                case GameState.GameStarted:
                    // Debug.Log("Game Started");
                    GameStarted();
                    break;
                default:
                    break;
            }


        }
    }

    private void SpawnScoreBoard()
    {
        GameObject o = Instantiate(ScoreBoardObject, Vector3.zero, Quaternion.identity);
        ScoreBoard s = o.GetComponent<ScoreBoard>();
        s.TcpController = this;
        scoreBoard = s;
        o.SetActive(false);
        
    }

    public void StartTcpClient()
    {
        gameManager.gameStarted = false;
        player.TcpClient.BeginConnect(IPAddress.Parse(IpAddress), Port, AcceptConnection, player.TcpClient);
        player.GameState = GameState.Connecting;
    }

    private void AcceptConnection(IAsyncResult result)
    {
        TcpClient client = (TcpClient)result.AsyncState;
        client.EndConnect(result);

        if (client.Connected) Debug.Log("client connected");
        else Debug.Log("Client connection refused");
    }

    private void GameStarted()
    {
        if (player.DataAvailable())
        {
            Message m = player.ReadMessage();
            switch (m.MessageType)
            {
                case MessageType.PlayerName:
                    break;
                case MessageType.NewPlayer:
                    SpawnNewPlayer(m);
                    break;
                case MessageType.PlayerMovement:
                    UpdatePlayerMovement(m);
                    break;
                case MessageType.Shoot:
                    UpdateShots(m);
                    break;
                case MessageType.Disconnected:
                    UpdateDisconnects(m);
                    break;
                case MessageType.gotHit:
                    UpdateHits(m);
                    break;
                case MessageType.Died:
                    UpdateDeath(m);
                    break;
                case MessageType.GameStart:
                    StartGame(m);
                    break;
                case MessageType.GameEnd:
                    EndGame(m);
                    break;
                case MessageType.Spawned:
                    SpawnOtherPlayer(m);
                    break;
                default:
                    break;
            }
        }

        if (Input.GetButtonDown("ScoreBoard")) { scoreBoard.gameObject.SetActive(true); Debug.Log("ScoreBoard On"); }
        if (Input.GetButtonUp("ScoreBoard")) { scoreBoard.gameObject.SetActive(false); Debug.Log("ScoreBoard off"); }
    }

    void EndGame(Message m)
    {
        PlayerInfo info = m.PlayerInfo;
        if (info != null) { GameObject o = playersList[info.Id]; gameManager.EndGameWithWinner(o); }
        else
        {
            gameManager.EndGameWithoutWinner();
        }
    }

    void UpdateDeath(Message m)
    {
        DeathInfo info = m.deathInfo;
        if (info.id != player.Id)
        {
            NPCPlayer p = playersList[info.id].transform.gameObject.GetComponent<NPCPlayer>();
            p.Die();
            ScoreUpdate(info);
        }
    }

    void ScoreUpdate(DeathInfo info)
    {
        if (gameManager.gameStarted)
        {            
            GameObject o = playersList[info.killer];
            Entity k = o.GetComponent<Entity>();
            k.score++;
            if (k is MainPlayer)
            {
                Debug.Log("Player Scored"); ///Scoring Notification
            }

            scoreBoard.UpdateScore();
        }
    }

    private void SpawnOtherPlayer(Message m)
    {
        PlayerInfo info = m.PlayerInfo;
        GameObject p = playersList[info.Id];
        Vector3 pos = new Vector3(info.X, info.Y, info.Z);
        Vector3 rotation = new Vector3(info.rX, info.rY, info.rZ);

        gameManager.SpawnEnemyPlayer(p, pos, rotation);
    }

    #region connecting
    private void Connected()
    {
        if (player.DataAvailable())
        {
            Debug.Log("Connected");
            Message message = player.ReadMessage();
            Debug.Log(message.Description);
            player.GameState = GameState.Sync;
        }
    }

    private void Connecting()
    {
        if (player.DataAvailable())
        {
            Player playerJson = player.ReadPlayer();
            player.Id = playerJson.Id;
            player.Name = playerNameInputText.text;
            player.score = 0;
            player.ready = false;
            player.alive = false;

            player.SendPlayer(player);
            player.GameState = GameState.Connected;
        }
    }
    #endregion

    private void Sync()
    {
       // Debug.Log("Syncing");
        if (player.DataAvailable())
        {
            // Debug.Log("Data to read");
            Message messageReceived = player.ReadMessage();
            switch (messageReceived.MessageType)
            {
                case MessageType.PlayerName:

                    break;
                case MessageType.NewPlayer:
                    SpawnNewPlayer(messageReceived);
                    break;
                case MessageType.PlayerMovement:
                    UpdatePlayerMovement(messageReceived);
                    break;
                case MessageType.Shoot:
                    UpdateShots(messageReceived);
                    break;
                case MessageType.FinishedSync:
                    FinishSync(messageReceived);
                    break;
                case MessageType.Disconnected:
                    UpdateDisconnects(messageReceived);
                    break;
                case MessageType.Information:
                    GetGameState(messageReceived);
                    break;
                case MessageType.Spawned:
                    SpawnOtherPlayer(messageReceived);
                    break;
            }
        }
    }

    private void GetGameState(Message m)
    {
        
        GameInfo info = m.gameInfo;
        try { gameManager.gameStarted = info.gameStarted; }
        catch  
        {
        
        }
        

        foreach (var p in playersList)
        {
            if (info.readyPlayers.Contains(p.Key))
            {
                Entity e = p.Value.GetComponent<Entity>();
                e.ready = true;
            }
        }
    }

    private void FinishSync(Message m)
    {
        Debug.Log("Finishing sync");
        foreach (var a in ConnectionUI)
        {
            a.SetActive(false);
        }
        GameObject p = Instantiate(mainPlayerPrefab, Vector3.zero, Quaternion.identity);
        mainPlayer = p.GetComponent<MainPlayer>();
        
        if (mainPlayer != null)
        {
            mainPlayer.SendMovementInfo();
            mainPlayer.id = player.Id;
            mainPlayer.nickName = player.Name;
            mainPlayer.alive = false;
        }
        else { Debug.Log("Player was null"); }

        p.SetActive(false);

        playersList.Add(player.Id, p);
        player.GameState = GameState.GameStarted;

        gameManager.RegisterMainPlayer(p);
        gameManager.SpawnSpectator();
    }

    private void SpawnNewPlayer(Message m)
    {
        GameObject playerGO = Instantiate(enemyPlayerPrefab, new Vector3(m.PlayerInfo.X, m.PlayerInfo.Y, m.PlayerInfo.Z), Quaternion.identity);
        // playerGO.GetComponent<PlayerUiController>().playerName.text = m.PlayerInfo.Name;
        playersList.Add(m.PlayerInfo.Id, playerGO);
        if (mainPlayer != null) mainPlayer.SendMovementInfo();
        else { Debug.Log("Player was null"); }

        NPCPlayer p = playerGO.GetComponent<NPCPlayer>();
        p.nickName = m.PlayerInfo.Name;
        p.id = m.PlayerInfo.Id;
        p.alive = m.PlayerInfo.alive;
        playerGO.SetActive(m.PlayerInfo.alive);
    }   

    private void UpdatePlayerMovement(Message m)
    {
        if (playersList.ContainsKey(m.PlayerInfo.Id))
        {
            playersList[m.PlayerInfo.Id].transform.position = new Vector3(m.PlayerInfo.X, m.PlayerInfo.Y, m.PlayerInfo.Z);
            playersList[m.PlayerInfo.Id].transform.forward = new Vector3(m.PlayerInfo.rX, m.PlayerInfo.rY, m.PlayerInfo.rZ);
        }
    }

    private void UpdateShots(Message m)
    {
        Shot s = m.shot;
        Vector3 position = new Vector3(s.xStart, s.yStart, s.zStart);
        Vector3 direction = new Vector3(s.xDir, s.yDir, s.zDir);

        Entity shooter = null;

        foreach (var p in playersList)
        {
            if (p.Key == m.shot.shooter) shooter = p.Value.GetComponent<Entity>();
        }

        shotsManager.SpawnLaser(position, direction, m.shot.damage, shooter);
    }

    private void UpdateHits(Message m)
    {
        HitInfo HI = m.hitInfo;

        if (HI.hitId != player.Id && playersList.ContainsKey(HI.hitId))
        {
            NPCPlayer p = playersList[HI.hitId].GetComponent<NPCPlayer>();

            Entity shooter = null;
            if (HI.shooter != null)
            {
                if (playersList.ContainsKey(HI.shooter)) shooter = playersList[HI.hitId].GetComponent<Entity>();
            }

            p.ReceiveDamage(HI.healthDamage, HI.shieldDamage, shooter);
        }
    }

    #region Disconnects
    private void Disconnect()
    {
        Message m = new Message();
        m.MessageType = MessageType.Disconnected;
        PlayerInfo info = new PlayerInfo();
        info.Id = player.Id;
        info.Name = player.Name;
        m.PlayerInfo = info;
        player.SendMessage(m);
    }

    private void UpdateDisconnects(Message m)
    {
        if (playersList.ContainsKey(m.PlayerInfo.Id))
        {
            Destroy(playersList[m.PlayerInfo.Id].transform.gameObject);
            Debug.Log($"Player {playersList[m.PlayerInfo.Id].name} Disconnected");
        }
        playersList.Remove(m.PlayerInfo.Id);
    }
    #endregion

    private void StartGame(Message m)
    {
        gameManager.StartGame();
        mainPlayer.GameStart();

        Debug.Log("Game has Started");
    }
}
