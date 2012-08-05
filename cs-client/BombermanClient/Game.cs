using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BombermanClient
{
    /// <summary>
    /// Gets given the player's username/password and the server address, and plays the game
    /// </summary>
    class Game
    {

        GameState currentGameState;

        Stream stream;

        String username;
        String password;

        StreamReader reader;
        StreamWriter writer;

        public Game(Stream stream, String username, String password)
        {
            this.stream = stream;
            this.username = username;
            this.password = password;

            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            writer.AutoFlush = true;
        }

        internal String Init()
        {
            currentGameState = new GameState();
            Logger.NewFile();
            String returnString = "";
            if (username != "username" && password != "password")
            {
                returnString = "REGISTER " + username + " " + password;
            }
            Logger.WriteLineServer(returnString);
            return returnString;
        }

        internal String Tick(int tickNumber)
        {
            string returnString = "";
            Logger.WriteLineClient(returnString);
            return returnString;
        }

        internal void End()
        {
            // Nothing goes here
        }

        internal void Scores(int scoreCount)
        {
            // Process the scores, if we care
            for (int scoreNo = 0; scoreNo < scoreCount; scoreNo++)
            {
                Logger.WriteLineServer(reader.ReadLine());
            }
        }

        internal void Play()
        {
            while (!reader.ReadLine().Equals("INIT")) ;
            String line = "INIT";
            while (true)
            {
                Logger.WriteLineServer(line);
                String[] lineParts = line.Split(' ');

                switch (lineParts[0])
                {
                    case "INIT":
                        writer.WriteLine(this.Init());
                        break;
                    case "MAP":
                        this.Map(Int32.Parse(lineParts[1]), Int32.Parse(lineParts[2]));
                        break;
                    case "PLAYERS":
                        this.Players(Int32.Parse(lineParts[1]));
                        break;
                    case "TICK":
                        writer.WriteLine(this.Tick(Int32.Parse(lineParts[1])));
                        break;
                    case "ACTIONS":
                        this.ProcessActions(Int32.Parse(lineParts[1]));
                        break;
                    case "DEAD":
                        this.ProcessDead(Int32.Parse(lineParts[1]));
                        break;
                    case "END":
                        this.End();
                        break;
                    case "SCORES":
                        this.Scores(Int32.Parse(lineParts[1]));
                        break;
                    case "REGISTERED":
                        // DON'T CARE
                        break;
                    case "E_WRONG_PASS":
                        // DON'T CARE
                        break;
                    case "E_NOT_PLAYING":
                        break;
                    case "LEFT":
                    case "RIGHT":
                    case "UP":
                    case "DOWN":
                    case "BOMB":
                        // DON'T CARE
                        break;
                    default:
                        throw new ProtocolError();
                }
                line = reader.ReadLine();
            }
        }

        private void ProcessDead(int playerCount)
        {
            for (int i = 0; i < playerCount; i++)
            {
                string line = reader.ReadLine();
                currentGameState.KillPlayer(line);
                Logger.WriteLineServer(line);
            }
        }

        private void ProcessActions(int actionCount)
        {
            // Process the lines into a TickActions, then bake into the gamestate
            TickActions actions = new TickActions();
            for (int rowCount = 0; rowCount < actionCount; rowCount++)
            {
                string line = reader.ReadLine();
                Logger.WriteLineServer(line);
                string[] lineParts = line.Split(' ');
                Player player = currentGameState.GetPlayer(lineParts[0]);
                switch (lineParts[1])
                {
                    case "UP":
                        actions.AddAction(player, Action.UP);
                        break;
                    case "DOWN":
                        actions.AddAction(player, Action.DOWN);
                        break;
                    case "LEFT":
                        actions.AddAction(player, Action.LEFT);
                        break;
                    case "RIGHT":
                        actions.AddAction(player, Action.RIGHT);
                        break;
                    case "BOMB":
                        actions.AddAction(player, Action.BOMB);
                        break;
                    default:
                        throw new ProtocolError();
                }
            }
            currentGameState.Bake(actions);
        }


        internal void Map(int x, int y)
        {
            Map map = new Map(x, y);
            for (int rowCount = 0; rowCount < x; rowCount++)
            {
                string line = reader.ReadLine();
                Logger.WriteLineServer(line);
                string[] lineParts = line.Split(' ');
                for (int columnCount = 0; columnCount < y; columnCount++)
                {
                    switch (lineParts[columnCount])
                    {
                        case "0":
                            map.AddBlock(rowCount, columnCount, BlockType.Empty);
                            break;
                        case "1":
                            map.AddBlock(rowCount, columnCount, BlockType.Destructible);
                            break;
                        case "2":
                            map.AddBlock(rowCount, columnCount, BlockType.Indestructible);
                            break;
                        default:
                            throw new ProtocolError();
                    }
                }
            }
            currentGameState.Map = map;
        }

        internal void Players(int playerCount)
        {
            for (int rowCount = 0; rowCount < playerCount; rowCount++)
            {
                string line = reader.ReadLine();
                Logger.WriteLineServer(line);
                string[] lineParts = line.Split(' ');
                currentGameState.AddPlayer(new Player(lineParts[0], Int32.Parse(lineParts[1]), Int32.Parse(lineParts[2])));
            }
        }

    }
}
