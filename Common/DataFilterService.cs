using System.Collections.Generic;
using System.Linq;
using App.Common.Data;
using App.Constants.Enums;
using Castle.Core.Internal;
using Med.Entity;
using Med.Service.Common;
using Med.ServiceModel.Common;
using Med.Common;
using App.Common.Base;
using Med.DbContext;
using App.Common.DI;
using Med.Service.Base;
using Med.Entity.Report;

namespace Med.Service.Impl.Common
{
    public class DataFilterService : BaseService, IDataFilterService
    {
        #region Fields
        #endregion

        #region Interface Implementation
        public IQueryable<PhieuNhap> GetValidReceiptNotes(string drugStoreCode,  
             FilterObject filter = null, int[] validStatuses = null)
        {
            IBaseRepository<PhieuNhap> repository = null;
            return GetValidReceiptNotes(drugStoreCode, out repository, filter, validStatuses);
        }
        public IQueryable<PhieuNhap> GetValidReceiptNotes(string drugStoreCode, out IBaseRepository<PhieuNhap> repository, 
             FilterObject filter = null, int[] validStatuses = null)
        {
            repository = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhap>>(); 
            var validItems = repository.GetAll().Where(r =>
                r.NhaThuoc_MaNhaThuoc == drugStoreCode && !r.Xoa && r.NgayNhap.HasValue);
            if (filter != null)
            {
                if (filter.FromDate.HasValue)
                {
                    validItems = validItems.Where(i => i.NgayNhap >= filter.FromDate);
                }
                if (filter.ToDate.HasValue)
                {
                    validItems = validItems.Where(i => i.NgayNhap <= filter.ToDate);
                }
            }
            if (validStatuses != null && validStatuses.Any())
            {
                validItems = validItems.Where(i => validStatuses.Contains(i.LoaiXuatNhap_MaLoaiXuatNhap.Value));
            }

            return validItems;
        }
        public IQueryable<PhieuXuat> GetValidDeliveryNotes(string drugStoreCode, 
             FilterObject filter = null, int[] validStatuses = null)
        {
            IBaseRepository<PhieuXuat> repository = null;
            return GetValidDeliveryNotes(drugStoreCode, out repository, filter, validStatuses);
        }

        public IQueryable<PhieuXuat> GetValidDeliveryNotes(string drugStoreCode, out IBaseRepository<PhieuXuat> repository,
             FilterObject filter = null, int[] validStatuses = null)
        {           
            repository = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuat>>();
            var validItems = repository.GetAll().Where(r =>
                r.NhaThuoc_MaNhaThuoc == drugStoreCode && !r.Xoa && r.NgayXuat.HasValue);
            if (filter != null)
            {
                if (filter.FromDate.HasValue)
                {
                    validItems = validItems.Where(i => i.NgayXuat >= filter.FromDate);
                }
                if (filter.ToDate.HasValue)
                {
                    validItems = validItems.Where(i => i.NgayXuat <= filter.ToDate);
                }
                if (filter.HasCustomerGroupIds)
                {
                    var customers = GetValidCustomers(drugStoreCode, filter);
                    validItems = (from i in validItems
                                  join c in customers on i.KhachHang_MaKhachHang equals c.MaKhachHang
                                  select i);
                }
                if (filter.HasCustomerIds)
                {
                    validItems = validItems.Where(i => filter.CustomerIds.Contains(i.KhachHang_MaKhachHang.Value));
                }
                if (filter.HasCustomerNames)
                {
                    var customers = GetValidCustomers(drugStoreCode, filter);
                    validItems = (from i in validItems
                                  join c in customers on i.KhachHang_MaKhachHang equals c.MaKhachHang
                                  select i);                   
                }

                if (filter.HasStaffIds)
                {
                    validItems = validItems.Where(i => filter.StaffIds.Contains(i.CreatedBy_UserId.Value));
                }
                if (filter.HasStaffNames)
                {
                    var staffs = GetValidUsers(drugStoreCode, filter);
                    validItems = (from i in validItems
                                  join s in staffs on i.CreatedBy_UserId equals s.UserId
                                  select i);
                }
                if (filter.HasDoctorIds)
                {
                    validItems = validItems.Where(i => filter.DoctorIds.Contains(i.BacSy_MaBacSy.Value));
                }
                if (filter.HasDoctorNames)
                {
                    var doctors = GetValidDoctors(drugStoreCode, filter);
                    validItems = (from i in validItems
                                  join d in doctors on i.BacSy_MaBacSy equals d.MaBacSy
                                  select i);
                }
                if (filter.HasDeliveryNoteIds)
                {
                    validItems = validItems.Where(i => filter.DeliveryNoteIds.Contains(i.MaPhieuXuat));
                }
            }
           
            if (validStatuses != null && validStatuses.Any())
            {
                validItems = validItems.Where(i => validStatuses.Contains(i.MaLoaiXuatNhap));
            }

            return validItems;
        }
        public IQueryable<Thuoc> GetValidDrugs(string drugStoreCode, FilterObject filter = null, bool onlyActiveDrug = true)
        {
            IBaseRepository<Thuoc> repository = null;
            return GetValidDrugs(drugStoreCode, filter, onlyActiveDrug, out repository);
        }
        public IQueryable<Thuoc> GetValidDrugs(string drugStoreCode, FilterObject filter, bool onlyActiveDrug,
            out IBaseRepository<Thuoc> repository)
        {
            repository = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Thuoc>>();
            IBaseRepository<NhaThuoc> drugStoreRepo = null;
            var drugStores = GetValidDrugStores(out drugStoreRepo);
            var validItems = (from dr in repository.GetAll()
                              join ds in drugStores on dr.NhaThuoc_MaNhaThuoc equals ds.MaNhaThuocCha
                              where ds.MaNhaThuoc == drugStoreCode && (!onlyActiveDrug || (onlyActiveDrug && dr.HoatDong))
                              select dr);
            if (string.IsNullOrEmpty(drugStoreCode))
            {
                validItems = (from dr in validItems
                    join ds in drugStores on dr.NhaThuoc_MaNhaThuoc equals ds.MaNhaThuoc
                    select dr);
            }
            if (filter != null)
            {
                if (filter.HasDrugIds)
                {
                    validItems = validItems.Where(i => filter.DrugIds.Contains(i.ThuocId));
                }
                if (filter.HasDrugNames)
                {
                    filter.DrugNames.ForEach(name =>
                    {
                        validItems = validItems.Where(i => i.TenThuoc.Contains(name));
                    });
                }
                if (filter.HasDrugGroupIds)
                {
                    var drugGroupIds = filter.DrugGroupIds.ToList();
                    var consultDrugs = filter.DrugGroupIds.Contains(MedConstants.ConsultDrugGroup);
                    if (consultDrugs)
                    {
                        drugGroupIds = drugGroupIds.Except(new int[] { MedConstants.ConsultDrugGroup }).ToList();                        
                    }
                    if (drugGroupIds.Any())
                    {
                        if (consultDrugs)
                        {
                            validItems = validItems.Where(i => drugGroupIds.Contains(i.NhomThuoc_MaNhomThuoc) || i.HangTuVan == true);
                        }
                        else
                        {
                            validItems = validItems.Where(i => drugGroupIds.Contains(i.NhomThuoc_MaNhomThuoc));
                        }
                    }
                    else if (consultDrugs)
                    {
                        validItems = validItems.Where(i => i.HangTuVan == true);
                    }
                }
            }

            return validItems;
        }
        public IQueryable<KhachHang> GetValidCustomers(string drugStoreCode, FilterObject filter = null)
        {
            IBaseRepository<KhachHang> repository = null;
            return GetValidCustomers(drugStoreCode, filter, out repository);
        }
        public IQueryable<KhachHang> GetValidCustomers(string drugStoreCode,
            FilterObject filter, out IBaseRepository<KhachHang> repository)
        {
            repository = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, KhachHang>>();
            var validItems = repository.GetAll().Where(r =>
                r.MaNhaThuoc == drugStoreCode);
            if (filter != null)
            {
                if (filter.HasCustomerGroupIds)
                {
                    validItems = validItems.Where(i => filter.CustomerGroupIds.Contains(i.MaNhomKhachHang));
                }
                if (filter.HasCustomerIds)
                {
                    validItems = validItems.Where(i => filter.CustomerIds.Contains(i.MaKhachHang));
                }
                if (filter.HasCustomerNames)
                {
                    filter.CustomerNames.ForEach(name =>
                    {
                        validItems = validItems.Where(i => i.TenKhachHang.Contains(name));
                    });
                }
            }

            return validItems;
        }
        public IQueryable<NhaCungCap> GetValidSupplyers(string drugStoreCode, FilterObject filter = null)
        {
            IBaseRepository<NhaCungCap> repository = null;
            return GetValidSupplyers(drugStoreCode, filter, out repository);
        }
        public IQueryable<NhaCungCap> GetValidSupplyers(string drugStoreCode,
            FilterObject filter, out IBaseRepository<NhaCungCap> repository)
        {
            repository = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhaCungCap>>();
            var validItems = repository.GetAll().Where(r =>
                r.MaNhaThuoc == drugStoreCode);
            if (filter != null)
            {
                if (filter.HasSupplyerIds)
                {
                    validItems = validItems.Where(i => filter.SupplyerIds.Contains(i.MaNhaCungCap));
                }
                if (filter.HasSupplyerNames)
                {
                    filter.SupplyerNames.ForEach(name =>
                    {
                        validItems = validItems.Where(i => i.TenNhaCungCap.Contains(name));
                    });
                }
            }

            return validItems;
        }
        public IQueryable<DonViTinh> GetValidUnits(string drugStoreCode)
        {
            IBaseRepository<DonViTinh> repository = null;
            return GetValidUnits(drugStoreCode, out repository);
        }

        public IQueryable<DonViTinh> GetValidUnits(string drugStoreCode,
            out IBaseRepository<DonViTinh> repository)
        {
            repository = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, DonViTinh>>();
            IBaseRepository<NhaThuoc> drugStoreRepo = null;
            var drugStores = GetValidDrugStores(out drugStoreRepo);
            var validItems = (from u in repository.GetAll()
                              join ds in drugStores on u.MaNhaThuoc equals ds.MaNhaThuocCha
                              where ds.MaNhaThuoc == drugStoreCode
                              select u);

            return validItems;
        }
        public IQueryable<PhieuXuatChiTiet> GetValidDeliveryItems(string drugStoreCode,
             FilterObject filter = null, int[] validStatuses = null)
        {
            var deliveryNotes = GetValidDeliveryNotes(drugStoreCode, filter, validStatuses);
            var drugs = GetValidDrugs(drugStoreCode, filter, false);           
            var deliveryNoteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();

            var validItems = (from di in deliveryNoteItemRepo.GetAll()
                join d in deliveryNotes on di.PhieuXuat_MaPhieuXuat equals d.MaPhieuXuat
                join dr in drugs on di.Thuoc_ThuocId equals dr.ThuocId
                select di);

            return validItems;
        }
        public IQueryable<DeliveryNoteItemInfo> GetValidDeliveryNoteItems(string drugStoreCode,
             FilterObject filter = null, int[] validStatuses = null)
        {
            var deliveryNotes = GetValidDeliveryNotes(drugStoreCode, filter, validStatuses);
            var drugs = GetValidDrugs(drugStoreCode, filter, false);
            var customers = GetValidCustomers(drugStoreCode, filter);
            var staffs = GetValidUsers(drugStoreCode, filter);
            var doctors = GetValidDoctors(drugStoreCode, filter);
            var supplyers = GetValidSupplyers(drugStoreCode, filter);
            var units = GetValidUnits(drugStoreCode);

            var deliveryNoteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();

            var validItems = (from di in deliveryNoteItemRepo.GetAll()
                join d in deliveryNotes on di.PhieuXuat_MaPhieuXuat equals d.MaPhieuXuat
                join dr in drugs on di.Thuoc_ThuocId equals dr.ThuocId
                join u in units on di.DonViTinh_MaDonViTinh equals u.MaDonViTinh
                join s in staffs on d.CreatedBy_UserId equals s.UserId into sdGroup
                from sd in sdGroup.DefaultIfEmpty()
                join c in customers on d.KhachHang_MaKhachHang equals c.MaKhachHang into cdGroup
                from cd in cdGroup.DefaultIfEmpty()
                join doc in doctors on d.BacSy_MaBacSy equals doc.MaBacSy into docDGroup
                from docD in docDGroup.DefaultIfEmpty()
                join sup in supplyers on d.NhaCungCap_MaNhaCungCap equals sup.MaNhaCungCap into supDGroup
                from supD in supDGroup.DefaultIfEmpty()
                select new DeliveryNoteItemInfo()
                {
                    NoteId = d.MaPhieuXuat,
                    NoteItemId = di.MaPhieuXuatCt,
                    DrugId = di.Thuoc_ThuocId.Value,
                    DrugName = dr.TenThuoc,
                    DrugCode = dr.MaThuoc,
                    DrugRetailUnitId = dr.DonViXuatLe_MaDonViTinh,
                    DrugUnitId = dr.DonViThuNguyen_MaDonViTinh,
                    DrugUnitFactors = dr.HeSo,
                    Quantity = (double) di.SoLuong,
                    RetailQuantity = di.RetailQuantity,
                    UnitId = di.DonViTinh_MaDonViTinh,
                    UnitName = u.TenDonViTinh,
                    NoteType = d.MaLoaiXuatNhap,
                    NoteDate = d.NgayXuat,
                    PreNoteDate = di.PreNoteItemDate,
                    CustomerId = cd != null ? cd.MaKhachHang : 0 ,
                    CustomerName = cd != null ? cd.TenKhachHang : string.Empty,
                    StaffId = sd != null ? sd.UserId : 0,
                    StaffName = sd != null ? sd.TenDayDu : string.Empty,
                    DoctorId = docD != null ? docD.MaBacSy : 0,
                    DoctorName = docD != null ? docD.TenBacSy : string.Empty,
                    SupplyerId = supD != null ? supD.MaNhaCungCap : 0,
                    SupplyerName = supD != null ? supD.TenNhaCungCap : string.Empty,
                    Discount = (double)di.ChietKhau,
                    Price = (double)di.GiaXuat,
                    RetailPrice = di.RetailPrice,
                    NoteNumber = d.SoPhieuXuat,
                    VAT = d.VAT,
                    ReduceQuantity = di.ReduceQuantity ?? 0,
                    DebtAmount = (double)(d.TongTien - d.DaTra),
                    IsDebt = d.IsDebt ?? false
                });
            if (filter != null)
            {
                if (filter.HasCustomerIds)
                {
                    validItems = validItems.Where(i => filter.CustomerIds.Contains(i.CustomerId.Value));
                }
                if (filter.HasCustomerNames)
                {
                    filter.CustomerNames.ForEach(name =>
                    {
                        validItems = validItems.Where(i => i.CustomerName.Contains(name));
                    });
                }
                if (filter.HasDoctorIds)
                {
                    validItems = validItems.Where(i => filter.DoctorIds.Contains(i.DoctorId.Value));
                }
                if (filter.HasDoctorNames)
                {
                    filter.DoctorNames.ForEach(name =>
                    {
                        validItems = validItems.Where(i => i.DoctorName.Contains(name));
                    });
                }
                if (filter.HasSupplyerIds)
                {
                    validItems = validItems.Where(i => filter.SupplyerIds.Contains(i.SupplyerId.Value));
                }
                if (filter.HasSupplyerNames)
                {
                    filter.SupplyerNames.ForEach(name =>
                    {
                        validItems = validItems.Where(i => i.SupplyerName.Contains(name));
                    });
                }
            }

            return validItems;
        }
        public IQueryable<PhieuNhapChiTiet> GetValidReceiptItems(string drugStoreCode,
             FilterObject filter = null, int[] validStatuses = null)
        {
            var receiptNotes = GetValidReceiptNotes(drugStoreCode, filter, validStatuses);
            var drugs = GetValidDrugs(drugStoreCode, filter);
            var receiptNoteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();

            var validItems = (from ri in receiptNoteItemRepo.GetAll()
                join r in receiptNotes on ri.PhieuNhap_MaPhieuNhap equals r.MaPhieuNhap
                join dr in drugs on ri.Thuoc_ThuocId equals dr.ThuocId select ri);

            return validItems;
        }
        public IQueryable<ReceiptNoteItemInfo> GetValidReceiptNoteItems(string drugStoreCode,
              FilterObject filter = null, int[] noteTypeIds = null)
        {
            var receiptNotes = GetValidReceiptNotes(drugStoreCode, filter, noteTypeIds);
            var drugs = GetValidDrugs(drugStoreCode, filter, false);
            var customers = GetValidCustomers(drugStoreCode, filter);
            var staffs = GetValidUsers(drugStoreCode, filter);         
            var units = GetValidUnits(drugStoreCode);
          
            var receiptNoteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();

            var validItems = (from di in receiptNoteItemRepo.GetAll()
                join d in receiptNotes on di.PhieuNhap_MaPhieuNhap equals d.MaPhieuNhap
                join dr in drugs on di.Thuoc_ThuocId equals dr.ThuocId
                join u in units on di.DonViTinh_MaDonViTinh equals u.MaDonViTinh
                join c in customers on d.KhachHang_MaKhachHang equals c.MaKhachHang into cdGroup
                from cd in cdGroup.DefaultIfEmpty()   
                join s in staffs on d.CreatedBy_UserId equals s.UserId into sdGroup
                from su in sdGroup.DefaultIfEmpty()
                select new ReceiptNoteItemInfo()
                {
                    NoteId = d.MaPhieuNhap,
                    NoteItemId = di.MaPhieuNhapCt,
                    DrugId = di.Thuoc_ThuocId.Value,
                    DrugCode = dr.MaThuoc,
                    DrugName = dr.TenThuoc,
                    DrugRetailUnitId = dr.DonViXuatLe_MaDonViTinh,
                    DrugUnitId = dr.DonViThuNguyen_MaDonViTinh,
                    DrugUnitFactors = dr.HeSo,
                    Quantity = (double) di.SoLuong,
                    RetailQuantity = (double) di.RetailQuantity,
                    UnitId = di.DonViTinh_MaDonViTinh.Value,
                    UnitName = u.TenDonViTinh,
                    NoteType = d.LoaiXuatNhap_MaLoaiXuatNhap.Value,
                    NoteDate = d.NgayNhap,
                    PreNoteDate = di.PreNoteItemDate,
                    CustomerId = cd != null ? cd.MaKhachHang : 0,
                    CustomerName = cd != null ? cd.TenKhachHang : string.Empty,
                    StaffId = su != null ? su.UserId : 0,
                    StaffName = su != null ? su.TenDayDu : string.Empty,
                    Discount = (double)di.ChietKhau,
                    Price = (double)di.GiaNhap,
                    RetailPrice = di.RetailPrice,
                    NoteNumber = d.SoPhieuNhap,
                    VAT = d.VAT,
                    ReduceQuantity = di.ReduceQuantity ?? 0,
                    ExpiredDate = di.HanDung,
                    RemainRefQuantity = di.RemainRefQuantity,
                    SerialNumber = di.SoLo,
                    Barcode = dr.BarCode
                });
            if (filter != null)
            {                
                if (filter.HasCustomerIds)
                {
                    validItems = validItems.Where(i => filter.CustomerIds.Contains(i.CustomerId.Value));
                }
                if (filter.HasCustomerNames)
                {
                    filter.CustomerNames.ForEach(name =>
                    {
                        validItems = validItems.Where(i => i.CustomerName.Contains(name));
                    });
                }
                if (filter.HasStaffIds)
                {
                    validItems = validItems.Where(i => filter.StaffIds.Contains(i.StaffId.Value));
                }
                if (filter.HasStaffNames)
                {
                    filter.StaffNames.ForEach(name =>
                    {
                        validItems = validItems.Where(i => i.StaffName.Contains(name));
                    });
                }
            }

            return validItems;
        }
        public IQueryable<PhieuThuChi> GetValidInOutCommingNotes(string drugStoreCode,
             FilterObject filter = null, int[] validStatuses = null)
        {
            IBaseRepository<PhieuThuChi> repository = null;
            return GetValidInOutCommingNotes(drugStoreCode, out repository, filter, validStatuses);
        }
        public IQueryable<PhieuThuChi> GetValidInOutCommingNotes(string drugStoreCode, out IBaseRepository<PhieuThuChi> repository,
             FilterObject filter = null, int[] validStatuses = null)
        {
            repository = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuThuChi>>();
            var validItems = repository.GetAll().Where(r =>r.NhaThuoc_MaNhaThuoc == drugStoreCode);

            if (filter != null)
            {
                if (filter.FromDate.HasValue)
                {
                    validItems = validItems.Where(i => i.NgayTao >= filter.FromDate);
                }
                if (filter.ToDate.HasValue)
                {
                    validItems = validItems.Where(i => i.NgayTao <= filter.ToDate);
                }
            }

            if (validStatuses != null && validStatuses.Any())
            {
                validItems = validItems.Where(i => validStatuses.Contains(i.LoaiPhieu));
            }

            return validItems;
        }
        public IQueryable<ReceiptDrugPriceRef> GetValidReceiptDrugPriceRefs(string drugStoreCode, FilterObject filter = null)
        {
            var validItems = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, ReceiptDrugPriceRef>>().GetAll();
            validItems = validItems.Where(i => !i.IsDeleted && i.DrugStoreCode == drugStoreCode);

            return validItems;
        }
        public IQueryable<NhaThuoc> GetValidDrugStores(out IBaseRepository<NhaThuoc> repository)
        {
            repository = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhaThuoc>>();
            var validItems = repository.GetAll();
            validItems = validItems.Where(i => i.HoatDong);

            return validItems;
        }
        public IQueryable<UserProfile> GetValidUsers(string drugStoreCode, FilterObject filter = null)
        {
            IBaseRepository<UserProfile> repository = null;
            return GetValidUsers(drugStoreCode, filter, out repository);
        }
        public IQueryable<UserProfile> GetValidUsers(string drugStoreCode,
            FilterObject filter, out IBaseRepository<UserProfile> repository)
        {
            var staffRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhanVienNhaThuoc>>();
            repository = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, UserProfile>>();
            var drugStoreCodes = GetDrugStoreCodes(drugStoreCode);
           
            var validItems = (from s in staffRepo.GetAll()
                          join u in repository.GetAll() on s.User_UserId equals u.UserId into suGroup
                          from su in suGroup.DefaultIfEmpty()
                          where drugStoreCodes.Contains(s.NhaThuoc_MaNhaThuoc)
                          select su).Distinct();
        
            if (filter != null)
            {
                if (filter.HasStaffIds)
                {
                    validItems = validItems.Where(i => filter.StaffIds.Contains(i.UserId));
                }
                if (filter.HasStaffNames)
                {
                    filter.StaffNames.ForEach(name =>
                    {
                        validItems = validItems.Where(i => i.UserName.Contains(name));
                    });
                }
            }

            return validItems;
        }
        public IQueryable<ReduceNoteItem> GetValidReduceNoteItems(string drugStoreCode, FilterObject filter = null)
        {
            IBaseRepository<ReduceNoteItem> repository = null;
            return GetValidReduceNoteItems(drugStoreCode, filter, out repository);
        }
        public IQueryable<ReduceNoteItem> GetValidReduceNoteItems(string drugStoreCode,
            FilterObject filter, out IBaseRepository<ReduceNoteItem> repository)
        {
            repository = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, ReduceNoteItem>>();
            var validItems = repository.GetAll().Where(i => i.RecordStatusId == (byte)RecordStatus.Activated);
            if (!string.IsNullOrEmpty(drugStoreCode))
            {
                validItems = validItems.Where(i => i.DrugStoreCode == drugStoreCode);
            }

            return validItems;
        }
        public IQueryable<SystemMessage> GetValidSystemMessages(string drugStoreCode, FilterObject filter = null)
        {
            IBaseRepository<SystemMessage> repository = null;
            return GetValidSystemMessages(drugStoreCode, filter, out repository);
        }
        public IQueryable<SystemMessage> GetValidSystemMessages(string drugStoreCode,
            FilterObject filter, out IBaseRepository<SystemMessage> repository)
        {
            repository = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, SystemMessage>>();
            var validItems = repository.GetAll().Where(i => i.RecordStatusId == (int)RecordStatus.Activated);
            if (!string.IsNullOrEmpty(drugStoreCode))
            {
                validItems = validItems.Where(i => i.DrugStoreCode == drugStoreCode);
            }

            return validItems;
        }
        public IQueryable<NhomThuoc> GetValidDrugGroups(string drugStoreCode)
        {
            IBaseRepository<NhomThuoc> repository = null;
            return GetValidDrugGroups(drugStoreCode, out repository);
        }
        public IQueryable<NhomThuoc> GetValidDrugGroups(string drugStoreCode, out IBaseRepository<NhomThuoc> repository)
        {
            repository = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhomThuoc>>();
            IBaseRepository<NhaThuoc> drugStoreRepo = null;
            var drugStores = GetValidDrugStores(out drugStoreRepo);
            var validItems = (from dg in repository.GetAll()
                              join ds in drugStores on dg.MaNhaThuoc equals ds.MaNhaThuocCha
                              where ds.MaNhaThuoc == drugStoreCode
                              select dg);
            if (string.IsNullOrEmpty(drugStoreCode))
            {
                validItems = (from dg in validItems
                              join ds in drugStores on dg.MaNhaThuoc equals ds.MaNhaThuoc
                              select dg);
            }            

            return validItems;
        }
        public IQueryable<NhomKhachHang> GetValidCustomerGroups(string drugStoreCode)
        {
            IBaseRepository<NhomKhachHang> repository = null;
            return GetValidCustomerGroups(drugStoreCode, out repository);
        }
        public IQueryable<NhomKhachHang> GetValidCustomerGroups(string drugStoreCode, out IBaseRepository<NhomKhachHang> repository)
        {
            repository = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhomKhachHang>>();
            var validItems = repository.GetAll().Where(r =>
                r.NhaThuoc_MaNhaThuoc == drugStoreCode);

            return validItems;
        }
        public IQueryable<NhomNhaCungCap> GetValidSupplyerGroups(string drugStoreCode)
        {
            IBaseRepository<NhomNhaCungCap> repository = null;
            return GetValidSupplyerGroups(drugStoreCode, out repository);
        }
        public IQueryable<NhomNhaCungCap> GetValidSupplyerGroups(string drugStoreCode, out IBaseRepository<NhomNhaCungCap> repository)
        {
            repository = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhomNhaCungCap>>();
            var validItems = repository.GetAll().Where(r =>
                r.MaNhaThuoc == drugStoreCode);

            return validItems;
        }
        public IQueryable<BacSy> GetValidDoctors(string drugStoreCode, FilterObject filter = null)
        {
            IBaseRepository<BacSy> repository = null;
            return GetValidDoctors(drugStoreCode, filter, out repository);
        }
        public IQueryable<BacSy> GetValidDoctors(string drugStoreCode,
            FilterObject filter, out IBaseRepository<BacSy> repository)
        {
            repository = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, BacSy>>();
            var validItems = repository.GetAll().Where(r =>
                r.MaNhaThuoc == drugStoreCode);
            if (filter != null)
            {
                if (filter.HasDoctorIds)
                {
                    validItems = validItems.Where(i => filter.DoctorIds.Contains(i.MaBacSy));
                }
                if (filter.HasDoctorNames)
                {
                    filter.DoctorNames.ForEach(name =>
                    {
                        validItems = validItems.Where(i => i.TenBacSy.Contains(name));
                    });
                }
            }

            return validItems;
        }
        public IQueryable<Setting> GetValidSetting(string drugStoreCode, string settingKey = "")
        {
            var repository = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Setting>>();
            var validItems = repository.GetAll().Where(r =>
                r.MaNhaThuoc == drugStoreCode);
            if (!string.IsNullOrEmpty(settingKey))
            {
                validItems = validItems.Where(i => i.Key == settingKey);
            }
            
            return validItems;
        }        

        public IQueryable<TinhThanhs> GetListProvinces()
        {
            var repository = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, TinhThanhs>>();
            var validItems = repository.GetAll();

            return validItems;

        }
        public IQueryable<DeliveryNoteItemSnapshotInfo> GetValidDeliveryItemSnapshots(string drugStoreCode,
            FilterObject filter = null)
        {
            var repository = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, DeliveryNoteItemSnapshotInfo>>();
            var validItems = repository.GetAll().Where(i =>
                i.DrugStoreCode == drugStoreCode && !i.IsDeleted);
            if (filter != null)
            {
                if (filter.FromDate.HasValue)
                {
                    validItems = validItems.Where(i => i.CreatedDateTime >= filter.FromDate);
                }
                if (filter.ToDate.HasValue)
                {
                    validItems = validItems.Where(i => i.CreatedDateTime <= filter.ToDate);
                }
            }            

            return validItems;
        }

        public List<string> GetDrugStoreCodes(string drugStoreCode)
        {
            var results = new List<string>();
            results.Add(drugStoreCode);
            try
            {
                var drugStoreSession = (DrugStoreSession)AppBase.Instance.SessionManager.CommonSessionData;
                if (drugStoreSession != null && drugStoreSession.IsChildDrugStore && drugStoreCode.Equals(drugStoreSession.DrugStoreCode))
                {
                    results.Add(drugStoreSession.ParentDrugStoreCode);
                }
            }
            catch
            {

            }

            return results;
        }


        public List<StaffInfo> GetStaffsHasDeliveryNotes(string drugStoreCode, FilterObject filter = null)
        {

            var staffs = GetValidUsers(drugStoreCode, filter);
            var deliveryNotes = GetValidDeliveryNotes(drugStoreCode, filter);
            var staffIds = deliveryNotes.Select(i => i.CreatedBy_UserId).Distinct().ToList();
            var results = staffs.Where(i => staffIds.Contains(i.UserId)).Select(i => new StaffInfo()
            {
                StaffId = i.UserId,
                StaffName = i.TenDayDu
            }).ToList();

            return results;
        }

        public IQueryable<Med.Entity.Admin.Role> GetValidRoles()
        {
            var repository = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Med.Entity.Admin.Role>>();
            var validItems = repository.GetAll().Where(i => !i.IsDeleted.HasValue || !i.IsDeleted.Value);

            return validItems;
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
