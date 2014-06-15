using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Clients;

namespace Database
{

    //class Test
    //{
    //    static void Main(string[] args)
    //    {
    //        ushort ID = 0;
    //        DatabaseHandler.ConnectionString = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\Mohamed\Documents\GitHub\Server\Server\Server\Database.mdf;Integrated Security=True;Connect Timeout=30";
    //        bool flag = DatabaseHandler.GetLatestAssignedID(out ID);
    //        if (flag)
    //        {
    //            ID++;
    //            DatabaseHandler.AddNewID(ID);
    //            DatabaseHandler.AddNewDevice(ID, 0);
    //        }
    //        else
    //        {
    //            DatabaseHandler.AddNewID(ID);
    //            DatabaseHandler.AddNewDevice(ID, 0);
    //        }
                
    //    }
    //}
    class DatabaseHandler
    {
        public static string ConnectionString, LoginTable = "LoginTable", UsersTable = "UsersTable", DevicesTable = "DevicesTable", IDTable = "IDTable";

        public static void AddUserAccount(string Username, string Password)
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

        public static bool UserIsAuthenticated(string Username, string Password)
        {
            using (SqlConnection Database = new SqlConnection(ConnectionString))
            {
                try
                {
                    Database.Open();
                    string Query = "SELECT Username, Password FROM " + LoginTable + " WHERE Username = " + "'" + Username + "' " + "AND Password = " + "'" + Password + "';";

                    using (SqlCommand Command = new SqlCommand(Query, Database))
                    {
                        SqlDataReader Reader = Command.ExecuteReader();
                        if (Reader.Read())
                        {
                            //Console.WriteLine("Authenticated!");
                            return true;
                        }

                        else
                        {
                            //Console.WriteLine("Please check your credentials and try again.");
                            return false;
                        }
                    }
                }
                catch (SqlException e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
        }

        public static void AddNewUser(ushort ID)
        {
            int Id = Convert.ToInt32(ID);
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
        }

        public static void AddNewDevice(ushort ID, int State)
        {
            int Id = Convert.ToInt32(ID);
            using (SqlConnection Database = new SqlConnection(ConnectionString))
            {
                try
                {
                    Database.Open();
                    using (SqlCommand Command = new SqlCommand("INSERT INTO " + DevicesTable + " (Id, State) VALUES (@Id, @State);", Database))
                    {
                        Command.Parameters.Add(new SqlParameter("@Id", Id));
                        Command.Parameters.Add(new SqlParameter("@State", State));
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

        public static bool TryGetDevice(int ID, out Device D)
        {
            int Id = Convert.ToInt32(ID);
            using (SqlConnection Database = new SqlConnection(ConnectionString))
            {
                try
                {
                    Database.Open();
                    string Query = "SELECT Id, State FROM " + DevicesTable + " WHERE Id = " + "'" + Id + "';";
                    using (SqlCommand Command = new SqlCommand(Query, Database))
                    {
                        SqlDataReader Reader = Command.ExecuteReader();
                        if (Reader.Read())
                        {
                            //Console.WriteLine("Device found!");
                            D = new Device(Convert.ToUInt16(Reader.GetInt32(0)), Reader.GetByte(1));
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

        public static bool TryGetUser(int ID, out User U)
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

        public static bool GetLatestAssignedID(out ushort LatestAssignedID)
        {
            using (SqlConnection Database = new SqlConnection(ConnectionString))
            {
                try
                {
                    Database.Open();
                    string Query = "SELECT TOP 1 Id FROM " + IDTable + " ORDER BY Id DESC;";
                    using (SqlCommand Command = new SqlCommand(Query, Database))
                    {
                        SqlDataReader Reader = Command.ExecuteReader();
                        if (Reader.Read())
                        {
                            LatestAssignedID = Convert.ToUInt16(Reader.GetInt32(0));
                            return true;
                        }
                        else
                        {
                            LatestAssignedID = 0;
                            return false;
                        }
                    }
                }
                catch (SqlException e)
                {
                    Console.WriteLine(e.Message);
                    LatestAssignedID = 0;
                    return false;
                }
            }
        }

        public static void AddNewID (ushort ID)
        {
            using (SqlConnection Database = new SqlConnection(ConnectionString))
            {
                try
                {
                    Database.Open();
                    string Query = "INSERT INTO " + IDTable + " (Id) VALUES (@Id);";
                    using (SqlCommand Command = new SqlCommand(Query, Database))
                    {
                        Command.Parameters.Add(new SqlParameter("@Id", Convert.ToInt32(ID)));
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
}
