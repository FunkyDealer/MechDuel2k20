using MechDuelCommon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{ 
    public bool gameStarted;
    TCPClientController tcpController;

    public List<SpawnPoint> spawnPoints;

    [SerializeField]
    GameObject spectator;

    GameObject mainPlayer;

    int SpawnPointsNumber;

    void Awake()
    {
        gameStarted = false;


    }

    // Start is called before the first frame update
    void Start()
    {
        SpawnPointsNumber = spawnPoints.Count;
    }

    // Update is called once per frame
    void Update()
    {
      
    }


    public void ChangeGameState()
    {
        if (gameStarted) EndGame();
        else StartGame();
    }

    public void StartGame()
    {
        gameStarted = true;
        ResetGame();
        
    }

    public void EndGame()
    {
        gameStarted = false;
        foreach (var p in tcpController.playersList)
        {
            Entity e = p.Value.GetComponent<Entity>();
            e.ready = false;
        }
    }

    public void ResetGame()
    {
        foreach (var p in tcpController.playersList)
        {
            Entity e = p.Value.GetComponent<Entity>();
            e.score = 0;
        }
    }

    public void getTcpController(TCPClientController controller)
    {
        this.tcpController = controller;
    }

    public void RegisterMainPlayer(GameObject p)
    {
        mainPlayer = p;        
    }

    public void SpawnMainPlayer()
    {       
        Vector3 pos = SelectSpawnPoint(SpawnPointsNumber);

        mainPlayer.transform.position = pos;
        mainPlayer.SetActive(true);        
        SendMainPlayerSpawnInformation();
    }

    public void SpawnEnemyPlayer(GameObject o, Vector3 pos, Vector3 rotation)
    {
        o.SetActive(true);
        o.transform.position = pos;
        o.transform.forward = rotation;
    }

    private Vector3 SelectSpawnPoint(int max)
    {
        int spawnPoint = Random.Range(0, max);
        if (spawnPoints[spawnPoint].isOcupied()) return spawnPoints[spawnPoint].spawnPos;
        else return SelectSpawnPoint(max);
    }
    
    public void MainPlayerDie(GameObject o)
    {
        o.SetActive(false);
        SpawnSpectator(o.transform.position, o.transform.forward);
    }

    public void SpawnSpectator()
    {
        int max = spawnPoints.Count;
        Vector3 pos = SelectSpawnPoint(max);

        GameObject o = Instantiate(spectator, pos, Quaternion.identity);
        //o.transform.forward = forward;
        Spectator s = o.GetComponent<Spectator>();
        s.manager = this;
    }

    public void SpawnSpectator(Vector3 position, Vector3 forward)
    {
        GameObject o = Instantiate(spectator, position, Quaternion.identity);
        o.transform.forward = forward;
        Spectator s = o.GetComponent<Spectator>();
        s.manager = this;
    }

    void SendMainPlayerSpawnInformation()
    {
        MainPlayer p = mainPlayer.GetComponent<MainPlayer>();

        Message m = new Message();
        m.MessageType = MessageType.Spawned;
        PlayerInfo info = new PlayerInfo();
        info.Id = p.id;
        info.Name = p.nickName;
        info.alive = true;
        info.X = mainPlayer.transform.position.x;
        info.Y = mainPlayer.transform.position.y;
        info.Z = mainPlayer.transform.position.z;
        info.rX = mainPlayer.transform.forward.x;
        info.rY = mainPlayer.transform.forward.y;
        info.rZ = mainPlayer.transform.forward.z;
        m.PlayerInfo = info;

        tcpController.player.SendMessage(m);
    }

    public void EndGameWithWinner(GameObject winner)
    {
        EndGame();
        Entity e = winner.GetComponent<Entity>();
        if (e is MainPlayer)
        {
            Debug.Log($"You Have Won the Game!");
        } else
        {
            Debug.Log($"Game has Ended with a winner: {e.nickName}");
        }        
    }    

    public void EndGameWithoutWinner()
    {
        EndGame();
        Debug.Log($"Game has Ended without a winner due to to few players");
    }



}
