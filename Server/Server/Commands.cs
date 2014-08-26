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
using ConnectionManager;

namespace CommandHandler
{
    class User_FirstConnection_SignIn : Command
    {
        public User_FirstConnection_SignIn()
        {
            Type = CommandType.User_FirstConnection_SignIn;
        }
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
        public User_FirstConnection_SignUp()
        {
            Type = CommandType.User_FirstConnection_SignUp;
        }
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
        public User_Reconnection_SignIn()
        {
            Type = CommandType.User_Reconnection_SignIn;
        }
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
        public User_Reconnection_SignUp()
        {
            Type = CommandType.User_Reconnection_SignUp;
        }
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
        public User_Action()
        {
            Type = CommandType.User_Action;
        }
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
        public User_Locate()
        {
            Type = CommandType.User_Locate;
        }
    }
    class Device_FirstConnection : Command
    {
        public Device_FirstConnection()
        {
            Type = CommandType.Device_FirstConnection;
        }

        public override bool Execute(Socket DeviceSocket)
        {
            //Assign name for device and add to database
            ushort AssignedName = DatabaseHandler.AddNewDevice();
            Device D = new Device(AssignedName, DeviceSocket, Action_State);

            Console.WriteLine("New name assigned to Device!");
            Console.WriteLine("Device Name: " + D.GetName());

            //Add Device to current devices list, and update users' lists
            ConnectionManager.DeviceConnection.Add_Device (D);

            //Name notification message to device
            byte[] Message = ConnectionManager.DeviceConnection.CreateNewNameMessage(AssignedName);
            if (!D.Send(Message))
                Console.WriteLine("Device.StartConnection (First Connection_ NewName): Send Failed");

            D.StartTimer();
            D.HandleConnection();
        }
    }
    class Device_Reconnection : Command
    {
        public Device_Reconnection()
        {
            Type = CommandType.Device_Reconnection;
        }
        public override bool Execute(Socket DeviceSocket)
        {
            //if device is on the database: reconnect.........................................a
            Device D;
            if (DatabaseHandler.TryGetDevice(SourceID, out D))
            {
                Console.WriteLine("Name exists in database!");
                Console.WriteLine("Connection accepted from Device " + D.GetName());

                D.BindSocket(DeviceSocket);
                D.SetState(Action_State);

                //Add device to list and update users' lists
                ConnectionManager.DeviceConnection.Add_Device(D);

                D.StartTimer();
                HandleConnection(D);
            }

            //if not: assign new name, add to database, send NewName command to device.......b
            else
            {
                string AssignedName;
                Console.WriteLine("Name: " + Cmd.SourceID + " Doesn't exist in Database!");
                AssignedName = DatabaseHandler.AddNewDevice();
                Console.WriteLine("New name assigned to Device!");
                Console.WriteLine("Device Name: " + AssignedName);

                D = new Device(AssignedName, DeviceSocket, Cmd.Action_State);

                //Add device to list and update users' lists
                ConnectionManager.DeviceConnection.Add_Device(D);

                //Send NewName message
                byte[] Message = ConnectionManager.DeviceConnection.CreateChangeNameMessage(Cmd.SourceID, AssignedName);
                if (!D.Send(Message))
                    Console.WriteLine("Device.StartConnection (reconnect_ChangeName) :Send failed");

                D.StartTimer();
                HandleConnection(D);
            }
        }
    }
    class Device_WatchDog : Command 
    {
        public Device_WatchDog()
        {
            Type = CommandType.Device_WatchDog;
        }

        public override bool Execute(Socket DeviceSocket)
        {
            Console.WriteLine("WatchDog recieved from device: " + SourceID);
            Device D;
            Tools.CurrentDeviceList.TryGetValue(SourceID, out D);
            D.resetTimer();
            /*
            if (D.GetState() != Cmd.Action_State)
            {
                //Update state of device
                D.SetState(Cmd.Action_State);  

                //Update current list and update users' lists
                Update_State(D);
            }
            */
            return true;
        }
    }
    class Device_Acknowledgement : Command
    {
        public Device_Acknowledgement()
        {
            Type = CommandType.Device_Acknowledgement;
        }

        public override bool Execute(Socket DeviceSocket)
        {
            User U;
            bool flag;
            string msg;

            Device D;
            Tools.CurrentDeviceList.TryGetValue(SourceID, out D);
            //Update state of device
            D.SetState(Action_State);

            //Update current list and update users' lists
            ConnectionManager.DeviceConnection.Update_State(D);

            //Not necessary anymore !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //if User is in current users list
            if (Tools.CurrentUserList.TryGetValue(DestinationID, out U))
            {
                msg = Convert.ToString(DestinationID) + ',' + Convert.ToString(SourceID) + ',' + Convert.ToString(Action_State) + '.';
                if (!U.Send(Encoding.GetEncoding(437).GetBytes(msg)))
                    Console.WriteLine("Device_Acknowledgement_Action: Send failed");
                flag = true;
            }

            //if user not in current users list
            else
            {
                Console.WriteLine("Error in DeviceConnection.Device_Acknowledgement_Action: User doesn't exist in database!");
                Console.WriteLine("Acknowledgement not sent");
                flag = false;
            }
            //Down to here !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            return flag;
        }
    }
    class Invalid : Command
    {
        public Invalid()
        {
            Type = CommandType.Invalid;
        }
        public override bool Execute(Socket S)
        {
            Console.WriteLine("Invalid Command!");
            return false;
        }
    }
}
