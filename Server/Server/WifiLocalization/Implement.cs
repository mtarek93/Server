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

        public LocationModel KNearestNeighbor(List<LocationModel> online, List<LocationModel> Offline)
        {
            
            //Dictionary<int, Double> dist = new Dictionary<int, double>();
            //List<Double[]> ap = Offline.RSSValueList;
            //Double[] us = user.RSSValue;
            //int knn = user.Property.KNN;
            //int exp = user.Property.Exponent;
            //int uNum = us.Length;
            //int apNum = ap.Count();
            //int locNum = ap[0].Length;
            //double sum;
            //double invExp = 1 / (double)exp;
             


            //for (int j = 0; j < locNum; j++)
            //{
            //    sum = 0;

            //    for (int i = 0; i < apNum; i++)
            //    {
            //        sum += Math.Pow(us[i] - ap[i][j], exp);

            //    }

            //    dist.Add(j, Math.Pow(sum, invExp));
            //}

            //var results = dist.OrderBy(i => i.Value).Take(knn);

            //double x = 0, y = 0;
            //foreach (KeyValuePair<int, double> result in results)
            //{
            //    x += Offline.X[result.Key];
            //    y += Offline.Y[result.Key];

            //}
            
            //DeviceModel KNNResult = new DeviceModel(user);

            //KNNResult.XLocation = x / knn;
            //KNNResult.YLocation = y / knn;
            LocationModel temp = new LocationModel();
            temp.X = 10;
            temp.Y = 20;
            return temp;
        }
        //public PropertyModel Statistics(PropertyModel property, Double[] original, Double[] predicted)
        //{


        //    int length = (predicted.Length <= original.Length) ? predicted.Length : original.Length;

        //    double[] dist = new double[length];

        //    if (length == 0)
        //        return null;
        //    else
        //    {
        //        int count = 0;
        //        for (int i = 0; i < length; i++)
        //        {
        //            dist[i] = Math.Abs(original[i] - predicted[i]);
        //            if (dist[i] <= property.Threshold)
        //                count++;
        //            i++;
        //        }
        //        property.Minimum = dist.Min();
        //        property.Maximum = dist.Max();
        //        property.Average = dist.Sum() / length;
        //        property.Accuracy = (double)count * 100 / length;

        //        return property;

        //    }

        //}
        //public LocationModel WKNearestNeighbor() { return null; }
        //public Array ExcelRead(string xlPath, int xlSheetNum, string xlRangestring)
        //{

        //    char[] delims = { ':' };
        //    String[] xlRange = xlRangestring.Split(delims);

        //    Excel.Application xlApp;
        //    Excel.Workbook xlWorkBook;
        //    Excel.Worksheet xlWorkSheet;
        //    object misValue = System.Reflection.Missing.Value;

        //    xlApp = new Excel.Application();
        //    xlWorkBook = xlApp.Workbooks.Open(xlPath);
        //    xlWorkSheet = (Excel.Worksheet)xlWorkBook.Sheets[xlSheetNum];

        //    System.Array readData = (System.Array)xlWorkSheet.get_Range(xlRange[0], xlRange[1]).Value2;

        //    xlWorkBook.Close(true, misValue, misValue);
        //    xlApp.Quit();

        //    releaseObject(xlWorkSheet);
        //    releaseObject(xlWorkBook);
        //    releaseObject(xlApp);

        //    return readData;
        //}
        public List<LocationModel> DataBaseQuerry(int MapNumber)
        {
            
            List<LocationModel> offlineMapList = new List<LocationModel>();
            LocationModel offlineMap = new LocationModel();
            WifiDataContext DatabaseContext = new WifiDataContext();
            
            //var all = DatabaseContext.OfflineTables.Select(row => row);
            //for (int i = 1; i <= 3; i++)
            //{
            //}

                var list = DatabaseContext.OfflineTables.Where(row => row.LocationNumber==1);
                foreach (var item in list)
                {

                    offlineMap = Mapper<OfflineTable, LocationModel>.MapTo(item, new LocationModel());
                    offlineMap.DisplayInfo();
                    offlineMapList.Add(offlineMap);

                }
  
            return offlineMapList; 
        }
       
    }
}

