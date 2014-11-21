using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WifiLocalization
{
    public class Manager : IManager
    {
        #region Manager Class Constructor
        private static Manager _instance;
        private static readonly object lockObject = new object();
        //private Manager() { }
        public static Manager Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (_instance == null)
                        _instance = new Manager();
                }
                return _instance;
            }
        }
        # endregion

        List<LocationModel> OfflineList = new List<LocationModel>();
        Implement _implement = Implement.Instance; 

        
        public List<LocationModel> ReadOfflineDB()
        {
            string DBConnectionString = Database.DatabaseHandler.ConnectionString;
            SqlConnection cnn = new SqlConnection(DBConnectionString);

            try
            {
                cnn.Open();
                Console.Write("Tee Wifi DB Connection Open ! \n");
                OfflineList = _implement.DataBaseQuerry(1);
                cnn.Close();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                Console.Write("Can not open Tee Wifi DB connection ! \n");
            }
            return OfflineList;
        }
        public LocationModel GetLocation(List<LocationModel> online)
        {
            if (OfflineList.Count == 0)
                OfflineList = ReadOfflineDB();
                return _implement.KNearestNeighbor(online, OfflineList);
        }       

    }
}
