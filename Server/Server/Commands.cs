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

namespace CommandHandler
{
    class FirstConnection_SignIn : Command
    {
        public override bool Execute(Socket UserSocket)
        {
            if (DatabaseHandler.UserIsAuthenticated(UserName, Password))
            {
                //Assign ID to user's device and add to database
                ushort AssignedID = DatabaseHandler.AddNewUser();

                //Add to current list
                User U = new User(AssignedID, UserSocket);
                Tools.CurrentUserList.Add(AssignedID, U);

                //Send assignedID, devicelist, and wait for new commands in HandleConnection()
                U.Send(Encoding.GetEncoding(437).GetBytes("Login Successful!," + AssignedID.ToString() + "."));
                //SendDeviceList(U);
                //HandleConnection(U);
                return true;
            }
            else
            {
                UserSocket.Send(Encoding.GetEncoding(437).GetBytes("Invalid credentials."));
                return false;
            }
        }   
    }
}
