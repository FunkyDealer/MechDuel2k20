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

        private bool gameStarted;
        

        public void StartServer()
        {
            int port = 7777;
            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            listener.Start();

            Console.WriteLine("Server is running");
            Console.WriteLine("Listening on port " + port);

            bool disconnectedPlayers = false;
            gameStarted = false;

            while (true)
            {                
                    if (listener.Pending())
                    {
                        Console.WriteLine("New pending connection");
                    listener.BeginAcceptTcpClient(AcceptClient, listener);
                    //if (playersList.Count < 2) listener.BeginAcceptTcpClient(AcceptClient, listener);
                    //else listener.

                }

                if (playersList.Count > 0)
                {
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
                            case GameState.Disconnected:
                                disconnectedPlayers = Disconnected(p);
                                break;
                        }
                    }
                    if (disconnectedPlayers) { RemoveDisconnectedPlayers(); disconnectedPlayers = false; }
                }
            }
        }

        private void RemoveDisconnectedPlayers()
        {
            playersList.RemoveAll(x => x.GameState == GameState.Disconnected);
            
        }

        private void GameStarted(Player player)
        {
            if (player.DataAvailable())
            {
               // Console.WriteLine("New player position ");
                Message message = player.ReadMessage();
                switch (message.MessageType)
                {
                    case MessageType.PlayerMovement:
                    case MessageType.Shoot:
                    case MessageType.gotHit:
                    case MessageType.Died:
                        if (message.MessageType != MessageType.PlayerMovement) Console.WriteLine($"message: {message.MessageType.ToString()}");
                        foreach (var p in playersList) //Send message to each player about what's happening
                        {
                            if (p.GameState == GameState.GameStarted)
                            {
                                p.SendMessage(message);
                            }
                        }
                        break;
                    case MessageType.Spawned:
                        PlayerSpawned(player, message);
                        break;
                    case MessageType.Disconnected: //Player is switched to disconnected state, next while loop will take case of it
                        player.GameState = GameState.Disconnected;
                        //Disconnected(player);
                        break;
                    case MessageType.PlayerReady:
                    case MessageType.PlayerUnready:
                        PlayerReadyStatus(player, message);
                        break;
                    default:
                        break;
                }
            }
        }

        private void PlayerSpawned(Player p, Message m)
        {
            p.alive = true;
            Console.WriteLine($"Player {p.Name} is Spawning");
            foreach (var NP in playersList)
            {
                if (NP.GameState == GameState.GameStarted)
                {
                    NP.SendMessage(m);
                }
            }
        }

        private void PlayerReadyStatus(Player p, Message m)
        {
            if (m.MessageType == MessageType.PlayerReady) { p.ready = true; Console.WriteLine($"player {p.Name} is now Ready"); }
            else if (m.MessageType == MessageType.PlayerUnready) { p.ready = false; Console.WriteLine($"player {p.Name} is no longer Ready"); }
                       
            foreach (var NP in playersList) //Send message to each player about what's happening
            {
                if (NP.GameState == GameState.GameStarted)
                {
                    NP.SendMessage(m);
                }
            }

            CheckIfGameCanStart();
        }

        private void CheckIfGameCanStart()
        {
            if (playersList.Count > 1)
            {
                bool areAllPlayersReady = true;

                foreach (var p in playersList) if (!p.ready) areAllPlayersReady = false;

                if (areAllPlayersReady)
                {
                    gameStarted = true;
                    Console.WriteLine("All Players are Ready, game is Starting");
                    foreach (var p in playersList)
                    {
                        //Message that game is Read to start;
                        Message m = new Message();
                        m.MessageType = MessageType.GameStart;

                        p.SendMessage(m);
                    }
                }
            }
        }

        private void Sync(Player p) //Sync player that are connecting
        {         
            // Process all new players
            SyncOtherPlayers(p);

            // Process all movements
            SyncPlayerMovements(p);

            SendGameInfo(p);

            // update game State
            Message msg = new Message();
            msg.MessageType = MessageType.FinishedSync;
            p.SendMessage(msg);
            p.GameState = GameState.GameStarted;
        }

        private void SyncDeaths(Player p)
        {

        }

        private void SendGameInfo(Player p)
        {
            Message m = new Message();
            m.MessageType = MessageType.Information;
            GameInfo info = new GameInfo();
            info.gameStarted = gameStarted;
            info.readyPlayers = ReadyPlayers();
            
            p.SendMessage(m);
        }

        private List<Guid> ReadyPlayers()
        {          
            if (playersList.Count > 0)
            {
                List<Guid> readyPlayers = new List<Guid>();
                foreach (var p in playersList)
                {
                    if (p.ready) readyPlayers.Add(p.Id);
                }
                return readyPlayers;
            }
            else
            {
                return null;
            }
        }        

        private void SyncOtherPlayers(Player player)
        {
            foreach (var p in playersList)
            {
                if (p.GameState == GameState.GameStarted)
                {
                    Message m = new Message();
                    m.MessageType = MessageType.NewPlayer;
                    PlayerInfo info = p.MessageList.FirstOrDefault(x => x.MessageType == MessageType.NewPlayer).PlayerInfo;
                    m.PlayerInfo = info;
                    m.PlayerInfo.alive = p.alive;
                    

                    player.SendMessage(m);
                }
            }
        }

        private void SyncPlayerMovements(Player player)
        {
            foreach (Player p in playersList)
            {
                if (p.GameState == GameState.GameStarted)
                {
                    Message last = p.MessageList.LastOrDefault(x => x.MessageType == MessageType.PlayerMovement);
                    if (last != null)
                    {
                        Message msg = new Message();
                        msg.MessageType = MessageType.PlayerMovement;
                        msg.PlayerInfo = last.PlayerInfo;
                        player.SendMessage(msg);
                    }
                }
            }
        }

        private void SyncShots(Player player)
        {
            foreach (Player p in playersList)
            {
                if (p.GameState == GameState.GameStarted)
                {
                    Message last = p.MessageList.LastOrDefault(x => x.MessageType == MessageType.Shoot);
                    if (last != null)
                    {
                        Message msg = new Message();
                        msg.shot = last.shot;
                        //Console.WriteLine("shot fired");
                        p.SendMessage(msg);
                    }
                }
            }
        }

        private bool Disconnected(Player p) //Send message to players that X player disconnected
        {
            if (p.DataAvailable())
            {
                Console.WriteLine($"Player {p.Name} has not disconnected");
                p.GameState = GameState.Sync;
                return false;
            }
            else
            {
                Console.WriteLine($"Player {p.Name} has disconnected");
                Message msg = new Message();
                msg.MessageType = MessageType.Disconnected;
                PlayerInfo info = new PlayerInfo();
                info.Id = p.Id;
                info.Name = p.Name;

                foreach (Player NP in playersList)
                {
                    NP.SendMessage(msg);
                }
                return true;
            }
        }
        
        private void Connecting(Player p)
        {
            if (p.DataAvailable())
            {                
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
                    info.Y = -10;
                    info.Z = 0;
                    info.rX = 0;
                    info.rY = 0;
                    info.rZ = 0;
                    info.alive = p.alive;
                    msg.PlayerInfo = info;

                    NP.SendMessage(msg);
                    NP.MessageList.Add(msg);                    
                }
                p.GameState = GameState.Sync;
                Console.WriteLine($"New player registered. Nick: {p.Name}");
            }
        }

        private Player GetPlayer(Guid id)
        {
            Player player = null;
            foreach (var p in playersList)
            {
                if (p.Id == id) player = p;
            }

            return player;
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
                p.score = 0;
                p.ready = false;
                p.alive = false;
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
