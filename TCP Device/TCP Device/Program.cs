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
using System.Text.RegularExpressions;

namespace TCP_Device
{
    public class Client
    {
        static Socket tcpSocket;
        static Thread SendThread, ReceiveThread;
        public static void Main()
        {
            string ID;
            try
            {
                Console.WriteLine("Connecting.....");
                tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tcpSocket.Connect("10.32.30.229", 14);
                Console.WriteLine("Connected");

                ID = RequestName();
                Console.WriteLine("Name received: " + ID);

                SendThread = new Thread(new ParameterizedThreadStart(SendWatchDog));
                SendThread.Start(ID);

                ReceiveThread = new Thread(new ThreadStart(ReceiveFunction));
                ReceiveThread.Start();

            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static string RequestName()
        {
            string Message = "060,,,,,";
            string Name = "";
            tcpSocket.Send(StringtoByteArray(Message)); 

            byte[] Data = null;
            if (Receive(tcpSocket, ref Data))
            {
                byte[] NameBytes = new byte[2];
                NameBytes[0] = Data[3];
                NameBytes[1] = Data[4];
                Name = ByteArrayToString(NameBytes);
                return Name;
            }
            else
            {
                Console.WriteLine("Name not received!");
                return Name;
            }
        }

        static void SendNameFunction(object _ID)
        {
            string ID = (string)_ID;
            byte[] Data = null;
            string nameString = "081," + ID + ",,,,";
            Data = Encoding.GetEncoding(437).GetBytes(nameString);
            Console.WriteLine("Transmitting.....");
            tcpSocket.Send(Data);
        }

        static void ReceiveFunction()
        {
            while (true)
            {
                byte[] Data = null;
                if (Receive(tcpSocket, ref Data))
                    Console.WriteLine("Received Command: " + Encoding.GetEncoding(437).GetString(Data));
                else
                {
                    Console.WriteLine("Disconnected from Server!");
                    break;
                }
            }
        }

        static void SendWatchDog(object _ID)
        {
            string ID = (string)_ID;
            while (true)
            {
                byte[] Data = Encoding.GetEncoding(437).GetBytes("082," + ID + ",,,,");
                Console.WriteLine("watchdogSent");
                tcpSocket.Send(Data);
                Thread.Sleep(4000);
            }
        }

        static bool Receive(Socket S, ref byte[] Data)
        {
            Data = new byte[11];
            int totalBytes = 0, bytesReceived = 0;

            try
            {
                //Receive the data
                totalBytes = 0;
                bytesReceived = totalBytes = S.Receive(Data, 0, Data.Length, SocketFlags.None);

                while (totalBytes < Data.Length && bytesReceived > 0)
                {
                    bytesReceived = S.Receive(Data, totalBytes, Data.Length - totalBytes, SocketFlags.None);
                    totalBytes += bytesReceived;
                }

                return true;
            }

            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        static byte[] StringtoByteArray(string s)
        {
            return Encoding.GetEncoding(437).GetBytes(s);
        }

        static string ByteArrayToString(byte[] Data)
        {
            return Encoding.GetEncoding(437).GetString(Data);
        }
    }
}
