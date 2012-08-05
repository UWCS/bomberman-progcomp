using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BombermanClient
{
    /// <summary>
    /// Stores the map of the game's blocks
    /// </summary>
    class Map
    {
        int maxX;
        int maxY;

        Block[,] map;

        public Map(int x, int y)
        {
            maxX = x;
            maxY = y;
            map = new Block[x, y];
        }

        public void AddBlock(int x, int y, BlockType type)
        {
            map[x, y] = new Block(type);
        }

        public bool IsEmptyForMove(int x, int y)
        {
            return x >= 0 && x < maxX && y >= 0 && y < maxY && map[x, y].Type == BlockType.Empty;
        }

        public Block GetBlockAt(int x, int y)
        {
            if (x >= 0 && x < maxX && y >= 0 && y < maxY)
            {
                return map[x, y];
            }
            return null;
        }

        internal void Draw()
        {
            throw new NotImplementedException();
        }

        internal void Draw(int xStart, int yStart)
        {
            int lineNo = xStart;
            Console.SetCursorPosition(yStart, lineNo);
            Console.BackgroundColor = ConsoleColor.White;
            Console.Write(new String(' ', maxY + 2));
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine();
            lineNo++;
            for (int x = 0; x < maxX; x++)
            {
                Console.SetCursorPosition(yStart, lineNo);
                Console.BackgroundColor = ConsoleColor.White;
                Console.Write(" ");
                for (int y = 0; y < maxY; y++)
                {
                    switch (map[x, y].Type)
                    {
                        case BlockType.Empty:
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.Write(" ");
                            break;
                        case BlockType.Destructible:
                            Console.BackgroundColor = ConsoleColor.Gray;
                            Console.Write(" ");
                            break;
                        case BlockType.Indestructible:
                            Console.BackgroundColor = ConsoleColor.White;
                            Console.Write(" ");
                            break;
                        default:
                            break;
                    }
                }
                Console.BackgroundColor = ConsoleColor.White;
                Console.Write(" ");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine();
                lineNo++;
            }
            Console.SetCursorPosition(yStart, lineNo);
            Console.BackgroundColor = ConsoleColor.White;
            Console.Write(new String(' ', maxY + 2));
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine();
        }

        public int Height
        {
            get
            {
                return maxX;
            }
        }
    }
}
