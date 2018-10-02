using Med.Common;
using System;

namespace Med.ServiceModel.Report
{
    public class ReportByGoodsItem: ReportByBaseItem
    {
        /// <summary>
        /// Retail Unit Name
        /// </summary>
        public string DrugUnit { get; set; }

        /// <summary>
        /// Quantity
        /// </summary>
        public double Quantity { get; set; }      

        /// <summary>
        /// Returned Quantity
        /// </summary>
        public double ReturnedQuantity { get; set; }

        /// <summary>
        /// Delivery Quantity
        /// </summary>
        public double DeliveryQuantity { get { return Quantity + ReturnedQuantity; } }
    }
}
