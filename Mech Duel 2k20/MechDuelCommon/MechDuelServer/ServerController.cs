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
        private int WinScore = 5;

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
                    if (disconnectedPlayers) { RemoveDisconnectedPlayers(); disconnectedPlayers = false; CheckIfGameCanContinue(); }
                }
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

        private void RemoveDisconnectedPlayers()
        {
            playersList.RemoveAll(x => x.GameState == GameState.Disconnected);
            
        }

        private void GameStarted(Player player)
        {
            if (player.DataAvailable())
            {
               // Console.WriteLine("New player position ");
                Message m = player.ReadMessage();
                switch (m.MessageType)
                {
                    case MessageType.PlayerMovement:
                    case MessageType.Shoot:
                    case MessageType.gotHit:
                        if (m.MessageType == MessageType.gotHit) Console.WriteLine($"player {player.Name} got shot by {GetPlayer(m.hitInfo.shooter).Name} for {m.hitInfo.healthDamage}");
                        SendMessageToAllOtherPlayers(m, player); //Send message to each player about what's happening
                        break;
                    case MessageType.Died:
                        PlayerDie(player, m);
                        break;
                    case MessageType.Spawned:
                        PlayerSpawned(player, m);
                        break;
                    case MessageType.Disconnected: //Player is switched to disconnected state, next while loop will take case of it
                        player.GameState = GameState.Disconnected;
                        break;
                    case MessageType.PlayerReady:
                    case MessageType.PlayerUnready:
                        PlayerReadyStatus(player, m);
                        break;
                    default:
                        break;
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
                msg.PlayerInfo = info;

                SendMessageToAllOtherPlayers(msg, p);                
                return true;
            }
        }
        
        private void Connecting(Player p)
        {
            if (p.DataAvailable())
            {                
                Player playerMsg = p.ReadPlayer();
                p.Name = playerMsg.Name;

                Message msg = new Message();
                msg.MessageType = MessageType.NewPlayer;
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

                foreach (Player NP in playersList)
                {               
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

        private void PlayerDie(Player p, Message m)
        {
            p.alive = false;
            Player killer = GetPlayer(m.deathInfo.killer);
            if (gameStarted) PlayerScore(killer);
            Console.WriteLine($"Player {p.Name} was killed by {killer.Name}!");
            SendMessageToAllOtherPlayers(m, p);
        }

        private void PlayerSpawned(Player p, Message m)
        {
            p.alive = true;
            Console.WriteLine($"Player {p.Name} is Spawning");
            SendMessageToAllOtherPlayers(m, p);
        }

        private void PlayerReadyStatus(Player p, Message m)
        {
            if (m.MessageType == MessageType.PlayerReady) { p.ready = true; Console.WriteLine($"player {p.Name} is now Ready"); }
            else if (m.MessageType == MessageType.PlayerUnready) { p.ready = false; Console.WriteLine($"player {p.Name} is no longer Ready"); }

            SendMessageToAllOtherPlayers(m, p);

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
                    StartGame();
                }
            }
        }

        private void StartGame()
        {
            ResetScores();
            gameStarted = true;
            Console.WriteLine("All Players are Ready, game is Starting");

            Message m = new Message(); //Message that game is Read to start;
            m.MessageType = MessageType.GameStart;

            SendMessageToAllPlayers(m);
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


        private void PlayerScore(Player killer) //Add score to the killer
        {
            killer.score++;
            CheckForGameEnd(killer);
        }

        private void CheckForGameEnd(Player winner) //check to see if the game can End
        {
            if (winner.score >= WinScore)
            {
                EndGameWithWinner(winner);
            }
        }

        private void EndGameWithWinner(Player winner) //End the game with Winner
        {
            Console.WriteLine($"Game is Over, the Winner is {winner.Name}");
            Message m = new Message();
            m.MessageType = MessageType.GameEnd;
            PlayerInfo info = new PlayerInfo();
            info.Id = winner.Id;
            info.Name = winner.Name;
            m.PlayerInfo = info;

            SendMessageToAllPlayers(m);
            ResetGame();
        }

        private void EndGameWithoutWinner() //end the game without winner (In case of to many disconnects);
        {
            Message m = new Message();
            m.MessageType = MessageType.GameEnd;           

            SendMessageToAllPlayers(m);
            ResetGame();
        }

        private void ResetScores() //Reset all player's Scores
        {
            Console.WriteLine("Reseting Scores");
            foreach (var p in playersList)
            {
                p.score = 0;
            }
        }

        private void ResetGame() //Reset the game status
        {
            gameStarted = false;
            foreach (var p in playersList)
            {
                p.ready = false;
            }
        }

        private void CheckIfGameCanContinue()
        {
            if (gameStarted)
            {
                if (playersList.Count < 2) EndGameWithoutWinner();
            }
        }


        private void SendMessageToAllOtherPlayers(Message m, Player sender)
        {
            foreach (var p in playersList) if (p != sender && p.GameState == GameState.GameStarted) p.SendMessage(m);
        }
        
        private void SendMessageToAllPlayers(Message m)
        {
            foreach (var p in playersList) if (p.GameState == GameState.GameStarted) p.SendMessage(m);
        }

        //End of Class
    }
}
