using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clients;

namespace LocationComponents
{
    public class Room
    {
        public int ID;
        public string Name;
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
        public int ID;
        public string Name;
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

    public class Location
    {
        public int xCoordinate { get; set; }
        public int yCoordinate { get; set; }
        public Room locationRoom;

        public Location(int x, int y, Room R)
        {
            xCoordinate = x;
            yCoordinate = y;
            locationRoom = R;
        }

        public Location(int x, int y)
        {
            xCoordinate = x;
            yCoordinate = y;
            locationRoom = null;
        }
    }
}
