using MechDuelCommon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MechDuelServer
{
    class ServerController
    {
        private List<Player> playersList;
        public ServerController()
        {
            playersList = new List<Player>();
        }

        public void StartServer()
        {
            int port = 7777;
            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            listener.Start();

            Console.WriteLine("Server is running");
            Console.WriteLine("Listening on port " + port);

            while (true)
            {
                    if (listener.Pending())
                    {
                        Console.WriteLine("New pending connection");
                        listener.BeginAcceptTcpClient(AcceptClient, listener);
                    }

                        foreach (var p in playersList)
                        {
                           // Console.WriteLine(p.GameState.ToString());
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
        }

        private void GameStarted(Player player)
        {
            if (player.DataAvailable())
            {
               // Console.WriteLine("New player position ");
                Message message = player.ReadMessage();
                if (message.MessageType == MessageType.PlayerMovement)
                {
                    //players.Where(s => s.GameState == GameState.GameStarted).
                    //    ToList().ForEach(p => p.SendMessage(message));
                    foreach (var p in playersList)
                    {
                        if (p.GameState == GameState.GameStarted)
                        {
                            p.SendMessage(message);
                        }
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
            p.SendMessage(msg);
            p.GameState = GameState.GameStarted;
        }

        private void SyncPlayerMovements(Player player)
        {
            foreach (var p in playersList)
            {
                if (p.GameState == GameState.GameStarted)
                {
                    var last = p.MessageList.LastOrDefault(x => x.MessageType == MessageType.PlayerMovement);
                    if (last != null)
                    {
                        Message msg = new Message();
                        msg.PlayerInfo = last.PlayerInfo;
                        p.SendMessage(msg);
                    }
                }
            }
        }


        private void SyncNewPlayers(Player player)
        {
            foreach (var p in playersList)
            {
                if (p.GameState == GameState.GameStarted)
                {
                    Message m = new Message();
                    m.MessageType = MessageType.NewPlayer;
                    PlayerInfo info = p.MessageList.FirstOrDefault(x => x.MessageType == MessageType.NewPlayer).PlayerInfo;
                    m.PlayerInfo = info;

                    player.SendMessage(m);
                }
            }
        }

        private void Connecting(Player p)
        {
            if (p.DataAvailable())
            {
                Console.WriteLine("New player registering");
                Player playerMsg = p.ReadPlayer();
                p.Name = playerMsg.Name;

                foreach (Player NP in playersList)
                {
                    Message msg = new Message();
                    msg.MessageType = MessageType.NewPlayer;
                    msg.Description = (NP == p) ?
                        "Successfully joined" :
                        $"Player {p.Name} has joined";
                    PlayerInfo info = new PlayerInfo();
                    info.Id = p.Id;
                    info.Name = p.Name;
                    info.X = 0;
                    info.Y = 0;
                    info.Z = 0;
                    info.rX = 0;
                    info.rY = 0;
                    info.rZ = 0;
                    msg.PlayerInfo = info;

                    NP.SendMessage(msg);
                    NP.MessageList.Add(msg);
                }
                p.GameState = GameState.Sync;
            }
        }

        private void AcceptClient(IAsyncResult result)
        {
            TcpListener listener = (TcpListener)result.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(result);

            if (client.Connected)
            {
                Console.WriteLine("accepted new client");
                Player p = new Player();
                p.MessageList = new List<Message>();
                p.Id = new Guid();
                p.Id = Guid.NewGuid();
                p.GameState = GameState.Connecting;
                p.TcpClient = client;
                playersList.Add(p); 

                p.SendPlayer(p);
            }
            else
            {
                Console.WriteLine("Connection refused");
            }
        }

        //End of Class
    }
}
