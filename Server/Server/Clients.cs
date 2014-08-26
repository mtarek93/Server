using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using ServerTools;
using CommandHandler;

namespace Clients
{
    public class User
    {
        ushort Name;
        Socket Sckt;
        //add field to find specific user

        public User(ushort _name)
        {
            this.Name = _name;
        }
        public User(ushort _name, Socket S)
        {
            this.Name = _name;
            this.Sckt = S;
        }
        public ushort GetName()
        {
            return this.Name;
        }
        public Socket GetSocket()
        {
            return this.Sckt;
        }
        public void BindSocket(Socket _S)
        {
            this.Sckt = _S;
        }
        public void ChangeName(ushort _name)
        {
            this.Name = _name;
        }
        public bool Send(byte[] Data)
        {
            try
            {
                this.Sckt.Send(Data);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in User.Send: " + e.Message);
                return false;
            }
        }
        public bool Receive(ref byte[] Data)
        {
            const int MAX_COMMAND_LENGTH = 99;
            byte[] commandLengthBuffer = new byte[2];
            int totalBytes = 0, bytesReceived = 0, commandLength = 0;

            try
            {
                //Start receiving command length
                bytesReceived = totalBytes = this.Sckt.Receive(commandLengthBuffer);

                //Recieve upto the length prefix
                while (bytesReceived < commandLengthBuffer.Length && bytesReceived > 0)
                {
                    bytesReceived = this.Sckt.Receive(commandLengthBuffer, totalBytes, commandLengthBuffer.Length - totalBytes, SocketFlags.None);
                    totalBytes += bytesReceived;
                }

                //Get the command length from prefix
                if (Int32.TryParse(Encoding.GetEncoding(437).GetString(commandLengthBuffer), out commandLength))
                {
                    Console.WriteLine("Length = " + commandLength.ToString());

                    //Check for commandLength maximum and create buffer to receive data
                    if (commandLength > MAX_COMMAND_LENGTH)
                        Data = new byte[MAX_COMMAND_LENGTH];
                    else
                        Data = new byte[commandLength];

                    //Receive the data
                    totalBytes = 0;
                    bytesReceived = totalBytes = this.Sckt.Receive(Data, 0, Data.Length, SocketFlags.None);
                    while (totalBytes < Data.Length && bytesReceived > 0)
                    {
                        bytesReceived = this.Sckt.Receive(Data, totalBytes, Data.Length - totalBytes, SocketFlags.None);
                        totalBytes += bytesReceived;
                    }

                    return true;
                }
                else
                {
                    Console.WriteLine("Wrong format for length prefix!");
                    this.Sckt.Disconnect(false);
                    this.Sckt.Dispose();
                    return false;
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("Exception in User.Receive: " + e.Message);
                return false;
            }
        }
        public void SendDeviceList()
        {
            if (Tools.CurrentDeviceList.Count == 0)
                Send(Tools.StringToByteArray("No devices are connected."));
            else
            {
                string DeviceList = "";
                foreach (var Device in Tools.CurrentDeviceList)
                    DeviceList += (Device.Key + Device.Value.GetState().ToString());

                DeviceList += ".";
                Send(Tools.StringToByteArray(DeviceList));
            }
        }
        public void HandleConnection()
        {
            Command Cmd;
            byte[] ReceivedData = null;
            while (true)
            {
                if (Receive(ref ReceivedData))
                {
                    Console.WriteLine("Command Received: " + Tools.ByteArrayToString(ReceivedData));
                    Cmd = CommandParser.ParseCommand(Tools.ByteArrayToString(ReceivedData));
                    Cmd.Execute(Sckt);
                }
                else
                {
                    Console.WriteLine("User is disconnected!");
                    Tools.CurrentUserList.Remove(Name);
                    break;
                }
            }
        }
    }
    public class Device
    {
        ushort Name;
        byte State;
        Socket Sckt;
        public static int WDInterval = 5000;                            //Watchdog Interval
        System.Timers.Timer T = new System.Timers.Timer(WDInterval);    //Timer

        public Device(ushort _name)
        {
            this.Name = _name;
        }
        public Device(ushort _name, byte _State)
        {
            this.Name = _name;
            this.State = _State;
        }
        public Device(ushort _name, Socket _S)
        {
            this.Name = _name;
            this.Sckt = _S;
        }
        public Device(ushort _name, Socket _S, byte _State)
        {
            this.Name = _name;
            this.State = _State;
            this.Sckt = _S;
        }
        public ushort GetName()
        {
            return this.Name;
        }
        public Socket GetSocket()
        {
            return this.Sckt;
        }
        public void StartTimer()                                           //Timer enable
        {
            this.T.Elapsed += T_Elapsed;                                   //Timer
            this.T.Enabled = true;                                         //Timer
            T.AutoReset = false;
        }
        void T_Elapsed(object sender, ElapsedEventArgs e)                  //Timer event
        {
            Console.WriteLine(" Watchdog not recieved for device: "+ this.Name);                   //Timer
            Tools.CurrentDeviceList.Remove(this.Name);          //Timer
            Console.WriteLine("Device: " + this.Name + " is disconnected"); //Timer
        }
        public void resetTimer()                                          //Timer
        {
            this.T.Interval = WDInterval;                                   //Timer
        }
        public byte GetState()
        {
            return this.State;
        }
        public void BindSocket(Socket _S)
        {
            this.Sckt = _S;
        }
        public void ChangeName(ushort _name)
        {
            this.Name = _name;
        }
        public void SetState(byte _State)
        {
            this.State = _State;
        }
        public bool Send(byte[] Data)
        {
            try
            {
                this.Sckt.Send(Data);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in Device.Send: " + e.Message);
                return false;
            }
        }
        public bool Receive(ref byte[] Data)
        {
            const int MAX_COMMAND_LENGTH = 99;
            byte[] commandLengthBuffer = new byte[2];
            int totalBytes = 0, bytesReceived = 0, commandLength;

            try
            {
                //Start receiving command length
                bytesReceived = totalBytes = this.Sckt.Receive(commandLengthBuffer);

                //Recieve upto the length prefix
                while (bytesReceived < commandLengthBuffer.Length && bytesReceived > 0)
                {
                    bytesReceived = this.Sckt.Receive(commandLengthBuffer, totalBytes, commandLengthBuffer.Length - totalBytes, SocketFlags.None);
                    totalBytes += bytesReceived;
                }

                //Get the command length from prefix
                if (Int32.TryParse(Encoding.GetEncoding(437).GetString(commandLengthBuffer), out commandLength))
                {
                    Console.WriteLine("Length = " + commandLength.ToString());

                    //Check for commandLength maximum and create buffer to receive data
                    if (commandLength > MAX_COMMAND_LENGTH)
                        Data = new byte[MAX_COMMAND_LENGTH];
                    else
                        Data = new byte[commandLength];

                    //Receive the data
                    totalBytes = 0;
                    bytesReceived = totalBytes = this.Sckt.Receive(Data, 0, Data.Length, SocketFlags.None);
                    while (totalBytes < Data.Length && bytesReceived > 0)
                    {
                        bytesReceived = this.Sckt.Receive(Data, totalBytes, Data.Length - totalBytes, SocketFlags.None);
                        totalBytes += bytesReceived;
                    }

                    return true;
                }
                else
                {
                    Console.WriteLine("Wrong format for length prefix!");
                    this.Sckt.Disconnect(false);
                    this.Sckt.Dispose();
                    return false;
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("Exception in Device.Receive: " + e.Message);
                return false;
            }
        }

        public void HandleConnection()
        {
            //Initialization----------------------------------------------------------------------------0
            byte[] ReceivedData = null;
            string Command;
            Command Cmd;

            while (true)
            {
                if (Receive(ref ReceivedData))
                {
                    Command = Tools.ByteArrayToString(ReceivedData);
                    Console.WriteLine("Command received was: " + Command);
                    Cmd = CommandParser.ParseCommand(Command);
                    Cmd.Execute(this.Sckt);
                }
                else
                {
                    Console.WriteLine("Device" + Name + "is disconnected!");
                    //Remove Device from list and update users' lists
                    ConnectionManager.DeviceConnection.Remove_Device(this);
                    break;
                }
            }
        }

        public void TurnOn()
        {
            Send(Tools.StringToByteArray(".4,xx," + Tools.ushortToString(Name) + "," + Convert.ToChar((byte)255) + "."));
        }

        public void TurnOff()
        {
            Send(Tools.StringToByteArray(".4,xx," + Tools.ushortToString(Name) + "," + Convert.ToChar((byte)0) + "."));
        }

        public void SendMagnitude(byte Magnitude)
        {
            Send(Tools.StringToByteArray(".4,xx," + Tools.ushortToString(Name) + "," + Convert.ToChar(Magnitude) + "."));
        }
    }
}