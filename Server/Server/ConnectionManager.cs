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
                            U.Send(Encoding.ASCII.GetBytes("Invalid Command!"));
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
                //Assign ID to user's device
                ushort AssignedID = Tools.AssignID();

                //Add to database and current list
                User U = new User(AssignedID, UserSocket);
                Tools.CurrentUserList.Add(AssignedID, U);
                DatabaseHandler.AddNewUser(AssignedID);
                
                //Send assignedID, devicelist, and wait for new commands in HandleConnection()
                U.Send(Encoding.ASCII.GetBytes("Login Successful!," + AssignedID.ToString() + "."));
                SendDeviceList(U);
                HandleConnection(U);
            }
            else
            {
                UserSocket.Send(Encoding.ASCII.GetBytes("Invalid credentials."));
                return;
            }
        }
        private static void FirstConnection_SignUp(Socket UserSocket, Command Cmd)
        {
            //Check if username exists
            if (DatabaseHandler.UsernameExists(Cmd.UserName))
            {
                UserSocket.Send(Encoding.ASCII.GetBytes("Username already exists!"));
                return;
            }
            else
            {
                //Assign ID to user's device
                ushort AssignedID = Tools.AssignID();

                //Create new user account
                DatabaseHandler.AddUserAccount(Cmd.UserName, Cmd.Password);

                //Add to database and current list
                User U = new User(AssignedID, UserSocket);
                Tools.CurrentUserList.Add(AssignedID, U);
                DatabaseHandler.AddNewUser(AssignedID);

                //Send assignedID, devicelist, and wait for new commands in HandleConnection()
                U.Send(Encoding.ASCII.GetBytes("Sign Up Successful!," + AssignedID.ToString() + "."));
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
                    U.Send(Encoding.ASCII.GetBytes("Login Successfull!"));
                    SendDeviceList(U);
                    HandleConnection(U);
                }
                else
                {
                    UserSocket.Send(Encoding.ASCII.GetBytes("Invalid credentials."));
                    return;
                }
            }
            else
            {
                UserSocket.Send(Encoding.ASCII.GetBytes("This device has not connected to the server before."));
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
                    UserSocket.Send(Encoding.ASCII.GetBytes("Username already exists!"));
                    return;
                }
                else
                {
                    DatabaseHandler.AddUserAccount(Cmd.UserName, Cmd.Password);

                    //Bind new socket and add to current list
                    U.BindSocket(UserSocket);
                    Tools.CurrentUserList.Add(Cmd.SourceID, U);

                    //Send DeviceList, and wait for new commands in HandleConnection()
                    U.Send(Encoding.ASCII.GetBytes("Sign Up Successful!"));
                    SendDeviceList(U);
                    HandleConnection(U);
                }
            }
            else
            {
                UserSocket.Send(Encoding.ASCII.GetBytes("This device has not connected to the server before."));
                return;
            }
        }
        private static void SendDeviceList(User U)
        {
            string DeviceList = "";
            foreach (var Device in Tools.CurrentDeviceList)
                DeviceList += (Device.Key + Device.Value.GetState().ToString());    //state is assumed to be off for now.....

            DeviceList += ".";
            U.Send(Encoding.ASCII.GetBytes(DeviceList));
        }
        private static void Send_Action(User U, Command Cmd)
        {
            Device D;

            //if destination device is currently connected
            if (Tools.CurrentDeviceList.TryGetValue(Cmd.DestinationID, out D))
                D.Send(Encoding.ASCII.GetBytes("1," + Cmd.SourceID.ToString() + Cmd.Action_State + "."));
            else
                U.Send(Encoding.ASCII.GetBytes("Device not connected!"));
        }
        private static void Locate(Command Cmd)
        {
        }
    }
    class DeviceConnection
    {   
        //Connection Managing functions----------------------------------------------------------------
        public static void StartConnection(Socket DeviceSocket, Command Cmd)
        {
            ushort AssignedName;

            //if Command was reconnect-------------------------------------------------------------------1
            if (Cmd.Type == CommandType.Device_Reconnection)
            {
                //if device is on the database: reconnect
                Device D;
                if (DatabaseHandler.TryGetDevice(Cmd.SourceID, out D))
                {
                    Console.WriteLine("Name exists in database!");
                    Console.WriteLine("Connection accepted from Device " + D.GetName());

                    //bind new socket, add to current devices list, and start watchdog timer
                    D.BindSocket(DeviceSocket);
                    Tools.CurrentDeviceList.Add(D.GetName(), D);
                    D.StartTimer();

                    HandleConnection(D);
                }
                else
                {
                    Console.WriteLine("Name: " + Cmd.SourceID + " Doesn't exist in Database!");
                    AssignedName = Tools.AssignID();
                    Console.WriteLine("New name assigned to Device!");
                    Console.WriteLine("Device Name: " + AssignedName);

                    D.ChangeName(AssignedName);
                    D.BindSocket(DeviceSocket);

                    Tools.CurrentDeviceList.Add(AssignedName, D);
                    DatabaseHandler.AddNewDevice(AssignedName, 0); 

                    byte[] Message = (Encoding.ASCII.GetBytes(".3," + Cmd.SourceID+ AssignedName.ToString() + ",M."));
                    D.Send(Message);

                    D.StartTimer();
                    HandleConnection(D);

                }
            }

            //if Command was FirstConnection-------------------------------------------------------------2
            else if (Cmd.Type == CommandType.Device_FirstConnection)
            {
                //Assign name for device
                AssignedName = Tools.AssignID();
                Device D = new Device(AssignedName, DeviceSocket);

                Console.WriteLine("New name assigned to Device!");
                Console.WriteLine("Device Name: " + D.GetName());

                //Add Device to both current devices list and database
                Tools.CurrentDeviceList.Add(AssignedName, D);
                DatabaseHandler.AddNewDevice(AssignedName, 0); //assuming state is off for now

                //Name notification message to device
                byte[] Message = CreateNewNameMessage(AssignedName);
                D.Send(Message);

                //Add to current devices list and start watchdog timer    
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
                                Device_Acknowledgement_Action(Cmd);
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
                    //Console.WriteLine("Device: " + D.GetName() + " was disconnected");
                    //Tools.CurrentDeviceList.Remove(D.GetName());
                    break;
                }
            }
        }     
        //Actions--------------------------------------------------------------------------------------
        private static bool Device_Acknowledgement_Action(Command Cmd)
        {
            User U;
            bool flag = false;
            string msg;

            //if User is in current users list
            if (Tools.CurrentUserList.TryGetValue(Cmd.DestinationID, out U))
            {
                msg = Convert.ToString(Cmd.DestinationID) + ',' + Convert.ToString(Cmd.SourceID) + ',' + Convert.ToString(Cmd.Action_State) + '.';
                U.Send(Encoding.ASCII.GetBytes(msg));
                Console.WriteLine("State sent to User: "+ Cmd.DestinationID+ " From Device: "+ Cmd.SourceID);
                flag = true;
            }

            //if user not in current users list
            else
            {
                Console.WriteLine("Error in DeviceCinnection.Device_Acknowledgement_Action: User doesn't exist in database!");
                Console.WriteLine("Acknowledgement not sent");
            }

            return flag;
        }
        private static bool Device_WatchDog_Action(Command Cmd, Device D)
        {
            Console.WriteLine("WatchDog recieved from device: " + Cmd.SourceID);
            D.resetTimer();
            return true;
        }

        private static byte[] CreateNewNameMessage(ushort Name)
        {
            var ByteList = new List<byte>();
            ByteList.Add(Convert.ToByte('.'));
            ByteList.Add(Convert.ToByte('1'));
            ByteList.Add(Convert.ToByte(','));

            byte[] NameAsBytes = BitConverter.GetBytes(Name);
            for (int i = 0; i < NameAsBytes.Length; i++)
                ByteList.Add(NameAsBytes[i]);

            ByteList.Add(Convert.ToByte(','));
            ByteList.Add(Convert.ToByte('2'));
            ByteList.Add(Convert.ToByte('3'));
            ByteList.Add(Convert.ToByte(','));
            ByteList.Add(Convert.ToByte('M'));
            ByteList.Add(Convert.ToByte('.'));
            byte[] Result = ByteList.ToArray();
            //string s = Encoding.ASCII.GetString(Result);
            return Result;
        }
    }
}
