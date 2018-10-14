using System;

namespace sThuoc.Models
{
    public class ShortDrugInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Barcode { get; set; }
        public Decimal Price { get; set; }
        public Decimal BatchPrice { get; set; }
        public Decimal TotalPrice { get; set; }
        public Decimal CurrentPrice { get; set; }
        public int Unit { get; set; }
        public int BatchUnit { get; set; }
        public string UnitName { get; set; }
        public string BatchUnitName { get; set; }
        public int SelectedUnit { get; set; }
        public double Quantity { get; set; }
        public string ItemCode { get; set; }
        public string OldItemCode { get; set; }
    }
}