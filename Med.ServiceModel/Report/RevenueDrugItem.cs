using Med.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.Report
{
    public class RevenueDrugItem
    {
        public string Order { get; set; }
        public string CustomerName { get; set; }
        public int DeliveryNoteId { get; set; }
        public double? VAT { get; set; }
        public string DrugCode { get; set; }
        public string DrugName { get; set; }
        public string UnitName { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        public double Amount { get; set; }
        public double Revenue { get; set; }
        public string NoteNumber { get; set; }
        public bool IsReturnFromCustomer { get; set; }
        public double DebtAmount { get; set; }
        public bool ShallWarning
        {
            get
            {
                return IsReturnFromCustomer || Revenue < 0;
            }
        }
    }
}
