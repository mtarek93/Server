using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WifiLocalization
{
    interface IManager
    {
        LocationModel GetLocation(List<LocationModel> online);
        List<LocationModel> ReadOfflineDB(string DBConnectionString);

    }
}
