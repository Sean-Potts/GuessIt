/*
* File : Program.cs
* Project : PROG2121 - A05 - Server
* Programmer : Sean Potts
* First Version : 11/15/2022
* Description : creates and starts the server, also allows for manually shutting down the server
*/


using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;


namespace Server
{
    internal class Program
    {
        static private readonly int max;
        static private readonly int min;
        static private readonly IPAddress IP;
        static private readonly Int32 port;


        //static constructor : Program
        //description : set the static constant values to the values in App.config
        static Program()
        {
            try
            {
                // Try to get the constants from the config file
                max = int.Parse(ConfigurationManager.AppSettings.Get("max"));
                min = int.Parse(ConfigurationManager.AppSettings.Get("min"));
                IP = IPAddress.Parse(ConfigurationManager.AppSettings.Get("IP"));
                port = int.Parse(ConfigurationManager.AppSettings.Get("port"));
            }
            catch // Catch all exceptions
            { // set constants to their default values
                max = 10;
                min = 1;
                IP = IPAddress.Parse("127.0.0.1");
                port = 13000;
            }
        }


        static void Main(string[] args)
        {
            HiLoServer hiLoServer = new HiLoServer(min, max, IP, port);
            Task serverTask = Task.Run(() => hiLoServer.Start());

            Console.WriteLine("press enter to shutdown");
            Console.ReadLine();


            hiLoServer.ShutDown();
        }        
    }
}

