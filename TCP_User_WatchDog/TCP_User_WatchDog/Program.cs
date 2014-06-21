using System;
using System.Timers;
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
        static Thread SendThread, ReceiveThread, WatchdogThread;
        public static void Main()
        {
            try
            {
                Console.Write("Enter Device ID: ");
                string ID = Console.ReadLine();

                Console.WriteLine("Connecting.....");

                tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tcpSocket.Connect("192.168.1.4", 14);
                // use the ipaddress as in the server program

                Console.WriteLine("Connected");
                SendThread = new Thread(new ParameterizedThreadStart(SendFunction));
                SendThread.Start(ID);
                ReceiveThread = new Thread(new ThreadStart(ReceiveFunction));
                ReceiveThread.Start();

            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        static void SendFunction(object _ID)
        {
            string ID = (string)_ID;
            byte[] Data = new byte[1024];
            while (true)
            {
                Console.Write("Enter the string to be transmitted : ");
                String str = Console.ReadLine();
                if (str.ElementAt(0) == 'w')
                {
                    WatchdogThread = new Thread(new ParameterizedThreadStart(WatchdogFunction));
                    WatchdogThread.Start(ID);
                }
                else
                {
                    Data = Encoding.ASCII.GetBytes(str);
                    Console.WriteLine("Transmitting.....");
                    tcpSocket.Send(Data);
                }
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
                Console.WriteLine(" ");
                Console.WriteLine("Received Command: " + Encoding.ASCII.GetString(Data));
            }
        }
        static void WatchdogFunction(object _ID)
        {
            string ID = (string)_ID;
            while (true)
            {
                byte []Data = Encoding.ASCII.GetBytes("2,"+ ID +",,,,.");
                Console.WriteLine("watchdogSent");
                tcpSocket.Send(Data);
                Thread.Sleep(4000);
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
