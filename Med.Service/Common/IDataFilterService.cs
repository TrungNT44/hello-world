using System.Collections.Generic;
using System.Linq;
using App.Common.Data;
using Med.Entity.Registration;
using Med.Entity;
using Med.ServiceModel.Common;
using System;
using Med.Entity.Report;

namespace Med.Service.Common
{
    public interface IDataFilterService
    {
        IQueryable<PhieuNhap> GetValidReceiptNotes(string drugStoreCode, 
            FilterObject filter = null, int[] validStatuses = null);
        IQueryable<PhieuNhap> GetValidReceiptNotes(string drugStoreCode, out IBaseRepository<PhieuNhap> repository, 
             FilterObject filter = null, int[] validStatuses = null);

        IQueryable<PhieuXuat> GetValidDeliveryNotes(string drugStoreCode, 
             FilterObject filter = null, int[] validStatuses = null);
        IQueryable<PhieuXuat> GetValidDeliveryNotes(string drugStoreCode,
            out IBaseRepository<PhieuXuat> repository, FilterObject filter = null, int[] validStatuses = null);

        IQueryable<Thuoc> GetValidDrugs(string drugStoreCode, FilterObject filter = null, bool onlyActiveDrug = true);
        IQueryable<Thuoc> GetValidDrugs(string drugStoreCode, FilterObject filter, bool onlyActiveDrug, out IBaseRepository<Thuoc> repository);

        IQueryable<KhachHang> GetValidCustomers(string drugStoreCode, FilterObject filter = null);
        IQueryable<KhachHang> GetValidCustomers(string drugStoreCode, FilterObject filter,
            out IBaseRepository<KhachHang> repository);

        IQueryable<NhaCungCap> GetValidSupplyers(string drugStoreCode, FilterObject filter = null);
        IQueryable<NhaCungCap> GetValidSupplyers(string drugStoreCode, FilterObject filter,
            out IBaseRepository<NhaCungCap> repository);

        IQueryable<DonViTinh> GetValidUnits(string drugStoreCode);
        IQueryable<DonViTinh> GetValidUnits(string drugStoreCode, 
            out IBaseRepository<DonViTinh> repository);

        IQueryable<PhieuXuatChiTiet> GetValidDeliveryItems(string drugStoreCode, 
             FilterObject filter = null, int[] validStatuses = null);
        IQueryable<PhieuNhapChiTiet> GetValidReceiptItems(string drugStoreCode, 
             FilterObject filter = null, int[] validStatuses = null);
        IQueryable<DeliveryNoteItemInfo> GetValidDeliveryNoteItems(string drugStoreCode,
             FilterObject filter = null, int[] validStatuses = null);
        IQueryable<ReceiptNoteItemInfo> GetValidReceiptNoteItems(string drugStoreCode,
             FilterObject filter = null, int[] noteTypeIds = null);

        IQueryable<PhieuThuChi> GetValidInOutCommingNotes(string drugStoreCode,
            FilterObject filter = null, int[] validStatuses = null);
        IQueryable<PhieuThuChi> GetValidInOutCommingNotes(string drugStoreCode, out IBaseRepository<PhieuThuChi> repository,
             FilterObject filter = null, int[] validStatuses = null);

        IQueryable<ReceiptDrugPriceRef> GetValidReceiptDrugPriceRefs(string drugStoreCode, FilterObject filter = null);

        IQueryable<NhaThuoc> GetValidDrugStores(out IBaseRepository<NhaThuoc> repository);

        IQueryable<UserProfile> GetValidUsers(string drugStoreCode, FilterObject filter = null);
        IQueryable<UserProfile> GetValidUsers(string drugStoreCode, FilterObject filter, out IBaseRepository<UserProfile> repository);

        IQueryable<ReduceNoteItem> GetValidReduceNoteItems(string drugStoreCode,
            FilterObject filter = null);
        IQueryable<ReduceNoteItem> GetValidReduceNoteItems(string drugStoreCode,
            FilterObject filter, out IBaseRepository<ReduceNoteItem> repository);

        IQueryable<SystemMessage> GetValidSystemMessages(string drugStoreCode,
            FilterObject filter = null);
        IQueryable<SystemMessage> GetValidSystemMessages(string drugStoreCode,
            FilterObject filter, out IBaseRepository<SystemMessage> repository);

        IQueryable<NhomThuoc> GetValidDrugGroups(string drugStoreCode);
        IQueryable<NhomThuoc> GetValidDrugGroups(string drugStoreCode,
            out IBaseRepository<NhomThuoc> repository);

        IQueryable<NhomKhachHang> GetValidCustomerGroups(string drugStoreCode);
        IQueryable<NhomKhachHang> GetValidCustomerGroups(string drugStoreCode,
            out IBaseRepository<NhomKhachHang> repository);
       
        IQueryable<NhomNhaCungCap> GetValidSupplyerGroups(string drugStoreCode);
        IQueryable<NhomNhaCungCap> GetValidSupplyerGroups(string drugStoreCode,
            out IBaseRepository<NhomNhaCungCap> repository);
        IQueryable<BacSy> GetValidDoctors(string drugStoreCode, FilterObject filter = null);
        IQueryable<BacSy> GetValidDoctors(string drugStoreCode,
            FilterObject filter, out IBaseRepository<BacSy> repository);
        IQueryable<Setting> GetValidSetting(string drugStoreCode, string settingKey = "");
        IQueryable<TinhThanhs> GetListProvinces();
        IQueryable<DeliveryNoteItemSnapshotInfo> GetValidDeliveryItemSnapshots(string drugStoreCode,
            FilterObject filter = null);

        List<StaffInfo> GetStaffsHasDeliveryNotes(string drugStoreCode, FilterObject filter = null);

        IQueryable<Med.Entity.Admin.Role> GetValidRoles();
    }
}
