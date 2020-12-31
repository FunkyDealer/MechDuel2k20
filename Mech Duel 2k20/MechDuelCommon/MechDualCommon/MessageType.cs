using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechDuelCommon
{
    public enum MessageType
    {
        PlayerName,
        NewPlayer,
        PlayerMovement,
        Shoot,
        gotHit,
        Died,
        Spawned,
        FinishedSync,
        PlayerReady,
        PlayerUnready,
        GameStart,
        GameEnd,
        Disconnected,
        Information,
        Warning,
        Error
    }
}
