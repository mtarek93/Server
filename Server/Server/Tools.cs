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

namespace ServerTools
{
    class Tools
    {
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
            //name = length of database;     //ex: if database has 0,1 then name =2 (length of database)
            return ID;
        }
    }
}
