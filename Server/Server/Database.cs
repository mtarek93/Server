using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using TCP_Server;

namespace Database
{
    class DatabaseHandler
    {
        public static string ConnectionString, LoginTable = "LoginTable", UsersTable = "UsersTable", DevicesTable = "DevicesTable";

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

        public static void AddNewUser(string name)
        {
            using (SqlConnection Database = new SqlConnection(ConnectionString))
            {
                try
                {
                    Database.Open();
                    using (SqlCommand Command = new SqlCommand("INSERT INTO " + UsersTable + " (Name) VALUES (@Name);", Database))
                    {
                        Command.Parameters.Add(new SqlParameter("@Name", name));
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

        public static void AddNewDevice(string name, string State)
        {
            using (SqlConnection Database = new SqlConnection(ConnectionString))
            {
                try
                {
                    Database.Open();
                    using (SqlCommand Command = new SqlCommand("INSERT INTO " + DevicesTable + " (Name, State) VALUES (@Name, @State);", Database))
                    {
                        Command.Parameters.Add(new SqlParameter("@Name", name));
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

        public static bool TryGetDevice(string name, out Device D)
        {
            using (SqlConnection Database = new SqlConnection(ConnectionString))
            {
                try
                {
                    Database.Open();
                    string Query = "SELECT Name, State FROM " + DevicesTable + " WHERE Name = " + "'" + name + "';";
                    using (SqlCommand Command = new SqlCommand(Query, Database))
                    {
                        SqlDataReader Reader = Command.ExecuteReader();
                        if (Reader.Read())
                        {
                            //Console.WriteLine("Device found!");
                            D = new Device(Convert.ToUInt16(Reader.GetString(0).Trim()), Reader.GetString(1).Trim());
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

        public static bool TryGetUser(string name, out User U)
        {
            using (SqlConnection Database = new SqlConnection(ConnectionString))
            {
                try
                {
                    Database.Open();
                    string Query = "SELECT Name FROM " + UsersTable + " WHERE Name = " + "'" + name + "';";

                    using (SqlCommand Command = new SqlCommand(Query, Database))
                    {
                        SqlDataReader Reader = Command.ExecuteReader();
                        if (Reader.Read())
                        {
                            //Console.WriteLine("User found!");
                            U = new User(Convert.ToUInt16(Reader.GetString(0).Trim()));
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
                    string Query = "SELECT Username FROM " + LoginTable + " WHERE UserName = " + "'" + Username + "';";

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
}
