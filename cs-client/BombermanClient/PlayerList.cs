using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BombermanClient
{
    class PlayerList
    {
        Dictionary<string, Player> players;
        int playerCount;

        public PlayerList()
        {
            playerCount = 0;
            players = new Dictionary<string, Player>();
        }

        public Player GetPlayer(int playerNo)
        {
            return new List<Player>(players.Values)[playerNo];
        }
        public Player GetPlayer(string playerName)
        {
            return players[playerName];
        }

        internal void Add(Player player)
        {
            players.Add(player.Name, player);
            playerCount++;
        }

        public int PlayerCount
        {
            get
            {
                return playerCount;
            }
        }


        internal IEnumerable<Player> PlayersAt(int x, int y)
        {
            List<Player> playersFound = new List<Player>();
            foreach (Player player in players.Values)
            {
                if (player.X == x && player.Y == y)
                {
                    playersFound.Add(player);
                }
            }
            return playersFound;
        }

        internal void Draw(int xStart, int yStart)
        {
            // Draw the players
            int origX = Console.CursorTop;
            int origY = Console.CursorLeft;
            if (players != null)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    if (GetPlayer(i).Alive)
                    {
                        switch (i)
                        {
                            case 0:
                                Console.BackgroundColor = ConsoleColor.Red;
                                break;
                            case 1:
                                Console.BackgroundColor = ConsoleColor.Blue;
                                break;
                            case 2:
                                Console.BackgroundColor = ConsoleColor.Green;
                                break;
                            case 3:
                                Console.BackgroundColor = ConsoleColor.Yellow;
                                break;
                            case 4:
                                Console.BackgroundColor = ConsoleColor.Magenta;
                                break;
                            case 5:
                                Console.BackgroundColor = ConsoleColor.Cyan;
                                break;
                            case 6:
                                Console.BackgroundColor = ConsoleColor.DarkBlue;
                                break;
                            default:
                                Console.BackgroundColor = ConsoleColor.DarkGreen;
                                break;
                        }
                        Console.SetCursorPosition(GetPlayer(i).Y + 1 + xStart, GetPlayer(i).X + 1 + yStart);
                        Console.Write("0");
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                }
            }
        }

        internal void KillPlayer(string playerName)
        {
            GetPlayer(playerName).Alive = false;
        }
    }
}
