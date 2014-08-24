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

        public void AddDevice(Device D)
        {
            DeviceList.Add(D);
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
    }

    class Zone
    {
        public string Name { get; set; }
        private List<Room> RoomList = new List<Room>();

        public void AddRoom(Room R)
        {
            RoomList.Add(R);
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
