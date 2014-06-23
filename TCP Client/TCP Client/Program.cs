﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

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
                tcpSocket.Connect("192.168.1.7", 14);
                // use the ipaddress as in the server program

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
                str = Regex.Unescape(str);    //to unescape escape sequences
                Data = Encoding.ASCII.GetBytes(str);

                Console.WriteLine("Transmitting.....");
                tcpSocket.Send(Data);
            }
        }

        static void ReceiveFunction()
        {
            while (true)
            {
                byte[] ReceivedData = new byte[1024];
                byte[] Data;
                int NumberofBytes = tcpSocket.Receive(ReceivedData);
                Data = FormatData(ReceivedData, NumberofBytes);
                Console.WriteLine(Encoding.ASCII.GetString(Data));
            }
        }

        static byte[] FormatData(byte[] Data, int NumberofReceivedBytes)
        {
            byte[] FormattedData = new byte[NumberofReceivedBytes];
            for (int i = 0; i < NumberofReceivedBytes; i++)
            {
                FormattedData[i] = Data[i];
            }
            return FormattedData;
        }

    }
}
