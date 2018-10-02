using Med.Common;
using System;

namespace Med.ServiceModel.InOutComming
{
    public class DebtNoteInfo
    {
        public int NoteId { get; set; }
        public long NoteNumber { get; set; }
        public double DebtAmount { get; set; }
        public double PaymentAmount { get; set; }
        public DateTime NoteDate { get; set; }
    }
    public class InOutcommingNoteModel
    {
        public int NoteId { get; set; }
        public int NoteTypeId { get; set; }
        public int TaskMode { get; set; }
        public DateTime NoteDate { get; set; }
        public long NoteNumber { get; set; }
        public string CreatedByName { get; set; }
        public int CreatedById { get; set; }
        public int ReceiverId { get; set; }
        public int ReceiverNoteId { get; set; }
        public double DebtAmount { get; set; }
        public double PaymentAmount { get; set; }
        public double PaymentAmountWithEsp
        {
            get
            {
                var retVal = PaymentAmount + MedConstants.EspAmount;
                if (DebtNote != null)
                {
                    retVal += DebtNote.PaymentAmount;
                }

                return retVal;
            }
        }
        public string Description { get; set; } 
        public int[] ReceiverNoteIds { get; set; }
        public DebtNoteInfo DebtNote { get; set; }
    }
}
