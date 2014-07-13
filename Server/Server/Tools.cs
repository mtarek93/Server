using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using CommandHandler;
using ConnectionManager;
using Clients;
using Database;
using System.Threading;

namespace ServerTools
{
    class Tools
    {
        //Lists of current Devices and Users---------------------------------------------------------------------------
        public static SynchronizedDictionary<ushort, User> CurrentUserList = new SynchronizedDictionary<ushort, User>();
        public static SynchronizedDictionary<ushort, Device> CurrentDeviceList = new SynchronizedDictionary<ushort, Device>();

        public static void AcceptConnection(object _Socket)
        {
            //Initialization----------------------------------------------------------------------------
            Socket S = (Socket)_Socket;
            string Command;
            Command Cmd;
            byte[] Data = null;

            //Recieving and parsing a command -----------------------------------------------------------
            if (Receive(S, ref Data))
            {
                Command = ByteArrayToString(Data);
                Console.WriteLine("Tools.AcceptConnection: Command received was: " + Command);
                Cmd = CommandParser.ParseCommand(Command);

                if (Cmd.Type == CommandType.Device_FirstConnection || Cmd.Type == CommandType.Device_Reconnection)
                    DeviceConnection.StartConnection(S, Cmd);
                else if (Cmd.Type == CommandType.User_FirstConnection_SignIn || Cmd.Type == CommandType.User_Reconnection_SignIn ||
                         Cmd.Type == CommandType.User_FirstConnection_SignUp || Cmd.Type == CommandType.User_Reconnection_SignUp)
                    UserConnection.StartConnection(S, Cmd);
                else
                    Console.WriteLine("Tools.AcceptConnection: Invalid command, Connection not accepted!");
            }
            else
            {
                Console.WriteLine("Receive failed! Tools.AcceptConnection: Connection not accepted!");
            }
        }
        public static string ByteArrayToString(byte[] Data)
        {
            return (Encoding.GetEncoding(437).GetString(Data));
        }
        public static string ushortToString(ushort Number)
        {
            return Encoding.GetEncoding(437).GetString(BitConverter.GetBytes(Number));
        }
        public static byte[] StringToByteArray(string data)
        {
            return Encoding.GetEncoding(437).GetBytes(data);      
        } 
        public static IPAddress GetMyIPAddress()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }
        public static void DisplayServerStatus()
        {
            while (true)
            {
                Console.WriteLine("Number of Users: " + CurrentUserList.Count.ToString());    
                Console.WriteLine("Number of Devices: " + CurrentDeviceList.Count.ToString());    
                Thread.Sleep(5000);
            }
        }
        static bool Receive(Socket S, ref byte[] Data)
        {
            const int MAX_COMMAND_LENGTH = 99;
            byte[] commandLengthBuffer = new byte[2];
            int totalBytes = 0, bytesReceived = 0, commandLength;

            try
            {
                //Start receiving command length
                bytesReceived = totalBytes = S.Receive(commandLengthBuffer);

                //Recieve upto the length prefix
                while (bytesReceived < commandLengthBuffer.Length && bytesReceived > 0)
                {
                    bytesReceived = S.Receive(commandLengthBuffer, totalBytes, commandLengthBuffer.Length - totalBytes, SocketFlags.None);
                    totalBytes += bytesReceived;
                    string s = Encoding.GetEncoding(437).GetString(Data);
                }

                //Get the command length from prefix
                commandLength = Convert.ToInt32(Encoding.GetEncoding(437).GetString(commandLengthBuffer));
                Console.WriteLine("Length = " + commandLength.ToString());
                //Check for commandLength maximum and create buffer to receive data
                if (commandLength > MAX_COMMAND_LENGTH)
                    Data = new byte[MAX_COMMAND_LENGTH];
                else
                    Data = new byte[commandLength];

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
    }
}
