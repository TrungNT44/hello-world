using Med.Common;
using Med.ServiceModel.Common;
using System;
using System.Collections.Generic;

namespace Med.ServiceModel.Utilities
{
    public class NoteNumberIdPair
    {
        public long NoteNumber { get; set; }
        public int NoteId { get; set; }
    }

    public class DeliveryNoteNumberPair
    {
        public NoteNumberIdPair DeliveryNumber { get; set; }
        public List<NoteNumberIdPair> RefReceiptNumbers { get; set; }
    }
    public class NegativeRevenueItem: BaseItem
    {
        /// <summary>
        /// Amount
        /// </summary>
        public double Amount { get; set; }      

        /// <summary>
        /// Delivery Note Numbers
        /// </summary>
        public List<DeliveryNoteNumberPair> DeliveryNoteNumbers { get; set; }
    }
}
