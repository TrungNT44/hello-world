using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Med.Common;

namespace Med.ServiceModel.Common
{
    public class BaseNoteItemInfo
    {
        public int NoteId { get; set; }
        public int NoteItemId { get; set; }
        public int DrugId { get; set; }
        public string DrugCode { get; set; }
        public string DrugName { get; set; }
        public int? DrugRetailUnitId { get; set; }
        public int? DrugUnitId { get; set; }
        public double DrugUnitFactors { get; set; }
        public double Quantity { get; set; }
        public double RetailQuantity { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public int NoteType { get; set; }
        public DateTime? NoteDate { get; set; }
        public DateTime? PreNoteDate { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int? StaffId { get; set; }
        public string StaffName { get; set; }
        public int? DoctorId { get; set; }
        public string DoctorName { get; set; }
        public int? SupplyerId { get; set; }
        public string SupplyerName { get; set; }
        public double Discount { get; set; }
        public double Price { get; set; }
        public double RetailPrice { get; set; }
        public long NoteNumber { get; set; }
        public double VAT { get; set; }
        public double ReduceQuantity { get; set; }
        public double Revenue { get; set; }
        public double DebtAmount { get; set; }
        public bool IsDebt { get; set; }
        public double RealQuantity
        {
            get
            {
                var qty = RetailQuantity;
                if (DrugUnitId.HasValue && UnitId == DrugUnitId.Value && DrugUnitFactors > MedConstants.EspQuantity)
                {
                    qty = qty/DrugUnitFactors;
                }

                return qty;
            }
        }

        public double FinalRealQuantity
        {
            get
            {
                var qty = FinalRetailQuantity;
                if (DrugUnitId.HasValue && UnitId == DrugUnitId.Value && DrugUnitFactors > MedConstants.EspQuantity)
                {
                    qty = qty / DrugUnitFactors;
                }

                return qty;
            }
        }

        public double Amount
        {
            get { return Quantity*Price; }
        }

        public double RetailAmount
        {
            get { return RetailQuantity * RetailPrice; }
        }

        public double FinalRetailQuantity
        {
            get { return (RetailQuantity - ReduceQuantity); }
        }

        public double FinalRetailAmount
        {
            get { return FinalRetailQuantity * FinalRetailPrice; }
        }

        public double FinalPrice
        {
            get
            {
                // Giá N/X = Giá N/X x (1 - CK/100) x (1 + VAT/100)
                return Price * (1 - (Discount / 100)) * (1 + (VAT / 100));
            }
        }

        public double FinalRetailPrice
        {
            get
            {
                // Giá N/X = Giá N/X x (1 - CK/100) x (1 + VAT/100)
                return RetailPrice * (1 - (Discount / 100)) * (1 + (VAT / 100));
            }
        }

        public double FinalAmount
        {
            get { return Quantity * FinalPrice; }
        }
    }
}
