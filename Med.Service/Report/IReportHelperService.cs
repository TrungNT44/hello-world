using System;
using System.Collections.Generic;
using Med.Entity.Registration;
using Med.ServiceModel.Common;
using Med.ServiceModel.Registration;
using Med.Entity;
using Med.ServiceModel.Report;

namespace Med.Service.Report
{
    public class AffectedItemsResult
    {
        public List<int> ItemIds { get; set; }
        public List<int> DrugIds { get; set; }
    }
    public interface IReportHelperService
    {
        int CreateInitialInventoryReceiptNote(string drugStoreID);
        AffectedItemsResult GetAffectedNoteItemsByDeliveryNotes(string drugStoreID, params int[] noteIds);
        AffectedItemsResult GetAffectedNoteItemsByReceiptNotes(string drugStoreID, params int[] noteIds);
        AffectedItemsResult GetAffectedNoteItemsByDeliveryNoteItems(string drugStoreID, params int[] noteItemIds);
        AffectedItemsResult GetAffectedNoteItemsByReceiptNoteItems(string drugStoreID, params int[] noteItemIds);
        void MakeAffectedChangesByDeliveryNotes(string drugStoreID, params int[] noteIds);
        void MakeAffectedChangesByReceiptNotes(string drugStoreID, params int[] noteIds);
        void MakeAffectedChangesByUpdatedDrugs(string drugStoreID, params int[] drugIds);        
        bool TryMakeAffectedChangesByDeliveryNotes(string drugStoreID, params int[] noteIds);
        bool TryToMakeAffectedChangesByReceiptNotes(string drugStoreID, params int[] noteIds);
        bool TryToMakeAffectedChangesByUpdatedDrugs(string drugStoreID, params int[] drugIds);
    }
}
