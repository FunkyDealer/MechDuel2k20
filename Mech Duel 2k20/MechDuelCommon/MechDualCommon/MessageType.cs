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
        Died,
        FinishedSync,
        Disconnected,
        Information,
        Warning,
        Error
    }
}
