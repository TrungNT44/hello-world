namespace Med.Service.Background
{
    using Med.Entity.Common;
    using global::System.Collections.Generic;
    using Med.Entity.Log;
    using Med.ServiceModel.Log;
    using Med.Common;

    public interface IBackgroundService
    {
        void UpdateDrugExpiredDate4InitInventory(string drugStoreCode, int drugId);
        void UpdateLastInventoryQuantity4CacheDrugs(string drugStoreCode, int[] drugIds);
        void UpdateExtraInfo4DeliveryNotes(string drugStoreCode, params int[] noteIds);
        void UpdateExtraInfo4ReceiptNotes(string drugStoreCode, params int[] noteIds);
    }
}
