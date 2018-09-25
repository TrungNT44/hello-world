using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using App.Common;
using App.Common.Extensions;
using App.Common.FaultHandling;
using App.Common.Helpers;
using App.Common.Data;
using App.Common.Validation;
using App.Constants.Enums;
using Med.Common;
using Med.Common.Enums;
using Med.Common.Enums.Receipt;
using Med.DbContext;
using App.Common.DI;
using System.Collections.Generic;
using Med.Repository.Factory;
using Med.Service.Base;
using Med.Service.Delivery;
using Med.Service.Report;
using Med.Entity;
using Med.Service.Receipt;
using Med.ServiceModel.Common;
using Med.Entity.Report;
using Med.ServiceModel.Drug;

namespace Med.Service.Impl.Report
{
    public class ReportGenDataService : MedBaseService, IReportGenDataService
    {
        #region Internal Classes
        protected class DeliveryItemCandidate
        {
            public int? DrugId { get; set; }
            public int DeliveryItemId { get; set; }
            public double Quantity { get; set; }
            public DateTime NoteDate { get; set; }
            public int NoteType { get; set; }
            public bool NeedToUpdate { get; set; }
            public double Discount { get; set; }
            public double VAT { get; set; }
            public double RetailPrice { get; set; }            
            public double FinalRetailPrice
            {
                get
                {
                    // Giá N/X = Giá N/X x (1 - CK/100) x (1 + VAT/100)
                    return RetailPrice * (1 - (Discount / 100)) * (1 + (VAT / 100));
                }
            }
            public double FinalRetailAmount
            {
                get { return Quantity * FinalRetailPrice; }
            }
        }

        protected class ReceiptItemCandidate
        {
            public int? DrugId { get; set; }
            public int ReceiptItemId { get; set; }
            public double RemainRefQuantity { get; set; }
            public double RetailPrice { get; set; }
            public DateTime NoteDate { get; set; }
            public bool NeedToUpdate { get; set; }
            public int NoteType { get; set; }
            public double Quantity { get; set; }
            public double Discount { get; set; }
            public double VAT { get; set; }
            public double FinalRetailPrice
            {
                get
                {
                    // Giá N/X = Giá N/X x (1 - CK/100) x (1 + VAT/100)
                    return RetailPrice * (1 - (Discount / 100)) * (1 + (VAT / 100));
                }
            }
        }

        private static readonly int[] ValidReceiptStatuses = new int[] { (int)NoteInOutType.Receipt, (int)NoteInOutType.InventoryAdjustment, (int)NoteInOutType.InitialInventory};
       
        #endregion

        #region Fields

        private List<int> _revertedModifiedDeliveryItemIds = new List<int>();
        private List<int> _revertedModifiedReceiptItemIds = new List<int>();

        private List<int> _revertedDeliveryItemIds = new List<int>();
        private List<int> _revertedReceiptItemIds = new List<int>();
        private Guid _batchGuid = new Guid();
        #endregion

        #region Interface Implementation

        public List<string> GetValidDrugStoreCodes()
        {
            var drugstoreRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhaThuoc>>();
            var drugstores = drugstoreRepo.GetAll().Where(s => s.HoatDong).Select(s => s.MaNhaThuoc).ToList();

            return drugstores;
        }

        private bool RevertAffectedNotesByModifiedNotes(string drugStoreCode)
        {
            var refItems = _dataFilterService.GetValidReceiptDrugPriceRefs(drugStoreCode);
            if (!refItems.Any()) return false;

            var deliveryItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
            var receiptItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            var deliveryRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuat>>();
            var receiptRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhap>>();
            var reduceItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, ReduceNoteItem>>();

            var deliveryNoteItems = (from ii in deliveryItemRepo.GetAll()
                join i in deliveryRepo.GetAll() on ii.PhieuXuat_MaPhieuXuat equals i.MaPhieuXuat
                where
                    ii.SoLuong > 0 && ii.NhaThuoc_MaNhaThuoc == drugStoreCode &&
                    (_revertedModifiedDeliveryItemIds.Count > 0 ? !_revertedModifiedDeliveryItemIds.Contains(ii.MaPhieuXuatCt) : true)
                select new
                {
                    NoteItemId = ii.MaPhieuXuatCt,
                    IsModified = ii.IsModified,
                    NoteType = i.MaLoaiXuatNhap
                });
            var receiptNoteItems = (from ii in receiptItemRepo.GetAll()
                join i in receiptRepo.GetAll() on ii.PhieuNhap_MaPhieuNhap equals i.MaPhieuNhap
                where ii.SoLuong > 0 && ii.NhaThuoc_MaNhaThuoc == drugStoreCode &&
                    (_revertedModifiedReceiptItemIds.Count > 0 ? !_revertedModifiedReceiptItemIds.Contains(ii.MaPhieuNhapCt) : true)
                select new
                {
                    NoteItemId = ii.MaPhieuNhapCt,
                    IsModified = ii.IsModified,
                    NoteType = i.LoaiXuatNhap_MaLoaiXuatNhap.Value
                });
            var modifiedDeliveryItemsQueryable = (from i in deliveryNoteItems
                where i.IsModified
                select new
                {
                    NoteItemId = i.NoteItemId,
                    NoteType = i.NoteType
                });
            var modifiedReceiptItemsQueryable = (from i in receiptNoteItems
                where i.IsModified
                select new
                {
                    NoteItemId = i.NoteItemId,
                    NoteType = i.NoteType
                });

            var modifiedDeliveryItems = modifiedDeliveryItemsQueryable.ToList();
            var modifiedReceiptItems = modifiedReceiptItemsQueryable.ToList();
            if (!modifiedDeliveryItems.Any() && !modifiedReceiptItems.Any()) return false;
            
            var modifiedDeliveryItemIds = modifiedDeliveryItems.Where(i => i.NoteType == (int)NoteInOutType.Delivery)
                .Select(i => i.NoteItemId).ToList();
            var modifiedReturnedToSupplyerItemIds = modifiedDeliveryItems.Where(i => i.NoteType == (int)NoteInOutType.ReturnToSupplier
                || i.NoteType == (int)NoteInOutType.InventoryAdjustment)
                .Select(i => i.NoteItemId).ToList();

            var modifiedReceiptItemIds = modifiedReceiptItems.Where(i => i.NoteType == (int)NoteInOutType.Receipt
                || i.NoteType == (int)NoteInOutType.InventoryAdjustment)
               .Select(i => i.NoteItemId).ToList();
            var modifiedReturnedFromCustomerItemIds = modifiedReceiptItems.Where(i => i.NoteType == (int)NoteInOutType.ReturnToSupplier)
                .Select(i => i.NoteItemId).ToList();

            var effectedReturnedItems = reduceItemRepo.GetAll().Where(i =>
                ((modifiedDeliveryItemIds.Contains(i.ReduceNoteItemId) &&
                  i.NoteTypeId == (int) NoteInOutType.ReturnFromCustomer)
                 ||
                 (modifiedReturnedFromCustomerItemIds.Contains(i.NoteItemId) &&
                  i.NoteTypeId == (int) NoteInOutType.ReturnFromCustomer)
                 ||
                 (modifiedReceiptItemIds.Contains(i.ReduceNoteItemId) &&
                  i.NoteTypeId == (int) NoteInOutType.ReturnToSupplier)
                 ||
                 (modifiedReturnedToSupplyerItemIds.Contains(i.NoteItemId) &&
                  i.NoteTypeId == (int) NoteInOutType.ReturnToSupplier))
                && i.RecordStatusId != (int) RecordStatus.Deleted)
                .Select(i => new
                {
                    ReturnedItemId = i.NoteItemId,
                    NoteType = i.NoteTypeId,
                    NoteItemId = i.ReduceNoteItemId,
                    ReduceId = i.ReduceId
                }).ToList();
            var effectedDeliveryItemIds = new List<int>();
            var effectedReceiptItemIds = new List<int>();
            var effectedReturnedFromCustomerItemIds = new List<int>();
            var effectedReturnedToSupplyerItemIds = new List<int>();
            if (effectedReturnedItems.Any())
            {
                effectedDeliveryItemIds = effectedReturnedItems.Where(i => i.NoteType == (int)NoteInOutType.ReturnFromCustomer)
                    .Select(i => i.NoteItemId).Distinct().ToList();
                effectedReceiptItemIds = effectedReturnedItems.Where(i => i.NoteType == (int)NoteInOutType.ReturnToSupplier)
                    .Select(i => i.NoteItemId).Distinct().ToList();
                effectedReturnedFromCustomerItemIds = effectedReturnedItems.Where(i => i.NoteType == (int)NoteInOutType.ReturnFromCustomer)
                   .Select(i => i.ReturnedItemId).Distinct().ToList();
                effectedReturnedToSupplyerItemIds = effectedReturnedItems.Where(i => i.NoteType == (int)NoteInOutType.ReturnToSupplier)
                    .Select(i => i.ReturnedItemId).Distinct().ToList();
            }

            effectedDeliveryItemIds.AddRange(modifiedDeliveryItemIds.ToArray());
            effectedDeliveryItemIds.AddRange(effectedReturnedToSupplyerItemIds.ToArray());
            effectedDeliveryItemIds = effectedDeliveryItemIds.Distinct().ToList();

            effectedReceiptItemIds.AddRange(modifiedReceiptItemIds.ToArray());
            effectedReceiptItemIds.AddRange(effectedReturnedFromCustomerItemIds.ToArray());
            effectedReceiptItemIds = effectedReceiptItemIds.Distinct().ToList();
           
            var effectedReduceItemIds = effectedReturnedItems.Select(i => i.ReduceId).ToList();
            var refModifiedDeliveryItemIds = new List<int>();
            var refModifiedReceiptItemIds = new List<int>();
            if (effectedDeliveryItemIds.Any())
            {
                refModifiedReceiptItemIds = refItems.Where(i => effectedDeliveryItemIds.Contains(i.DeliveryNoteItemId))
                    .Select(i => i.ReceiptNoteItemId).Distinct().ToList();
            }
            if (effectedReceiptItemIds.Any())
            {
                refModifiedDeliveryItemIds = refItems.Where(i => effectedReceiptItemIds.Contains(i.ReceiptNoteItemId))
                    .Select(i => i.DeliveryNoteItemId).Distinct().ToList();
            }

            effectedDeliveryItemIds.AddRange(refModifiedDeliveryItemIds.ToArray());
            effectedDeliveryItemIds = effectedDeliveryItemIds.Distinct().ToList();
            effectedReceiptItemIds.AddRange(refModifiedReceiptItemIds.ToArray());
            effectedReceiptItemIds = effectedReceiptItemIds.Distinct().ToList();

            RevertRefItems(effectedDeliveryItemIds, effectedReceiptItemIds, drugStoreCode);

            reduceItemRepo.UpdateMany(i => effectedReduceItemIds.Contains(i.ReduceId), i => new ReduceNoteItem()
            {
                RecordStatusId = (int)RecordStatus.Deleted
            });
          
            _revertedModifiedDeliveryItemIds = deliveryNoteItems.Where(i => i.IsModified).Select(i => i.NoteItemId).ToList();
            _revertedModifiedReceiptItemIds = receiptNoteItems.Where(i => i.IsModified).Select(i => i.NoteItemId).ToList();

            return true;
        }

        private void RevertNotes(string drugStoreCode)
        {
            var deliveryItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
            var reduceItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, ReduceNoteItem>>();
            var refItems = _dataFilterService.GetValidReceiptDrugPriceRefs(drugStoreCode);
            
            _revertedModifiedDeliveryItemIds.Clear();
            _revertedModifiedReceiptItemIds.Clear();
            if(!RevertAffectedNotesByModifiedNotes(drugStoreCode)) return;

            var deliveryRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuat>>();
            var receiptRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhap>>();
            var receiptItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();

            var deliveryNoteItems = (from ii in deliveryItemRepo.GetAll()
                join i in deliveryRepo.GetAll() on ii.PhieuXuat_MaPhieuXuat equals i.MaPhieuXuat
                where ii.SoLuong > 0 && ii.NhaThuoc_MaNhaThuoc == drugStoreCode && i.NgayXuat.HasValue
                select new
                {
                    DrugId = ii.Thuoc_ThuocId,
                    MinItemDate = ii.PreNoteItemDate < i.NgayXuat ? ii.PreNoteItemDate : i.NgayXuat,
                    PreNoteItemDate = ii.PreNoteItemDate,
                    NoteItemDate = i.NgayXuat,
                    NoteItemId = ii.MaPhieuXuatCt,
                    IsModified = ii.IsModified,
                    NoteType = i.MaLoaiXuatNhap
                });
             var receiptNoteItems = (from ii in receiptItemRepo.GetAll()
                join i in receiptRepo.GetAll() on ii.PhieuNhap_MaPhieuNhap equals i.MaPhieuNhap
                where ii.SoLuong > 0 && ii.NhaThuoc_MaNhaThuoc == drugStoreCode && i.NgayNhap.HasValue
                select new
                {
                    DrugId = ii.Thuoc_ThuocId,
                    MinItemDate = ii.PreNoteItemDate < i.NgayNhap? ii.PreNoteItemDate: i.NgayNhap,
                    PreNoteItemDate = ii.PreNoteItemDate,
                    NoteItemDate = i.NgayNhap,
                    NoteItemId = ii.MaPhieuNhapCt,
                    IsModified = ii.IsModified,
                    NoteType = i.LoaiXuatNhap_MaLoaiXuatNhap.Value
                });

            var refFromDrugs = refItems.Where(i => i.ReferencePriceTypeId == (int) ReferencePriceTypeId.Drug)
                .Select(i => i.DrugId).Distinct().ToList();
            if (refFromDrugs.Any())
            {
                var drugIdsWithRceiptNoteAtFirstTime = (from i in receiptNoteItems
                    where
                        i.NoteType == (int) NoteInOutType.Receipt && refFromDrugs.Contains(i.DrugId.Value)
                    select i.DrugId
                    ).Distinct().ToList();
                deliveryItemRepo.UpdateMany(i => drugIdsWithRceiptNoteAtFirstTime.Contains(i.Thuoc_ThuocId) && i.NhaThuoc_MaNhaThuoc == drugStoreCode,
                   i => new PhieuXuatChiTiet()
                   {
                       IsModified = true,
                       RequestUpdateFromBkgService = true
                   });
            }

            var receiptNoteTypeIds = new int[] { (int)NoteInOutType.InitialInventory, 
                (int)NoteInOutType.InventoryAdjustment, (int)NoteInOutType.Receipt };
            var minModifiedReceiptDrugItems = (from i in receiptNoteItems
                where i.IsModified && receiptNoteTypeIds.Contains(i.NoteType)
                group i.MinItemDate by i.DrugId
                into g
                select new
                {
                    DrugId = g.Key,
                    MinModifiedDate = g.Min()
                });
            var deliveryNoteTypeIds = new int[] { (int)NoteInOutType.Delivery, (int)NoteInOutType.InventoryAdjustment};
            var minModifiedDeliveryDrugItems = (from i in deliveryNoteItems
                where i.IsModified && deliveryNoteTypeIds.Contains(i.NoteType)
                group i.MinItemDate by i.DrugId
                into g
                select new
                {
                    DrugId = g.Key,
                    MinModifiedDate = g.Min()
                });

            var effectedReceiptItemIdsByMinModifiedReceiptItems = (from i in receiptNoteItems
                join minI in minModifiedReceiptDrugItems
                    on i.DrugId equals minI.DrugId
                where !i.IsModified && i.NoteItemDate >= minI.MinModifiedDate && receiptNoteTypeIds.Contains(i.NoteType)
                select i.NoteItemId).ToList();
            var effectedDeliveryItemIdsByMinModifiedReceiptItems = (from i in deliveryNoteItems
                join minI in minModifiedReceiptDrugItems
                    on i.DrugId equals minI.DrugId
                where
                    !i.IsModified && i.NoteItemDate >= minI.MinModifiedDate && deliveryNoteTypeIds.Contains(i.NoteType)
                select i.NoteItemId).ToList();
            deliveryItemRepo.UpdateMany(i => effectedDeliveryItemIdsByMinModifiedReceiptItems.Contains(i.MaPhieuXuatCt),
                i => new PhieuXuatChiTiet()
                {
                    IsModified = true,
                    RequestUpdateFromBkgService = true
                });
            receiptItemRepo.UpdateMany(i => effectedReceiptItemIdsByMinModifiedReceiptItems.Contains(i.MaPhieuNhapCt),
                i => new PhieuNhapChiTiet()
                {
                    IsModified = true,
                    RequestUpdateFromBkgService = true
                });

            var effectedDeliveryItemIdsByMinModifiedDeliveryItems = (from i in deliveryNoteItems
                join minI in minModifiedDeliveryDrugItems
                    on i.DrugId equals minI.DrugId
                where
                    !i.IsModified && i.NoteItemDate >= minI.MinModifiedDate && deliveryNoteTypeIds.Contains(i.NoteType)
                select i.NoteItemId).ToList();
            deliveryItemRepo.UpdateMany(i => effectedDeliveryItemIdsByMinModifiedDeliveryItems.Contains(i.MaPhieuXuatCt),
               i => new PhieuXuatChiTiet()
               {
                   IsModified = true,
                   RequestUpdateFromBkgService = true
               });
            RevertAffectedNotesByModifiedNotes(drugStoreCode);
        }

        private void ResetDataForReGenerate(string drugStoreCode)
        {
            LogHelper.Info("Rest data for regenerate.");
            var deliveryItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
            var reduceItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, ReduceNoteItem>>();           

            var sqlBuilder = new StringBuilder();
            sqlBuilder.AppendFormat(
                "UPDATE PhieuNhapChiTiets SET IsModified = 1 WHERE NhaThuoc_MaNhaThuoc = {0};", drugStoreCode);
            sqlBuilder.AppendFormat(
                "UPDATE PhieuXuatChiTiets SET IsModified = 1 WHERE NhaThuoc_MaNhaThuoc = {0};", drugStoreCode);
            deliveryItemRepo.ExecuteSqlCommand(sqlBuilder.ToString());

            sqlBuilder.Length = 0;
            sqlBuilder.AppendFormat(
                "DELETE ReduceNoteItem WHERE DrugStoreCode = {0};", drugStoreCode);
            sqlBuilder.AppendFormat(
                "DELETE DeliveryNoteItemSnapshotInfo WHERE DrugStoreCode = {0};", drugStoreCode);
            sqlBuilder.AppendFormat(
                    "DELETE ReceiptDrugPriceRef WHERE DrugStoreCode = {0};", drugStoreCode);
            reduceItemRepo.ExecuteSqlCommand(sqlBuilder.ToString());
        }

        public bool GenerateReceiptDrugPriceRefs(string drugStoreCode, bool isReGenData = false)
        {
            if (string.IsNullOrEmpty(drugStoreCode)) return false;
            LogHelper.Debug("---------------------START GENERATE REPORT DATA OF DRUG STORE: {0}-------------------------", drugStoreCode);
            LogHelper.StartExeMethodMeasurement(drugStoreCode);            
            try
            {
                _batchGuid = Guid.NewGuid();
                var deliveryItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();            
                
                IBaseRepository<NhaThuoc> drugStoreRepo = null;
                var drugStores = _dataFilterService.GetValidDrugStores(out drugStoreRepo);
                var isReportDataGenerating = drugStores.Where(i =>
                    i.MaNhaThuoc == drugStoreCode && i.IsReportDataGenerating.HasValue &&
                    i.IsReportDataGenerating.Value).Any();
                if (isReportDataGenerating) return false;

                var refItems = _dataFilterService.GetValidReceiptDrugPriceRefs(drugStoreCode);
                if (isReGenData || !refItems.Any())
                {
                    ResetDataForReGenerate(drugStoreCode);
                }
                else
                {
                    RevertNotes(drugStoreCode);
                }                

                var deliveryNotes = _dataFilterService.GetValidDeliveryNotes(drugStoreCode);
                var receiptNotes = _dataFilterService.GetValidReceiptNotes(drugStoreCode);
                var receiptItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
                var deliveryItems = (from di in deliveryItemRepo.GetAll()
                                     join d in deliveryNotes on di.PhieuXuat_MaPhieuXuat equals d.MaPhieuXuat
                                     where di.NhaThuoc_MaNhaThuoc == drugStoreCode
                                     select new
                                     {
                                         DeliveryItemId = di.MaPhieuXuatCt,
                                         DrugId = di.Thuoc_ThuocId,
                                         RetailQuantity = di.RetailQuantity,
                                         NoteDate = d.NgayXuat.Value,
                                         NotType = d.MaLoaiXuatNhap,
                                         IsModified = di.IsModified,
                                         ReduceQuantity = di.ReduceQuantity ?? 0,
                                         RetailPrice = di.RetailPrice,
                                         Discount = (double)di.ChietKhau,
                                         VAT = (double)d.VAT,
                                         Quantity = di.SoLuong,
                                     });

                var validDeliveryStatuses = new int[]
                {
                        (int) NoteInOutType.Delivery, (int) NoteInOutType.InventoryAdjustment,
                        (int) NoteInOutType.InitialInventory, (int) NoteInOutType.ReturnToSupplier
                };
                var modifiedDeliveryItems = deliveryItems.Where(di => validDeliveryStatuses.Contains(di.NotType) && di.IsModified == true);
                var deliveryItemCandidates = deliveryItems.Where(di => di.NotType == (int)NoteInOutType.Delivery && 
                    di.IsModified == true && di.Quantity > 0)
                    .Select(di => new DeliveryItemCandidate()
                    {
                        DeliveryItemId = di.DeliveryItemId,
                        DrugId = di.DrugId,
                        Quantity = di.RetailQuantity - di.ReduceQuantity,
                        NoteDate = di.NoteDate,
                        RetailPrice = di.RetailPrice,
                        VAT = di.VAT,
                        Discount = di.Discount
                    });

                var validReceiptStatuses = new int[]
                {
                        (int) NoteInOutType.Receipt, (int) NoteInOutType.InventoryAdjustment,
                        (int) NoteInOutType.InitialInventory, (int) NoteInOutType.ReturnFromCustomer
                };
                var receiptItems = (from ri in receiptItemRepo.GetAll()
                                    join r in receiptNotes on ri.PhieuNhap_MaPhieuNhap equals r.MaPhieuNhap
                                    where ri.NhaThuoc_MaNhaThuoc == drugStoreCode
                                    select new
                                    {
                                        ReceiptItemId = ri.MaPhieuNhapCt,
                                        NotType = r.LoaiXuatNhap_MaLoaiXuatNhap.Value,
                                        IsModified = ri.IsModified,
                                        Quantity = ri.SoLuong
                                    });
                var modifiedReceiptItems = receiptItems.Where(i => validReceiptStatuses.Contains(i.NotType)
                    && i.IsModified == true && i.Quantity > 0);

                if (!(modifiedDeliveryItems.Any() || modifiedReceiptItems.Any()))
                {
                    LogHelper.Info("There is no changes to generate.");
                    return false;
                }

                drugStoreRepo.UpdateMany(i => i.MaNhaThuoc == drugStoreCode, i => new NhaThuoc()
                {
                    IsReportDataGenerating = true
                });

                PreProcessDeliveryAndReceiptNotes(drugStoreCode);
                var candiates = deliveryItemCandidates.ToList().GroupBy(d => d.DrugId).ToList();
                LogHelper.Debug("Number of drugs need to be generation: {0}.", candiates.Count);
                foreach (var cand in candiates)
                {
                    GenerateReceiptDrugPriceRefsByDrug(cand.Key, cand.ToList(), drugStoreCode);
                }

                //sqlBuilder.Length = 0;
                //sqlBuilder.AppendFormat(
                //    "UPDATE PhieuNhapChiTiets SET IsModified = 0, IsQuantityUpdated = 1 WHERE NhaThuoc_MaNhaThuoc = {0};", drugStoreCode);
                //sqlBuilder.AppendFormat(
                //    "UPDATE PhieuXuatChiTiets SET IsModified = 0, IsQuantityUpdated = 1 WHERE NhaThuoc_MaNhaThuoc = {0};", drugStoreCode);
                //deliveryItemRepo.ExecuteSqlCommand(sqlBuilder.ToString());
                deliveryItemRepo.UpdateMany(i => i.NhaThuoc_MaNhaThuoc == drugStoreCode && i.IsModified == true,
                    i => new PhieuXuatChiTiet()
                    {
                        IsModified = false
                    });
                receiptItemRepo.UpdateMany(i => i.NhaThuoc_MaNhaThuoc == drugStoreCode && i.IsModified == true,
                    i => new PhieuNhapChiTiet()
                    {
                        IsModified = false
                    });
                drugStoreRepo.UpdateMany(i => i.MaNhaThuoc == drugStoreCode, i => new NhaThuoc()
                {
                    IsReportDataGenerating = false
                });
            }
            catch (Exception ex)
            {
                FaultHandler.Instance.Handle(ex, this, string.Format("GenerateReceiptDrugPriceRefs: {0}", drugStoreCode));
            }
            finally
            {
                var drugStoreRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhaThuoc>>();
                drugStoreRepo.ExecuteSqlCommand(string.Format("UPDATE NhaThuocs SET IsReportDataGenerating = 0 WHERE MaNhaThuoc = {0}", drugStoreCode));
            }
            LogHelper.StopExeMethodMeasurement(drugStoreCode);

            return true;
        }

        #endregion

        #region Private Methods

        private void UpdateRealQuantityOfDeliveryAndReceiptNoteItems(string drugStoreCode)
        {
            UpdateRealQuantity4DeliveryNoteItems(drugStoreCode);
            UpdateRealQuantity4ReceiptNoteItems(drugStoreCode);  
        }
        private void PreProcessDeliveryAndReceiptNotes(string drugStoreCode)
        {
            _revertedDeliveryItemIds.Clear();
            _revertedReceiptItemIds.Clear();

            
            CreateInitialInventoryReceiptNote(drugStoreCode);
            UpdatePreNoteItemDates(drugStoreCode);
            UpdateRealQuantityOfDeliveryAndReceiptNoteItems(drugStoreCode);
            var receiptRefs = _dataFilterService.GetValidReceiptDrugPriceRefs(drugStoreCode);
            if (receiptRefs.Any())
            {
                RevertReceiptDrugPriceRefsRelateModifiedReceipts(drugStoreCode);
                UpdateRealQuantityOfDeliveryAndReceiptNoteItems(drugStoreCode);
            }
           
            ProcessReturnedDrugsFromCustomers(drugStoreCode);
            ProcessReturnedDrugsToSupplyers(drugStoreCode);
            RevertAffectedNotesByModifiedNotes(drugStoreCode);
            //UpdateRealQuantityOfDeliveryAndReceiptNoteItems(drugStoreCode, uow);
        }

        private void CreateInitialInventoryReceiptNote(string drugStoreCode)
        {
            IBaseRepository<NhaThuoc> drugStoreRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhaThuoc>>();
            var isMainDrugStore = drugStoreRepo.GetAll().Any(i => i.MaNhaThuocCha == drugStoreCode);

            IBaseRepository<PhieuNhap> receiptNoteRepo = null;
            var receiptNotes = _dataFilterService.GetValidReceiptNotes(drugStoreCode, out receiptNoteRepo);
            var receiptNoteId = receiptNotes.Where(r => r.LoaiXuatNhap_MaLoaiXuatNhap == (int)NoteInOutType.InitialInventory)
                .Select(i => i.MaPhieuNhap).FirstOrDefault();
            if (receiptNoteId <= 0)
            {
                var receiptNote = new PhieuNhap()
                {
                    NhaThuoc_MaNhaThuoc = drugStoreCode,
                    NgayNhap = MedConstants.InitialInventoryDeliveryNoteDate,
                    LoaiXuatNhap_MaLoaiXuatNhap = (int)NoteInOutType.InitialInventory,
                    Created = MedConstants.InitialInventoryDeliveryNoteDate,
                    Modified = MedConstants.InitialInventoryDeliveryNoteDate,
                    ReceiptNoteStatusId = (int)ReceiptNoteStatus.Activated
                };
                receiptNoteRepo.Add(receiptNote);
                receiptNoteRepo.Commit();
                receiptNoteId = receiptNote.MaPhieuNhap;
            }

            if (receiptNoteId <= 0) return;

            var receiptItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            var existingReceiptDrugIds = receiptItemRepo.GetAll().Where(i => i.PhieuNhap_MaPhieuNhap == receiptNoteId)
                .Select(i => i.Thuoc_ThuocId).Distinct().ToList();

            var espQuantity = (decimal) MedConstants.EspQuantity;
            var drugs = _dataFilterService.GetValidDrugs(drugStoreCode).Where(d => !existingReceiptDrugIds.Contains(d.ThuocId))
                .Select(d => new
                {
                    DrugId = d.ThuocId,
                    InitialQuantity = d.SoDuDauKy,
                    InitialPrice = d.SoDuDauKy < espQuantity ? d.GiaNhap : d.GiaDauKy,
                    UnitId = d.DonViXuatLe_MaDonViTinh,
                    ExpireDate = d.HanDung,
                    CreatedDate = d.Created
                }).ToList();

          
            var receiptItems = drugs.Select(d => new PhieuNhapChiTiet()
            {
                NhaThuoc_MaNhaThuoc = drugStoreCode,
                PhieuNhap_MaPhieuNhap = receiptNoteId,
                Thuoc_ThuocId = d.DrugId,
                GiaNhap = d.InitialPrice,
                SoLuong = isMainDrugStore ? d.InitialQuantity : 0,
                DonViTinh_MaDonViTinh = d.UnitId,
                IsModified = true,
                HandledStatusId = (int) NoteItemHandledStatus.None,
                HanDung =  d.ExpireDate > MedConstants.MinProductionDataDate ? d.ExpireDate : null
            }).ToList();
            receiptItemRepo.InsertMany(receiptItems);

            receiptItemRepo.Commit();
        }

        private void UpdatePreNoteItemDates(string drugStoreCode)
        {
            var deliveryItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
            var receiptItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();

            var deliveryItems = _dataFilterService.GetValidDeliveryNoteItems(drugStoreCode).Where(c => !c.PreNoteDate.HasValue);
            var receiptItems = _dataFilterService.GetValidReceiptNoteItems(drugStoreCode).Where(c => !c.PreNoteDate.HasValue);
            if (deliveryItems.Any())
            {
                var candiates = deliveryItems.Select( c => new 
                    {
                        NoteItemId = c.NoteItemId,
                        PreNoteItemDate = c.NoteDate
                    }).ToList();

                var updateCandidates = candiates.Select(c => new PhieuXuatChiTiet()
                {
                    MaPhieuXuatCt = c.NoteItemId,
                    PreNoteItemDate = c.PreNoteItemDate
                }).ToList();
                deliveryItemRepo.UpdateMany(updateCandidates, c => c.MaPhieuXuatCt, c => c.PreNoteItemDate);
            }

            if (receiptItems.Any())
            {
                var candiates = receiptItems.Select(c => new
                {
                    NoteItemId = c.NoteItemId,
                    PreNoteItemDate = c.NoteDate
                }).ToList();

                var updateCandidates = candiates.Select(c => new PhieuNhapChiTiet()
                {
                    MaPhieuNhapCt = c.NoteItemId,
                    PreNoteItemDate = c.PreNoteItemDate
                }).ToList();
                receiptItemRepo.UpdateMany(updateCandidates, c => c.MaPhieuNhapCt, c => c.PreNoteItemDate);
            }
        }

        private void UpdateRealQuantity4DeliveryNoteItems(string drugStoreCode)
        {
            LogHelper.Info("Update real quantity of delivery note items.");
            IBaseRepository<PhieuXuat> deliveryNoteRepo = null;
            var deliveryNotes = _dataFilterService.GetValidDeliveryNotes(drugStoreCode, out deliveryNoteRepo);
            var deliveryItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
            var drugs = _dataFilterService.GetValidDrugs(drugStoreCode);
            var deliveryItems = (from d in deliveryNotes
                join di in deliveryItemRepo.GetAll() on d.MaPhieuXuat equals di.PhieuXuat_MaPhieuXuat
                join dr in drugs on di.Thuoc_ThuocId equals dr.ThuocId
                where di.IsModified && di.SoLuong > 0 && di.NhaThuoc_MaNhaThuoc == drugStoreCode
                select new ItemInfoCandidate
                {
                    NoteId = d.MaPhieuXuat,
                    NoteItemId = di.MaPhieuXuatCt,
                    Quantity = (double) di.SoLuong,
                    DrugId = dr.ThuocId,
                    ItemUnitId = di.DonViTinh_MaDonViTinh,
                    RetailUnitId = dr.DonViXuatLe_MaDonViTinh,
                    UnitId = dr.DonViThuNguyen_MaDonViTinh,
                    Factors = dr.HeSo,
                    NoteType = d.MaLoaiXuatNhap,
                    NoteDate = d.NgayXuat,
                    Price = (double)di.GiaXuat,
                    VAT = (double)d.VAT,
                    Discount = (double)di.ChietKhau,
                    ReduceQuantity = di.ReduceQuantity??0
                }).ToList();
            if (!deliveryItems.Any()) return;
           
            var updateCands = new List<PhieuXuatChiTiet>();
            foreach (var item in deliveryItems)
            {
                var factors = 1.0;
                if (item.UnitId.HasValue && item.ItemUnitId == item.UnitId.Value && item.Factors > MedConstants.EspQuantity)
                {
                    factors = item.Factors;
                }
                item.RetailQuantity = item.Quantity * factors;
                item.RetailPrice = item.Price / factors;
               
                updateCands.Add(new PhieuXuatChiTiet()
                {
                    MaPhieuXuatCt = item.NoteItemId,
                    RetailQuantity = item.RetailQuantity,
                    RetailPrice = item.RetailPrice,
                    ReduceQuantity = 0,
                    ReduceNoteItemIds = string.Empty,
                    RequestUpdateFromBkgService = true,
                    HandledStatusId = (int)NoteItemHandledStatus.None
                });
            }
           
            deliveryItemRepo.UpdateMany(updateCands, d => d.MaPhieuXuatCt, d => d.RetailQuantity, 
                d => d.RetailPrice, d => d.ReduceQuantity, d => d.ReduceNoteItemIds, d => d.HandledStatusId,
                d => d.RequestUpdateFromBkgService);
        }

        private void UpdateRealQuantity4ReceiptNoteItems(string drugStoreCode)
        {
            LogHelper.Info("Update real quantity of receipt note items.");
            IBaseRepository<PhieuNhap> receiptNoteRepo = null;
            var receiptNotes = _dataFilterService.GetValidReceiptNotes(drugStoreCode, out receiptNoteRepo);
            var receiptItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            var drugs = _dataFilterService.GetValidDrugs(drugStoreCode);
            var receiptItems = (from r in receiptNotes
                join ri in receiptItemRepo.GetAll() on r.MaPhieuNhap equals ri.PhieuNhap_MaPhieuNhap
                join dr in drugs on ri.Thuoc_ThuocId equals dr.ThuocId
                where ri.IsModified && ri.NhaThuoc_MaNhaThuoc == drugStoreCode
                select new ItemInfoCandidate
                {
                    NoteId = r.MaPhieuNhap,
                    NoteItemId = ri.MaPhieuNhapCt,
                    Quantity = (double) ri.SoLuong,
                    DrugId = dr.ThuocId,
                    ItemUnitId = ri.DonViTinh_MaDonViTinh,
                    RetailUnitId = dr.DonViXuatLe_MaDonViTinh,
                    UnitId = dr.DonViThuNguyen_MaDonViTinh,
                    Factors = dr.HeSo,
                    NoteType = r.LoaiXuatNhap_MaLoaiXuatNhap.Value,
                    NoteDate = r.NgayNhap,
                    Price = (double) ri.GiaNhap,
                    VAT = (double) r.VAT,
                    Discount = (double) ri.ChietKhau,
                    ReduceQuantity = ri.ReduceQuantity ?? 0
                }).ToList();
            if (!receiptItems.Any()) return;

            var updateCands = new List<PhieuNhapChiTiet>();
            foreach (var item in receiptItems)
            {
                var factors = 1.0;
                if (item.UnitId.HasValue && item.ItemUnitId == item.UnitId.Value && item.Factors > MedConstants.EspQuantity)
                {
                    factors = item.Factors;
                }
                item.RetailQuantity = item.Quantity * factors;
                item.RetailPrice = item.Price / factors;
               
                updateCands.Add(new PhieuNhapChiTiet()
                {
                    MaPhieuNhapCt = item.NoteItemId,
                    RetailQuantity = item.RetailQuantity,
                    RemainRefQuantity = item.RetailQuantity,
                    RetailPrice = item.RetailPrice,
                    ReduceQuantity = 0,
                    ReduceNoteItemIds = string.Empty,
                    RequestUpdateFromBkgService = true,
                    HandledStatusId = (int)NoteItemHandledStatus.None
                });
            }
            
            receiptItemRepo.UpdateMany(updateCands, d => d.MaPhieuNhapCt, d => d.RetailQuantity, 
                d => d.RemainRefQuantity, d => d.RetailPrice, 
                d => d.ReduceQuantity, d => d.ReduceNoteItemIds, d => d.HandledStatusId,
                d => d.RequestUpdateFromBkgService);
        }

        private List<int> ProcessReturnedDrugsFromCustomers(string drugStoreCode)
        {
            LogHelper.Info("Process returned drugs from customers.");
            var affectedItemIds = new List<int>();
            IBaseRepository<PhieuNhap> receiptNoteRepo = null;
            var receiptNotes = _dataFilterService.GetValidReceiptNotes(drugStoreCode, out receiptNoteRepo);
            var receiptItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            var validStatuses = new int[] { (int)NoteInOutType.ReturnFromCustomer};
            var receiptItems = (from r in receiptNotes
                join ri in receiptItemRepo.GetAll() on r.MaPhieuNhap equals ri.PhieuNhap_MaPhieuNhap
                where ri.HandledStatusId != (int) NoteItemHandledStatus.Handled
                      && validStatuses.Contains(r.LoaiXuatNhap_MaLoaiXuatNhap.Value)
                      && ri.RetailQuantity > 0 && ri.SoLuong > 0
                      && ri.NhaThuoc_MaNhaThuoc == drugStoreCode
                select new ItemInfoCandidate
                {
                    NoteId = r.MaPhieuNhap,
                    NoteItemId = ri.MaPhieuNhapCt,
                    RetailQuantity = ri.RetailQuantity,
                    DrugId = ri.Thuoc_ThuocId,
                    NoteDate = r.NgayNhap,
                    NoteType = r.LoaiXuatNhap_MaLoaiXuatNhap.Value,
                    ItemHandledStatusId = (int)NoteItemHandledStatus.Unhandled
                }).ToList().OrderByDescending(i => i.NoteType).ThenBy(i => i.NoteDate).ToList();

            if (receiptItems.Count < 1)
            {
                LogHelper.Info("There are no returned drugs from customers.");
                return affectedItemIds;
            }

            foreach (var item in receiptItems)
            {
                var affectedIds = ApplyReturnedDrugItemFromCustomer(item, drugStoreCode);
                affectedItemIds.AddRange(affectedIds);
            }

            var validUpdateStatus = new int[] { (int)NoteItemHandledStatus.Handled, (int)NoteItemHandledStatus.Handling };
            var updateItems = receiptItems.Where(r => validUpdateStatus.Contains(r.ItemHandledStatusId))
                .Select(r => new PhieuNhapChiTiet()
                {
                    MaPhieuNhapCt = r.NoteItemId,
                    HandledStatusId = r.ItemHandledStatusId,
                    ReduceQuantity = r.ReduceQuantity,
                    RequestUpdateFromBkgService = true
                }).ToList();

            receiptItemRepo.UpdateMany(updateItems, r => r.MaPhieuNhapCt, r => r.HandledStatusId, r => r.ReduceQuantity, r => r.RequestUpdateFromBkgService);

            return affectedItemIds;
        }

        private List<int> ApplyReturnedDrugItemFromCustomer(ItemInfoCandidate item, string drugStoreCode)
        {
            var affectedItemIds = new List<int>();
            var deliveryNotes = _dataFilterService.GetValidDeliveryNotes(drugStoreCode);
            var deliveryItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();

            var minDateTime = item.NoteDate.Value.AddDays(-MedConstants.LimitDaysAllowToReturnDrugFromCustomer);
            var maxDateTime = item.NoteDate.Value.AbsoluteEnd();
            var deliveryItemsByDrugQable = (from di in deliveryItemRepo.GetAll()
                join d in deliveryNotes on di.PhieuXuat_MaPhieuXuat equals d.MaPhieuXuat
                where
                    di.Thuoc_ThuocId == item.DrugId && di.RetailQuantity > 0 && di.SoLuong > 0 &&
                    di.NhaThuoc_MaNhaThuoc == drugStoreCode &&
                    d.MaLoaiXuatNhap == (int) NoteInOutType.Delivery && d.NgayXuat >= minDateTime &&
                    d.NgayXuat <= maxDateTime
                select new
                {
                    DeliveryItemId = di.MaPhieuXuatCt,
                    RetailQuantity = di.RetailQuantity,
                    NoteDate = d.NgayXuat,
                    ReduceQuantity = di.ReduceQuantity ?? 0,
                    ReduceNoteItemIds = di.ReduceNoteItemIds ?? string.Empty
                });

            var deliveryItemsByDrug = deliveryItemsByDrugQable.Where(i => (i.RetailQuantity - i.ReduceQuantity) > 0)
                .OrderByDescending(i => i.NoteDate).ThenByDescending(i => i.RetailQuantity).ToList();
            var reduceItems = new List<ReduceNoteItem>();
            var updateItems = new List<PhieuXuatChiTiet>();
            var quantity = 0.0;
            while (quantity < item.RetailQuantity && deliveryItemsByDrug.Count > 0)
            {
                var remainQuantity = item.RetailQuantity - quantity;
                var firstItem = deliveryItemsByDrug.First();
                deliveryItemsByDrug.Remove(firstItem);
                var receiptRemainQuantity = firstItem.RetailQuantity - firstItem.ReduceQuantity;
                var usedQuantity = Math.Min(receiptRemainQuantity, remainQuantity);
                var reduceQuantity = firstItem.ReduceQuantity + usedQuantity;
                var reduceItemIds = string.Format("{0}{1}:{2}", string.IsNullOrEmpty(firstItem.ReduceNoteItemIds) ? string.Empty : firstItem.ReduceNoteItemIds + "; ",
                     item.NoteItemId, usedQuantity);
                var itemRemainedQuantity = receiptRemainQuantity - usedQuantity;
               
                updateItems.Add(new PhieuXuatChiTiet()
                {
                    MaPhieuXuatCt = firstItem.DeliveryItemId,
                    ReduceNoteItemIds = reduceItemIds,
                    ReduceQuantity = reduceQuantity,
                    IsModified = true,
                    RequestUpdateFromBkgService = true
                });
                reduceItems.Add(new ReduceNoteItem()
                {
                    ReduceNoteItemId = firstItem.DeliveryItemId,
                    ReduceQuantity = usedQuantity,
                    RecordStatusId = (int)RecordStatus.Activated,
                    NoteTypeId = (int)NoteInOutType.ReturnFromCustomer,
                    NoteItemId = item.NoteItemId,
                    DrugStoreCode = drugStoreCode
                });
                quantity += usedQuantity;
                affectedItemIds.Add(firstItem.DeliveryItemId);
            }
         
            var rQuantity = Math.Abs(item.RetailQuantity - quantity);
            if (rQuantity < MedConstants.EspQuantity)
            {
                item.ItemHandledStatusId = (int)NoteItemHandledStatus.Handled;
            }
            else if (rQuantity > MedConstants.EspQuantity && quantity > MedConstants.EspQuantity)
            {
                item.ItemHandledStatusId = (int)NoteItemHandledStatus.Handling;
            }
            item.ReduceQuantity = quantity;
            if (quantity > item.RetailQuantity)
            {
                item.ReduceQuantity = item.RetailQuantity;
            }
            if (reduceItems.Any())
            {
                var reduceItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, ReduceNoteItem>>();
                reduceItemRepo.InsertMany(reduceItems);
            }
            deliveryItemRepo.UpdateMany(updateItems, d => d.MaPhieuXuatCt, d => d.RequestUpdateFromBkgService,
                d => d.IsModified, d => d.ReduceNoteItemIds, d => d.ReduceQuantity);

            return affectedItemIds;
        }

        private List<int> ProcessReturnedDrugsToSupplyers(string drugStoreCode)
        {
            LogHelper.Info("Process returned drugs to supplyers.");
            var affectedItemIds = new List<int>();
            IBaseRepository<PhieuXuat> deliveryNoteRepo = null;
            var delivryNotes = _dataFilterService.GetValidDeliveryNotes(drugStoreCode, out deliveryNoteRepo);
            var deliveryItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
            var validStatuses = new int[] { (int)NoteInOutType.ReturnToSupplier, (int)NoteInOutType.InventoryAdjustment };
            var deliveryItems = (from d in delivryNotes
                join di in deliveryItemRepo.GetAll() on d.MaPhieuXuat equals di.PhieuXuat_MaPhieuXuat
                where di.HandledStatusId != (int) NoteItemHandledStatus.Handled
                      && validStatuses.Contains(d.MaLoaiXuatNhap)
                      && di.RetailQuantity > 0 && di.SoLuong > 0
                      && di.NhaThuoc_MaNhaThuoc == drugStoreCode
                select new ItemInfoCandidate
                {
                    NoteId = d.MaPhieuXuat,
                    NoteItemId = di.MaPhieuXuatCt,
                    RetailQuantity = di.RetailQuantity,
                    DrugId = di.Thuoc_ThuocId,
                    NoteDate = d.NgayXuat,
                    NoteType = d.MaLoaiXuatNhap,
                    ItemHandledStatusId = (int)NoteItemHandledStatus.Unhandled
                }).ToList().OrderByDescending(i => i.NoteType).ThenBy(i => i.NoteDate).ToList();
            if (deliveryItems.Count < 1)
            {
                LogHelper.Info("There are no returned drugs to supplyers.");
                return affectedItemIds;
            }

            foreach (var item in deliveryItems)
            {
                var affectedIds =  ApplyReturnedDrugItemToSupplyer(item, drugStoreCode);
                affectedItemIds.AddRange(affectedIds);
            }

            var validUpdateStatus = new int[] { (int)NoteItemHandledStatus.Handled, (int)NoteItemHandledStatus.Handling };
            var updateItems = deliveryItems.Where(r => validUpdateStatus.Contains(r.ItemHandledStatusId))
                .Select(r => new PhieuXuatChiTiet()
                {
                    MaPhieuXuatCt = r.NoteItemId,
                    HandledStatusId = r.ItemHandledStatusId,
                    ReduceQuantity = r.ReduceQuantity,
                    RequestUpdateFromBkgService = true
                }).ToList();

            deliveryItemRepo.UpdateMany(updateItems, r => r.MaPhieuXuatCt, r => r.HandledStatusId,
                r => r.ReduceQuantity, r => r.RequestUpdateFromBkgService);

            return affectedItemIds;
        }

        private List<int> ApplyReturnedDrugItemToSupplyer(ItemInfoCandidate item, string drugStoreCode)
        {
            var affectedItemIds = new List<int>();

            var receiptNotes = _dataFilterService.GetValidReceiptNotes(drugStoreCode);
            var receiptItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();

            var validStatuses = new int[] { (int)NoteInOutType.Receipt, (int)NoteInOutType.InventoryAdjustment, (int)NoteInOutType.InitialInventory };
            var minDateTime = item.NoteDate.Value.AddDays(-MedConstants.LimitDaysAllowToReturnDrugToSupplier);
            var maxDateTime = item.NoteDate.Value.AbsoluteEnd();
            var receiptItemsByDrugQable = (from ri in receiptItemRepo.GetAll()
                join r in receiptNotes on ri.PhieuNhap_MaPhieuNhap equals r.MaPhieuNhap
                where
                    ri.Thuoc_ThuocId == item.DrugId && ri.RetailQuantity > 0 && ri.SoLuong > 0 &&
                    ri.NhaThuoc_MaNhaThuoc == drugStoreCode &&
                    validStatuses.Contains(r.LoaiXuatNhap_MaLoaiXuatNhap.Value) && r.NgayNhap >= minDateTime &&
                    r.NgayNhap <= maxDateTime
                select new
                {
                    ReceiptItemId = ri.MaPhieuNhapCt,
                    RetailQuantity = ri.RetailQuantity,
                    NoteDate = r.NgayNhap,
                    ReduceQuantity = ri.ReduceQuantity ?? 0,
                    ReduceNoteItemIds = ri.ReduceNoteItemIds ?? string.Empty
                });
            var receiptItemsByDrug = receiptItemsByDrugQable.Where(i => (i.RetailQuantity - i.ReduceQuantity) > 0)
                 .OrderByDescending(i => i.NoteDate).ThenByDescending(i => i.RetailQuantity).ToList();
            var reduceItems = new List<ReduceNoteItem>();
            var updateItems = new List<PhieuNhapChiTiet>();
            var quantity = 0.0;
            while (quantity < item.RetailQuantity && receiptItemsByDrug.Count > 0)
            {
                var remainQuantity = item.RetailQuantity - quantity;
                var firstItem = receiptItemsByDrug.First();
                receiptItemsByDrug.Remove(firstItem);
                var receiptRemainQuantity = firstItem.RetailQuantity - firstItem.ReduceQuantity;
                var usedQuantity = Math.Min(receiptRemainQuantity, remainQuantity);
                var reduceQuantity = firstItem.ReduceQuantity + usedQuantity;
                var reduceItemIds = string.Format("{0}{1}:{2}", string.IsNullOrEmpty(firstItem.ReduceNoteItemIds)?string.Empty: firstItem.ReduceNoteItemIds + "; ", 
                     item.NoteItemId, usedQuantity);
                var itemRemainedQuantity = receiptRemainQuantity - usedQuantity;
                updateItems.Add(new PhieuNhapChiTiet()
                {
                    MaPhieuNhapCt = firstItem.ReceiptItemId,
                    RemainRefQuantity = itemRemainedQuantity,
                    ReduceNoteItemIds = reduceItemIds,
                    ReduceQuantity = reduceQuantity,
                    IsModified = true,
                    RequestUpdateFromBkgService = true
                });
                reduceItems.Add(new ReduceNoteItem()
                {
                    ReduceNoteItemId = firstItem.ReceiptItemId,
                    ReduceQuantity = usedQuantity,
                    RecordStatusId = (int)RecordStatus.Activated,
                    NoteTypeId = (int)NoteInOutType.ReturnToSupplier,
                    NoteItemId = item.NoteItemId,
                    DrugStoreCode = drugStoreCode
                });
                quantity += usedQuantity;
                affectedItemIds.Add(firstItem.ReceiptItemId);
            }
      
            var rQuantity = Math.Abs(item.RetailQuantity - quantity);
            if (rQuantity < MedConstants.EspQuantity)
            {
                item.ItemHandledStatusId = (int)NoteItemHandledStatus.Handled;
            }
            else if (rQuantity > MedConstants.EspQuantity && quantity > MedConstants.EspQuantity)
            {
                item.ItemHandledStatusId = (int)NoteItemHandledStatus.Handling;
            }
            item.ReduceQuantity = quantity;
            if (quantity > item.RetailQuantity)
            {
                item.ReduceQuantity = item.RetailQuantity;
            }
            if (reduceItems.Any())
            {
                var reduceItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, ReduceNoteItem>>();
                reduceItemRepo.InsertMany(reduceItems);
            }

            receiptItemRepo.UpdateMany(updateItems, d => d.MaPhieuNhapCt, d => d.RemainRefQuantity,
                d => d.ReduceNoteItemIds, d => d.IsModified, d => d.ReduceQuantity, d => d.RequestUpdateFromBkgService);

            return affectedItemIds;
        }

        private void RevertReceiptDrugPriceRefsRelateModifiedReceipts(string drugStoreCode)
        {
            LogHelper.Info("Revert receipt drug price references relate to modified receipts & modified delivery notes.");

            var deliveryService = IoC.Container.Resolve<IDeliveryNoteService>();
            var receiptService = IoC.Container.Resolve<IReceiptNoteService>();

            // 1. Get all affected receipt note items. 
            var receiptItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            var modifiedReceiptItemIds = receiptService.GetModifiedReceiptNoteItemIds(drugStoreCode);

            // 2. Get all affected price & quantity references for delivery note items.           
            var refItems = _dataFilterService.GetValidReceiptDrugPriceRefs(drugStoreCode);
            var relatedDeliveryItemIds = refItems.Where(r => modifiedReceiptItemIds.Contains(r.ReceiptNoteItemId))
                .Select(r => r.DeliveryNoteItemId).Distinct().ToList();

            // 3. Get all affected receipt note items. 
            var deliveryItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
            List<MinModifiedDateItem> minModifiedDateDeliveryItems = null;
            var modifiedDeliveryItemIds = deliveryService.GetModifiedDeliveryNoteItemIds(drugStoreCode, 
                out minModifiedDateDeliveryItems, relatedDeliveryItemIds);
            

            // 4. Get all affected receipt items by min modified delivery item dates.
            List<MinModifiedDateItem> minModifiedDateReceiptItems = null;
            var affectedReceiptItemIds = receiptService.GetAffectedReceiptItemsByMinModifiedDeliveryItems(
                minModifiedDateDeliveryItems, drugStoreCode, out minModifiedDateReceiptItems);
           
            // 5. Get affected delivery items/receipt items by min receipt items/min delivery items
            //var affectedReceiptItemIdsByMinModifiedDeliveryItems = receiptService.GetAffectedReceiptItemsByReturnedItemsToSuppliers(
            //    drugStoreCode, minModifiedDateDeliveryItems, uow);
            //var affectedDeliveryItemIdsByMinModifiedReceiptItems = deliveryService.GetAffectedDeliveryItemsByReturnedItemsFromCustomers(
            //    drugStoreCode, minModifiedDateReceiptItems, uow);

            // 6. Merge affected reduce items in receipt & delivery notes (returned from customers/supplyers).
            //affectedReceiptItemIds.AddRange(affectedReceiptItemIdsByMinModifiedDeliveryItems);
            //modifiedDeliveryItemIds.AddRange(affectedDeliveryItemIdsByMinModifiedReceiptItems);
          

            // 8. Delete receipt drug price references.
            RevertRefItems(modifiedDeliveryItemIds, affectedReceiptItemIds, drugStoreCode);
        }

        private void RevertRefItems(List<int> deliveryItemIds, List<int> receiptItemIds, 
            string drugStoreCode)
        {
            if ((deliveryItemIds == null || !deliveryItemIds.Any()) && (receiptItemIds == null || !receiptItemIds.Any())) return;
            if (deliveryItemIds == null)
            {
                deliveryItemIds = new List<int>();
            }
            if (receiptItemIds == null)
            {
                receiptItemIds = new List<int>();
            }

            var receiptDrugPriceRefRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, ReceiptDrugPriceRef>>();
            var refItems = _dataFilterService.GetValidReceiptDrugPriceRefs(drugStoreCode);
            var modifiedRefItems = refItems.Where(i =>
                receiptItemIds.Contains(i.ReceiptNoteItemId) ||
                deliveryItemIds.Contains(i.DeliveryNoteItemId));

            var effectedRefItems = modifiedRefItems.Select(i => new {i.ReceiptNoteItemId, i.DeliveryNoteItemId, i.Id}).ToList();
            if (!effectedRefItems.Any()) return;
            
            var modifiedDeliveryItemIds = effectedRefItems.Select(i => i.DeliveryNoteItemId).Distinct().ToList();
            var modifiedReceiptItemIds = effectedRefItems.Select(i => i.ReceiptNoteItemId).Distinct().ToList();
            var modifiedRefItemIds = effectedRefItems.Select(i => i.Id).Distinct().ToList();
            var deliveryItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
            var receiptItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            receiptDrugPriceRefRepo.UpdateMany(i => modifiedRefItemIds.Contains(i.Id), i =>
                new ReceiptDrugPriceRef()
                {
                    IsDeleted = true
                });
            deliveryItemRepo.UpdateMany(i => modifiedDeliveryItemIds.Contains(i.MaPhieuXuatCt), i =>
               new PhieuXuatChiTiet()
               {
                   IsModified = true,
                   RequestUpdateFromBkgService = true
               });
            receiptItemRepo.UpdateMany(i => modifiedReceiptItemIds.Contains(i.MaPhieuNhapCt), i =>
              new PhieuNhapChiTiet()
              {
                  IsModified = true,
                  RequestUpdateFromBkgService = true
              });
        }

        private void GenerateReceiptDrugPriceRefsByDrug(int? drugId, List<DeliveryItemCandidate> candidates,
             string drugStoreCode)
        {
            LogHelper.Debug("Generate price & quantity references of drug: {0}", drugId.Value);
            var drugInfo = _commonService.GetDrugInfo(drugId);

            candidates = candidates.OrderBy(c => c.NoteDate).ToList(); 
            var receiptNotes = _dataFilterService.GetValidReceiptNotes(drugStoreCode);
            var receiptItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            var receiptNoteItems = (from ri in receiptItemRepo.GetAll()
                join r in receiptNotes on ri.PhieuNhap_MaPhieuNhap equals r.MaPhieuNhap
                where ValidReceiptStatuses.Contains(r.LoaiXuatNhap_MaLoaiXuatNhap.Value) &&
                      ri.Thuoc_ThuocId == drugId && 
                      ri.NhaThuoc_MaNhaThuoc == drugStoreCode
                orderby r.NgayNhap descending
                select new ReceiptItemCandidate
                {
                    DrugId = ri.Thuoc_ThuocId,
                    ReceiptItemId = ri.MaPhieuNhapCt,
                    RemainRefQuantity = ri.RemainRefQuantity,
                    RetailPrice = ri.RetailPrice,
                    NoteDate = r.NgayNhap.Value,
                    Quantity = (double)ri.SoLuong,
                    NoteType = r.LoaiXuatNhap_MaLoaiXuatNhap.Value,
                    Discount = (double)ri.ChietKhau,
                    VAT = (double)r.VAT
                });

            var receiptCandidates = receiptNoteItems.Where(ri => ri.RemainRefQuantity > MedConstants.EspQuantity && ri.Quantity > 0)
                .OrderBy(r => r.NoteDate).ToList();

            var bakReceiptCandidates = new List<ReceiptItemCandidate>();
            bakReceiptCandidates.AddRange(receiptCandidates.ToArray());
 
            var receiptPriceRefs = new List<ReceiptDrugPriceRef>();
            var newestReceiptItem = receiptNoteItems.FirstOrDefault();
            if (newestReceiptItem == null)
            {
                newestReceiptItem = receiptNoteItems.Where(i => i.NoteType == (int) NoteInOutType.InitialInventory).FirstOrDefault();
            }

            var generatedDeliveryItemIds = new List<int>();
            var zeroQuantityCands = candidates.Where(i => i.Quantity < MedConstants.EspQuantity).ToList();
            var zeroQuantityCandIds = zeroQuantityCands.Select(i => i.DeliveryItemId).ToList();
            if (zeroQuantityCandIds.Any())
            {
                zeroQuantityCands.ForEach(i =>
                {
                    i.NeedToUpdate = true;
                });
                LogHelper.Debug("GenerateReceiptDrugPriceRefsByDrug {0}- Drug Id: {1}. Zero quantity (by reduce) delivery item ids: {2}",
                drugStoreCode, drugId, string.Join(",", zeroQuantityCandIds));

            }
            var deliveryCands = candidates.Where(i => i.Quantity > MedConstants.EspQuantity).OrderBy(i => i.NoteDate).ToList();
            var deliveryNoteItemSnapshots = new List<DeliveryNoteItemSnapshotInfo>();
            var createdDateTime = DateTime.Now;

            foreach (var cand in deliveryCands)
            {
                var quantity = 0.0;
                var revenue = 0.0;
                while (quantity < cand.Quantity && receiptCandidates.Count > 0)
                {
                    var remainQuantiy = cand.Quantity - quantity;
                    var firstReceiptItem = receiptCandidates[0];
                    var usedQuantity = Math.Min(firstReceiptItem.RemainRefQuantity, remainQuantiy);
                    quantity += usedQuantity;
                    var refItem = new ReceiptDrugPriceRef()
                    {
                        DrugId = cand.DrugId.Value,
                        DeliveryNoteItemId = cand.DeliveryItemId,
                        ReceiptNoteItemId = firstReceiptItem.ReceiptItemId,
                        Quantity = usedQuantity,
                        Price = firstReceiptItem.RetailPrice,
                        ReferencePriceTypeId = (int)ReferencePriceTypeId.Receipt,
                        DrugStoreCode = drugStoreCode,
                        BatchGuid = _batchGuid,
                        CreatedDateTime = createdDateTime
                    };
                    receiptPriceRefs.Add(refItem);
                    revenue += (refItem.Quantity * firstReceiptItem.FinalRetailPrice);
                    generatedDeliveryItemIds.Add(cand.DeliveryItemId);
                    firstReceiptItem.RemainRefQuantity -= usedQuantity;
                    firstReceiptItem.NeedToUpdate = true;
                    if (firstReceiptItem.RemainRefQuantity <= MedConstants.EspQuantity)
                    {
                        receiptCandidates.RemoveAt(0);
                    }
                }               
                if (Math.Abs(quantity - cand.Quantity) <= MedConstants.EspQuantity)
                {
                    
                }
                else if (quantity < cand.Quantity)
                {
                    if (newestReceiptItem != null)
                    {
                        var refItem = new ReceiptDrugPriceRef()
                        {
                            DrugId = cand.DrugId.Value,
                            DeliveryNoteItemId = cand.DeliveryItemId,
                            ReceiptNoteItemId = newestReceiptItem.ReceiptItemId,
                            Quantity = cand.Quantity - quantity,
                            Price = newestReceiptItem.RetailPrice,
                            ReferencePriceTypeId = (newestReceiptItem.NoteType == (int)NoteInOutType.InitialInventory ?
                                (int)ReferencePriceTypeId.Drug : (int)ReferencePriceTypeId.NewestReceiptNote),
                            DrugStoreCode = drugStoreCode,
                            BatchGuid = _batchGuid,
                            CreatedDateTime = DateTime.Now
                        };
                        receiptPriceRefs.Add(refItem);
                        revenue += (refItem.Quantity * newestReceiptItem.FinalRetailPrice);
                        generatedDeliveryItemIds.Add(cand.DeliveryItemId);
                    }
                }
                cand.NeedToUpdate = true;
                var diSnapshotItem = new DeliveryNoteItemSnapshotInfo()
                {
                    BatchGuid = _batchGuid,
                    CreatedDateTime = cand.NoteDate,
                    DeliveryNoteItemId = cand.DeliveryItemId,
                    DrugId = cand.DrugId.Value,
                    DrugStoreCode = drugStoreCode,
                    Revenue = cand.FinalRetailAmount - revenue
                };
                diSnapshotItem.NegativeRevenue = diSnapshotItem.Revenue < 0;
                deliveryNoteItemSnapshots.Add(diSnapshotItem);
            }

            // Update relate entities
            var receiptPriceRefRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, ReceiptDrugPriceRef>>();
            receiptPriceRefRepo.UpdateMany(d => generatedDeliveryItemIds.Contains(d.DeliveryNoteItemId) && d.DrugStoreCode == drugStoreCode, 
                d => new ReceiptDrugPriceRef()
                {
                    IsDeleted = true
                });
            receiptPriceRefRepo.InsertMany(receiptPriceRefs);
            
            var nonGeneratedDeliveryItemIds = deliveryCands.Select(i => i.DeliveryItemId).ToList()
                .Except(generatedDeliveryItemIds).ToList();
            if (nonGeneratedDeliveryItemIds.Any())
            {
                LogHelper.Debug("GenerateReceiptDrugPriceRefsByDrug {0}- Drug Id: {1}. Not generated delivery item ids: {2}",
                drugStoreCode, drugId, string.Join(",", nonGeneratedDeliveryItemIds));
            }
            var updateCandidates = bakReceiptCandidates.Where(c => c.NeedToUpdate)
                .ToDictionary(c => c.ReceiptItemId, c => c);
            var updateReceiptItems = updateCandidates.Select(c => new PhieuNhapChiTiet()
            {
                MaPhieuNhapCt = c.Value.ReceiptItemId,
                RemainRefQuantity = c.Value.RemainRefQuantity,
                RequestUpdateFromBkgService = true
            }).ToList();
            receiptItemRepo.UpdateMany(updateReceiptItems, r => r.MaPhieuNhapCt, r => r.RemainRefQuantity, r => r.RequestUpdateFromBkgService);

            // Update delivery item snapshot infos
            var deliveryItemSnapshotRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, DeliveryNoteItemSnapshotInfo>>();
            deliveryItemSnapshotRepo.UpdateMany(d => generatedDeliveryItemIds.Contains(d.DeliveryNoteItemId) && d.DrugStoreCode == drugStoreCode, 
                d => new DeliveryNoteItemSnapshotInfo()
                {
                    IsDeleted = true
                });

            deliveryItemSnapshotRepo.InsertMany(deliveryNoteItemSnapshots);
        }
        #endregion
    }
}
