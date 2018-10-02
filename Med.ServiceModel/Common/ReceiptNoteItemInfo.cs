using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.Common
{
    public class ReceiptNoteItemInfo : BaseNoteItemInfo
    {
        public DateTime? ExpiredDate { get; set; }
        public double RemainRefQuantity { get; set; }
        public string SerialNumber { get; set; }
        public string Barcode { get; set; }
    }
}
