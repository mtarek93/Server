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
using LocationComponents;
using WifiLocalization;

namespace CommandHandler
{
    class User_FirstConnection_SignIn : Command
    {
        public User_FirstConnection_SignIn()
        {
            Type = CommandType.User_FirstConnection_SignIn;
        }
        public override void Execute(Socket UserSocket)
        {
            //Assign ID to user's device and add to database
            ushort AssignedID = DatabaseHandler.AddNewUser();
            int LoginID = DatabaseHandler.UserIsAuthenticated(UserName, Password);
            if (LoginID > 0)
            {
                //Add to current list
                User U = new User(AssignedID, UserSocket, LoginID);
                Tools.CurrentUserList.Add(AssignedID, U);

                //Send assignedID, devicelist, and wait for new commands in HandleConnection()
                U.Send(Encoding.GetEncoding(437).GetBytes("4," + Tools.ushortToString(AssignedID) + ",Y,,.!"));
                U.SendDeviceList();
                U.HandleConnection();
            }
            else
            {
                UserSocket.Send(Encoding.GetEncoding(437).GetBytes("4," + Tools.ushortToString(AssignedID) + ",N,,.!"));
                UserSocket.Shutdown(SocketShutdown.Both);
                UserSocket.Close();
            }
        }
    }
    class User_FirstConnection_SignUp : Command
    {
        public User_FirstConnection_SignUp()
        {
            Type = CommandType.User_FirstConnection_SignUp;
        }
        public override void Execute(Socket UserSocket)
        {
            //Assign ID to user's device and add to database
            ushort AssignedID = DatabaseHandler.AddNewUser();

            //Check if username exists
            if (DatabaseHandler.UsernameExists(UserName))
            {
                UserSocket.Send(Encoding.GetEncoding(437).GetBytes("6," + Tools.ushortToString(AssignedID) + ",N,,.!"));
            }
            else
            {
                //Create new user account
                DatabaseHandler.AddUserAccount(UserName, Password);

                //Add to current list
                User U = new User(AssignedID, UserSocket);
                Tools.CurrentUserList.Add(AssignedID, U);

                //Send assignedID, devicelist, and wait for new commands in HandleConnection()
                U.Send(Encoding.GetEncoding(437).GetBytes("6," + Tools.ushortToString(AssignedID) + ",Y,,.!"));
                U.SendDeviceList();
                U.HandleConnection();
            }
        }
    }
    class User_Reconnection_SignIn : Command
    {
        public User_Reconnection_SignIn()
        {
            Type = CommandType.User_Reconnection_SignIn;
        }
        public override void Execute(Socket UserSocket)
        {
            User U;
            //if user's device has connected before
            if (DatabaseHandler.TryGetUser(SourceID, out U))
            {
                int LoginID = DatabaseHandler.UserIsAuthenticated(UserName, Password);
                if (LoginID > 0)
                {
                    U.LoginID = LoginID;
                    U.BindSocket(UserSocket);
                    Tools.CurrentUserList.Add(SourceID, U);
                    U.Send(Encoding.GetEncoding(437).GetBytes("5," + Tools.ushortToString(U.GetName()) + ",Y,,.!"));
                    U.SendDeviceList();
                    U.HandleConnection();
                }
                else
                {
                    UserSocket.Send(Encoding.GetEncoding(437).GetBytes("5," + Tools.ushortToString(U.GetName()) + ",N,,.!"));
                }
            }
            else
            {
                UserSocket.Send(Encoding.GetEncoding(437).GetBytes("This device has not connected to the server before."));
            }
        }
    }
    class User_Reconnection_SignUp : Command
    {
        public User_Reconnection_SignUp()
        {
            Type = CommandType.User_Reconnection_SignUp;
        }
        public override void Execute(Socket UserSocket)
        {
            User U;

            //if user's device has connected before
            if (DatabaseHandler.TryGetUser(SourceID, out U))
            {
                if (DatabaseHandler.UsernameExists(UserName))
                {
                    UserSocket.Send(Encoding.GetEncoding(437).GetBytes("7," + Tools.ushortToString(U.GetName()) + ",N,,.!"));
                }
                else
                {
                    DatabaseHandler.AddUserAccount(UserName, Password);

                    //Bind new socket and add to current list
                    U.BindSocket(UserSocket);
                    Tools.CurrentUserList.Add(SourceID, U);

                    //Send DeviceList, and wait for new commands in HandleConnection()
                    U.Send(Encoding.GetEncoding(437).GetBytes("7," + Tools.ushortToString(U.GetName()) + ",Y,,.!"));
                    U.SendDeviceList();
                    U.HandleConnection();
                }
            }
            else
            {
                UserSocket.Send(Encoding.GetEncoding(437).GetBytes("This device has not connected to the server before."));
            }
        }
    }
    class User_Action : Command
    {
        public User_Action()
        {
            Type = CommandType.User_Action;
        }
        public override void Execute(Socket UserSocket)
        {
            Device D;
            //if destination device is currently connected
            if (Tools.CurrentDeviceList.TryGetValue(DestinationID, out D))
            {
                if (Action_State == (byte)49)
                    D.TurnOn();
                else //Action_State == 0
                    D.TurnOff();
            }
            else
            {
                UserSocket.Send(Encoding.GetEncoding(437).GetBytes("Device not connected!"));
            }
        }
    }
    class Device_FirstConnection : Command
    {
        public Device_FirstConnection()
        {
            Type = CommandType.Device_FirstConnection;
        }

        public override void Execute(Socket DeviceSocket)
        {
            //Assign name for device and add to database
            ushort AssignedName = DatabaseHandler.AddNewDevice();
            Device D = new Device(AssignedName, DeviceSocket, Action_State);

            Console.WriteLine("New name assigned to Device!");
            Console.WriteLine("Device Name: " + D.GetName());

            //Add Device to current devices list, and update users' lists
            Tools.UpdateListAndBroadcast_AddDevice(D);

            //Name notification message to device
            byte[] Message = CreateNewNameMessage(AssignedName);
            if (!D.Send(Message))
                Console.WriteLine("Device.StartConnection (First Connection_ NewName): Send Failed");

            D.StartTimer();
            D.HandleConnection();
        }

        private static byte[] CreateNewNameMessage(ushort Name)
        {
            string NameMessage = ".1," + Tools.ushortToString(Name) + ",23,M.";
            return Encoding.GetEncoding(437).GetBytes(NameMessage);
        }
    }
    class Device_Reconnection : Command
    {
        public Device_Reconnection()
        {
            Type = CommandType.Device_Reconnection;
        }
        public override void Execute(Socket DeviceSocket)
        {
            Device D;

            //if device is not already connected
            if (!Tools.CurrentDeviceList.TryGetValue(SourceID, out D))
            {
                //if device is on the database: reconnect.........................................a
                if (DatabaseHandler.TryGetDevice(SourceID, out D))
                {
                    Console.WriteLine("Name exists in database!");
                    Console.WriteLine("Connection accepted from Device " + D.GetName());

                    D.BindSocket(DeviceSocket);
                    D.SetState(Action_State);

                    //Add device to list and update users' lists
                    Tools.UpdateListAndBroadcast_AddDevice(D);

                    D.StartTimer();
                    D.HandleConnection();
                }

                //if not: assign new name, add to database, send NewName command to device.......b
                else
                {
                    ushort AssignedName;
                    Console.WriteLine("Name: " + SourceID + " Doesn't exist in Database!");
                    AssignedName = DatabaseHandler.AddNewDevice();
                    Console.WriteLine("New name assigned to Device!");
                    Console.WriteLine("Device Name: " + AssignedName);

                    D = new Device(AssignedName, DeviceSocket, Action_State);

                    //Add device to list and update users' lists
                    Tools.UpdateListAndBroadcast_AddDevice(D);

                    //Send NewName message
                    byte[] Message = CreateChangeNameMessage(SourceID, AssignedName);
                    if (!D.Send(Message))
                        Console.WriteLine("Device.StartConnection (reconnect_ChangeName) :Send failed");

                    D.StartTimer();
                    D.HandleConnection();
                }
            }
        }

        private static byte[] CreateChangeNameMessage(ushort OldName, ushort NewName)
        {
            string Message = ".3," + Tools.ushortToString(OldName) + "," + Tools.ushortToString(NewName) + ",M.";
            return Encoding.GetEncoding(437).GetBytes(Message);
        }
    }
    class Device_WatchDog : Command
    {
        public Device_WatchDog()
        {
            Type = CommandType.Device_WatchDog;
        }

        public override void Execute(Socket DeviceSocket)
        {
            //Console.WriteLine("WatchDog recieved from device: " + SourceID);
            Device D;
            if (Tools.CurrentDeviceList.TryGetValue(SourceID, out D))
            {
                D.resetTimer();

                if (D.GetState() != Action_State)
                {
                    //Update state of device
                    D.SetState(Action_State);

                    //Update current list and update users' lists
                    Tools.UpdateListAndBroadcast_ChangeState(D);
                }
            }
        }
    }
    class Device_Acknowledgement : Command
    {
        public Device_Acknowledgement()
        {
            Type = CommandType.Device_Acknowledgement;
        }

        public override void Execute(Socket DeviceSocket)
        {
            //User U;
            //string msg;

            Device D;
            Tools.CurrentDeviceList.TryGetValue(SourceID, out D);
            //Update state of device
            D.SetState(Action_State);

            //Update users' lists
            Tools.UpdateListAndBroadcast_ChangeState(D);

            //Not necessary anymore !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            /*
            //if User is in current users list
            if (Tools.CurrentUserList.TryGetValue(DestinationID, out U))
            {
                msg = Convert.ToString(DestinationID) + ',' + Convert.ToString(SourceID) + ',' + Convert.ToString(Action_State) + '.';
                if (!U.Send(Encoding.GetEncoding(437).GetBytes(msg)))
                    Console.WriteLine("Device_Acknowledgement_Action: Send failed");
            }

            //if user not in current users list
            else
            {
                Console.WriteLine("Error in DeviceConnection.Device_Acknowledgement_Action: User doesn't exist in database!");
                Console.WriteLine("Acknowledgement not sent");
            }
            */
        }
    }
    class Invalid : Command
    {
        public Invalid()
        {
            Type = CommandType.Invalid;
        }
        public override void Execute(Socket S)
        {
            Console.WriteLine("Invalid Command!");
            S.Send(Tools.StringToByteArray("Invalid Command!"));
        }
    }

    #region Wifi-Localization (Tee)
    class User_Locate : Command
    {
        private WifiLocalization.Manager _wifiManager = WifiLocalization.Manager.Instance;
        
        //static Room R = new Room(1, "TestRoom");
        //static Sector S1 = new Sector(1);
        //static Sector S2 = new Sector(2);
        //static Location L1 = new Location(0, 0, R, S1);
        //static Location L2 = new Location(0, 1, R, S2);
        //static int x = 0;

        public List<WifiReading> ReadingsList;

        static Random RandomGen = new Random(2);

        public User_Locate(int ListSize = 0)
        {
            GetLocation(); // remove this line
            Type = CommandType.User_Locate;
            if (ListSize != 0)
                ReadingsList = new List<WifiReading>(ListSize);
        }

        public override void Execute(Socket S)
        {
            User U;
            Console.WriteLine(SourceID);
            Tools.CurrentUserList.TryGetValue(SourceID, out U);
            U.CurrentLocation = GetLocation();
            //DatabaseHandler.CheckUserActions(U);
            PrintReadingsList();
            //Console.WriteLine(U.CurrentLocation.xCoordinate + U.CurrentLocation.yCoordinate);
            //Location tempLoc = GetLocation();
            U.Send(Tools.StringToByteArray("2," + U.CurrentLocation.X + "," + U.CurrentLocation.Y + ".!"));
        }

        private Location GetLocation()
        {
            Location userLocation = new Location(RandomGen.Next(0, 100), RandomGen.Next(0, 100));
            LocationModel locationToModel =  Mapper<Location, LocationModel>.MapTo(userLocation, new LocationModel());
            List<LocationModel> mappedList = new List<LocationModel>();
            mappedList.Add(locationToModel);
            Location ModelToLocation =  Mapper<LocationModel, Location>.MapTo(_wifiManager.GetLocation(mappedList),new Location(0,0));
            return ModelToLocation;
           // return new Location(RandomGen.Next(0, 100), RandomGen.Next(0, 100));
        }

        private void PrintReadingsList()
        {
            foreach (WifiReading Reading in ReadingsList)
            {
                Console.WriteLine(Reading.BSSID + "    " + Reading.RSSI);
            }
        }
    }
    #endregion
}
