using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Data.Linq;
using System.Text;
using System.Reflection;
using Server.WifiLocalization;
//using Excel = Microsoft.Office.Interop.Excel;

namespace WifiLocalization
{
    public class Implement
    {
        #region Class Constructor
        private static Implement _instance;
        public static Implement Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Implement();

                return _instance;
            }
        }
        #endregion
        private static int K = 5;

        internal List<LocationModel> DataBaseQuerry(int MapNumber)
        {

            List<LocationModel> offlineMapList = new List<LocationModel>();
            LocationModel offlineMap = new LocationModel();
            WifiDataContext DatabaseContext = new WifiDataContext();

            var list = DatabaseContext.AverageOfflineTables.Where(row => row.MapNumber == 1);
            foreach (var item in list)
            {

                offlineMap = Mapper<AverageOfflineTable, LocationModel>.MapTo(item, new LocationModel());
                offlineMapList.Add(offlineMap);

            }

            return offlineMapList;
        }
        internal LocationModel LocalizationAlgorithm(List<LocationModel> online, List<LocationModel> offlineList)
        {
            List<LocationModel> data = MinimumDistance(online, offlineList);
            List<LocationModel> sorted = new List<LocationModel>();

            var sortedData = data.OrderBy(x => x.RSSI);
            foreach (var sd in sortedData)
                sorted.Add(sd);

            LocationModel Euclidean = sorted.First();
            LocationModel KNN = new LocationModel();
            LocationModel WKNN = new LocationModel();
            int k = 0;
            double W_d = 0;
            double knn_x = 0, knn_y = 0;
            double wknn_x = 0, wknn_y = 0;

            var list = sorted.Take(5);
            foreach (var item in list)
            {
                knn_x += item.X;
                knn_y += item.Y;
                wknn_x += (item.X / item.RSSI);
                wknn_y += (item.Y / item.RSSI);
                W_d += (1 / item.RSSI);
                k++;
            }
            KNN.X = knn_x / K;
            KNN.Y = knn_y / K;
            WKNN.X = wknn_x / W_d;
            WKNN.Y = wknn_y / W_d;

            return Euclidean;
        }
        public List<LocationModel> MinimumDistance(List<LocationModel> online, List<LocationModel> offlineList)
        {
            List<LocationModel> minDist = new List<LocationModel>();
            for (int i = 0; i < offlineList.Count() / 3; i++)
            {
                LocationModel d = new LocationModel() { RSSI = 0 };
                var rssVectors = offlineList.Where(row => row.LocationNumber == i + 1);

                for (int j = 0; j < online.Count(); j++)
                {
                    var val = rssVectors.Where(s => String.Equals(s.MAC, Helper.MacFormat(online[j].MAC), StringComparison.CurrentCultureIgnoreCase));
                    d.RSSI += Math.Pow(Math.Abs(online[j].RSSI) - Math.Abs(val.FirstOrDefault().RSSI), 2);
                }
                var obj = rssVectors.First();
                d.X = obj.X;
                d.Y = obj.Y;
                d.MAC = "";
                d.Room = obj.Room;
                d.Sector = obj.Sector;
                d.LocationNumber = i + 1;
                d.RSSI = Math.Sqrt(d.RSSI);
                minDist.Add(d);

            }

            return minDist;
        }

    }
}

