using System;

namespace Med.ServiceModel.Report
{
    public class DrugTransHistoryItem
    {
        public int ItemId { get; set; }
        public int Order { get; set; }        
        public int DrugId { get; set; } 
        public string DrugCode { get; set; }
        public string DrugName { get; set; }
        public DateTime ItemDate { get; set; }
        public int ItemNumber { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        public double Amount { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
    }
}
