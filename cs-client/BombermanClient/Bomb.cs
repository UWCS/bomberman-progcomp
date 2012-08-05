using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BombermanClient
{
    class Bomb
    {
        int x;
        int y;
        int timer;
        const int STARTING_TIMER = 6;

        public Bomb(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.timer = STARTING_TIMER;
        }

        public int X
        {
            get
            {
                return x;
            }
        }

        public int Y
        {
            get
            {
                return y;
            }
        }

        internal void Tick()
        {
            timer--;
        }

        public int Timer 
        { 
            get
            {
                return timer;
            }
        }
    }
}
