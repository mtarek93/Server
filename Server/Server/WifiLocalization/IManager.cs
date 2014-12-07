using System.Collections.Generic;

namespace WifiLocalization
{
    interface IManager
    {
        LocationModel GetLocation(List<LocationModel> online);
        List<LocationModel> ReadOfflineDB();

    }
}
