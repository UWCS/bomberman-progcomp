using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BombermanClient
{
    class Block
    {
        BlockType type;

        public Block(BlockType type)
        {
            this.type = type;
        }

        public BlockType Type
        {
            get
            {
                return type;
            }
        }


        internal bool Destroy()
        {
            switch (type)
            {
                case BlockType.Empty:
                    return true;
                case BlockType.Destructible:
                    type = BlockType.Empty;
                    return false;
                default:
                    return false;
            }
        }
    }
}
