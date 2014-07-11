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
            byte[] ReceiveBuffer = new byte[1024];
            byte[] Data;
            int NumberofReceivedBytes = 0;

            //Recieving and parsing a command -----------------------------------------------------------
            NumberofReceivedBytes = S.Receive(ReceiveBuffer);
            Data = FormatData(ReceiveBuffer, NumberofReceivedBytes);

            Command = ByteArrayToString(Data);
            Console.WriteLine("Tools.AcceptConnection: Command received was: " + Command);
            Cmd = CommandParser.ParseCommand(Command);

            if (Cmd.Type == CommandType.Device_FirstConnection || Cmd.Type == CommandType.Device_Reconnection)
                DeviceConnection.StartConnection(S, Cmd);
            else if (Cmd.Type == CommandType.User_FirstConnection_SignIn || Cmd.Type == CommandType.User_Reconnection_SignIn ||
                     Cmd.Type == CommandType.User_FirstConnection_SignUp || Cmd.Type == CommandType.User_Reconnection_SignUp)
                UserConnection.StartConnection(S, Cmd);
            else
                Console.WriteLine("Tools.AcceptConnection: Connection not accepted!");
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
        public static byte[] FormatData(byte[] Data, int NumberofReceivedBytes)
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
