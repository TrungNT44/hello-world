using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.Drug
{
    public class ItemInfoCandidate
    {
        public int? DrugId { get; set; }
        public int NoteId { get; set; }
        public int NoteItemId { get; set; }
        public double Quantity { get; set; }
        public double RetailQuantity { get; set; }
        public double ReduceQuantity { get; set; }
        public int? ItemUnitId { get; set; }
        public int? UnitId { get; set; }
        public int? RetailUnitId { get; set; }
        public double Factors { get; set; }
        public double Price { get; set; }
        public double VAT { get; set; }
        public double Discount { get; set; }
        public double RetailPrice { get; set; }
        public int NoteType { get; set; }
        public DateTime? NoteDate { get; set; }
        public int ItemHandledStatusId { get; set; }
        public double FinalPrice
        {
            get
            {
                // Giá N/X = Giá N/X x (1 - CK/100) x (1 + VAT/100)
                return Price * (1 - Discount / 100) * (1 + VAT / 100);
            }
        }
        public double OutPrice { get; set; }
        public double RetailOutPrice { get; set; } 

    }
}
