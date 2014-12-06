using Server.WifiLocalization;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private LocationModel FuzzyMethod(List<LocationModel> online, List<LocationModel> offlineList)
        {
            List<List<FuzzyThesis>> Weights = new List<List<FuzzyThesis>>();
            online = CoordinateToIndex(online, offlineList);
            for (int i = 0; i < online.Count(); i++)
            {
                Weights.Add(DataBaseQuerry<FuzzyThesis>("FuzzyThesisTable", online[i].LocationNumber));
            }

           // return FuzzyAverage(Weights, offlineList);
            return FuzzyMaximum(Weights, offlineList);

        }
        private LocationModel FuzzyAverage(List<List<FuzzyThesis>> Weights, List<LocationModel> offlineList)
        {
            LocationModel Fuzzy = new LocationModel();
            Fuzzy.X = 0;
            Fuzzy.Y = 0;
            double[] sumx = new double[3] { 0, 0, 0 };
            double[] sumy = new double[3] { 0, 0, 0 };

            for (int i = 0; i < Weights.First().Count(); i++)
            {
                sumx[0] += Weights[0][i].Euc * offlineList[i].X;
                sumy[0] += Weights[0][i].Euc * offlineList[i].Y;
                sumx[1] += Weights[1][i].KNN * offlineList[i].X;
                sumy[1] += Weights[1][i].KNN * offlineList[i].Y;
                sumx[2] += Weights[2][i].WKNN * offlineList[i].X;
                sumy[2] += Weights[2][i].WKNN * offlineList[i].Y;
            }

            for (int s = 0; s < 3; s++)
            {
                Fuzzy.X += sumx[s];
                Fuzzy.Y += sumy[s];
            }

            Fuzzy.X = Fuzzy.X / 3;
            Fuzzy.Y = Fuzzy.Y / 3;
            return Fuzzy;
        }
        private LocationModel FuzzyMaximum(List<List<FuzzyThesis>> Weights, List<LocationModel> offlineList)
        {
            double[] rssWeightSum = new double[Weights.First().Count()];
            for (int i = 0; i < Weights.First().Count(); i++)
            {
                rssWeightSum[i] = Weights[0][i].Euc + Weights[1][i].KNN + Weights[2][i].WKNN;

            }
            return offlineList[rssWeightSum.ToList().IndexOf(rssWeightSum.Max())];

        }
        private List<LocationModel> MinimumDistance(List<LocationModel> online, List<LocationModel> offlineList)
        {
            List<LocationModel> minDist = new List<LocationModel>();
            for (int i = 0; i < offlineList.Count() / 3; i++)
            {
                LocationModel d = new LocationModel() { RSSI = 0, X = 0, Y = 0, LocationNumber = 0 };
                var Vectors = offlineList.Where(row => row.LocationNumber == i + 1);

                for (int j = 0; j < online.Count(); j++)
                {
                    var val = Vectors.Where(s => String.Equals(s.MAC, Helper.MacFormat(online[j].MAC), StringComparison.CurrentCultureIgnoreCase));
                    if (val.Count() != 0)
                        d.RSSI += Math.Pow(Math.Abs(online[j].RSSI) - Math.Abs(val.FirstOrDefault().RSSI), 2);
                }

                var obj = Vectors.First();
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
        private List<LocationModel> CoordinateToIndex(List<LocationModel> online, List<LocationModel> offlineList)
        {
            var Vectors = offlineList.Where(row => row.ApNumber == 1);

            for (int i = 0; i < online.Count(); i++)
            {
                double x2 = 0, y2 = 0;
                List<LocationModel> dList = new List<LocationModel>();


                foreach (var v in Vectors)
                {
                    LocationModel d = new LocationModel() { RSSI = 0, X = 0, Y = 0, LocationNumber = 0 };
                    x2 = Math.Pow(Math.Abs(online[i].X) - Math.Abs(v.X), 2);
                    y2 = Math.Pow(Math.Abs(online[i].Y) - Math.Abs(v.Y), 2);
                    d.RSSI = Math.Sqrt(x2 + y2);
                    d.LocationNumber = v.LocationNumber;
                    d.MAC = "";
                    d.Room = v.Room;
                    d.Sector = v.Sector;
                    d.X = online[i].X;
                    d.Y = online[i].Y;
                    dList.Add(d);
                }
                online[i] = dList.OrderBy(x => x.RSSI).First();

            }
            return online;
        }
        public LocationModel LocalizationAlgorithm(List<LocationModel> online, List<LocationModel> offlineList)
        {
            List<LocationModel> data = MinimumDistance(online, offlineList);
            List<LocationModel> sorted = new List<LocationModel>();
            List<LocationModel> methodsOutput = new List<LocationModel>();

            var sortedData = data.OrderBy(x => x.RSSI);
            foreach (var sd in sortedData)
                sorted.Add(sd);

            methodsOutput.Add(sorted.First());
            LocationModel KNN = new LocationModel();
            LocationModel WKNN = new LocationModel();
            int k = 0;
            double W_d = 0;
            double knn_x = 0, knn_y = 0;
            double wknn_x = 0, wknn_y = 0;

            var list = sorted.Take(K);
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
            methodsOutput.Add(KNN);
            WKNN.X = wknn_x / W_d;
            WKNN.Y = wknn_y / W_d;
            methodsOutput.Add(WKNN);


            return FuzzyMethod(methodsOutput, offlineList);
        }
        public List<type> DataBaseQuerry<type>(string p, int i)
        {

            List<type> ListData = new List<type>();
            WifiDataContext DatabaseContext = new WifiDataContext();

            if (p == "AverageOfflineTable")
            {
                var list = DatabaseContext.AverageOfflineTables.Where(row => row.MapNumber == i);

                foreach (var item in list)
                    ListData.Add(Helper.Mapper<AverageOfflineTable, type>(item, Activator.CreateInstance<type>()));

            }
            if (p == "OnlineTable")
            {
                var list = DatabaseContext.OfflineTables.Where(row => row.LocationNumber == i && row.MapNumber == 13);

                foreach (var item in list)
                    ListData.Add(Helper.Mapper<OfflineTable, type>(item, Activator.CreateInstance<type>()));

            }
            if (p == "FuzzyThesisTable")
            {
                var list = DatabaseContext.FuzzyThesis.Where(row => row.LocationNumber == i);
                foreach (var item in list)
                    ListData.Add(Helper.Mapper<FuzzyThesis, type>(item, Activator.CreateInstance<type>()));

            }
            if (p == "OfflineTable")
            {
                var list = DatabaseContext.OfflineTables.Where(row => row.MapNumber == i);

                foreach (var item in list)
                    ListData.Add(Helper.Mapper<OfflineTable, type>(item, Activator.CreateInstance<type>()));
            }

            return ListData;
        }
    }
}

