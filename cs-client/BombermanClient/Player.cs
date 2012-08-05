using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BombermanClient
{
    class Player
    {
        string name;
        int x;
        int y;
        bool alive;
        public Player(string name, int x, int y)
        {
            this.name = name;
            this.x = x;
            this.y = y;
            this.alive = true;
        }

        public bool Alive
        {
            get
            {
                return alive;
            }
            set
            {
                if (value == false)
                {
                    Logger.WriteLineInternal(name + " should be dead");
                }
                alive = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
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

        internal void MoveUp()
        {
            x--;
        }

        internal void MoveDown()
        {
            x++;
        }

        internal void MoveLeft()
        {
            y--;
        }

        internal void MoveRight()
        {
            y++;
        }
    }
}
