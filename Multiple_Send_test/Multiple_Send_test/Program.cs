using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using Database;
//using CommandHandler;
using ServerTools;

namespace Server
{
    class Server
    {
        public static void Main()
        {
            const int Port = 14;
            IPAddress ipAd = Tools.GetMyIPAddress();
            //List<Socket> SocketList = new List<Socket>();

            /* Initializes the Listener */
            TcpListener Server = new TcpListener(ipAd, Port);
            Console.WriteLine("The local End point is  :" + Server.LocalEndpoint);
            while (true)
            {
                try
                {
                    /* Start Listening at the specified port */
                    Server.Start();
                    Socket S = Server.AcceptSocket();
                    Thread T = new Thread(new ParameterizedThreadStart(Tools.Send1));
                    T.Start(S);
                    Thread T2 = new Thread(new ParameterizedThreadStart(Tools.Send2));
                    T2.Start(S);
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine("Server.main: " + e.Message);
                }
            }
        }
    }
}
