using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BombermanClient
{
    enum BlockType
    {
        Empty, Destructible, Indestructible
    }

    enum Action
    {
        NOTHING, UP, DOWN, LEFT, RIGHT, BOMB
    }
}
