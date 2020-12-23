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
        player = new Player();
        playersList = new Dictionary<Guid, GameObject>();
        player.GameState = GameState.Disconnected;
        player.TcpClient = new TcpClient();
    }

    void Start()
    {

    }

    public void StartTcpClient()
    {
        player.TcpClient.BeginConnect(IPAddress.Parse(IpAddress), Port, AcceptConnection, player.TcpClient);
        player.GameState = GameState.Connecting;
    }

    private void AcceptConnection(IAsyncResult ar)
    {
        TcpClient client = (TcpClient)ar.AsyncState;
        client.EndConnect(ar);

        if (client.Connected)
        {
            Debug.Log("client connected");
            player.BinaryReader = new System.IO.BinaryReader(client.GetStream());
            player.BinaryWriter = new System.IO.BinaryWriter(client.GetStream());
            player.MessageList = new List<Message>();
        }
        else
        {
            Debug.Log("Client connection refused");
        }
    }

    void Update()
    {
        if (player.TcpClient.Connected)
        {
            //Debug.Log(player.GameState.ToString());
            switch (player.GameState)
            {
                case GameState.Connecting:
                    //Debug.Log("connecting");
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

    private void GameStarted()
    {
        if (player.TcpClient.GetStream().DataAvailable)
        {
            Message message = ReceiveMessage();

            if (message.MessageType == MessageType.NewPlayer) SpawnNewPlayer(message);
            else if (message.MessageType == MessageType.PlayerMovement) UpdatePlayerMovement(message);
        }
    }

    private Message ReceiveMessage()
    {
        string json = player.BinaryReader.ReadString();
        Message m = JsonConvert.DeserializeObject<Message>(json);
        player.MessageList.Add(m);
        return m;
    }

    #region connecting

    private void Connected()
    {
        if (player.DataAvailable())
        {
            Debug.Log("Connected");
            Message message = ReceiveMessage();
            Debug.Log(message.Description);
            player.GameState = GameState.Sync;
        }
    }

    private void Connecting()
    {
        if (player.DataAvailable())
        {
            string playerJsonString = player.BinaryReader.ReadString();
            Player temp = JsonConvert.DeserializeObject<Player>(playerJsonString);
            player.Id = temp.Id;
            player.MessageList.Add(player.MessageList.FirstOrDefault());
            player.Name = playerNameInputText.text;

            Message message = new Message();
            message.MessageType = MessageType.PlayerName;
            player.MessageList.Add(message);

            string newPlayerJsonString = JsonConvert.SerializeObject(player);
            player.BinaryWriter.Write(newPlayerJsonString);
            player.GameState = GameState.Connected;
        }
    }
    #endregion

    private void Sync()
    {
       // Debug.Log("Syncing");
        if (player.TcpClient.GetStream().DataAvailable)
        {
           // Debug.Log("Data to read");
            Message messageReceived = ReceiveMessage();

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
            GameObject p = playersList[m.PlayerInfo.Id];
            p.transform.position = new Vector3(m.PlayerInfo.X, m.PlayerInfo.Y, m.PlayerInfo.Z);
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


    public void SendMessage(Message m)
    {
        string json = JsonConvert.SerializeObject(m);
        player.BinaryWriter.Write(json);
        player.MessageList.Add(m);
    }
}
