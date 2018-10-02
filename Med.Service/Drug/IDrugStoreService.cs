using System.Collections.Generic;
using Med.Entity;
using App.Common.Data;
using Med.ServiceModel.Common;

namespace Med.Service.Drug
{
    public interface IDrugStoreService
    {
        List<string> GetValidDrugStoreCodes();
        List<string> GetOwnerDrugStoreCodes(string drugStoreCode);
        bool DeleteDrugStore(string drugStoreCode);
        bool RollbackDrugStore(string drugStoreCode);
        List<DrugStoreInfo> GetRelatedDrugStores(string drugStoreCode, bool excludeCurrentDrugStore = false);
    }
}
