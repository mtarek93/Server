using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ServerTools
{
    class Tools
    {
        public static void Send1(object _Socket)
        {
            string D;
            D = "Message Sent !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!: 1";
            byte[] Data = StringToByteArray(D);
            Socket S = _Socket as Socket;
            while (true)
            {
                Console.WriteLine("Sending Message: 1" );
                S.Send(Data);
                Thread.Sleep(50);
            }
        }
        public static void Send2(object _Socket)
        {
            string D;
            D = "Message Sent !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!: 2";
            byte[] Data = StringToByteArray(D);
            Socket S = _Socket as Socket;
            while (true)
            {
                Console.WriteLine("Sending Message: 2" );
                S.Send(Data);
                Thread.Sleep(25);
            }

        }
        public static byte[] StringToByteArray(string data)
        {
            return Encoding.GetEncoding(437).GetBytes(data);      
        } 
        public static IPAddress GetMyIPAddress()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }
    }
}
