using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BombermanClient
{
    /// <summary>
    /// Contains the complete state of the game for a single turn
    /// </summary>
    class GameState
    {
        Map map;
        PlayerList players;
        BombList bombs;
        int tickCount;

        public GameState()
        {
            bombs = new BombList();
            players = new PlayerList();
            map = new Map(0, 0);
            tickCount = 0;
        }

        public void Bake(TickActions actionList)
        {
            tickCount++;
            // Update players
            foreach (KeyValuePair<Player, Action> playerAction in actionList.ActionDict)
            {
                Player player = playerAction.Key;
                Action action = playerAction.Value;

                switch (action)
                {
                    case Action.UP:
                        if (IsEmptyForMove(player.X - 1, player.Y))
                        {
                            player.MoveUp();
                        }
                        break;
                    case Action.DOWN:
                        if (IsEmptyForMove(player.X + 1, player.Y))
                        {
                            player.MoveDown();
                        }
                        break;
                    case Action.LEFT:
                        if (IsEmptyForMove(player.X, player.Y - 1))
                        {
                            player.MoveLeft();
                        }
                        break;
                    case Action.RIGHT:
                        if (IsEmptyForMove(player.X, player.Y + 1))
                        {
                            player.MoveRight();
                        }
                        break;
                    case Action.BOMB:
                        bombs.Add(new Bomb(player.X, player.Y));
                        break;
                    default:
                        throw new ProtocolError();
                }
            }
            // Also update the bombs, and destroy players/walls
            bombs.Tick(map, players);
            DrawState();
        }

        private void DrawState()
        {
            Console.Clear();
            map.Draw(0, 0);
            players.Draw(0, 0);
            bombs.Draw(0, 0);
            Console.SetCursorPosition(1, map.Height + 2);
        }

        internal bool IsEmptyForMove(int x, int y)
        {
            return (map.IsEmptyForMove(x, y) && bombs.IsEmptyForMove(x, y));
        }

        public Map Map
        {
            get
            {
                return map;
            }
            set
            {
                map = value;
            }
        }
        internal void AddPlayer(Player player)
        {
            players.Add(player);
        }

        internal Player GetPlayer(string playerName)
        {
            return players.GetPlayer(playerName);
        }

        internal void KillPlayer(string playerName)
        {
            players.KillPlayer(playerName);
        }
    }
}
