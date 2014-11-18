using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WifiLocalization
{
    public class LocationModel
    {
        public LocationModel() { }
        public string BSSID { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int RSSI { get; set; }
        public int Index { get; set; }
        public int MapNumber { get; set; }
        public int Room { get; set; }
        public int Sector { get; set; }
        public void DisplayInfo()
        {
            Console.WriteLine(String.Format("x={0}, y={1}", this.X, this.Y));
        }
    }
    public class PropertyModel
    {
        public PropertyModel() { }

        public int KNN { get; set; }
        public int Exponent { get; set; }
        public Double Threshold { get; set; }
        public Double Accuracy { get; set; }
        public Double Average { get; set; }
        public Double Minimum { get; set; }
        public Double Maximum { get; set; }
        public void DisplayInfo()
        {
            Console.WriteLine(String.Format("KNN={0}, Exp={1}, Thres={2}\nAcc={3}, Avg={4}, Max={5}, Min={6}",
                this.KNN, this.Exponent, this.Threshold, this.Accuracy, this.Average, this.Maximum, this.Minimum));
        }

    }
}
