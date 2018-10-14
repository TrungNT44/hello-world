namespace Med.Service.Background
{
    using Med.Entity.Common;
    using global::System.Collections.Generic;
    using Med.Entity.Log;
    using Med.ServiceModel.Log;
    using Med.Common;

    public interface IBackgroundService
    {
        void MakeAffectedChangesRelatedDeliveryNotes(string drugStoreID, params int[] noteIds);
        void MakeAffectedChangesRelatedReceiptNotes(string drugStoreID, params int[] noteIds);
        void MakeAffectedChangesByUpdatedDrugs(string drugStoreID, params int[] drugIds);
        void DeleteForeverDrugs(string drugStoreID, params int[] drugIds);
    }
}
