using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechDuelServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerController server = new ServerController();
            server.StartServer(7);
        }
    }
}
