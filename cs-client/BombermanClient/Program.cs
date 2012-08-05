using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace BombermanClient
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect("uwcs.co.uk", 8037);

            Stream stream = tcpClient.GetStream();
            //TODO: Enter a username and password to register
            Game game = new Game(stream, "username", "password");

            game.Play();
        }
    }
}
