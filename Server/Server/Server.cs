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

namespace Server
{
    class Server
    {
        public static void Main()
        {

            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!Additional Device for testing !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //Device DF = new Device("r?");                          //remove this
            //ConnectionManager.CurrentDeviceList.Add("r?", DF);     //remove this
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            const int Port = 14;
            IPAddress ipAd = Tools.GetMyIPAddress();
            //List<Socket> SocketList = new List<Socket>();

            /* Initializes the Listener */
            TcpListener Server = new TcpListener(ipAd, Port);
            Console.WriteLine("The local End point is  :" +
                                Server.LocalEndpoint);
            Console.WriteLine("Waiting for a connection.....");
            Thread StatusThread = new Thread(new ThreadStart(Tools.DisplayServerStatus));
            StatusThread.Start();

            DatabaseHandler.ConnectionString = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\Mohamed\Documents\GitHub\Prototype\Mickey_Server\Mickey_Server\Database.mdf;Integrated Security=True;Connect Timeout=30";

            while (true)
            {
                try
                {
                    /* Start Listening at the specified port */
                    Server.Start();
                    Thread T = new Thread(new ParameterizedThreadStart(ConnectionManager.AcceptConnection));
                    T.Start(Server.AcceptSocket());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
