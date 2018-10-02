using Med.Common;
using Med.ServiceModel.Common;
using System;
using System.Collections.Generic;

namespace Med.ServiceModel.Utilities
{
    public class RemainQuantityReceiptDrugItem : BaseItem
    {
        public int NoteId { get; set; }
        public int NoteItemId { get; set; }
        public double Quantity { get; set; }
        public string UnitName { get; set; }
        public string SerialNumber { get; set; }
        public double NonTransNumDays { get; set; }
        public double ExpiredNumDays { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string ExpiredDateString {
            get
            {
                var retVal = string.Empty;
                if (ExpiredDate.HasValue)
                {
                    retVal = ExpiredDate.Value.ToString("dd/MM/yyyy");
                }

                return retVal;
            }
        }
        public bool IsExpired { get; set; }
        public bool IsLittleTrans { get; set; }
        public int NoteTypeID { get; set; }
    }
}

