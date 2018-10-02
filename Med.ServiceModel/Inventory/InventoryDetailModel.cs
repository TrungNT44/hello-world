using System;
using System.Collections.Generic;
using Med.Common;
using Med.Common.Enums;
using Med.ServiceModel.Response;

namespace Med.ServiceModel.Inventory
{
    public class InventoryDetailModel
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string FullName { get; set; }
        public DateTime? CreateTime { get; set; }
        public bool DaCanKho { get; set; }
        public int DrugQuantity { get; set; }
        public List<ThuocModel> MedicineList { get; set; }
        public List<PhieuCanKhoItem> PhieuCanKhoChiTiet { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public string NhaThuoc_MaNhaThuoc { get; set; }

    }

    public class ThuocModel
    {
        public int MaNhomThuoc { get; set; }
        public int ThuocId { get; set; }
        public string TenNhomThuoc { get; set; }
        public string MaThuoc { get; set; }
        public string TenThuoc { get; set; }
        public decimal TonKho { get; set; }
        public decimal? ThucTe { get; set; }
        public string TenDonViTinh { get; set; }
        public int MaPhieuKiemKeTonTai { get; set; }
        public decimal Gia { get; set; }
        public string SoLo { get; set; }
        public DateTime? HanDung { get; set; }
    }

    public class PhieuCanKhoItem
    {
        public int MaPhieu { get; set; }
        public long SoPhieu { get; set; }
        public NoteInOutType LoaiPhieu { get; set; }
        public decimal SoLuong { get; set; }
    }

    public class InventoryEditModel
    {
        public int InventoryId { get; set; }
        public bool DaCanKho { get; set; }
        public string MaThuoc { get; set; }
        public string TenThuoc { get; set; }
        public decimal Gia { get; set; }
        public string SoLo { get; set; }
        public DateTime? HanDung { get; set; }
    }

    public class InventoryResponseModel
    {
        public List<InventoryDetailModel> InventoryDetailModels { get; set; }
        public List<ThuocModel> ThuocModels { get; set; }
    }
}
