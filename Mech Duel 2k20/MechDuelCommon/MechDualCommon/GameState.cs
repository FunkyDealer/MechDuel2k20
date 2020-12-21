using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechDuelCommon
{
    public enum GameState
    {
        Disconnected,
        Connecting,
        Connected,
        Sync,
        GameStarted
    }
}
