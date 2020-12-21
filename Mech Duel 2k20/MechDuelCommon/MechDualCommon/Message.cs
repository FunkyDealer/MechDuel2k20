﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechDuelCommon
{
    public class Message
    {
        public string Description { get; set; }
        public PlayerInfo PlayerInfo { get; set; }
        public MessageType MessageType { get; set; }
    }
}
