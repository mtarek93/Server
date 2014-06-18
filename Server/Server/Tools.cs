﻿using System;
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
        public static SortedDictionary<ushort, User> CurrentUserList = new SortedDictionary<ushort, User>();
        public static SortedDictionary<ushort, Device> CurrentDeviceList = new SortedDictionary<ushort, Device>();

        public static void AcceptConnection(object _Socket)
        {
            //Initialization----------------------------------------------------------------------------
            Socket S = (Socket)_Socket;
            string Command;
            Command Cmd;
            CommandParser.InitializeCommandsDictionary();
            byte[] ReceivedData = new byte[100];

            //Recieving and parsing a command -----------------------------------------------------------
            S.Receive(ReceivedData);
            Command = ByteArrayToString(ReceivedData);
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
            return (System.Text.Encoding.ASCII.GetString(Data));
        }
     
        public static ushort AssignID()
        {
            ushort ID = 0;
            bool AssignedIDBefore;
            AssignedIDBefore = DatabaseHandler.GetLatestAssignedID(out ID);

            if (AssignedIDBefore)
                return ++ID;
            else
                return ID;
        }
        
        public static IPAddress GetMyIPAddress()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }

        public static void DisplayServerStatus()
        {
            while (true)
            {
                Console.WriteLine("Number of Users: " + Tools.CurrentUserList.Count.ToString());     ///
                Console.WriteLine("Number of Devices: " + Tools.CurrentDeviceList.Count.ToString());     ///
                Thread.Sleep(10000);
            }
        }
    }
}
