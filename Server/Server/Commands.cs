using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Database;
using ServerTools;
using Clients;

namespace CommandHandler
{
    class User_FirstConnection_SignIn : Command
    {
        public override bool Execute(Socket UserSocket)
        {
            if (DatabaseHandler.UserIsAuthenticated(UserName, Password))
            {
                //Assign ID to user's device and add to database
                ushort AssignedID = DatabaseHandler.AddNewUser();

                //Add to current list
                User U = new User(AssignedID, UserSocket);
                Tools.CurrentUserList.Add(AssignedID, U);

                //Send assignedID, devicelist, and wait for new commands in HandleConnection()
                U.Send(Encoding.GetEncoding(437).GetBytes("Login Successful!," + AssignedID.ToString() + "."));
                U.SendDeviceList();
                U.HandleConnection();
                return true;
            }
            else
            {
                UserSocket.Send(Encoding.GetEncoding(437).GetBytes("Invalid credentials."));
                return false;
            }
        }   
    }
    class User_FirstConnection_SignUp : Command
    {
        public override bool Execute (Socket UserSocket)
        {
            //Check if username exists
            if (DatabaseHandler.UsernameExists(UserName))
            {
                UserSocket.Send(Encoding.GetEncoding(437).GetBytes("Username already exists!"));
                return false;
            }
            else
            {
                //Assign ID to user's device and add to database
                ushort AssignedID = DatabaseHandler.AddNewUser();

                //Create new user account
                DatabaseHandler.AddUserAccount(UserName, Password);

                //Add to current list
                User U = new User(AssignedID, UserSocket);
                Tools.CurrentUserList.Add(AssignedID, U);

                //Send assignedID, devicelist, and wait for new commands in HandleConnection()
                U.Send(Encoding.GetEncoding(437).GetBytes("Sign Up Successful!," + AssignedID.ToString() + "."));
                U.SendDeviceList();
                U.HandleConnection();
                return true;
            }
        }
    }
    class User_Reconnection_SignIn : Command
    {
        public override bool Execute(Socket UserSocket)
        {
            User U;
            //if user's device has connected before
            if (DatabaseHandler.TryGetUser(SourceID, out U))
            {
                if (DatabaseHandler.UserIsAuthenticated(UserName, Password))
                {
                    U.BindSocket(UserSocket);
                    Tools.CurrentUserList.Add(SourceID, U);
                    U.Send(Encoding.GetEncoding(437).GetBytes("Login Successfull!"));
                    U.SendDeviceList();
                    U.HandleConnection();
                    return true;
                }
                else
                {
                    UserSocket.Send(Encoding.GetEncoding(437).GetBytes("Invalid credentials."));
                    return false;
                }
            }
            else
            {
                UserSocket.Send(Encoding.GetEncoding(437).GetBytes("This device has not connected to the server before."));
                return false;
            }
        }
    }
    class User_Reconnection_SignUp : Command
    {
        public override bool Execute(Socket UserSocket)
        {
            User U;

            //if user's device has connected before
            if (DatabaseHandler.TryGetUser(SourceID, out U))
            {
                if (DatabaseHandler.UsernameExists(UserName))
                {
                    UserSocket.Send(Encoding.GetEncoding(437).GetBytes("Username already exists!"));
                    return false;
                }
                else
                {
                    DatabaseHandler.AddUserAccount(UserName, Password);

                    //Bind new socket and add to current list
                    U.BindSocket(UserSocket);
                    Tools.CurrentUserList.Add(SourceID, U);

                    //Send DeviceList, and wait for new commands in HandleConnection()
                    U.Send(Encoding.GetEncoding(437).GetBytes("Sign Up Successful!"));
                    U.SendDeviceList();
                    U.HandleConnection();
                    return true;
                }
            }
            else
            {
                UserSocket.Send(Encoding.GetEncoding(437).GetBytes("This device has not connected to the server before."));
                return false;
            }
        }
    }
    class User_Action : Command
    {
        public override bool Execute(Socket UserSocket)
        {
            Device D;
            //String to be sent to device
            string ActionString = "";

            //if destination device is currently connected
            if (Tools.CurrentDeviceList.TryGetValue(DestinationID, out D))
            {
                ActionString = ".2," + Tools.ushortToString(SourceID) + "," + Tools.ushortToString(DestinationID) + "," + Convert.ToChar(Action_State) + ".";
                ActionString = ActionString.Length.ToString() + ActionString;
                D.Send(Encoding.GetEncoding(437).GetBytes(ActionString));
                return true;
            }
            else
            {
                UserSocket.Send(Encoding.GetEncoding(437).GetBytes("Device not connected!"));
                return false;
            }
        }
    }
    class User_Locate : Command
    {
    }
    class Device_FirstConnection : Command
    {
    }
    class Device_Reconnection : Command
    {
    }
    class Device_WatchDog : Command 
    {
    }
    class Device_Acknowledgement : Command
    {
    }
    class Invalid : Command
    {
        public override bool Execute(Socket S)
        {
            Console.WriteLine("Invalid Command!");
            return false;
        }
    }
}
