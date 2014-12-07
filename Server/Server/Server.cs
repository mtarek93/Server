using CommandHandler;
using Database;
using Scheduler;
using ServerTools;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using WifiLocalization;

namespace Server
{
    class Server
    {
        public static void Main()
        {

            const int Port = 14;
            IPAddress ipAd = Tools.GetMyIPAddress();

            /* Initializes the Listener */
            TcpListener Server = new TcpListener(ipAd, Port);
            Console.WriteLine("The local End point is  :" +
                                Server.LocalEndpoint);
            Console.WriteLine("Waiting for a connection.....");
            Thread StatusThread = new Thread(new ThreadStart(Tools.DisplayServerStatus));
            StatusThread.Start();
            DatabaseHandler.ConnectionString = Helper.ConnectionStringHandler();
            CommandParser.InitializeCommandsDictionary();
            ScheduleHandler.InitializeScheduler();
            User_Locate l = new User_Locate();

            while (true)
            {
                try
                {
                    /* Start Listening at the specified port */
                    Server.Start();
                    Thread T = new Thread(new ParameterizedThreadStart(Tools.AcceptConnection));
                    T.Start(Server.AcceptSocket());

                }
                catch (Exception e)
                {
                    Console.WriteLine("Server.main: " + e.Message);
                }
            }
        }
    }
}

