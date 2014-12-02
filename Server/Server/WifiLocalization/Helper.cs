using Server.WifiLocalization;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WifiLocalization
{
    public static class Helper
    {
        internal static List<type> DataBaseQuerry<type>(int MapNumber)
        {

            List<type> ListData = new List<type>();
            WifiDataContext DatabaseContext = new WifiDataContext();
            var list = DatabaseContext.AverageOfflineTables.Where(row => row.MapNumber == 1);
            foreach (var item in list)
            {
                type Data = Helper.Mapper<AverageOfflineTable, type>(item, Activator.CreateInstance<type>());
                ListData.Add(Data);

            }

            return ListData;
        }
        internal static List<LocationModel> RandomOnlineReadings(int i)
        {
            string DBConnectionString = Database.DatabaseHandler.ConnectionString;
            SqlConnection cnn = new SqlConnection(DBConnectionString);
            List<LocationModel> onlineList = new List<LocationModel>();
            LocationModel online = new LocationModel();
            WifiDataContext DatabaseContext = new WifiDataContext();

            try
            {
                cnn.Open();
                var list = DatabaseContext.OfflineTables.Where(row => row.LocationNumber == i && row.MapNumber == 13);
                foreach (var item in list)
                {
                    online = Helper.Mapper<OfflineTable, LocationModel>(item, new LocationModel());
                    onlineList.Add(online);
                }

                cnn.Close();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                Console.Write("Can not open Tee Wifi DB connection ! \n");
            }
            return onlineList;
        }
        internal static typeB Mapper<typeA, typeB>(typeA a, typeB b)
        {
            Type tB = b.GetType();
            foreach (PropertyInfo property in a.GetType().GetProperties())
            {
                if (!property.CanRead || (property.GetIndexParameters().Length > 0))
                    continue;

                PropertyInfo other = tB.GetProperty(property.Name);
                if ((other != null) && (other.CanWrite))
                    other.SetValue(b, property.GetValue(a, null), null);
            }

            return b;
        }
        internal static string MacFormat(string mac)
        {

            Regex rgx = new Regex("[^a-fA-F0-9]");
            mac = rgx.Replace(mac, "");
            mac = string.Join(": ", Enumerable.Range(0, 6)
                .Select(i => mac.Substring(i * 2, 2)));
            return mac;
        }
    }
}
