using MechDuelCommon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MechDuelServer
{
    class ServerController
    {

        private List<Player> _players;
        public ServerController()
        {
            _players = new List<Player>();
        }

        public void StartServer(int port)
        {
            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            listener.Start();

            Console.WriteLine("Server is running");
            Console.WriteLine("Listening on port " + port);

            while (true)
            {
                try
                {
                    if (listener.Pending())
                    {
                        Console.WriteLine("New pending connection");
                        listener.BeginAcceptTcpClient(AcceptClient, listener);

                        foreach (var p in _players)
                        {
                            Console.WriteLine(p.GameState.ToString());
                            switch (p.GameState)
                            {
                                case GameState.Connecting:
                                    Connecting(p);
                                    break;
                                case GameState.Sync:
                                    Sync(p);
                                    break;
                                case GameState.GameStarted:
                                    GameStarted(p);
                                    break;
                            }
                        }
                    }
                    // TcpClient client = await listener.
                    // HandleConnectionAsync(client);
                }
                catch (Exception)
                {
                    Console.WriteLine("Error occurred");
                }
            }
        }

        private void GameStarted(Player p)
        {
            if (p.DataAvailable())
            {
                string json = p.BinaryReader.ReadString();
                Console.WriteLine($"new Player position: {json}");
                Message message = JsonConvert.DeserializeObject<Message>(json);
                if (message.MessageType == MessageType.PlayerMovement)
                {
                    foreach (var i in _players)
                    {
                        p.BinaryWriter.Write(json);
                    }
                }
            }
        }

        private void Sync(Player p)
        {
            // Process all new players
            SyncNewPlayers(p);

            // Process all movements
            SyncPlayerMovements(p);


            // update game State
            Message msg = new Message();
            msg.MessageType = MessageType.FinishedSync;
            string json = JsonConvert.SerializeObject(msg);
            p.BinaryWriter.Write(json);
            p.GameState = GameState.GameStarted;
        }

        private void SyncPlayerMovements(Player player)
        {
            foreach (var p in _players)
            {
                if (p.GameState == GameState.GameStarted)
                {
                    var last = p.MessageList.LastOrDefault(x => x.MessageType == MessageType.PlayerMovement);
                    if (last != null)
                    {
                        var info = last.PlayerInfo;
                        Message msg = new Message();
                        msg.PlayerInfo = info;
                        SendMessage(player, msg);
                    }
                }
            }
        }

        private void SendMessage(Player player, Message msg)
        {
            string json = JsonConvert.SerializeObject(msg);
            player.BinaryWriter.Write(json);
        }

        private void SyncNewPlayers(Player player)
        {
            foreach (var p in _players)
            {
                if (p.GameState == GameState.GameStarted)
                {
                    Message m = new Message();
                    m.MessageType = MessageType.NewPlayer;
                    PlayerInfo info = p.MessageList.FirstOrDefault(x =>
                                                   x.MessageType == MessageType.NewPlayer).PlayerInfo;
                    m.PlayerInfo = info;

                    SendMessage(player, m);
                }
            }
        }

        private void Connecting(Player p)
        {
            if (p.DataAvailable())
            {
                Console.WriteLine("New player registering");
                string playerJson = p.BinaryReader.ReadString();
                Player playerMsg = JsonConvert.DeserializeObject<Player>(playerJson);
                p.Name = playerMsg.Name;

                foreach (Player notifyPlayer in _players)
                {
                    Message msg = new Message();
                    msg.MessageType = MessageType.NewPlayer;
                    msg.Description = (notifyPlayer == p) ?
                        "Successfully joined" :
                        "Player " + p.Name + " has joined";
                    PlayerInfo playerInfo = new PlayerInfo();
                    playerInfo.Id = p.Id;
                    playerInfo.Name = p.Name;
                    playerInfo.X = 0;
                    playerInfo.Y = 0;
                    playerInfo.Z = 0;
                    msg.PlayerInfo = playerInfo;

                    string msgJson = JsonConvert.SerializeObject(msg);
                    notifyPlayer.BinaryWriter.Write(msgJson);
                    notifyPlayer.MessageList.Add(msg);
                    Console.WriteLine(msgJson);
                }
                p.GameState = GameState.Sync;
            }
        }

        private void AcceptClient(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(ar);

            if (client.Connected)
            {
                Console.WriteLine("accepted new client");

                Player player = new Player();
                Message message = new Message();
                message.Description = "Hello new player";
                message.MessageType = MessageType.Information;

                player.MessageList = new List<Message>();
                player.MessageList.Add(message);
                player.TcpClient = client;
                player.BinaryReader = new System.IO.BinaryReader(client.GetStream());
                player.BinaryWriter = new System.IO.BinaryWriter(client.GetStream());
                player.Id = Guid.NewGuid();
                player.GameState = GameState.Connecting;

                //add player to list of player
                _players.Add(player);

                string json = JsonConvert.SerializeObject(player);
                Console.WriteLine(json);
                player.BinaryWriter.Write(json);

            }
            else
            {
                Console.WriteLine("Connection refused");
            }
        }

        //End of Class
    }
}
