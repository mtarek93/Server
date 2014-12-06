using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.RegularExpressions;

namespace WifiLocalization
{
    public static class Helper
    {

        internal static string ConnectionStringHandler()
        {
            Dictionary<string, string> DeviceMac = new Dictionary<string, string>()
                    {
                        { "Miky" , "" }, 
                        { "MT"   , "" },
                        { "Teefa", "" }, 
                        { "Tee"  , "4C: EB: 42: 6F: 82: 09" }, 
                        { "Bahaa", "" }
   
                    };


            string s = "";
            string macAddr = Helper.MacFormat(
                           (
                               from nic in NetworkInterface.GetAllNetworkInterfaces()
                               where nic.OperationalStatus == OperationalStatus.Up
                               select nic.GetPhysicalAddress().ToString()
                           ).FirstOrDefault());

            s = DeviceMac.FirstOrDefault(x => x.Value == macAddr).Key;
            Console.WriteLine("Welcome " + s);

            switch (s)
            {
                case "Miky":    // Miky
                    s = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\Michael\Documents\GitHub\Server\Server\Server\Database .mdf;Integrated Security=True;Connect Timeout=30";
                    break;
                case "MT":      // Mt
                    s = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\Mohamed\Dropbox\THESIS PROJECT\Thesis II - EENG 491\T2\Database.mdf;Integrated Security=True;Connect Timeout=30";
                    break;
                case "Teefa":   // Teefa
                    s = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\mosta_000\Documents\GitHub\Server\Server\Server\Database.mdf;Integrated Security=True;Connect Timeout=30";
                    break;
                case "Tee":     // Tee
                    s = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=E:\Dropbox\THESIS PROJECT\Thesis II - EENG 491\T2\Database.mdf;Integrated Security=True;Connect Timeout=30";
                    break;
                case "Bahaa":   // Baha2
                    s = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\GitHub\Server\Server\Server\Database.mdf;Integrated Security=True;Connect Timeout=30";
                    break;
            }
            return s;
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
