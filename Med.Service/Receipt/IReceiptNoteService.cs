using System.Collections.Generic;
using Med.Entity.Registration;
using Med.ServiceModel.Common;
using Med.ServiceModel.Registration;
using Med.Entity;
using System;
using App.Common.Data;

namespace Med.Service.Receipt
{
    public interface IReceiptNoteService
    {
        List<int> GetModifiedReceiptNoteItemIds(string drugStoreCode, List<int> relatedReceiptItemIds = null);
        List<int> GetAffectedReceiptItemsByMinModifiedDeliveryItems(List<MinModifiedDateItem> minModifiedDateDeliveryItems, 
            string drugStoreCode, out List<MinModifiedDateItem> minModifiedDateItems);
        List<ReceiptNoteItemInfo> GetReceiptNoteItems(string drugStoreCode,
             FilterObject filter = null, int[] validStatuses = null);
        List<int> GetAffectedReceiptItemsByReturnedItemsToSuppliers(string drugStoreCode,
            List<MinModifiedDateItem> minModifiedDateItems);
        Dictionary<int, double> GetReceiptRefQuantityByDeliveryItems(string drugStoreCode, 
            params int[] deliveryItemIds);
        int DeleteReceiptNote(string drugStoreCode, int noteId, int? actorId);
        long GetNewReceiptNoteNumber(string drugStoreCode);
    }
}
