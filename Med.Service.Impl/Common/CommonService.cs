using System.Collections.Generic;
using System.Linq;
using App.Common.Data;
using Med.Entity.Registration;
using Med.Entity;
using Med.Repository.Factory;
using Med.Service.Base;
using Med.Service.Common;
using Med.ServiceModel.Drug;
using Med.ServiceModel.Common;
using System;
using Med.ServiceModel.Report;
using Med.DbContext;
using App.Common.DI;
using Med.Common;
using Med.Common.Enums;

namespace Med.Service.Impl.Common
{
    public class CommonService : BaseService, ICommonService
    {
        #region Fields
        #endregion

        #region Interface Implementation
        public double GetDrugRetailFactors(int drugId, int? drugUnitIdOnNote)
        {
            var repository = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Thuoc>>();
            var unit = repository.GetAll().Where(d => d.ThuocId == drugId).Select(d => new
            {
                RetailUnitId = d.DonViXuatLe_MaDonViTinh,
                UnitId = d.DonViThuNguyen_MaDonViTinh,
                Factors = d.HeSo
            }).FirstOrDefault();

            if (unit == null) return 0;

            var factors = 1.0;
            if (drugUnitIdOnNote.HasValue && unit.UnitId.HasValue && drugUnitIdOnNote.Value == unit.UnitId.Value)
            {
                factors = unit.Factors;
            }

            return factors;
        }

        public double GetDrugRetailFactors(DrugInfo drug, int? drugUnitIdOnNote)
        {
            var factors = 1.0;
            if (drugUnitIdOnNote.HasValue && drug.UnitId.HasValue && drugUnitIdOnNote.Value == drug.UnitId.Value)
            {
                factors = drug.Factors;
            }

            return factors;
        }

        public DrugInfo GetDrugInfo(int? drugId)
        {
            var repository = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Thuoc>>();
            var drugInfo = repository.GetAll().Where(d => d.ThuocId == drugId.Value).Select(d => new DrugInfo()
            {
                DrugId = d.ThuocId,
                RetailUnitId = d.DonViXuatLe_MaDonViTinh,
                UnitId = d.DonViThuNguyen_MaDonViTinh,
                Factors = d.HeSo
            }).FirstOrDefault();

            return drugInfo;
        }
        public int CreateUniqueInternalSupplier(string drugStoreCode, string supplierName, int userId)
        {
            var groupRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhomNhaCungCap>>();
            var supplierRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhaCungCap>>();
            var groupId = groupRepo.GetAll().Where(i => i.MaNhaThuoc == drugStoreCode && i.IsDefault == true)
                .Select(i => i.MaNhomNhaCungCap).FirstOrDefault();
            if (groupId <= MedConstants.InvalidIdValue) return MedConstants.InvalidIdValue;

            var internalSupplierId = supplierRepo.GetAll().Where(i => i.MaNhomNhaCungCap == groupId && i.SupplierTypeId == (int)SupplierType.Internal)
                .Select(i => i.MaNhaCungCap).FirstOrDefault();
            if (internalSupplierId <= MedConstants.InvalidIdValue)
            {
                var supplier = new NhaCungCap()
                {
                    Created = DateTime.Now,
                    CreatedBy_UserId = userId,
                    Active = true,
                    MaNhaThuoc = drugStoreCode,
                    MaNhomNhaCungCap = groupId,
                    SupplierTypeId = (int)SupplierType.Internal,
                    TenNhaCungCap = supplierName
                };
                supplierRepo.Add(supplier);
                supplierRepo.Commit();

                internalSupplierId = supplier.MaNhaCungCap;
            }

            return internalSupplierId;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
