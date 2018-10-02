using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Med.Common;

namespace Med.ServiceModel.Common
{
    public class MinModifiedDateItem
    {
        public int NoteItemId { get; set; }
        public int DrugId { get; set; }
        public double RetailQuantity { get; set; }
        public DateTime? NoteDate { get; set; }
    }
}
