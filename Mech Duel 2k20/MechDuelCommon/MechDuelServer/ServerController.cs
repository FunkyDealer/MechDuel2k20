﻿using MechDuelCommon;
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

            bool disconnectedPlayers = false;

            while (true)
            {                
                    if (listener.Pending())
                    {
                        Console.WriteLine("New pending connection");
                        listener.BeginAcceptTcpClient(AcceptClient, listener);
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
                    case MessageType.Disconnected: //Player is switched to disconnected state, next while loop will take case of it
                        player.GameState = GameState.Disconnected;
                        //Disconnected(player);
                        break;
                    default:
                        break;
                }
            }
        }

        private void Sync(Player p) //Sync player that are connecting
        {
            // Process all new players
            SyncNewPlayers(p);

            // Process all movements
            SyncPlayerMovements(p);

            // process Shots
            SyncShots(p);

            SyncDeaths(p);

            // update game State
            Message msg = new Message();
            msg.MessageType = MessageType.FinishedSync;
            p.SendMessage(msg);
            p.GameState = GameState.GameStarted;
        }

        private void SyncDeaths(Player p)
        {

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
                        msg.PlayerInfo = last.PlayerInfo;
                        p.SendMessage(msg);
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
                        Console.WriteLine("shot fired");
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
                Console.WriteLine($"New player registered. Nick: {p.Name}");
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
