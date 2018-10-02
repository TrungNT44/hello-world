using System.Collections.Generic;
using Med.Entity;
using App.Common.Data;
using Med.ServiceModel.Common;
using Med.Entity.Drug;
using Med.ServiceModel.Drug;
using System;
using Med.ServiceModel.CacheObjects;

namespace Med.Service.Drug
{
    public interface IInventoryService
    {
        List<Inventory> GenerateInventory4Drugs(string drugStoreCode, bool reGen4AllDrugs, bool updatePrices, params int[] drugIds);
        void GetNoteItemQuantities(string drugStoreCode, out List<NoteItemQuantity> receiptItemQuantities,
            out List<NoteItemQuantity> deliveryItemQuantities, FilterObject filter);
        Dictionary<int, DrugInventoryInfo> GetDrugInventoryValues(string drugStoreCode, int[] drugIds,
            DateTime? fromDate = null, DateTime? toDate = null);
        Dictionary<int, double> GetLastInventoryQuantities(string drugStoreCode, bool fromCache, params int[] drugIds);
        void UpdateLastQuantity4CacheDrugs(string drugStoreCode, List<CacheDrug> drugs);
        List<ItemInfoCandidate> GetLastestDeliveryItemsByDrugs(string drugStoreCode, params int[] drugIds);
        List<ItemInfoCandidate> GetLastestReceiptItemsByDrugs(string drugStoreCode, params int[] drugIds);
        List<Inventory> UpdateLastestInventoryPrices(string drugStoreCode, params int[] drugIds);
    }
}
