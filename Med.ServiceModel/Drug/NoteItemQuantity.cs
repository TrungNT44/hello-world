using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.Drug
{
    public class NoteItemQuantity
    {
        public int DrugId { get; set; }
        public int NoteItemId { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        public int NoteType { get; set; }
        public int UnitId { get; set; }
        public int? DrugUnitId { get; set; }
        public double DrugUnitFactors { get; set; }
        public double RetailQuantity { get; set; }
        public double RetailPrice { get; set; }
        public DateTime? NoteDate { get; set; }
    }
}
