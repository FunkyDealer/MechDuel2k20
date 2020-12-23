using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.IO;

namespace MechDuelCommon
{
    public class Player
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Message> MessageList { get; set; }
        [JsonIgnore]
        public TcpClient TcpClient { get; set; }
        public GameState GameState { get; set; }
        public List<Message> Messages { get; set; }
        public BinaryReader BinaryReader;
        public BinaryWriter BinaryWriter;


        public bool DataAvailable()
        {
            return TcpClient.GetStream().DataAvailable;
        }

        public void SendMessage(Message msg)
        {
            if (BinaryWriter == null)
            {
                BinaryWriter = new BinaryWriter(TcpClient.GetStream());
            }
            BinaryWriter.Write(JsonConvert.SerializeObject(msg));
        }

        public void SendPlayer(Player p)
        {
            if (BinaryWriter == null)
            {
                BinaryWriter = new BinaryWriter(TcpClient.GetStream());
            }
            BinaryWriter.Write(JsonConvert.SerializeObject(p));
        }

        public Player ReadPlayer()
        {
            if (BinaryReader == null)
                BinaryReader = new BinaryReader(TcpClient.GetStream());

            string msg = BinaryReader.ReadString();
            return JsonConvert.DeserializeObject<Player>(msg);
        }

        public Message ReadMessage()
        {
            string msg = BinaryReader.ReadString();
            return JsonConvert.DeserializeObject<Message>(msg);
        }

    }
}
