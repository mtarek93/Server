using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Clients;
using LocationComponents;
using ServerTools;
using System.Data.OleDb;

namespace Database
{
    class Test
    {
        //static void Main(string[] args)
        //{
        //}
    }

    class DatabaseHandler
    {
        public static string ConnectionString;
        private const string LoginTable = "LoginTable", 
                             UsersTable = "UsersTable", 
                             DevicesTable = "DevicesTable", 
                             IDTable = "IDTable",
                             UserActionsTable = "UserActionsTable";

        private static object LoginTableLock = new object();
        private static object DbWriteLock = new object();

        public static void AddUserAccount(string Username, string Password)
        {
            lock (LoginTableLock)
            {
                using (SqlConnection Database = new SqlConnection(ConnectionString))
                {
                    try
                    {
                        Database.Open();
                        using (SqlCommand Command = new SqlCommand("INSERT INTO " + LoginTable + " (Username, Password) VALUES (@Username, @Password);", Database))
                        {
                            Command.Parameters.Add(new SqlParameter("@Username", Username));
                            Command.Parameters.Add(new SqlParameter("@Password", Password));
                            Command.ExecuteNonQuery();
                        }
                        Database.Close();
                    }
                    catch (SqlException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }

        //Returns LoginID if successful and 0 if failed
        public static int UserIsAuthenticated(string Username, string Password)
        {
            lock (LoginTableLock)
            {
                using (SqlConnection Database = new SqlConnection(ConnectionString))
                {
                    try
                    {
                        Database.Open();
                        string Query = "SELECT Id, Username, Password FROM " + LoginTable + " WHERE Username = " + "'" + Username + "' " + "AND Password = " + "'" + Password + "';";

                        using (SqlCommand Command = new SqlCommand(Query, Database))
                        {
                            SqlDataReader Reader = Command.ExecuteReader();
                            if (Reader.Read())
                            {
                                return Reader.GetInt32(0);
                            }

                            else
                            {
                                return 0;
                            }
                        }
                    }
                    catch (SqlException e)
                    {
                        Console.WriteLine(e.Message);
                        return 0;
                    }
                }
            }
        }
        // Adds a new user to the database and returns an Assigned ID
        public static ushort AddNewUser()
        {
            lock (DbWriteLock)
            {
                int Id = GetNextID();
                using (SqlConnection Database = new SqlConnection(ConnectionString))
                {
                    try
                    {
                        Database.Open();
                        using (SqlCommand Command = new SqlCommand("INSERT INTO " + UsersTable + " (Id) VALUES (@Id);", Database))
                        {
                            Command.Parameters.Add(new SqlParameter("@Id", Id));
                            Command.ExecuteNonQuery();
                        }
                        Database.Close();
                    }
                    catch (SqlException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                AddNewID(Id);
                return (ushort)Id;
            }
        }
        // Adds a new device to the database and returns an Assigned ID
        public static ushort AddNewDevice()
        {
            lock (DbWriteLock)
            {
                int Id = GetNextID();
                using (SqlConnection Database = new SqlConnection(ConnectionString))
                {
                    try
                    {
                        Database.Open();
                        using (SqlCommand Command = new SqlCommand("INSERT INTO " + DevicesTable + " (Id) VALUES (@Id);", Database))
                        {
                            Command.Parameters.Add(new SqlParameter("@Id", Id));
                            Command.ExecuteNonQuery();
                        }
                        Database.Close();
                    }
                    catch (SqlException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                AddNewID(Id);
                return (ushort)Id;
            }
        }
        public static bool TryGetDevice(ushort ID, out Device D)
        {
            int Id = Convert.ToInt32(ID);
            using (SqlConnection Database = new SqlConnection(ConnectionString))
            {
                try
                {
                    Database.Open();
                    string Query = "SELECT Id FROM " + DevicesTable + " WHERE Id = " + "'" + Id + "';";
                    using (SqlCommand Command = new SqlCommand(Query, Database))
                    {
                        SqlDataReader Reader = Command.ExecuteReader();
                        if (Reader.Read())
                        {
                            //Console.WriteLine("Device found!");
                            D = new Device(Convert.ToUInt16(Reader.GetInt32(0)));
                            return true;
                        }

                        else
                        {
                            //Console.WriteLine("Device not registered.");
                            D = null;
                            return false;
                        }
                    }
                }
                catch (SqlException e)
                {
                    Console.WriteLine(e.Message);
                    D = null;
                    return false;
                }
            }
        }
        public static bool TryGetUser(ushort ID, out User U)
        {
            int Id = Convert.ToInt32(ID);
            using (SqlConnection Database = new SqlConnection(ConnectionString))
            {
                try
                {
                    Database.Open();
                    string Query = "SELECT Id FROM " + UsersTable + " WHERE Id = " + "'" + Id + "';";

                    using (SqlCommand Command = new SqlCommand(Query, Database))
                    {
                        SqlDataReader Reader = Command.ExecuteReader();
                        if (Reader.Read())
                        {
                            //Console.WriteLine("User found!");
                            U = new User(Convert.ToUInt16(Reader.GetInt32(0)));
                            return true;
                        }

                        else
                        {
                            //Console.WriteLine("User not registered.");
                            U = null;
                            return false;
                        }
                    }
                }
                catch (SqlException e)
                {
                    Console.WriteLine(e.Message);
                    U = null;
                    return false;
                }
            }
        }
        public static bool UsernameExists(string Username)
        {
            lock (LoginTableLock)
            {
                using (SqlConnection Database = new SqlConnection(ConnectionString))
                {
                    try
                    {
                        Database.Open();
                        string Query = "SELECT Username FROM " + LoginTable + " WHERE Username = " + "'" + Username + "';";

                        using (SqlCommand Command = new SqlCommand(Query, Database))
                        {
                            SqlDataReader Reader = Command.ExecuteReader();
                            if (Reader.Read())
                                return true;
                            else
                                return false;
                        }
                    }
                    catch (SqlException e)
                    {
                        Console.WriteLine(e.Message);
                        return false;
                    }
                }
            }
        }
        private static int GetNextID()
        {
            using (SqlConnection Database = new SqlConnection(ConnectionString))
            {
                try
                {
                    Database.Open();
                    string Query = "SELECT COUNT (Id) FROM " + IDTable + ";";
                    using (SqlCommand Command = new SqlCommand(Query, Database))
                    {
                        SqlDataReader Reader = Command.ExecuteReader();
                        if (Reader.Read())
                            return Reader.GetInt32(0);
                        else
                            return -1;
                    }
                }
                catch (SqlException e)
                {
                    Console.WriteLine(e.Message);
                    return -1;
                }
            }
        }
        private static void AddNewID (int ID)
        {
            using (SqlConnection Database = new SqlConnection(ConnectionString))
            {
                try
                {
                    Database.Open();
                    string Query = "INSERT INTO " + IDTable + " (Id) VALUES (@Id);";
                    using (SqlCommand Command = new SqlCommand(Query, Database))
                    {
                        Command.Parameters.Add(new SqlParameter("@Id", ID));
                        Command.ExecuteNonQuery();
                    }
                    Database.Close();
                }
                catch (SqlException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public static void AddUserAction(int UserID, int DeviceID, int ZoneID, int RoomID, int SectorID, string Action)
        {
            using (SqlConnection Database = new SqlConnection(ConnectionString))
            {
                try
                {
                    Database.Open();
                    string Query = "INSERT INTO " + UserActionsTable + " (LoginId, DeviceId, Zone, Room, Sector, Action) VALUES (@LoginId, @DeviceId, @Zone, @Room, @Sector, @Action);";
                    using (SqlCommand Command = new SqlCommand(Query, Database))
                    {
                        Command.Parameters.Add(new SqlParameter("@LoginId", UserID));
                        Command.Parameters.Add(new SqlParameter("@DeviceId", DeviceID));
                        Command.Parameters.Add(new SqlParameter("@Zone", ZoneID));
                        Command.Parameters.Add(new SqlParameter("@Room", RoomID));
                        Command.Parameters.Add(new SqlParameter("@Sector", SectorID));
                        Command.Parameters.Add(new SqlParameter("@Action", Action));
                        Command.ExecuteNonQuery();
                    }
                    Database.Close();
                }
                catch (SqlException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public static void CheckUserActions(User U)
        {
            using (SqlConnection Database = new SqlConnection(ConnectionString))
            {
                try
                {
                    Database.Open();
                    string Query = "SELECT DeviceId, Action FROM " + UserActionsTable + " WHERE Room = " + "'" + U.CurrentLocation.locationRoom.ID.ToString() + "' " + "AND Sector = " + "'" + U.CurrentLocation.locationSector.ID.ToString() + "';";
                    using (SqlCommand Command = new SqlCommand(Query, Database))
                    {
                        SqlDataReader Reader = Command.ExecuteReader();
                        Device D;
                        
                        while(Reader.Read())
                        {
                            ushort DeviceID = (ushort)Reader.GetInt32(0);
                            string Action = Reader.GetString(1);
                            if (Tools.CurrentDeviceList.TryGetValue(DeviceID, out D))
                                D.SendMagnitude(Convert.ToByte(Action[0]));
                            else
                                Console.WriteLine("Automation: Device {0} is not connected!", DeviceID);
                        }
                    }
                    Database.Close();
                }
                catch (SqlException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public static void ExcelToDbTable(string excelFilePath, string tableName)
        {
            //declare variables - edit these based on your particular situation
            string ssqltable = tableName;
            // make sure your sheet name is correct, here sheet name is sheet1, so you can change your sheet name if have different
            string myexceldataquery = "select Id,MAC,Room,Sector,LocationNumber,ApNumber,MapNumber,X,Y,RSSI from [ThesisLab$]";
            try
            {
                //create our connection strings
                string sexcelconnectionstring = @"provider=microsoft.jet.oledb.4.0;data source=" + excelFilePath + ";extended properties=" + "\"excel 8.0;hdr=yes;\"";
                string ssqlconnectionstring = ConnectionString;
                //execute a query to erase any previous data from our destination table
                string sclearsql = "delete from " + ssqltable;
                SqlConnection sqlconn = new SqlConnection(ssqlconnectionstring);
                SqlCommand sqlcmd = new SqlCommand(sclearsql, sqlconn);
                sqlconn.Open();
                sqlcmd.ExecuteNonQuery();
                sqlconn.Close();
                //series of commands to bulk copy data from the excel file into our sql table
                OleDbConnection oledbconn = new OleDbConnection(sexcelconnectionstring);
                OleDbCommand oledbcmd = new OleDbCommand(myexceldataquery, oledbconn);
                oledbconn.Open();
                OleDbDataReader dr = oledbcmd.ExecuteReader();
                SqlBulkCopy bulkcopy = new SqlBulkCopy(ssqlconnectionstring);
                bulkcopy.DestinationTableName = ssqltable;
                bulkcopy.WriteToServer(dr);
                oledbconn.Close();
            }
            catch (Exception ex)
            {
                //handle exception
                Console.WriteLine("Exception in ExcelToDbTable: " + ex.Message);
            }
        }

    }
}
