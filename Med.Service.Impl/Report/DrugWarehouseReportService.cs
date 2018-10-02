using System;
using System.Dynamic;
using System.Linq;
using App.Common;
using App.Common.Extensions;
using App.Common.Helpers;
using App.Common.Data;
using App.Common.Validation;
using Med.Common.Enums;
using Med.DbContext;
using Med.Entity.Registration;
using App.Common.DI;
using System.Collections.Generic;
using Castle.Core.Internal;
using Med.Repository.Factory;
using Med.Service.Base;
using Med.Service.Delivery;
using Med.Service.Receipt;
using Med.Service.Report;
using Med.ServiceModel.Common;
using Med.ServiceModel.Registration;
using Med.Service.Registration;
using Med.Entity;
using Med.Repository.Registration;
using Med.ServiceModel.Report;
using Med.ServiceModel.Response;

namespace Med.Service.Impl.Report
{
    public class DrugWarehouseReportService : MedBaseService, IDrugWarehouseReportService
    {
        #region Fields
        #endregion

        #region Interface Implementation
        public DrugWarehouseResponse GetDrugWarehouses(string drugStoreCode, FilterObject filter)
        {
            GenerateReportData(drugStoreCode);
            var drugWarehouseItems = new List<DrugWarehouseItem>();
            var result = new DrugWarehouseResponse();
            var rpService = IoC.Container.Resolve<IReportService>();
            var totalCount = 0;
            using (var trans = TransactionScopeHelper.CreateReadUncommitted())
            {
                var drugUnitRep = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, DonViTinh>>();
                var drugQueryable = (from dr in _dataFilterService.GetValidDrugs(drugStoreCode, filter, false)
                                     join u in drugUnitRep.GetAll() on dr.DonViXuatLe_MaDonViTinh equals u.MaDonViTinh
                                     select new
                                     {
                                         DrugId = dr.ThuocId,
                                         DrugCode = dr.MaThuoc,
                                         DrugName = dr.TenThuoc,
                                         DrugRetailUnitName = u.TenDonViTinh,
                                         DrugGroupId = dr.NhomThuoc_MaNhomThuoc
                                     });

                totalCount = drugQueryable.Count();
                var drugCandidates = drugQueryable.OrderBy(i => i.DrugGroupId)
                    .ThenByDescending(i => i.DrugName).ToPagedQueryable(filter.PageIndex, filter.PageSize, totalCount);
                var drugs = drugCandidates.ToDictionary(i => i.DrugId, i => i);
                var drugIds = drugs.Keys.ToArray();
                filter.DrugIds = drugIds;
                var drugWarehouses = rpService.GetDrugWarehouseSyntheises(drugStoreCode, filter);
                var order = filter.PageIndex * filter.PageSize;
                drugWarehouses.ForEach(item =>
                {
                    order++;
                    var cand = item.Value;
                    var drug = drugs[cand.DrugId];
                    drugWarehouseItems.Add(new DrugWarehouseItem()
                    {
                        DrugId = cand.DrugId,
                        Order = order,
                        DrugCode = drug.DrugCode,
                        DrugName = drug.DrugName,
                        DrugRetailUnitName = drug.DrugRetailUnitName,
                        FirstInventoryQuantity = cand.FirstInventoryQuantity,
                        FirstInventoryValue = cand.FirstInventoryValue,
                        ReceiptInventoryQuantityInPeriod = cand.ReceiptInventoryQuantityInPeriod,
                        ReceiptInventoryValueInPeriod = cand.ReceiptInventoryValueInPeriod,
                        DeliveryInventoryQuantityInPeriod = cand.DeliveryInventoryQuantityInPeriod,
                        DeliveryInventoryValueInPeriod = cand.DeliveryInventoryValueInPeriod,
                        LastInventoryQuantity = cand.LastInventoryQuantity,
                        LastInventoryValue = cand.LastInventoryValue
                    });
                });

                result.DeliveryValueTotal = drugWarehouseItems.Sum(i => i.DeliveryInventoryValueInPeriod);
                result.FirsInventoryValueTotal = drugWarehouseItems.Sum(i => i.FirstInventoryValue);
                result.LastInventoryValueTotal = drugWarehouseItems.Sum(i => i.LastInventoryValue);
                result.ReceiptValueTotal = drugWarehouseItems.Sum(i => i.ReceiptInventoryValueInPeriod);

                result.PagingResultModel = new PagingResultModel<DrugWarehouseItem>(drugWarehouseItems, totalCount);
            }

            return result;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
