using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.Delivery
{
    public class DrugDeliveryItem
    {
        public int DrugId { get; set; }
        public string DrugCode { get; set; }
        public string DrugName { get; set; }
        public DrugUnit[] DrugUnits { get; set; }
        public double RetailPrice { get; set; }
        public double Price { get; set; }
        public int SelectedUnitId { get; set; }
        public int RetailUnitId { get; set; }
        public int UnitId { get; set; }
        public double Quantity { get; set; }
        public double QuantityPerOneDose { get; set; }
        public double TotalAmount { get; set; }
        public double Factors { get; set; }
        public bool EditMode { get; set; }
        public double LastInventoryQuantity { get; set; }
        public double RemainQuantity { get; set; }
    }
}
