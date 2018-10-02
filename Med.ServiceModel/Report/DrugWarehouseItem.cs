using Med.Common;

namespace Med.ServiceModel.Report
{
    public class DrugWarehouseItem
    {
        public int DrugId { get; set; }
        public int Order { get; set; }
        public string DrugCode { get; set; }
        public string DrugName { get; set; }
        public string DrugRetailUnitName { get; set; }
        public double FirstInventoryQuantity { get; set; }
        public double FirstInventoryValue { get; set; }
        public double ReceiptInventoryQuantityInPeriod { get; set; }
        public double ReceiptInventoryValueInPeriod { get; set; }
        public double DeliveryInventoryQuantityInPeriod { get; set; }
        public double DeliveryInventoryValueInPeriod { get; set; }

        public double LastInventoryQuantity { get; set; }
        public double LastInventoryValue { get; set; }
    }
}
