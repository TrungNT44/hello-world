using System.Collections.Generic;
using System.Linq;
using Med.Common;
using Med.Common.Enums;
using Med.Entity.Registration;
using Med.Entity;
using Med.Service.Base;
using Med.Service.Receipt;
using System;
using App.Common.Data;
using Med.Repository.Factory;
using Med.ServiceModel.Common;
using Med.ServiceModel.Delivery;
using Med.DbContext;
using App.Common;
using App.Common.Helpers;
using App.Common.FaultHandling;
using App.Common.Extensions;
using App.Common.DI;
using App.Constants.Enums;
using Med.Service.Background;

namespace Med.Service.Impl.Receipt
{
    public class ReceiptNoteService: MedBaseService, IReceiptNoteService
    {
        #region Fields
        #endregion

        #region Interface Implementation
        public List<int> GetModifiedReceiptNoteItemIds(string drugStoreCode, List<int> relatedReceiptItemIds = null)
        {
            if (relatedReceiptItemIds == null)
            {
                relatedReceiptItemIds = new List<int>();
            }
            var receiptNotes = _dataFilterService.GetValidReceiptNotes(drugStoreCode);
            var receiptItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            var noteTypeIds = new int[] { (int)NoteInOutType.Receipt, (int)NoteInOutType.InitialInventory, (int)NoteInOutType.InventoryAdjustment };
            var receiptNoteItems = (from ri in receiptItemRepo.GetAll()
                join r in receiptNotes on ri.PhieuNhap_MaPhieuNhap equals r.MaPhieuNhap
                where ri.SoLuong > 0 && ri.NhaThuoc_MaNhaThuoc == drugStoreCode &&
                    (ri.IsModified || relatedReceiptItemIds.Contains(ri.MaPhieuNhapCt))
                    && noteTypeIds.Contains(r.LoaiXuatNhap_MaLoaiXuatNhap.Value)
                select new
                {
                    NoteItemId = ri.MaPhieuNhapCt,
                    MinNoteItemDate = ri.PreNoteItemDate < r.NgayNhap ? ri.PreNoteItemDate : r.NgayNhap,
                    PreNoteItemDate = ri.PreNoteItemDate,
                    NoteItemDate = r.NgayNhap,
                    DrugId = ri.Thuoc_ThuocId
                });
            var modifiedReceiptDrugItems = (from ri in receiptNoteItems
                group ri.MinNoteItemDate by ri.DrugId
                into g
                select new
                {
                    DrugId = g.Key,
                    MinModifiedDate = g.Min() 
                });

            var modifiedReceiptItemIds = (from ri in receiptNoteItems
                join d in modifiedReceiptDrugItems on ri.DrugId equals d.DrugId
                where !ri.PreNoteItemDate.HasValue || ri.MinNoteItemDate >= d.MinModifiedDate
                select ri.NoteItemId).Distinct().ToList();

            return modifiedReceiptItemIds;
        }
        public List<int> GetAffectedReceiptItemsByMinModifiedDeliveryItems(
            List<MinModifiedDateItem> minModifiedDateDeliveryItems, string drugStoreCode, out List<MinModifiedDateItem> minModifiedDateItems)
        {
            var affectedReceiptItemIds = new List<int>();

            var validReceiptDrugPriceRefs = _dataFilterService.GetValidReceiptDrugPriceRefs(drugStoreCode);
            var receiptNotes = _dataFilterService.GetValidReceiptNotes(drugStoreCode);
            var receiptItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            var deliveryItemIds = minModifiedDateDeliveryItems.Select(i => i.NoteItemId).Distinct().ToList();
            var drugIds = minModifiedDateDeliveryItems.Select(i => i.DrugId).Distinct().ToList();
            var effectedReceiptItemIds = validReceiptDrugPriceRefs.Where(i => deliveryItemIds.Contains(i.DeliveryNoteItemId) && drugIds.Contains(i.DrugId))
                .Select(i => i.ReceiptNoteItemId).Distinct().ToList();
            var affectedReceiptNoteDatesByDrugs = (from ri in receiptItemRepo.GetAll()
                where affectedReceiptItemIds.Contains(ri.MaPhieuNhapCt)
                      && ri.SoLuong > 0 && ri.NhaThuoc_MaNhaThuoc == drugStoreCode
                group ri.PreNoteItemDate by ri.Thuoc_ThuocId
                into g
                select
                    new
                    {
                        DrugId = g.Key,
                        MaxNoteDate = g.Max()
                    });

            var affetedReceiptItemCandidates = (from ri in receiptItemRepo.GetAll()
                join ar in affectedReceiptNoteDatesByDrugs on ri.Thuoc_ThuocId equals ar.DrugId
                where ri.PreNoteItemDate <= ar.MaxNoteDate && ri.SoLuong > 0
                select
                new MinModifiedDateItem()
                {
                    DrugId = ri.Thuoc_ThuocId.Value,
                    RetailQuantity = ri.RetailQuantity,
                    NoteItemId = ri.MaPhieuNhapCt,
                    NoteDate = ri.PreNoteItemDate
                }).GroupBy(ar => ar.DrugId).ToDictionary(ar => ar.Key, ar => ar.ToList());

            minModifiedDateItems = new List<MinModifiedDateItem>();
            foreach (var di in minModifiedDateDeliveryItems)
            {
                if (!affetedReceiptItemCandidates.ContainsKey(di.DrugId)) continue;
                
                var rItemsByDrug = affetedReceiptItemCandidates[di.DrugId].OrderByDescending(r => r.NoteDate).ToList();
                var rMinItem = rItemsByDrug.Last();
                var quantiy = 0.0;
                var findItem = false;
                while (quantiy < di.RetailQuantity && rItemsByDrug.Count > 0)
                {
                    var rItem = rItemsByDrug[0];
                    var remainQuantity = di.RetailQuantity - quantiy;
                    var usedQuantity = Math.Min(remainQuantity, rItem.RetailQuantity);
                    quantiy += usedQuantity;
                    if (rItem.RetailQuantity < remainQuantity)
                    {
                        rItemsByDrug.Remove(rItem);
                    }
                    else
                    {
                        findItem = true;
                        minModifiedDateItems.Add(rItem);
                        break;
                    }
                }
                if (!findItem && quantiy < di.RetailQuantity)
                {
                    minModifiedDateItems.Add(rMinItem);
                }
            }

            foreach (var item in minModifiedDateItems)
            {
                var receiptItemIds = receiptItemRepo.GetAll()
                    .Where(ri => ri.Thuoc_ThuocId == item.DrugId && ri.PreNoteItemDate >= item.NoteDate
                        && ri.SoLuong > 0)
                    .Select(ri => ri.MaPhieuNhapCt).Distinct().ToList();
                affectedReceiptItemIds.AddRange(receiptItemIds);
            }
            affectedReceiptItemIds = affectedReceiptItemIds.Distinct().ToList();

            return affectedReceiptItemIds;
        }
        public List<ReceiptNoteItemInfo> GetReceiptNoteItems(string drugStoreCode, FilterObject filter = null, 
            int[] validStatuses = null)
        {
            var validItems = _dataFilterService.GetValidReceiptNoteItems(drugStoreCode, filter);
            if (validStatuses != null && validStatuses.Any())
            {
                validItems = validItems.Where(di => validStatuses.Contains(di.NoteType));
            }
            var items = validItems.ToList();
            
            return items;
        }
        public List<int> GetAffectedReceiptItemsByReturnedItemsToSuppliers(string drugStoreCode,
            List<MinModifiedDateItem> minModifiedDateItems)
        {
            var affectedItemIds = new List<int>();
            var minDeliveryItemsIds = minModifiedDateItems.Select(i => i.NoteItemId).Distinct().ToList();
            var deliveryItemsQueryable = _dataFilterService.GetValidDeliveryNoteItems(drugStoreCode, null)
                .Where(i => i.NoteType == (int)NoteInOutType.Delivery);
            var minDeliveryItems = deliveryItemsQueryable.Where(i => minDeliveryItemsIds.Contains(i.NoteItemId));
            var returnedFromCusNoteStatues = new int[] {(int)NoteInOutType.ReturnFromCustomer, (int)NoteInOutType.InventoryAdjustment};
            var receiptItems = _dataFilterService.GetValidReceiptNoteItems(drugStoreCode, null)
                .Where(i => returnedFromCusNoteStatues.Contains(i.NoteType));
            var receiptCandidatesQueryable = (from ri in receiptItems
                join di in minDeliveryItems on ri.DrugId equals di.DrugId
                where ri.NoteDate >= di.NoteDate
                select new
                {
                    ri.RetailQuantity,
                    ri.NoteDate,
                    ri.DrugId
                });
            var maxReceiptItemsQueryable = (from ri in receiptCandidatesQueryable
                group ri by ri.DrugId
                into g
                select new
                {
                    DrugId = g.Key,
                    MaxNoteDate = g.Max(i => i.NoteDate)
                });

            var deliveryItemCandiates = (from di in deliveryItemsQueryable
                join minDi in minDeliveryItems
                    on di.DrugId equals minDi.DrugId
                join maxRi in maxReceiptItemsQueryable on di.DrugId equals maxRi.DrugId
                where di.NoteDate >= minDi.NoteDate && di.NoteDate <= maxRi.MaxNoteDate
                select new
                {
                    di.DrugId,
                    di.RetailQuantity,
                    di.NoteDate
                }).ToList().GroupBy(i => i.DrugId).ToDictionary(i => i.Key, i => i);

            var receiptCandidates = receiptCandidatesQueryable.ToList().GroupBy(i => i.DrugId).ToList();

            foreach (var cand in receiptCandidates)
            {
                var subCands = cand.ToList();
                var totaReturnedQuantity = subCands.Sum(i => i.RetailQuantity);
                var totalDeliveryQuantity = 0.0;
                var recalcReturnedQuantity = 0.0;
                if (deliveryItemCandiates.ContainsKey(cand.Key))
                {
                    totalDeliveryQuantity = deliveryItemCandiates[cand.Key].Sum(i => i.RetailQuantity);
                }
                recalcReturnedQuantity = totaReturnedQuantity - totalDeliveryQuantity;
                if (recalcReturnedQuantity < MedConstants.EspQuantity) continue;

                var minDeliveryItem = deliveryItemCandiates[cand.Key].Min(i => i.NoteDate);
                var lowerDate = minDeliveryItem.Value.AddDays(-MedConstants.LimitDaysAllowToReturnDrugFromCustomer);
                var deliveryItemCands = deliveryItemsQueryable.Where(i => i.NoteDate > lowerDate && i.NoteDate < minDeliveryItem)
                    .OrderBy(i => i.NoteDate).Select(i => new
                    {
                        i.NoteItemId,
                        i.RetailQuantity
                    }).ToList();
               
                var quantity = 0.0;
                while (quantity < recalcReturnedQuantity && deliveryItemCands.Count > 0)
                {
                    var remainQuantity = recalcReturnedQuantity - quantity;
                    var firstItem = deliveryItemCands.First();
                    deliveryItemCands.Remove(firstItem);
                    var usedQuantity = Math.Min(firstItem.RetailQuantity, remainQuantity);
                    quantity += usedQuantity;
                    affectedItemIds.Add(firstItem.NoteItemId);
                }
            }

            return affectedItemIds;
        }
        public Dictionary<int, double> GetReceiptRefQuantityByDeliveryItems(string drugStoreCode, params int[] deliveryItemIds)
        {
            var result = new Dictionary<int, double>();
            if (deliveryItemIds == null || !deliveryItemIds.Any()) return result;

            var validReceiptPriceRefs = _dataFilterService.GetValidReceiptDrugPriceRefs(drugStoreCode);
            var priceRefs = validReceiptPriceRefs
                .Where(i => deliveryItemIds.Contains(i.DeliveryNoteItemId))
                .Select(i => new {i.ReceiptNoteItemId, i.Quantity })
                .ToList();

            result = priceRefs.GroupBy(i => i.ReceiptNoteItemId).ToDictionary(i => i.Key, i => i.Sum(ii => ii.Quantity));

            return result;
        }
        public int DeleteReceiptNote(string drugStoreCode, int noteId)
        {
            var noteTypeId = 0;
            if (noteId <= 0) return noteTypeId;

            try
            {
                var validItems = _dataFilterService.GetValidReceiptNotes(drugStoreCode);
                var currNote = validItems.Where(i => i.MaPhieuNhap == noteId).Select(i => new
                {
                    NoteDate = i.NgayNhap,
                    NoteType = i.LoaiXuatNhap_MaLoaiXuatNhap
                }).FirstOrDefault();
                if (currNote == null) return noteTypeId;

                noteTypeId = currNote.NoteType.Value;
                var currNoteDate = (DateTime?)currNote.NoteDate.Value.AbsoluteStart();
                var effectedNoteIds = validItems.Where(i => i.NgayNhap >= currNoteDate && i.LoaiXuatNhap_MaLoaiXuatNhap == currNote.NoteType)
                    .Select(i => i.MaPhieuNhap).ToList();
                effectedNoteIds.Add(noteId);
                var receiptItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();

                var drugIds = receiptItemRepo.GetAll().Where(i => i.PhieuNhap_MaPhieuNhap == noteId)
                    .Select(i => i.Thuoc_ThuocId.Value).Distinct().ToArray();
                using (var tran = TransactionScopeHelper.CreateReadCommittedForWrite())
                {                    
                    receiptItemRepo.UpdateMany(i => effectedNoteIds.Contains(i.PhieuNhap_MaPhieuNhap),
                    i => new PhieuNhapChiTiet()
                    {
                        IsModified = true
                    });

                    var receiptRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhap>>();
                    receiptRepo.UpdateMany(i => i.MaPhieuNhap == noteId, i => new PhieuNhap()
                    {
                        Xoa = true
                    });

                    var whTransitRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, WarehouseTransition>>();
                    whTransitRepo.UpdateMany(i => i.ReceiptNoteId == noteId, i => new WarehouseTransition()
                    {
                        RecordStatusId = (int)RecordStatus.Deleted
                    });

                    tran.Complete();
                }
                BackgroundServiceJobHelper.EnqueueUpdateLastInventoryQuantity4CacheDrugs(drugStoreCode, drugIds);
            }
            catch (Exception ex)
            {
                noteTypeId = 0;
                FaultHandler.Instance.Handle(ex, this);
            }

            return noteTypeId;
        }
        public long GetNewReceiptNoteNumber(string drugStoreCode)
        {
            var notes = _dataFilterService.GetValidReceiptNotes(drugStoreCode);
            var maxNumber = notes.Any() ? notes.Max(i => i.SoPhieuNhap) : 0;

            return maxNumber + 1;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
