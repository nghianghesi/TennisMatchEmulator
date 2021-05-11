using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TennisEmulator
{
    public class BallHittedEvent
    {
    }

    public class FailedHitEvent
    {
    }

    public class MatchUnitFinishedEvent
    {
        public MatchTeam WinningTeam
        { get; private set; }

        public MatchUnitFinishedEvent(MatchTeam winningTeam)
        {
            this.WinningTeam = winningTeam;
        }
    }
}
