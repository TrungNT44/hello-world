using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.InOutComming
{
    public class IncommingNoteModel
    {
        public int NoteId { get; set; }
        public int NoteTypeId { get; set; }
        public DateTime NoteDate { get; set; }
        public long NoteNumber { get; set; }
        public string CreatedByName { get; set; }
        public int CreatedById { get; set; }
        public int ReceiverId { get; set; }
        public int ReceiverNoteId { get; set; }
        public double DebtAmount { get; set; }
        public double PaymentAmount { get; set; }
        public string Description { get; set; }    
    }
}
