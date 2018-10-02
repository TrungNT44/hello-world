using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.InOutComming
{
    public class ReceiverDebtNote
    {
        public int NoteId { get; set; }
        public string NoteInfo { get; set; }
        public double DebtAmount { get; set; }
    }
    public class ReceiverDebtInfo
    {
        public double DebtAmount { get; set; }
        public List<ReceiverDebtNote> DebtNotes { get; set; }       
    }
}
