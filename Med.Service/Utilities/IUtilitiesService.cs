using Med.Common;
using Med.Common.Enums;
using Med.ServiceModel.Common;
using Med.ServiceModel.Response;
using Med.ServiceModel.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Med.Service.Utilities
{
    public interface IUtilitiesService
    {
        CanhBaoHetHanResponse CanhBaoHangHetHan(string sMaNhaThuoc, string sKeySettingNumExpireDay, string sKeySettingNumDayNoTrans, string sType, int? sNhomThuocId, string sMaThuoc);
        NegativeRevenueResponse GetNegativeRevenueWarningData(string drugStoreCode, FilterObject filter);
        DrugStoreSetting GetDrugStoreSetting(string drugStoreCode);
        NearExpiredDrugResponse GetNearExpiredDrugWarningData(string drugStoreCode, DrugStoreSetting setting, 
            FilterObject filter, int expiredOption = (int)ExpiredFilterType.OnlyExpired);
        List<RemainQuantityReceiptDrugItem> GetRemainQuantityReceiptDrugItems(string drugStoreCode, DrugStoreSetting setting);
    }
}
