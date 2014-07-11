using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using CommandHandler;
using Clients;
using ServerTools;
using Database;

namespace ConnectionManager
{
    class UserConnection
    {
        public static void StartConnection(Socket UserSocket, Command Cmd)
        {
            switch (Cmd.Type)
            {
                case CommandType.User_FirstConnection_SignIn:
                    FirstConnection_SignIn(UserSocket, Cmd);
                    break;
                case CommandType.User_FirstConnection_SignUp:
                    FirstConnection_SignUp(UserSocket, Cmd);
                    break;
                case CommandType.User_Reconnection_SignIn:
                    Reconnection_SignIn(UserSocket, Cmd);
                    break;
                case CommandType.User_Reconnection_SignUp:
                    Reconnection_SignUp(UserSocket, Cmd);
                    break;
            }
        }
        private static void HandleConnection(User U)
        {
            Command Cmd;
            byte [] ReceivedData = new byte[100];
            while (true)
            {
                if (U.Receive(ReceivedData))
                {
                    Console.WriteLine("Command Received: " + Tools.ByteArrayToString(ReceivedData));
                    Cmd = CommandParser.ParseCommand(Tools.ByteArrayToString(ReceivedData));
                    switch (Cmd.Type)
                    {
                        case CommandType.User_Action:
                            Send_Action(U, Cmd);
                            break;
                        case CommandType.User_Locate:
                            Locate(Cmd);
                            break;
                        default:
                            U.Send(Encoding.GetEncoding(437).GetBytes("Invalid Command!"));
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("User is disconnected!");
                    Tools.CurrentUserList.Remove(U.GetName());
                    break;
                }
            }
        }
        private static void FirstConnection_SignIn(Socket UserSocket, Command Cmd)
        {
            if (DatabaseHandler.UserIsAuthenticated(Cmd.UserName, Cmd.Password))
            {
                //Assign ID to user's device and add to database
                ushort AssignedID = DatabaseHandler.AddNewUser();

                //Add to current list
                User U = new User(AssignedID, UserSocket);
                Tools.CurrentUserList.Add(AssignedID, U);
                
                //Send assignedID, devicelist, and wait for new commands in HandleConnection()
                U.Send(Encoding.GetEncoding(437).GetBytes("Login Successful!," + AssignedID.ToString() + "."));
                SendDeviceList(U);
                HandleConnection(U);
            }
            else
            {
                UserSocket.Send(Encoding.GetEncoding(437).GetBytes("Invalid credentials."));
                return;
            }
        }
        private static void FirstConnection_SignUp(Socket UserSocket, Command Cmd)
        {
            //Check if username exists
            if (DatabaseHandler.UsernameExists(Cmd.UserName))
            {
                UserSocket.Send(Encoding.GetEncoding(437).GetBytes("Username already exists!"));
                return;
            }
            else
            {
                //Assign ID to user's device and add to database
                ushort AssignedID = DatabaseHandler.AddNewUser();

                //Create new user account
                DatabaseHandler.AddUserAccount(Cmd.UserName, Cmd.Password);

                //Add to current list
                User U = new User(AssignedID, UserSocket);
                Tools.CurrentUserList.Add(AssignedID, U);

                //Send assignedID, devicelist, and wait for new commands in HandleConnection()
                U.Send(Encoding.GetEncoding(437).GetBytes("Sign Up Successful!," + AssignedID.ToString() + "."));
                SendDeviceList(U);
                HandleConnection(U);
            }
        }
        private static void Reconnection_SignIn(Socket UserSocket, Command Cmd)
        {
            User U;

            //if user's device has connected before
            if (DatabaseHandler.TryGetUser(Cmd.SourceID, out U))
            {
                if (DatabaseHandler.UserIsAuthenticated(Cmd.UserName, Cmd.Password))
                {
                    U.BindSocket(UserSocket);
                    Tools.CurrentUserList.Add(Cmd.SourceID, U);
                    U.Send(Encoding.GetEncoding(437).GetBytes("Login Successfull!"));
                    SendDeviceList(U);
                    HandleConnection(U);
                }
                else
                {
                    UserSocket.Send(Encoding.GetEncoding(437).GetBytes("Invalid credentials."));
                    return;
                }
            }
            else
            {
                UserSocket.Send(Encoding.GetEncoding(437).GetBytes("This device has not connected to the server before."));
                return;
            }
        }
        private static void Reconnection_SignUp(Socket UserSocket, Command Cmd)
        {
            User U;

            //if user's device has connected before
            if (DatabaseHandler.TryGetUser(Cmd.SourceID, out U))
            {
                if (DatabaseHandler.UsernameExists(Cmd.UserName))
                {
                    UserSocket.Send(Encoding.GetEncoding(437).GetBytes("Username already exists!"));
                    return;
                }
                else
                {
                    DatabaseHandler.AddUserAccount(Cmd.UserName, Cmd.Password);

                    //Bind new socket and add to current list
                    U.BindSocket(UserSocket);
                    Tools.CurrentUserList.Add(Cmd.SourceID, U);

                    //Send DeviceList, and wait for new commands in HandleConnection()
                    U.Send(Encoding.GetEncoding(437).GetBytes("Sign Up Successful!"));
                    SendDeviceList(U);
                    HandleConnection(U);
                }
            }
            else
            {
                UserSocket.Send(Encoding.GetEncoding(437).GetBytes("This device has not connected to the server before."));
                return;
            }
        }
        private static void SendDeviceList(User U)
        {
            string DeviceList = "";
            foreach (var Device in Tools.CurrentDeviceList)
                DeviceList += (Device.Key + Device.Value.GetState().ToString());  

            DeviceList += ".";
            U.Send(Encoding.GetEncoding(437).GetBytes(DeviceList));
        }
        private static void Send_Action(User U, Command Cmd)
        {
            Device D;
            //String to be sent to device
            string ActionString = "";

            //if destination device is currently connected
            if (Tools.CurrentDeviceList.TryGetValue(Cmd.DestinationID, out D))
            {
                ActionString = ".2," + Tools.ushortToString(Cmd.SourceID) + "," + Tools.ushortToString(Cmd.DestinationID) + "," + Convert.ToChar(Cmd.Action_State) + ".";
                ActionString = ActionString.Length.ToString() + ActionString;
                D.Send(Encoding.GetEncoding(437).GetBytes(ActionString));
            }
            else
                U.Send(Encoding.GetEncoding(437).GetBytes("Device not connected!"));
        }
        private static void Locate(Command Cmd)
        {
        }
    }
    class DeviceConnection
    {   
        //Connection Managing functions------------------------------------------------------------
        public static void StartConnection(Socket DeviceSocket, Command Cmd)
        {
            ushort AssignedName;

//if Command was reconnect-------------------------------------------------------------------1
            if (Cmd.Type == CommandType.Device_Reconnection)
            {
        //if device is on the database: reconnect.........................................a
                Device D;
                if (DatabaseHandler.TryGetDevice(Cmd.SourceID, out D))
                {
                    Console.WriteLine("Name exists in database!");
                    Console.WriteLine("Connection accepted from Device " + D.GetName());

                    D.BindSocket(DeviceSocket);
                    D.SetState(Cmd.Action_State);

                    //Add device to list and update users' lists
                    Add_Device(D);

                    D.StartTimer();
                    HandleConnection(D);
                }

        //if not: assign new name, add to database, send NewName command to device.......b
                else
                {
                    Console.WriteLine("Name: " + Cmd.SourceID + " Doesn't exist in Database!");
                    AssignedName = DatabaseHandler.AddNewDevice();
                    Console.WriteLine("New name assigned to Device!");
                    Console.WriteLine("Device Name: " + AssignedName);

                    D = new Device(AssignedName, DeviceSocket, Cmd.Action_State);

                    //Add device to list and update users' lists
                    Add_Device(D);
                    
                    //Send NewName message
                    byte[] Message = CreateChangeNameMessage(Cmd.SourceID, AssignedName);
                    if (!D.Send(Message))
                        Console.WriteLine("Device.StartConnection (reconnect_ChangeName) :Send failed");

                    D.StartTimer();
                    HandleConnection(D);
                }
            }

//if Command was FirstConnection-------------------------------------------------------------2
            else if (Cmd.Type == CommandType.Device_FirstConnection)
            {
                //Assign name for device and add to database
                AssignedName = DatabaseHandler.AddNewDevice();
                Device D = new Device(AssignedName, DeviceSocket, Cmd.Action_State); 

                Console.WriteLine("New name assigned to Device!");
                Console.WriteLine("Device Name: " + D.GetName());

                //Add Device to current devices list, and update users' lists
                Add_Device(D);

                //Name notification message to device
                byte[] Message = CreateNewNameMessage(AssignedName);
                if(!D.Send(Message))
                    Console.WriteLine("Device.StartConnection (First Connection_ NewName): Send Failed");
   
                D.StartTimer();
                HandleConnection(D);
            }

//If Command format wasn't correct----------------------------------------------------------3
            else
            {
                Console.WriteLine("Error in DeviceConnection.Accept Connection: wrong command format ya teefa!");
                Console.WriteLine("Connection was not accepted");
            }
        }
        private static void HandleConnection(Device D)
        {
            //Initialization----------------------------------------------------------------------------0
            byte[] ReceivedData = new byte[10];
            string Command;         
            Command Cmd;                        

            while (true)
            {
                try
                {
                    //if command not recieved successfully
                    if (!D.Receive(ReceivedData))
                    {
                        //Console.WriteLine("Recieve(D): " + D.GetName() + " is disconnected!");
                        //Tools.CurrentDeviceList.Remove(D.GetName()); 
                        //break;
                    }
                    else
                    {
                        Command = Tools.ByteArrayToString(ReceivedData);
                        Console.WriteLine("Command received was: " + Command);
                        Cmd = CommandParser.ParseCommand(Command);

                        //Switching different Actions for different received Commands--------------------------------1
                        switch (Cmd.Type)
                        {
                            case CommandType.Device_Acknowledgement:
                                Device_Acknowledgement_Action(Cmd, D);
                                break;
                            case CommandType.Device_WatchDog:
                                Device_WatchDog_Action(Cmd, D);
                                break;
                            default:
                                Console.WriteLine("Error in DeviceConnection.HandleConnection: wrong command format ya teefa!");
                                break;
                        }
                    }
                }
                //Catching exceptions------------------------------------------------------------------------2
                catch (Exception e)
                {
                    Console.WriteLine("Exception in DeviceConnection.HandleConnection: " + e.Message);
                    Console.WriteLine("Device: " + D.GetName() + "is disconnected");
                    //Remove Device from list and update users' lists
                    Remove_Device(D);
                    break;
                }
            }
        }     
        //Actions----------------------------------------------------------------------------------
        private static bool Device_Acknowledgement_Action(Command Cmd, Device D)
        {
            User U;
            bool flag;
            string msg;

            //Update state of device
            D.SetState(Cmd.Action_State);

            //Update current list and update users' lists
            Update_State(D);

            //Not necessary anymore !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //if User is in current users list
            if (Tools.CurrentUserList.TryGetValue(Cmd.DestinationID, out U))
            {
                msg = Convert.ToString(Cmd.DestinationID) + ',' + Convert.ToString(Cmd.SourceID) + ',' + Convert.ToString(Cmd.Action_State) + '.';
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
        private static bool Device_WatchDog_Action(Command Cmd, Device D)
        {
            Console.WriteLine("WatchDog recieved from device: " + Cmd.SourceID);
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
        //Updates----------------------------------------------------------------------------------
        private static void Add_Device(Clients.Device D)
        {
            string CMD;
            byte[] Add_Cmd;
            string Device_Name = Tools.ushortToString(D.GetName());
            string Device_State = D.GetState().ToString();

            Tools.CurrentDeviceList.Add(D.GetName(),D);
            foreach (var User in Tools.CurrentUserList)
            {
                //9,UserID,A,	 ,DestID,State.!
                CMD = "9," + Tools.ushortToString(User.Value.GetName()) + ",A,," +
                    Device_Name + "," + Device_State + ".!";
                Add_Cmd = Tools.StringToByteArray(CMD);
                User.Value.GetSocket().Send(Add_Cmd);
            }
        }
        private static void Remove_Device(Clients.Device D)
        {
            string CMD;
            byte[] Add_Cmd;
            string Device_Name = Tools.ushortToString(D.GetName());
            string Device_State = D.GetState().ToString();

            Tools.CurrentDeviceList.Remove(D.GetName());
            foreach (var User in Tools.CurrentUserList)
            {
                //9,UserID,R,	 ,DestID,State.!
                CMD = "9," + Tools.ushortToString(User.Value.GetName()) + ",R,," +
                    Device_Name + "," + Device_State + ".!";
                Add_Cmd = Tools.StringToByteArray(CMD);
                User.Value.GetSocket().Send(Add_Cmd);
            }
        }
        private static void Update_State(Clients.Device D)
        {
            string CMD;
            byte[] Add_Cmd;
            string Device_Name = Tools.ushortToString(D.GetName());
            string Device_State = D.GetState().ToString();

            Tools.CurrentDeviceList[D.GetName()] = D;
            foreach (var User in Tools.CurrentUserList)
            {
                //9,UserID,C,	 ,DestID,State.!
                CMD = "9," + Tools.ushortToString(User.Value.GetName()) + ",C,," +
                    Device_Name + "," + Device_State + ".!";
                Add_Cmd = Tools.StringToByteArray(CMD);
                User.Value.GetSocket().Send(Add_Cmd);
            }
        }
        //tools------------------------------------------------------------------------------------
        private static byte[] CreateNewNameMessage(ushort Name)
        {
            string NameMessage = ".1," + Tools.ushortToString(Name) + ",23,M.";
            return Encoding.GetEncoding(437).GetBytes(NameMessage);
        }
        private static byte[] CreateChangeNameMessage(ushort OldName, ushort NewName)
        {
            string Message = ".3," + Tools.ushortToString(OldName) + "," + Tools.ushortToString(NewName) + ",M.";
            return Encoding.GetEncoding(437).GetBytes(Message);
        }
    }
}
