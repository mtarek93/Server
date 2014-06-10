using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

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
    }
    public class Device
    {
        ushort Name;
        string State;
        Socket Sckt;
        public static int WDInterval = 5000;                            //Watchdog Interval
        System.Timers.Timer T = new System.Timers.Timer(WDInterval);    //Timer

        public Device(ushort _name)
        {
            this.Name = _name;
        }
        public Device(ushort _name, string _State)
        {
            this.Name = _name;
            this.State = _State;
        }
        public Device(ushort _name, Socket _S)
        {
            this.Name = _name;
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
            ConnectionManager.CurrentDeviceList.Remove(this.Name);          //Timer
            Console.WriteLine("Device: " + this.Name + " is disconnected"); //Timer
        }
        public void resetTimer()                                          //Timer
        {
            this.T.Interval = WDInterval;                                   //Timer
        }
        public string GetState()
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
        public void SetState(string _State)
        {
            this.State = _State;
        }
    }
}