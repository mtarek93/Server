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
    }
    class DeviceConnection
    {
        //Updates----------------------------------------------------------------------------------
        public static void Add_Device(Clients.Device D)
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
                User.Value.Send(Add_Cmd);
            }
        }
        public static void Remove_Device(Clients.Device D)
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
                User.Value.Send(Add_Cmd);
            }
        }
        public static void Update_State(Clients.Device D)
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
                User.Value.Send(Add_Cmd);
            }
        }
        //tools------------------------------------------------------------------------------------
        public static byte[] CreateNewNameMessage(ushort Name)
        {
            string NameMessage = ".1," + Tools.ushortToString(Name) + ",23,M.";
            return Encoding.GetEncoding(437).GetBytes(NameMessage);
        }
        public static byte[] CreateChangeNameMessage(ushort OldName, ushort NewName)
        {
            string Message = ".3," + Tools.ushortToString(OldName) + "," + Tools.ushortToString(NewName) + ",M.";
            return Encoding.GetEncoding(437).GetBytes(Message);
        }
    }
}
