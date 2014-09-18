using System;
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

                tcpSocket.Connect("192.168.1.3", 14);
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
            byte[] Data = null;
            string initialString = "5,\0\0,,,mt,mt";
            initialString = PrependLength(initialString);
            tcpSocket.Send(Encoding.GetEncoding(437).GetBytes(initialString));

            while (true)
            {
                Console.Write("Enter the string to be transmitted : ");
                String str = Console.ReadLine();
                //string str = "8,\0\0,\0\0," + Convert.ToChar((byte)255) + ",,";
                //str = str.Length.ToString() + str;
                Data = ReformatCommand(str);
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
                Console.WriteLine(Encoding.GetEncoding(437).GetString(Data));
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

        static string CreateActionString(string ID, string DestinationID, string Action)
        {
            string ActionString = "8," + ushortToString(Convert.ToUInt16(ID)) + "," + ushortToString(Convert.ToUInt16(DestinationID)) + "," + (char)Convert.ToByte(Action) + ",,";
            ActionString = ActionString.Length.ToString() + ActionString;
            return ActionString;
        }

        static string ushortToString(ushort Number)
        {
            return Encoding.GetEncoding(437).GetString(BitConverter.GetBytes(Number));
        }

        static string PrependLength(string Command)
        {
            int CommandLength = Command.Length;
            if (CommandLength < 10)
                return "0" + CommandLength.ToString() + Command;
            else
                return CommandLength.ToString() + Command;
        }

        static byte[] ReformatCommand(string Command)
        {
            string[] SplittedCommand;
            ushort ID, DestID;
            byte State;
            string IDstr, DestIDstr, Statestr, FormattedCommand = "";

            SplittedCommand = Command.Split(',');
            UInt16.TryParse(SplittedCommand[1], out ID);
            UInt16.TryParse(SplittedCommand[2], out DestID);
            Byte.TryParse(SplittedCommand[3], out State);

            IDstr = ushortToString(ID);
            DestIDstr = ushortToString(DestID);
            Statestr = Convert.ToChar(State).ToString();

            SplittedCommand[1] = IDstr;
            SplittedCommand[2] = DestIDstr;
            SplittedCommand[3] = Statestr;

            foreach (string s in SplittedCommand)
                FormattedCommand += s + ",";
            FormattedCommand = FormattedCommand.Remove(FormattedCommand.Length - 1);
            FormattedCommand = PrependLength(FormattedCommand);

            return Encoding.GetEncoding(437).GetBytes(FormattedCommand);
        }
    }
}
