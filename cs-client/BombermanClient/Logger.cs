using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BombermanClient
{
    class Logger
    {
        static Logger activeLogger = new Logger(@"C:\temp\gameLog" + DateTime.Now.ToString("yyyyMMddhhmmssF") + " .txt");

        public static void WriteLineServer(string line)
        {
            activeLogger.WriteLine("< " + line);
        }

        public static void WriteLineClient(string line)
        {
            activeLogger.WriteLine("> " + line);
        }

        public static void WriteLineInternal(string line)
        {
            activeLogger.WriteLine(line);
        }

        public static void NewFile()
        {
            activeLogger.Close();
            activeLogger = new Logger(@"C:\temp\gameLog" + DateTime.Now.ToString("yyyyMMddhhmmssF") + " .txt");
        }



        FileStream fileStream;
        StreamWriter streamWriter;

        private Logger(string fileName)
        {
            fileStream = new FileStream(fileName, FileMode.Create);
            streamWriter = new StreamWriter(fileStream);
        }

        private static void NewFile(string fileName)
        {
            activeLogger = new Logger(fileName);
        }

        private void WriteLine(string line)
        {
            streamWriter.WriteLine(line);
        }   
     
        private void Close()
        {
            streamWriter.Flush();
            streamWriter.Close();
            fileStream.Close();
        }
    }
}
