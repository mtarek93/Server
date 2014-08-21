using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clients;

namespace LocationComponents
{
    class Room
    {
        public string Name { get; set; }
        private List<Device> DeviceList = new List<Device>();

        void AddDevice(Device D)
        {
            DeviceList.Add(D);
        }

        void RemoveDevice(Device D)
        {
            DeviceList.Remove(D);
        }
    }

    class Zone
    {
        public string Name { get; set; }
        private List<Room> RoomList = new List<Room>();

        void AddRoom(Room R)
        {
            RoomList.Add(R);
        }

        void RemoveRoom(Room R)
        {
            RoomList.Remove(R);
        }
    }

    class Home
    {
    }

    class Position
    {
        public int xCoordinate { get; set; }
        public int yCoordinate { get; set; }
        public Room positionRoom;

        public Position(int x, int y)
        {
            xCoordinate = x;
            yCoordinate = y;
        }
    }
}
