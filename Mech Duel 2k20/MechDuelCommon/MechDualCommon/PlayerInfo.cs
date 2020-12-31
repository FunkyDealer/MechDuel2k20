using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechDuelCommon
{
    public class PlayerInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool alive { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }  
        public float rX { get; set; }
        public float rY { get; set; }
        public float rZ { get; set; }
    }
}
