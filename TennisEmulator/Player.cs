using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TennisEmulator
{
    public class Player
    {
        private Random randomer = new Random();

        public int HitRate
        {
            get;private set;
        }

        public void HanleBall()
        { 
            if (this.randomer.Next() % 100 > this.HitRate)
            {
                DI.Resolver.Resolve<IEventDispatcher>().Dispatch(this, new BallHittedEvent());
            }
        }
    }
}
