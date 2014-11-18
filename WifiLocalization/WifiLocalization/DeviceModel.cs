using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WifiLocalization
{
    public class DeviceModel
    {
        public DeviceModel()
        {
            RSSValue = new double[] { };
            Property = new PropertyModel();
        }
        public DeviceModel(DeviceModel d)
        {
            Id = d.Id;
            Mac = d.Mac;
            Name = d.Name;
            XLocation = d.XLocation;
            YLocation = d.YLocation;
            RSSValue = d.RSSValue;
            Property = new PropertyModel(d.Property);
        }
        public int Id { get; set; }
        public string Mac { get; set; }
        public string Name { get; set; }
        public double XLocation { get; set; }
        public double YLocation { get; set; }
        public double[] RSSValue { get; set; }
        public PropertyModel Property { get; set; }
        public void DisplayInfo()
        {
            Console.WriteLine(String.Format("Id={0}, Mac={1}, Name={2}", this.Id, this.Mac, this.Name));
        }
        public void GetLocation(LocationModel model)
        {
            var temp = WifiLocalization.Manager.Instance.GetLocation(this, model);
            XLocation = temp.XLocation;
            YLocation = temp.YLocation;
        }
        public void GetStatistics(Double[] a, Double[] b)
        {
            Property = new PropertyModel(WifiLocalization.Manager.Instance.GetStatistics(this.Property, a, b));
        }
    }
}
