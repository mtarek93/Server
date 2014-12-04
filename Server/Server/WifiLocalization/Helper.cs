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
