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

    private Dictionary<Guid, GameObject> playersList;

    [SerializeField]
    private GameObject mainPlayerPrefab;    
    [SerializeField]
    GameObject enemyPlayerPrefab;
    [SerializeField]
    private List<GameObject> ConnectionUI;
    [SerializeField]
    private List<GameObject> spawnPoints;

    void Awake()
    {       
        playersList = new Dictionary<Guid, GameObject>();
        player = new Player();
        player.GameState = GameState.Disconnected;
        player.TcpClient = new TcpClient();
    }

    void Start()
    {

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

    public void StartTcpClient()
    {
        player.TcpClient.BeginConnect(IPAddress.Parse(IpAddress), Port, AcceptConnection, player.TcpClient);
        player.GameState = GameState.Connecting;
    }

    private void AcceptConnection(IAsyncResult result)
    {
        TcpClient client = (TcpClient)result.AsyncState;
        client.EndConnect(result);

        if (client.Connected)
        {
            Debug.Log("client connected");
        }
        else
        {
            Debug.Log("Client connection refused");
        }
    }



    private void GameStarted()
    {
        if (player.DataAvailable())
        {
            Message message = player.ReadMessage();

            if (message.MessageType == MessageType.NewPlayer) SpawnNewPlayer(message);
            else if (message.MessageType == MessageType.PlayerMovement) UpdatePlayerMovement(message);
        }
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

            //string json = player.BinaryReader.ReadString();
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
                case MessageType.FinishedSync:
                    FinishSync(messageReceived);
                    break;
                case MessageType.Information:
                    break;
                case MessageType.Warning:
                    break;
                case MessageType.Error:
                    break;
            }
        }
    }

    private void SpawnNewPlayer(Message m)
    {
        GameObject playerGO = Instantiate(enemyPlayerPrefab, new Vector3(m.PlayerInfo.X, m.PlayerInfo.Y, m.PlayerInfo.Z), Quaternion.identity);
       // playerGO.GetComponent<PlayerUiController>().playerName.text = m.PlayerInfo.Name;
        playersList.Add(m.PlayerInfo.Id, playerGO);
    }

    private void UpdatePlayerMovement(Message m)
    {
        if (playersList.ContainsKey(m.PlayerInfo.Id))
        {
            playersList[m.PlayerInfo.Id].transform.position = new Vector3(m.PlayerInfo.X, m.PlayerInfo.Y, m.PlayerInfo.Z);
            playersList[m.PlayerInfo.Id].transform.forward = new Vector3(m.PlayerInfo.rX, m.PlayerInfo.rY, m.PlayerInfo.rZ);
        }
    }

    private void FinishSync(Message m)
    {
        Debug.Log("Finishing sync");
        foreach (var a in ConnectionUI)
        {
            a.SetActive(false);
        }
        GameObject p = Instantiate(mainPlayerPrefab, spawnPoints[0].transform.position, Quaternion.identity);
       // playerGO.GetComponent<PlayerUiController>().playerName.text = player.Name;

        playersList.Add(player.Id, p);
        player.GameState = GameState.GameStarted;
    }

}
