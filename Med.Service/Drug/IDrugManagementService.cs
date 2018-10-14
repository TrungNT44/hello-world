using System.Collections.Generic;
using Med.Entity;
using Med.ServiceModel.CacheObjects;
using Med.ServiceModel.Drug;
using System;
using App.Common.Data;
using System.Linq;
using Med.Common;

namespace Med.Service.Drug
{
    public interface IDrugManagementService
    {
        void UpdateDrugPrice(string drugCode, string drugStoreCode, double inPrice, double outPriceL, double outPriceB, int unitCode, DateTime? dateModify);
        void UpdateOutDrugPrice(string drugCode, string drugStoreCode, double price, int unitCode);
        void UpdateExpiredDateDrug(int noteItemId, string batchNumber, DateTime? expiredDate, string drugStoreCode, DrugStoreSetting setting);
        DrugInfo GetDrugInfo(string drugCode, string drugStoreCode, int drugUnit);
        bool MarkAsDeleteForeverDrugs(string drugStoreCode, params int[] drugIds);
        bool DeleteDrugGroups(string drugStoreCode, params int[] drugGroupIds);
        Dictionary<string, List<CacheDrug>> GetAllCacheDrugs();
        List<CacheDrug> GetCacheDrugs(string drugStoreCode, params int[] drugIds);
        List<CreateReserveItem> GetListDrugForCreateReserve(string drugStoreCode, int type, int provider, int group_drug, string name_drug, bool get_drug_empty);
        List<ProviderInfo> GetListProvider(string drugStoreCode);
        List<GroupDrugInfo> GetListGroupDrug(string drugStoreCode);
        Dictionary<int, double> GetLastDrugPriceOnReceiptNotes(string drugStoreCode, params int[] drugIds);
        Dictionary<int, double> GetLastDrugPriceOnDeliveryNotes(string drugStoreCode, params int[] drugIds);
        Dictionary<int, double> GetAvailableDrugQuantities(string drugStoreCode, DateTime? toDate, params int[] drugIds);
        List<DrugPriceModel> GetLastestDrugMinPrices(params int[] drugIds);
    }
}
