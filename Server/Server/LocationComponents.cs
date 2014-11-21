using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clients;

namespace LocationComponents
{
    public class WifiReading
    {
        public string MAC { get; set; }
        public int RSSI { get; set; }

        public WifiReading(string _MAC, int _RSSI)
        {
            this.MAC = _MAC;
            this.RSSI = _RSSI;
        }
    }

    public class Room
    {
        public int ID { get; set; }
        public string Name { get; set; }
        private List<Device> DeviceList = new List<Device>();

        public Room(int _ID, string _Name)
        {
            ID = _ID;
            Name = _Name;
        }

        public void AddDevice(Device D)
        {
            if (!DeviceList.Contains(D))
                DeviceList.Add(D);
            else
                Console.WriteLine("Room already contains device: " + D.GetName());
        }

        public void RemoveDevice(Device D)
        {
            DeviceList.Remove(D);
        }

        public void TurnOnDevices()
        {
            foreach (Device D in DeviceList)
                D.TurnOn();
        }

        public void TurnOffDevices()
        {
            foreach (Device D in DeviceList)
                D.TurnOff();
        }

        public static bool operator ==(Room R1, Room R2)
        {
            return R1.ID == R2.ID;
        }

        public static bool operator !=(Room R1, Room R2)
        {
            return R1.ID != R2.ID;
        }

        public override bool Equals(object o)
        {
            Room R = o as Room;
            if (R != null)
            {
                return R == this;
            }
            return false;
        }

        public bool Equals(Room R)
        {
            if (R != null)
            {
                return R == this;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ID;
        }
    }

    public class Zone
    {
        public int ID { get; set; }
        public string Name { get; set; }
        private List<Room> RoomList = new List<Room>();

        public Zone(int _ID, string _Name)
        {
            ID = _ID;
            Name = _Name;
        }

        public void AddRoom(Room R)
        {
            if (!RoomList.Contains(R))
                RoomList.Add(R);
            else
                Console.WriteLine("Zone already contains room: " + R.Name);
        }

        public void RemoveRoom(Room R)
        {
            RoomList.Remove(R);
        }

        public void TurnOnDevices()
        {
            foreach (Room R in RoomList)
                R.TurnOnDevices();
        }

        public void TurnOffDevices()
        {
            foreach (Room R in RoomList)
                R.TurnOffDevices();
        }

        public static bool operator ==(Zone Z1, Zone Z2)
        {
            return Z1.ID == Z2.ID;
        }

        public static bool operator !=(Zone Z1, Zone Z2)
        {
            return Z1.ID != Z2.ID;
        }

        public override bool Equals(object o)
        {
            Zone Z = o as Zone;
            if (Z != null)
            {
                return Z == this;
            }
            return false;
        }

        public bool Equals(Zone Z)
        {
            if (Z != null)
            {
                return Z == this;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ID;
        }
    }

    public class Home
    {
    }

    public class Sector
    {
        public int ID { get; set; }

        public Sector(int _ID)
        {
            this.ID = _ID;
        }
    }

    public class Location
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Room locationRoom;
        public Sector locationSector;

        public Location(double x, double y, Room R = null, Sector S = null)
        {
            X = x;
            Y = y;
            locationRoom = R;
            locationSector = S;
        }
             
    }
}