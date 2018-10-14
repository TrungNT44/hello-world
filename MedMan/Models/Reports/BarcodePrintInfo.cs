using System.Drawing;

namespace MedMan.Models.Reports
{
    public class BarcodePrintInfo
    {
        public string Barcode { get; set; }
        public string EncodedBarcode { get; set; }
        public string ExtraInfo { get; set; }
        public string Name { get; set; }
    }
}