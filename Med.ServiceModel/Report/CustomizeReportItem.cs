using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Common.Utility;

namespace Med.ServiceModel.Report
{
    public class CustomizeReportItemResponse
    {
        public List<CustomizeReportItem> CustomizeReportItems { get; set; }
        public List<string> DrugNames { get; set; }
        public List<decimal> TotalQuantityAndPrice { get; set; }

        public CustomizeReportItemResponse()
        {
            CustomizeReportItems = new List<CustomizeReportItem>();
            DrugNames = new List<string>();
            TotalQuantityAndPrice = new List<decimal>();
        }
    }
    public class CustomizeReportItem
    {
        public int? DeliveryId { get; set; }
        public int? DoctorId { get; set; }
        public int? CustomerId { get; set; }
        public string DoctorName { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public DateTime? ExportDate { get; set; }
        public Dictionary<int, CustomizeDrugItem> CustomizeDrugItems { get; set; }
        public List<decimal> DrugQuantityAndPrice { get; set; }
    }

    public class CustomizeDrugItem
    {
        public int Id { get; set; }
        public string Code { get; set;}
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal PriceAfterDiscount { get; set; }
        
        //public CustomizeDrugItem DeepCopy()
        //{
        //    return this.DeepCopyByExpressionTree();
        //}
    }
}
