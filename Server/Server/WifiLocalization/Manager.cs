using System.Collections.Generic;

namespace WifiLocalization
{
    public class Manager : IManager
    {
        #region Manager class Constructor
        private static Manager _instance;
        private static readonly object lockObject = new object();
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

        public LocationModel GetLocation(List<LocationModel> online)
        {
            if (OfflineList.Count == 0)
                OfflineList = ReadOfflineDB();
                return _implement.LocalizationAlgorithm(online, OfflineList);
        }
        public List<LocationModel> ReadOfflineDB()
        {
            return _implement.DataBaseQuerry<LocationModel>("AverageOfflineTable",1);

        }
    }
}
