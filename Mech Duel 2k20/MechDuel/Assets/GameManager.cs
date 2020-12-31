using MechDuelCommon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{ 
    public bool gameStarted;
    TCPClientController tcpController;

    public List<SpawnPoint> spawnPoints;

    void Awake()
    {
        gameStarted = false;


    }

    // Start is called before the first frame update
    void Start()
    {
        
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
        ResetScore();
    }

    public void EndGame()
    {
        gameStarted = false;
    }



    public void ResetScore()
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

    public void SpawnMainPlayer(GameObject p)
    {
        int max = spawnPoints.Count;
        Vector3 pos = SelectSpawnPoint(max);

        
        p.SetActive(true);
        p.transform.position = pos;

        MainPlayer e = p.GetComponent<MainPlayer>();

        //SEND SPAWN MESSAGE TO SERVER
        Message m = new Message();
        m.MessageType = MessageType.Spawned;
        PlayerInfo info = new PlayerInfo();
        info.Id = e.id;
        info.Name = e.nickName;
        info.alive = true;
        info.X = p.transform.position.x;
        info.Y = p.transform.position.y;
        info.Z = p.transform.position.z;
        info.rX = p.transform.forward.x;
        info.rY = p.transform.forward.y;
        info.rZ = p.transform.forward.z;
        m.PlayerInfo = info;

        tcpController.player.SendMessage(m);
        
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
        if (spawnPoints[spawnPoint].CanSpawn()) return spawnPoints[spawnPoint].spawnPos;
        else return SelectSpawnPoint(max);
    }
}
