using Med.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.Report
{
    public class DrugWarehouseSynthesis
    {
        public int DrugId { get; set; }
        public double FirstInventoryQuantity
        {
            get { return Math.Round(InitReceiptQuantity + FirstReceiptQuantity - FirstDeliveryQuantity, 1); }
        }
        public double FirstInventoryValue { get; set; }
        public double LastInventoryQuantity
        {
            get { return Math.Round(InitReceiptQuantity + LastReceiptQuantity - LastDeliveryQuantity, 1); }
        }
        public double LastInventoryValue { get; set; }

        public double ReceiptInventoryQuantityInPeriod { get { return LastReceiptQuantity - FirstReceiptQuantity; } }
        public double ReceiptInventoryValueInPeriod { get; set; }
        public double DeliveryInventoryQuantityInPeriod { get { return LastDeliveryQuantity - FirstDeliveryQuantity; } }
        public double DeliveryInventoryValueInPeriod { get; set; }
        public double InitReceiptQuantity { get; set; }
        public double InitReceiptValue { get; set; }
        public double FirstReceiptQuantity { get; set; }
        public double FirstReceiptValue { get; set; }
        public double LastReceiptQuantity { get; set; }
        public double LastReceiptValue { get; set; }
        public double FirstDeliveryQuantity { get; set; }
        public double FirstDeliveryValue { get; set; }
        public double LastDeliveryQuantity { get; set; }
        public double LastDeliveryValue { get; set; }
        public double InitRetailPrice { get; set; }
        public double LastReceiptRetailPrice { get; set; }
        public bool IsActivated { get; set; }
        public int LimitQuantityWarning { get; set; }
        public bool IsOutOfStock { get { return LastInventoryQuantity <= LimitQuantityWarning; } }
        public bool IsNagativeStock { get { return LastInventoryQuantity < 0; } }
    }
}
