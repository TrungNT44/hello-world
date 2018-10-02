using System;
using System.Collections.Generic;
using App.Common.Data;
using Med.Entity.Registration;
using Med.ServiceModel.Common;
using Med.ServiceModel.Delivery;
using Med.ServiceModel.Registration;
using Med.Entity;

namespace Med.Service.Delivery
{
    public interface IDeliveryNoteService
    {
        Dictionary<int, DrugDeliveryInfo> GetDrugDeliveryTotalValues(string drugStoreCode, params int[] deliveryItemIds);
        List<DeliveryNoteItemInfo> GetDeliveryNoteItems(string drugStoreCode,
             FilterObject filter = null, int[] validStatuses = null);

        List<int> GetModifiedDeliveryNoteItemIds(string drugStoreCode, out List<MinModifiedDateItem> minModifiedDateItems, 
             List<int> relatedDeliveryItemIds = null);

        List<int> GetAffectedDeliveryItemsByReturnedItemsFromCustomers(string drugStoreCode,
           List<MinModifiedDateItem> minModifiedDateItems);

        int DeleteDeliveryNote(string drugStoreCode, int noteId);

        double GetDeliveryRevenueTotal(string drugStoreCode, FilterObject filter = null);
        long GetNewDeliveryNoteNumber(string drugStoreCode);
        DrugDeliveryItem GetDrugDeliveryItem(string drugStoreCode, int drugId, string barcode);
        int SaveDeliveryNote(string drugStoreCode, int userId, List<DrugDeliveryItem> deliveryItems, double paymentAmount, int noteNumber,
           DateTime? noteDate, int? customerId, int? doctorId, string description);
        bool CanTransitWarehouse(string drugStoreCode, int noteId);       
    }
}
