using Server.WifiLocalization;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WifiLocalization
{
    public static class Mapper<typeA, typeB>
    {
        public static typeB MapTo(typeA a,typeB b)
        {
            Type typeB = b.GetType();
            foreach (PropertyInfo property in a.GetType().GetProperties())
            {
                if (!property.CanRead || (property.GetIndexParameters().Length > 0))
                    continue;

                PropertyInfo other = typeB.GetProperty(property.Name);
                if ((other != null) && (other.CanWrite))
                    other.SetValue(b, property.GetValue(a, null), null);
            }

            return b;
        }
    }
    public static class Helper
    {
        public static List<LocationModel> RandomOnlineReadings(int i)
        {
            string DBConnectionString = Database.DatabaseHandler.ConnectionString;
            SqlConnection cnn = new SqlConnection(DBConnectionString);
            List<LocationModel> onlineList = new List<LocationModel>();
            LocationModel online = new LocationModel();
            WifiDataContext DatabaseContext = new WifiDataContext();

            try
            {
                cnn.Open();
                Console.Write("Getting Online Reading From Database ! \n");
                var list = DatabaseContext.AverageOfflineTables.Where(row => row.LocationNumber == i);
                foreach (var item in list)
                {
                    online = Mapper<AverageOfflineTable, LocationModel>.MapTo(item, new LocationModel());
                    online.DisplayInfo();
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
        //public static Double[] Magnitude(Double[] x, Double[] y)
        //{
        //    int length = x.Length;
        //    Double[] s = new Double[length];
        //    for (int i = 0; i < length; i++)
        //    {
        //        s[i] = Math.Sqrt(Math.Pow(x[i], 2) + Math.Pow(y[i], 2));
        //    }
        //    return s;
        //}
        //public static Double[] ObjectToDouble(Array values)
        //{

        //    int length = values.Length;
        //    Double[] result = new Double[length];
        //    int i = 0;

        //    foreach (object value in values)
        //    {
        //        try
        //        {
        //            result[i++] = Convert.ToDouble(value);
        //            // Console.WriteLine("Converted the {0} value {1} to {2}.",
        //            //                value.GetType().Name, value, result);
        //        }
        //        catch (FormatException)
        //        {
        //            // Console.WriteLine("The {0} value {1} is not recognized as a valid Double value.",
        //            //                value.GetType().Name, value);
        //        }
        //        catch (InvalidCastException)
        //        {
        //            // Console.WriteLine("Conversion of the {0} value {1} to a Double is not supported.",
        //            //                value.GetType().Name, value);
        //        }
        //    }
        //    return result;
        //}

    }
}
