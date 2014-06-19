﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace TCP_Client
{
    public class Client
    {
        static Socket tcpSocket;
        static Thread SendThread, ReceiveThread;
        public static void Main()
        {
            try
            {
                Console.WriteLine("Connecting.....");

                tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tcpSocket.Connect("10.98.226.33", 14);
                //use the ipaddress as in the server program

                Console.WriteLine("Connected");
                SendThread = new Thread(new ThreadStart(SendFunction));
                SendThread.Start();
                ReceiveThread = new Thread(new ThreadStart(ReceiveFunction));
                ReceiveThread.Start();
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void SendFunction()
        {
            byte[] Data = new byte[1024];
            while (true)
            {
                Console.Write("Enter the string to be transmitted : ");
                String str = Console.ReadLine();
                Data = Encoding.ASCII.GetBytes(str);

                Console.WriteLine("Transmitting.....");
                tcpSocket.Send(Data);
            }
        }

        static void ReceiveFunction()
        {
            byte[] ReceivedData = new byte[1024];
            tcpSocket.Receive(ReceivedData);
            Console.WriteLine(Encoding.ASCII.GetString(ReceivedData));
        }

    }
}
