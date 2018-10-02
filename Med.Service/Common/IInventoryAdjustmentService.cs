using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Med.ServiceModel;
using Med.ServiceModel.Inventory;
//using Med.ServiceModel.PhieuKiemKe;

namespace Med.Service.Common
{
    public interface IInventoryAdjustmentService
    {
        // lấy danh sách toàn bộ Phiếu Kiểm kê
        InventoryResponseModel GetInventoryList(String maNhaThuoc, int thuocId, DateTime? fromDate, DateTime? toDate);

        // lấy thông tin thuốc chưa được Kiểm kê
        InventoryResponseModel GetDrugsHaveNotInventoried(String maNhaThuoc, DateTime? fromDate, DateTime? toDate);

        // lấy thông tin chi tiết Phiếu Kiểm kê 
        InventoryDetailModel GetInventoryDetailInfo(String maNhaThuoc, int? Id);

        // xóa Phiếu Kiểm kê 
        bool DeleteInventory(String maNhaThuoc, int userId, int Id);

        // Lưu Phiếu Kiểm kê 
        int SaveInventory(String maNhaThuoc, int userId, InventoryDetailModel model);

        // lấy thông tin chi tiết của thuốc theo mã nhóm thuốc hoặc theo thuocId
        List<ThuocModel> GetDrugInfo(String maNhaThuoc, int? maNhomThuoc, int?[] drugIds, string ngayTao, string barcode = "");

        // cập nhật giá/lô/hạn dùng của thuốc (cho cả phiếu chưa/đã cân kho)
        void UpdateDrugSerialNoAndExpDate(String maNhaThuoc, InventoryEditModel inventoryEditModel);
    }
}

