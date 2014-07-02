using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Clients;

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
        public static string ConnectionString, LoginTable = "LoginTable", UsersTable = "UsersTable", DevicesTable = "DevicesTable", IDTable = "IDTable";
        static object DatabaseLock = new object();

        public static void AddUserAccount(string Username, string Password)
        {
            lock (DatabaseLock)
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
        public static bool UserIsAuthenticated(string Username, string Password)
        {
            lock (DatabaseLock)
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
        }
        public static void AddNewUser(ushort ID)
        {
            lock (DatabaseLock)
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
                AddNewID(ID);
            }
        }
        public static void AddNewDevice(ushort ID, int State)
        {
            lock (DatabaseLock)
            {
                int Id = Convert.ToInt32(ID);
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
                AddNewID(ID);
            }
        }
        public static bool TryGetDevice(int ID, out Device D)
        {
            lock (DatabaseLock)
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
        }
        public static bool TryGetUser(int ID, out User U)
        {
            lock (DatabaseLock)
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
        }
        public static bool UsernameExists(string Username)
        {
            lock (DatabaseLock)
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
        public static ushort GetNumberofIDs()
        {
            lock (DatabaseLock)
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
                                return Convert.ToUInt16(Reader.GetInt32(0));
                            else
                                return (ushort)0;
                        }
                    }
                    catch (SqlException e)
                    {
                        Console.WriteLine(e.Message);
                        return (ushort)0;
                    }
                }
            }
        }
        private static void AddNewID (ushort ID)
        {
            lock (DatabaseLock)
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
}
