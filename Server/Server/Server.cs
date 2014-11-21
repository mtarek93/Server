using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Database;
using CommandHandler;
using ServerTools;
using Scheduler;

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

            //   Miky
            //DatabaseHandler.ConnectionString = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\Michael\Documents\GitHub\Server\Server\Server\Database .mdf;Integrated Security=True;Connect Timeout=30";
            
            //   Mt
            //DatabaseHandler.ConnectionString = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\Mohamed\Documents\GitHub\Server\Server\Server\Database.mdf;Integrated Security=True;Connect Timeout=30";
            
            //   Teefa
            // DatabaseHandler.ConnectionString = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\mosta_000\Documents\GitHub\Server\Server\Server\Database.mdf;Integrated Security=True;Connect Timeout=30";
        
            //   Tee
            DatabaseHandler.ConnectionString = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\Tarek\Documents\GitHub\Server\Server\Server\Database.mdf;Integrated Security=True;Connect Timeout=30";
         
            //DatabaseHandler.ConnectionString = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\mosta_000\Documents\GitHub\Server\Server\Server\Database.mdf;Integrated Security=True;Connect Timeout=30";

            // Baha2
            //DatabaseHandler.ConnectionString = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\GitHub\Server\Server\Server\Database.mdf;Integrated Security=True;Connect Timeout=30";
           
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
    
