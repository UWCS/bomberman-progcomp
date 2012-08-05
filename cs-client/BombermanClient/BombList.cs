using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BombermanClient
{
    /// <summary>
    /// Holds a list of bombs and determines when bombs have blown up
    /// </summary>
    class BombList
    {
        List<Bomb> bombs;
        List<Point2D> recentlyExplodedPoints;

        public BombList()
        {
            bombs = new List<Bomb>();
            recentlyExplodedPoints = new List<Point2D>();
        }

        public bool IsEmptyForMove(int x, int y)
        {
            foreach (Bomb bomb in bombs)
            {
                if (bomb.X == x && bomb.Y == y)
                {
                    return false;
                }
            }
            return true;
        }

        public void Add(Bomb bomb)
        {
            bombs.Add(bomb);
        }

        List<Bomb> BombsAt(int x, int y)
        {
            List<Bomb> bombsFound = new List<Bomb>();
            foreach (Bomb bomb in bombs)
            {
                if (bomb.X == x && bomb.Y == y)
                {
                    bombsFound.Add(bomb);
                }
            }
            return bombsFound;
        }

        public void Tick(Map map, PlayerList players)
        {
            recentlyExplodedPoints = new List<Point2D>();
            // Make a list of all the bombs about to explode
            List<Bomb> bombsExploding = new List<Bomb>();
            foreach (Bomb bomb in bombs)
            {
                bomb.Tick();
                if (bomb.Timer == 0)
                {
                    bombsExploding.Add(bomb);
                }
            }
            // Detonate each bomb in the list.
            List<Bomb> bombsExploded = new List<Bomb>() ;
            List<Bomb> bombsExplodingNext = new List<Bomb>();
            do
            {
                foreach (Bomb bomb in bombsExploding)
                {
                    // if the bomb is on a player, kill the player
                    foreach (Player player in players.PlayersAt(bomb.X, bomb.Y))
                    {
                        if (player.X == bomb.X && player.Y == bomb.Y)
                        {
                            Logger.WriteLineInternal(player.X + ", " + player.Y + " and " + bomb.X + ", " + bomb.Y);
                            player.Alive = false;
                        }
                    }
                    // if the bomb is on a bomb, add that bomb to the list
                    foreach (Bomb stackedBomb in BombsAt(bomb.X, bomb.Y))
                    {
                        if (!(bombsExploding.Contains(stackedBomb) || bombsExplodingNext.Contains(stackedBomb) || bombsExploded.Contains(stackedBomb)))
                        {
                            bombsExplodingNext.Add(stackedBomb);
                        }
                    }
                    recentlyExplodedPoints.Add(new Point2D(bomb.X, bomb.Y));
                    // For each direction
                    for (int xDiff = -1; xDiff <= 1; xDiff++)
                    {
                        for (int yDiff = -1; yDiff <= 1; yDiff++)
                        {
                            if ((xDiff == 0 || yDiff == 0) && xDiff != yDiff)
                            {
                                // Cardinal direction. Maybe
                                // for 1 to 3
                                for (int i = 1; i <= 3; i++)
                                {
                                    int xPos = bomb.X + i * xDiff;
                                    int yPos = bomb.Y + i * yDiff;

                                    // if that position has a wall, destroy the wall then stop
                                    Block block = map.GetBlockAt(xPos, yPos);
                                    if (block == null || !block.Destroy())
                                    {
                                        break;
                                    }
                                    recentlyExplodedPoints.Add(new Point2D(xPos, yPos));
                                    // if that position is a player, kill the player
                                    foreach (Player player in players.PlayersAt(xPos, yPos))
                                    {
                                        Logger.WriteLineInternal(player.X + ", " + player.Y + " and " + xPos + ", " + yPos);
                                        player.Alive = false;
                                    }

                                    // if that position is a bomb, add it to the list
                                    foreach (Bomb stackedBomb in BombsAt(xPos, yPos))
                                    {
                                        if (!(bombsExploding.Contains(stackedBomb) || bombsExplodingNext.Contains(stackedBomb) || bombsExploded.Contains(stackedBomb)))
                                        {
                                            bombsExplodingNext.Add(stackedBomb);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                bombsExploded.AddRange(bombsExploding);
                bombsExploding = bombsExplodingNext;
                bombsExplodingNext = new List<Bomb>();
            }
            while (bombsExploding.Count > 0);

            foreach (Bomb bomb in bombsExploded)
            {
                Console.WriteLine("Bomb REMOVED");
                bombs.Remove(bomb);
            }
        }

        internal void Draw(int xStart, int yStart)
        {
            // Draw the bombs
            if (bombs != null)
            {
                foreach (Bomb bomb in bombs)
                {
                    Console.SetCursorPosition(bomb.Y + 1 + xStart, bomb.X + 1 + yStart);
                    Console.Write("ó");
                }
            }
            foreach (Point2D point in recentlyExplodedPoints)
            {
                Console.SetCursorPosition(point.Y + 1 + xStart, point.X + 1 + yStart);
                Console.BackgroundColor = ConsoleColor.DarkYellow;
                Console.Write(" ");
                Console.BackgroundColor = ConsoleColor.Black;
            }
        }
    }
}
