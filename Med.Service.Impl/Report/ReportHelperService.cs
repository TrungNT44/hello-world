using System;
using System.Linq;
using App.Common.Helpers;
using App.Common.Data;
using Med.Common.Enums;
using Med.DbContext;
using App.Common.DI;
using System.Collections.Generic;
using Med.Service.Base;
using Med.Service.Report;
using Med.Entity;
using Med.Entity.Report;
using App.Common.FaultHandling;
using Med.Service.Drug;
using Med.Service.Caching;
using Med.Common;
using Med.Common.Enums.Receipt;
using Med.ServiceModel.CacheObjects;
using Med.Entity.Drug;
using Med.ServiceModel.Common;
using Med.ServiceModel.Drug;
using App.Constants.Enums;

namespace Med.Service.Impl.Report
{
    public class ReportHelperService : MedBaseService, IReportHelperService
    {
        protected class ModiNoteItem
        {
            public int DrugID { get; set; }
            public int NoteID { get; set; }
            public int NoteItemID { get; set; }
            public DateTime NoteDate { get; set; }
            public DateTime PreNoteDate { get; set; }
            public DateTime MinDate { get; set; }
            public bool IsModified { get; set; }
        }
        #region Fields
        #endregion

        #region Interface Implementation
        private IQueryable<ModiNoteItem> GetDeliveryItemsQable(string drugStoreID)
        {
            var noteRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuat>>();
            var noteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
            var noteTypeIds = new int[] { (int)NoteInOutType.Delivery, (int)NoteInOutType.InitialInventory, (int)NoteInOutType.InventoryAdjustment };
            var itemsQable =
              (from i in noteItemRepo.GetAll()
               join n in noteRepo.GetAll() on i.PhieuXuat_MaPhieuXuat equals n.MaPhieuXuat
               where n.NhaThuoc_MaNhaThuoc == drugStoreID && noteTypeIds.Contains(n.MaLoaiXuatNhap)
               select new ModiNoteItem
               {
                   DrugID = i.Thuoc_ThuocId.Value,
                   NoteItemID = i.MaPhieuXuatCt,
                   NoteDate = n.NgayXuat.Value,
                   PreNoteDate = n.PreNoteDate.HasValue ? n.PreNoteDate.Value : n.NgayXuat.Value,
                   IsModified = i.IsModified,
                   NoteID = n.MaPhieuXuat,
                   MinDate = (n.NgayXuat < n.PreNoteDate ? n.PreNoteDate.Value : n.NgayXuat.Value)
               });

            return itemsQable;
        }
        private IQueryable<ModiNoteItem> GetReceiptItemsQable(string drugStoreID)
        {
            var noteRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhap>>();
            var noteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            var noteTypeIds = new int[] { (int)NoteInOutType.Receipt, (int)NoteInOutType.InitialInventory, (int)NoteInOutType.InventoryAdjustment };
            var itemsQable =
              (from i in noteItemRepo.GetAll()
               join n in noteRepo.GetAll() on i.PhieuNhap_MaPhieuNhap equals n.MaPhieuNhap
               where n.NhaThuoc_MaNhaThuoc == drugStoreID && noteTypeIds.Contains(n.LoaiXuatNhap_MaLoaiXuatNhap.Value)
               select new ModiNoteItem
               {
                   DrugID = i.Thuoc_ThuocId.Value,
                   NoteItemID = i.MaPhieuNhapCt,
                   NoteDate = n.NgayNhap.Value,
                   PreNoteDate = n.PreNoteDate.HasValue ? n.PreNoteDate.Value : n.NgayNhap.Value,
                   IsModified = i.IsModified,
                   NoteID = n.MaPhieuNhap,
                   MinDate = (n.NgayNhap < n.PreNoteDate ? n.PreNoteDate.Value : n.NgayNhap.Value)
               });

            return itemsQable;
        }
        public AffectedItemsResult GetAffectedNoteItemsByDeliveryNotes(string drugStoreID, params int[] noteIds)
        {            
            if (noteIds == null || !noteIds.Any() || string.IsNullOrWhiteSpace(drugStoreID)) return null;

            var itemsQable = GetDeliveryItemsQable(drugStoreID);
            var minModiItemsQable = itemsQable.Where(i => noteIds.Contains(i.NoteID) && i.IsModified);
            var modiDrugIds = minModiItemsQable.Select(i => i.DrugID).Distinct().ToList();

            if (!modiDrugIds.Any()) return null;

            var modiItemIds = (from i in itemsQable
                                join m in minModiItemsQable on i.DrugID equals m.DrugID
                                where i.NoteDate >= m.MinDate || i.PreNoteDate >= m.MinDate
                                select i.NoteItemID).ToList();

            return new AffectedItemsResult() { DrugIds = modiDrugIds, ItemIds = modiItemIds };
        }
        public AffectedItemsResult GetAffectedNoteItemsByReceiptNotes(string drugStoreID, params int[] noteIds)
        {            
            if (noteIds == null || !noteIds.Any() || string.IsNullOrWhiteSpace(drugStoreID)) return null;
            
            var itemsQable = GetReceiptItemsQable(drugStoreID);
            var minModiItemsQable = itemsQable.Where(i => noteIds.Contains(i.NoteID) && i.IsModified);
            var modiDrugIds = minModiItemsQable.Select(i => i.DrugID).Distinct().ToList();

            if (!modiDrugIds.Any()) return null;

            var modiItemIds = (from i in itemsQable
                                join m in minModiItemsQable on i.DrugID equals m.DrugID
                                where i.NoteDate >= m.MinDate || i.PreNoteDate >= m.MinDate
                                select i.NoteItemID).ToList();
            return new AffectedItemsResult() { DrugIds = modiDrugIds, ItemIds = modiItemIds };
        }
        public AffectedItemsResult GetAffectedNoteItemsByDeliveryNoteItems(string drugStoreID, params int[] noteItemIds)
        {
            if (noteItemIds == null || !noteItemIds.Any() || string.IsNullOrWhiteSpace(drugStoreID)) return null;
       
            var itemsQable = GetDeliveryItemsQable(drugStoreID);
            var minModiItemsQable = itemsQable.Where(i => noteItemIds.Contains(i.NoteItemID));
            var modiDrugIds = minModiItemsQable.Select(i => i.DrugID).Distinct().ToList();

            if (!modiDrugIds.Any()) return null;

            var modiItemIds = (from i in itemsQable
                               join m in minModiItemsQable on i.DrugID equals m.DrugID
                               where i.NoteDate >= m.MinDate || i.PreNoteDate >= m.MinDate
                               select i.NoteItemID).ToList();

            return new AffectedItemsResult() { DrugIds = modiDrugIds, ItemIds = modiItemIds };
        }
        public AffectedItemsResult GetAffectedNoteItemsByReceiptNoteItems(string drugStoreID, params int[] noteItemIds)
        {
            if (noteItemIds == null || !noteItemIds.Any() || string.IsNullOrWhiteSpace(drugStoreID)) return null;
            
            var itemsQable = GetReceiptItemsQable(drugStoreID);
            var minModiItemsQable = itemsQable.Where(i => noteItemIds.Contains(i.NoteItemID) && i.IsModified);
            var modiDrugIds = minModiItemsQable.Select(i => i.DrugID).Distinct().ToList();

            if (!modiDrugIds.Any()) return null;

            var modiItemIds = (from i in itemsQable
                               join m in minModiItemsQable on i.DrugID equals m.DrugID
                               where i.NoteDate >= m.MinDate || i.PreNoteDate >= m.MinDate
                               select i.NoteItemID).ToList();
            return new AffectedItemsResult() { DrugIds = modiDrugIds, ItemIds = modiItemIds };
        }
        private void UpdateAffectedItems(string drugStoreID, List<int> deliveryItemIds, List<int> receiptItemIds, List<int> drugIds)
        {
            deliveryItemIds = deliveryItemIds.Distinct().ToList();
            receiptItemIds = receiptItemIds.Distinct().ToList();
            drugIds = drugIds.Distinct().ToList();
            var dNoteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
            var rNoteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            var refPriceRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, ReceiptDrugPriceRef>>();
            using (var trans = TransactionScopeHelper.CreateReadCommittedForWrite())
            {
                if (deliveryItemIds.Any())
                {
                    dNoteItemRepo.UpdateMany(i => deliveryItemIds.Contains(i.MaPhieuXuatCt),
                        i => new PhieuXuatChiTiet() { IsModified = true });
                }
                if (receiptItemIds.Any())
                {
                    rNoteItemRepo.UpdateMany(i => receiptItemIds.Contains(i.MaPhieuNhapCt),
                        i => new PhieuNhapChiTiet() { IsModified = true });
                }
                if ((deliveryItemIds.Any() || receiptItemIds.Any()) && drugIds.Any())
                {
                    refPriceRepo.UpdateMany(i => (receiptItemIds.Contains(i.ReceiptNoteItemId) || deliveryItemIds.Contains(i.DeliveryNoteItemId))
                        && i.DrugStoreCode == drugStoreID && drugIds.Contains(i.DrugId) && !i.IsDeleted,
                        i => new ReceiptDrugPriceRef() { IsDeleted = true });
                }
                
                trans.Complete();
            }
        }
        public void MakeAffectedChangesByDeliveryNotes(string drugStoreID, params int[] noteIds)
        {
            var allModiDeliveryItemIds = new List<int>();
            var allModiReceiptItemIds = new List<int>();
            var allModiDrugIds = new List<int>();
          
            var refPriceRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, ReceiptDrugPriceRef>>();
            using (var trans = TransactionScopeHelper.CreateReadUncommitted())
            {
                var modiResult = GetAffectedNoteItemsByDeliveryNotes(drugStoreID, noteIds);
                if (modiResult == null) return;
                
                var modiRItemIDs = refPriceRepo.GetAll().Where(i => i.DrugStoreCode == drugStoreID && !i.IsDeleted
                    && modiResult.ItemIds.Contains(i.DeliveryNoteItemId) && modiResult.DrugIds.Contains(i.DrugId))
                    .Select(i => i.ReceiptNoteItemId).Distinct().ToList();
                var modiReceiptResult = GetAffectedNoteItemsByReceiptNoteItems(drugStoreID, modiRItemIDs.ToArray());

                allModiDeliveryItemIds.AddRange(modiResult.ItemIds);
                allModiDrugIds.AddRange(modiResult.DrugIds);
                if (modiReceiptResult != null)
                {
                    allModiReceiptItemIds.AddRange(modiReceiptResult.ItemIds);
                    allModiDrugIds.AddRange(modiReceiptResult.DrugIds);
                }
                trans.Complete();
            }
            UpdateAffectedItems(drugStoreID, allModiDeliveryItemIds, allModiReceiptItemIds, allModiDrugIds);
        }
        public void MakeAffectedChangesByReceiptNotes(string drugStoreID, params int[] noteIds)
        {
            var allModiDeliveryItemIds = new List<int>();
            var allModiReceiptItemIds = new List<int>();
            var allModiDrugIds = new List<int>();          
            var refPriceRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, ReceiptDrugPriceRef>>();        
           
            using (var trans = TransactionScopeHelper.CreateReadUncommitted())
            {
                var modiResult = GetAffectedNoteItemsByReceiptNotes(drugStoreID, noteIds);
                if (modiResult == null) return;

                var modiDItemIds = refPriceRepo.GetAll().Where(i => i.DrugStoreCode == drugStoreID && !i.IsDeleted
                    && modiResult.ItemIds.Contains(i.ReceiptNoteItemId) && modiResult.DrugIds.Contains(i.DrugId))
                    .Select(i => i.DeliveryNoteItemId).Distinct().ToList();
                var modiDeliveryResult = GetAffectedNoteItemsByDeliveryNoteItems(drugStoreID, modiDItemIds.ToArray());

                allModiReceiptItemIds.AddRange(modiResult.ItemIds);
                allModiDrugIds.AddRange(modiResult.DrugIds);
                if (modiDeliveryResult != null)
                {
                    allModiDeliveryItemIds.AddRange(modiDeliveryResult.ItemIds);
                    allModiDrugIds.AddRange(modiDeliveryResult.DrugIds);
                }
                trans.Complete();
            }

            UpdateAffectedItems(drugStoreID, allModiDeliveryItemIds, allModiReceiptItemIds, allModiDrugIds);
        }

        public bool TryMakeAffectedChangesByDeliveryNotes(string drugStoreID, params int[] noteIds)
        {
            var retVal = true;
            return retVal;
            try
            {
                MakeAffectedChangesByDeliveryNotes(drugStoreID, noteIds);
            }
            catch (Exception ex)
            {
                FaultHandler.Instance.Handle(ex, this, "TryMakeAffectedChangesByDeliveryNotes");
                retVal = false;
            }

            return retVal;
        }
        public bool TryToMakeAffectedChangesByReceiptNotes(string drugStoreID, params int[] noteIds)
        {
            var retVal = true;
            return retVal;
            try
            {
                MakeAffectedChangesByReceiptNotes(drugStoreID, noteIds);
            }
            catch (Exception ex)
            {
                FaultHandler.Instance.Handle(ex, this, "TryToMakeAffectedChangesByReceiptNotes");
                retVal = false;
            }

            return retVal;
        }
        public void MakeAffectedChangesByUpdatedDrugs(string drugStoreID, params int[] drugIds)
        {
            var dsManService = IoC.Container.Resolve<IDrugManagementService>();
            var cacheDrugs = dsManService.GetCacheDrugs(drugStoreID, drugIds);
            if (!cacheDrugs.Any()) return;
            // Update prices for drug inventories
            var invService = IoC.Container.Resolve<IInventoryService>();
            invService.GenerateInventory4Drugs(drugStoreID, false, false, false, drugIds);            

            // Update initial inventory receipt note items
            var initialInvCands = cacheDrugs.Where(i => i.ShouldUpdateInitialInventoryReceiptItems()).ToList();          
            UpdateInitialInventoryReceiptNote(drugStoreID, initialInvCands);

            // Update retail price/quantity for related note items if drug factors changed
            var factorChangedCands = cacheDrugs.Where(i => i.HasFactorsChanged()).ToList();
            UpdateNoteItemsByFactorChanged(drugStoreID, factorChangedCands);

            // Update data relate reports. Mark all report items to Deleted
            var rptChangedCands = cacheDrugs.Where(i => i.HasFactorsChanged() || i.HasInitQuantityChanged()).ToList();
            var rptDrugIds = rptChangedCands.Select(i => i.DrugId).ToArray();
            UpdateChangesToReportData(drugStoreID, rptDrugIds);            

            MedCacheManager.Instance.UpdateCacheDrugs(drugStoreID, cacheDrugs);
        }
        public bool TryToMakeAffectedChangesByUpdatedDrugs(string drugStoreID, params int[] drugIds)
        {
            var retVal = true;
            try
            {
                MakeAffectedChangesByUpdatedDrugs(drugStoreID, drugIds);
            }
            catch (Exception ex)
            {
                FaultHandler.Instance.Handle(ex, this, "TryToMakeAffectedChangesByUpdatedDrugs");
                retVal = false;
            }

            return retVal;
        }
        #endregion

        #region Private Methods        
        private void UpdateChangesToReportData(string drugStoreID, params int[] drugIds)
        {
            if (drugIds == null || !drugIds.Any()) return;
           
            var refPriceRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, ReceiptDrugPriceRef>>();
            var snapshotRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, DeliveryNoteItemSnapshotInfo>>();
            var reduceRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, ReduceNoteItem>>();

            refPriceRepo.UpdateMany(i => i.DrugStoreCode == drugStoreID && drugIds.Contains(i.DrugId)
                && !i.IsDeleted, i => new ReceiptDrugPriceRef() { IsDeleted = true });
            snapshotRepo.UpdateMany(i => i.DrugStoreCode == drugStoreID && drugIds.Contains(i.DrugId)
                && !i.IsDeleted, i => new DeliveryNoteItemSnapshotInfo() { IsDeleted = true });
            reduceRepo.UpdateMany(i => i.DrugStoreCode == drugStoreID && drugIds.Contains(i.DrugID)
                && i.RecordStatusId == (byte)RecordStatus.Activated, 
                i => new ReduceNoteItem() { RecordStatusId = (byte)RecordStatus.Deleted });
        }
        private void UpdateNoteItemsByFactorChanged(string drugStoreID, List<CacheDrug> factorChangedCands)
        {
            if (factorChangedCands == null || !factorChangedCands.Any()) return;
            var drugIds = factorChangedCands.Select(i => i.DrugId).Distinct().ToArray();
            var filter = new FilterObject()
            {
                DrugIds = drugIds
            };
            var dNoteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
            var rNoteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            //var retailUnitChangedCands = factorChangedCands.Where(i => i.PreRetailUnitID != i.RetailUnitId).ToList();
            //var unitChangedCands = factorChangedCands.Where(i => i.PreUnitID != i.UnitId).ToList();
            //retailUnitChangedCands.ForEach(i =>
            //{
            //    dNoteItemRepo.UpdateMany(it => it.NhaThuoc_MaNhaThuoc == drugStoreID
            //        && it.Thuoc_ThuocId == i.DrugId
            //        && it.DonViTinh_MaDonViTinh == i.PreRetailUnitID
            //        && it.RecordStatusID == (int)RecordStatus.Activated,
            //        it => new PhieuXuatChiTiet() { DonViTinh_MaDonViTinh = i.RetailUnitId.Value });
            //    rNoteItemRepo.UpdateMany(it => it.NhaThuoc_MaNhaThuoc == drugStoreID
            //        && it.Thuoc_ThuocId == i.DrugId
            //        && it.DonViTinh_MaDonViTinh == i.PreRetailUnitID
            //        && it.RecordStatusID == (int)RecordStatus.Activated,
            //        it => new PhieuNhapChiTiet() { DonViTinh_MaDonViTinh = i.RetailUnitId.Value });
            //});
            //unitChangedCands.ForEach(i =>
            //{
            //    dNoteItemRepo.UpdateMany(it => it.NhaThuoc_MaNhaThuoc == drugStoreID
            //        && it.Thuoc_ThuocId == i.DrugId
            //        && it.DonViTinh_MaDonViTinh == i.PreUnitID
            //        && it.RecordStatusID == (int)RecordStatus.Activated,
            //        it => new PhieuXuatChiTiet() { DonViTinh_MaDonViTinh = i.UnitId.Value });
            //    rNoteItemRepo.UpdateMany(it => it.NhaThuoc_MaNhaThuoc == drugStoreID
            //        && it.Thuoc_ThuocId == i.DrugId
            //        && it.DonViTinh_MaDonViTinh == i.PreUnitID
            //        && it.RecordStatusID == (int)RecordStatus.Activated,
            //        it => new PhieuNhapChiTiet() { DonViTinh_MaDonViTinh = i.UnitId.Value });
            //});


            List<NoteItemQuantity> receiptItems = null;
            List<NoteItemQuantity> deliveryItems = null;
            var invService = IoC.Container.Resolve<IInventoryService>();
            invService.GetNoteItemQuantities(drugStoreID, out receiptItems, out deliveryItems, filter);
            var updateReceiptItems = receiptItems.Where(i => i.RetailValuesChanged).Select(i => new PhieuNhapChiTiet()
            {
                MaPhieuNhapCt = i.NoteItemId,
                RetailPrice = i.RetailPrice,
                RetailQuantity = i.RetailQuantity,
                IsModified = true
            });
            var updateDeliveryItems = deliveryItems.Where(i => i.RetailValuesChanged).Select(i => new PhieuXuatChiTiet()
            {
                MaPhieuXuatCt = i.NoteItemId,
                RetailPrice = i.RetailPrice,
                RetailQuantity = i.RetailQuantity,
                IsModified = true
            });
            rNoteItemRepo.UpdateMany(updateReceiptItems, i => i.RetailPrice, i => i.RetailQuantity, i => i.IsModified);
            dNoteItemRepo.UpdateMany(updateDeliveryItems, i => i.RetailPrice, i => i.RetailQuantity, i => i.IsModified);
        }        
        private void UpdateInitialInventoryReceiptNote(string drugStoreID, List<CacheDrug> affectedCandidates)
        {
            if (!affectedCandidates.Any()) return;

            var receiptNoteId = MedCacheManager.Instance.GetInitialInventoryReceiptNoteID(drugStoreID);
            if (receiptNoteId > 0)
            {
                var affectedDrugIds = affectedCandidates.Select(i => i.DrugId).ToList();
                var noteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
                var noteItems = noteItemRepo.GetAll().Where(i => i.NhaThuoc_MaNhaThuoc == drugStoreID
                    && affectedDrugIds.Contains(i.Thuoc_ThuocId.Value) && i.PhieuNhap_MaPhieuNhap == receiptNoteId)
                    .Select(i => new { NoteItemID = i.MaPhieuNhapCt, DrugID = i.Thuoc_ThuocId.Value }).ToList();
                if (!noteItems.Any()) return;
              
                var updateNoteItems = noteItems.Select(i => new PhieuNhapChiTiet()
                {
                    MaPhieuNhapCt = i.NoteItemID,
                    Thuoc_ThuocId = i.DrugID
                }).ToList();
                updateNoteItems.ForEach(i =>
                {
                    var drug = affectedCandidates.FirstOrDefault(d => d.DrugId == i.Thuoc_ThuocId.Value);
                    i.GiaNhap = (decimal)drug.InitPrice;
                    i.RetailPrice = (double)drug.InitPrice;
                    i.SoLuong = drug.InitQuantity;
                    i.RetailQuantity = (double)drug.InitQuantity;
                    i.HanDung = drug.ExpiredDateTime;
                    i.IsModified = drug.HasInitQuantityChanged();
                });

                noteItemRepo.UpdateMany(updateNoteItems,
                    i => i.GiaNhap, i => i.RetailPrice, i => i.SoLuong,
                    i => i.RetailQuantity, i => i.IsModified, i => i.HanDung);
            }
        }
        private void UpdatePricesForDrugInventories(string drugStoreID, List<CacheDrug> drugs)
        {
            if (drugs == null || !drugs.Any()) return;
            var invRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Inventory>>();
            var drugIds = drugs.Select(i => i.DrugId).Distinct().ToList();
            var updateInvs = invRepo.GetAll().Where(i => i.DrugStoreID == drugStoreID && drugIds.Contains(i.DrugID))
                .ToList();
            updateInvs.ForEach(i =>
            {
                var drug = drugs.FirstOrDefault(d => d.DrugId == i.DrugID);
                if (Math.Abs(drug.RetailOutPrice - i.RetailOutPrice) > MedConstants.Esp 
                    || drug.RetailUnitId != i.DrugUnitID) 
                {
                    i.RetailOutPrice = drug.RetailOutPrice;
                    i.DrugUnitID = drug.RetailUnitId ?? 0;
                    i.NeedUpdate = true;
                }
            });
            var needUpdateInvs = updateInvs.Where(i => i.NeedUpdate).ToList();
            invRepo.UpdateMany(needUpdateInvs, i => i.RetailOutPrice, i => i.DrugUnitID);
        }
        private void UpdateChangesByDrugs(string drugStoreID, params int[] drugIds)
        {
            if (drugIds == null || !drugIds.Any()) return;

            var dNoteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
            var rNoteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            var refPriceRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, ReceiptDrugPriceRef>>();
            using (var trans = TransactionScopeHelper.CreateReadCommittedForWrite())
            {
                dNoteItemRepo.UpdateMany(i => i.NhaThuoc_MaNhaThuoc == drugStoreID && drugIds.Contains(i.Thuoc_ThuocId.Value),
                    i => new PhieuXuatChiTiet() { IsModified = true });
            }
        }
        public int CreateInitialInventoryReceiptNote(string drugStoreID)
        {
            IBaseRepository<NhaThuoc> drugStoreRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhaThuoc>>();
            var isMainDrugStore = drugStoreRepo.GetAll().Any(i => i.MaNhaThuocCha == drugStoreID);

            IBaseRepository<PhieuNhap> receiptNoteRepo = null;
            var receiptNotes = _dataFilterService.GetValidReceiptNotes(drugStoreID, out receiptNoteRepo);
            var receiptNoteId = receiptNotes.Where(r => r.LoaiXuatNhap_MaLoaiXuatNhap == (int)NoteInOutType.InitialInventory)
                .Select(i => i.MaPhieuNhap).FirstOrDefault();
            if (receiptNoteId <= 0)
            {
                var receiptNote = new PhieuNhap()
                {
                    NhaThuoc_MaNhaThuoc = drugStoreID,
                    NgayNhap = MedConstants.InitialInventoryDeliveryNoteDate,
                    LoaiXuatNhap_MaLoaiXuatNhap = (int)NoteInOutType.InitialInventory,
                    Created = MedConstants.InitialInventoryDeliveryNoteDate,
                    Modified = MedConstants.InitialInventoryDeliveryNoteDate,
                    RecordStatusID = (byte)RecordStatus.Activated
                };
                receiptNoteRepo.Add(receiptNote);
                receiptNoteRepo.Commit();
                receiptNoteId = receiptNote.MaPhieuNhap;
            }

            if (receiptNoteId <= 0) return receiptNoteId;

            var receiptItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            var existingReceiptDrugIds = receiptItemRepo.GetAll().Where(i => i.PhieuNhap_MaPhieuNhap == receiptNoteId)
                .Select(i => i.Thuoc_ThuocId).Distinct().ToList();

            var espQuantity = (decimal)MedConstants.EspQuantity;
            var drugs = _dataFilterService.GetValidDrugs(drugStoreID).Where(d => !existingReceiptDrugIds.Contains(d.ThuocId))
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
                NhaThuoc_MaNhaThuoc = drugStoreID,
                PhieuNhap_MaPhieuNhap = receiptNoteId,
                Thuoc_ThuocId = d.DrugId,
                GiaNhap = d.InitialPrice,
                RetailPrice = (double)d.InitialPrice,
                SoLuong = isMainDrugStore ? d.InitialQuantity : 0,
                RetailQuantity = (double)(isMainDrugStore ? d.InitialQuantity : 0),
                DonViTinh_MaDonViTinh = d.UnitId,
                IsModified = true,
                HandledStatusId = (int)NoteItemHandledStatus.None,
                HanDung = d.ExpireDate > MedConstants.MinProductionDataDate ? d.ExpireDate : null
            }).ToList();
            receiptItemRepo.InsertMany(receiptItems);

            return receiptNoteId;
        }
        #endregion
    }
}
