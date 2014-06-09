using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using CommandHandler;
using Clients;

namespace ConnectionManager
{
    class UserConnection
    {
        public static void AcceptConnection(object _Socket)
        {
          
        }

        static void HandleConnection()
        {

        }

        static bool Send(User _User, byte[] Data)
        {
            try
            {
                Socket S = _User.GetSocket();
                S.Send(Data);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }

    class DeviceConnection
    {
        public static void AcceptConnection(object _Socket)
        {

        }

        static bool Send(Device _Device, byte[] Data)
        {
            try
            {
                Socket S = _Device.GetSocket();
                S.Send(Data);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}
