using System.Collections.Generic;
using System.Linq;
using App.Common.Data;
using App.Common.Extensions;
using App.Constants.Enums;
using Med.Common.Enums;
using Med.Entity.Registration;
using Med.Entity;
using Med.Repository.Factory;
using Med.Service.Base;
using Med.Service.Delivery;
using Med.ServiceModel.Common;
using System;
using Med.Common;
using Med.ServiceModel.Delivery;
using App.Common.Helpers;
using Med.DbContext;
using App.Common.FaultHandling;
using App.Common;
using App.Common.DI;
using Med.Service.Drug;
using Med.Service.Report;
using Med.Service.Caching;
using Med.Service.Background;

namespace Med.Service.Impl.Delivery
{
    public class DeliveryNoteService: MedBaseService, IDeliveryNoteService
    {
        #region Fields
        #endregion

        #region Interface Implementation

        public Dictionary<int, DrugDeliveryInfo> GetDrugDeliveryTotalValues(string drugStoreCode, params int[] deliveryItemIds)
        {
            var result = new Dictionary<int, DrugDeliveryInfo>();
            if (deliveryItemIds == null || !deliveryItemIds.Any()) return result;

            var validReceiptPriceRefs = _dataFilterService.GetValidReceiptDrugPriceRefs(drugStoreCode);
            var priceRefs = validReceiptPriceRefs
                .Where(i => deliveryItemIds.Contains(i.DeliveryNoteItemId))
                .Select(i => new {i.DrugId, i.DeliveryNoteItemId, i.ReceiptNoteItemId, i.Quantity })
                .ToList();

            var receiptItems = _dataFilterService.GetValidReceiptNoteItems(drugStoreCode, null);
            var refReceiptItemIds = priceRefs.Select(i => i.ReceiptNoteItemId).Distinct().ToList();
            var refReceiptItems = receiptItems.Where(i => refReceiptItemIds.Contains(i.NoteItemId)).ToList();
            result = (from p in priceRefs
                join r in refReceiptItems on p.ReceiptNoteItemId equals r.NoteItemId
                select new {p.DrugId, p.DeliveryNoteItemId, p.Quantity, r.FinalRetailPrice}).GroupBy(i => i.DrugId)
                .ToDictionary(i => i.Key, i=> new DrugDeliveryInfo()
                {
                    DrugId = i.Key,
                    QuantityTotal = i.Sum(it => it.Quantity),
                    ValueTotal = i.Sum(it => it.Quantity * it.FinalRetailPrice)
                });

            return result;
        }
       
        public List<DeliveryNoteItemInfo> GetDeliveryNoteItems(string drugStoreCode, FilterObject filter = null, 
            int[] validStatuses = null)
        {
            var validItems = _dataFilterService.GetValidDeliveryNoteItems(drugStoreCode, filter);
            if (validStatuses != null && validStatuses.Any())
            {
                validItems = validItems.Where(di => validStatuses.Contains(di.NoteType));
            }
            var items = validItems.ToList();
            if (items.Count <= 0) return items;

            var validReceiptPriceRefs = _dataFilterService.GetValidReceiptDrugPriceRefs(drugStoreCode);
            var deliveryItemIds = items.Select(i => i.NoteItemId).Distinct().ToList();
            var drugIds = items.Select(i => i.DrugId).Distinct().ToList();
            var priceRefs = validReceiptPriceRefs
                .Where(i => deliveryItemIds.Contains(i.DeliveryNoteItemId) && drugIds.Contains(i.DrugId))
                .Select(i => new {i.DeliveryNoteItemId, i.ReceiptNoteItemId, i.Quantity})
                .ToList();

            var receiptItems = _dataFilterService.GetValidReceiptNoteItems(drugStoreCode);
            var refReceiptItemIds = priceRefs.Select(i => i.ReceiptNoteItemId).Distinct().ToList();
            var refReceiptItems = receiptItems.Where(i => refReceiptItemIds.Contains(i.NoteItemId)).ToList();
            var priceRefsDict = (from p in priceRefs
                join r in refReceiptItems on p.ReceiptNoteItemId equals r.NoteItemId
                select new {p.DeliveryNoteItemId, p.Quantity, r.FinalRetailPrice}).GroupBy(i => i.DeliveryNoteItemId)
                .ToDictionary(i => i.Key, i => i.Sum(it => it.Quantity * it.FinalRetailPrice));

            foreach (var item in items)
            {
                if (priceRefsDict.ContainsKey(item.NoteItemId))
                {
                    item.Revenue = item.FinalRetailAmount - priceRefsDict[item.NoteItemId];
                }
            }
            
            return items;
        }

        public double GetDeliveryRevenueTotal(string drugStoreCode, FilterObject filter = null)
        {
            var deliveryStatuses = new int[] { (int)NoteInOutType.Delivery };
            var deliveryItems = GetDeliveryNoteItems(drugStoreCode, filter, deliveryStatuses);

            return deliveryItems.Sum(i => i.Revenue);
        }

        public List<int> GetModifiedDeliveryNoteItemIds(string drugStoreCode, out List<MinModifiedDateItem> minModifiedDateItems,
            List<int> relatedDeliveryItemIds = null)
        {
            if (relatedDeliveryItemIds == null)
            {
                relatedDeliveryItemIds = new List<int>();
            }
            var deliveryNotes = _dataFilterService.GetValidDeliveryNotes(drugStoreCode);
            var drugs = _dataFilterService.GetValidDrugs(drugStoreCode, null, false);
            var deliveryItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
            var deliveryNoteItems = (from di in deliveryItemRepo.GetAll()
                join d in deliveryNotes on di.PhieuXuat_MaPhieuXuat equals d.MaPhieuXuat
                join dr in drugs on di.Thuoc_ThuocId equals dr.ThuocId
                where di.NhaThuoc_MaNhaThuoc == drugStoreCode && di.SoLuong > 0
                      && di.IsModified || relatedDeliveryItemIds.Contains(di.MaPhieuXuatCt)
                      && d.MaLoaiXuatNhap == (int)NoteInOutType.Delivery
                select new
                {
                    NoteItemId = di.MaPhieuXuatCt,
                    ItemUnitId = di.DonViTinh_MaDonViTinh,
                    MinNoteItemDate = di.PreNoteItemDate < d.NgayXuat ? di.PreNoteItemDate : d.NgayXuat,
                    PreNoteItemDate = di.PreNoteItemDate,
                    NoteItemDate = d.NgayXuat,
                    DrugId = di.Thuoc_ThuocId,
                    DrugUnitId = dr.DonViThuNguyen_MaDonViTinh,
                    Quantity = di.SoLuong,
                    Factors = dr.HeSo
                });

            var modifiedDeliveryDrugItems = (from di in deliveryNoteItems
                group di by di.DrugId
                into g
                select new
                {
                    DrugId = g.Key,
                    MinModifiedDateItem = g.OrderBy(i => i.MinNoteItemDate).FirstOrDefault()
                });

            var modifiedDeliveryDrugItemsDict = modifiedDeliveryDrugItems.ToDictionary(di => di.DrugId, di => di);
            
            minModifiedDateItems = modifiedDeliveryDrugItemsDict.Values.Select(i => new MinModifiedDateItem()
            {
                DrugId = i.DrugId.Value,
                NoteItemId = i.MinModifiedDateItem.NoteItemId,
                RetailQuantity = 0
            }).ToList();
            foreach (var item in minModifiedDateItems)
            {
                var factors = 1.0;
                var deliveryItem = modifiedDeliveryDrugItemsDict[item.DrugId].MinModifiedDateItem;
                if (deliveryItem.DrugUnitId.HasValue && deliveryItem.ItemUnitId == deliveryItem.DrugUnitId.Value 
                    && deliveryItem.Factors > MedConstants.EspQuantity)
                {
                    factors = deliveryItem.Factors;
                }
                item.RetailQuantity = (double)deliveryItem.Quantity * factors;
            }
            
            var modifiedDeliveryItemIds = (from di in deliveryNoteItems
                join d in modifiedDeliveryDrugItems on di.DrugId equals d.DrugId
                where !di.PreNoteItemDate.HasValue || di.NoteItemDate >= d.MinModifiedDateItem.MinNoteItemDate
                select di.NoteItemId).ToList().Distinct().ToList();

            return modifiedDeliveryItemIds;
        }

        public List<int> GetAffectedDeliveryItemsByReturnedItemsFromCustomers(string drugStoreCode,
            List<MinModifiedDateItem> minModifiedDateItems)
        {
            var affectedItemIds = new List<int>();
            var minReceiptItemsIds = minModifiedDateItems.Select(i => i.NoteItemId).Distinct().ToList();
            var receiptItemsQueryable = _dataFilterService.GetValidReceiptNoteItems(drugStoreCode)
                .Where(i => i.NoteType == (int)NoteInOutType.Receipt);
            var minReceiptItems = receiptItemsQueryable.Where(i => minReceiptItemsIds.Contains(i.NoteItemId));
            var returnedToSupplyerNoteStatues = new int[] { (int)NoteInOutType.ReturnToSupplier, (int)NoteInOutType.InventoryAdjustment };
            var deliveryItems = _dataFilterService.GetValidDeliveryNoteItems(drugStoreCode)
                .Where(i => returnedToSupplyerNoteStatues.Contains(i.NoteType));
            var deliveryCandidatesQueryable = (from di in deliveryItems
                join ri in minReceiptItems on di.DrugId equals ri.DrugId
                where di.NoteDate >= ri.NoteDate
                select new
                {
                    di.RetailQuantity,
                    di.NoteDate,
                    di.DrugId
                });
            var maxDeliveryItemsQueryable = (from di in deliveryCandidatesQueryable
                group di by di.DrugId
                into g
                select new
                {
                    DrugId = g.Key,
                    MaxNoteDate = g.Max(i => i.NoteDate)
                });

            var receiptItemCandiates = (from ri in receiptItemsQueryable
                join minRi in minReceiptItems
                    on ri.DrugId equals minRi.DrugId
                join maxDi in maxDeliveryItemsQueryable on ri.DrugId equals maxDi.DrugId
                where ri.NoteDate >= minRi.NoteDate && ri.NoteDate <= maxDi.MaxNoteDate
                select new
                {
                    ri.DrugId,
                    ri.RetailQuantity,
                    ri.NoteDate
                }).ToList().GroupBy(i => i.DrugId).ToDictionary(i => i.Key, i => i);

            var deliveryCandidates = deliveryCandidatesQueryable.ToList().GroupBy(i => i.DrugId).ToList();

            foreach (var cand in deliveryCandidates)
            {
                var subCands = cand.ToList();
                var totaReturnedQuantity = subCands.Sum(i => i.RetailQuantity);
                var totalDeliveryQuantity = 0.0;
                var recalcReturnedQuantity = 0.0;
                if (receiptItemCandiates.ContainsKey(cand.Key))
                {
                    totalDeliveryQuantity = receiptItemCandiates[cand.Key].Sum(i => i.RetailQuantity);
                }
                recalcReturnedQuantity = totaReturnedQuantity - totalDeliveryQuantity;
                if (recalcReturnedQuantity < MedConstants.EspQuantity) continue;

                var minDeliveryItem = receiptItemCandiates[cand.Key].Min(i => i.NoteDate);
                var lowerDate = minDeliveryItem.Value.AddDays(-MedConstants.LimitDaysAllowToReturnDrugFromCustomer);
                var deliveryItemCands = receiptItemsQueryable.Where(i => i.NoteDate > lowerDate && i.NoteDate < minDeliveryItem)
                    .OrderBy(i => i.NoteDate).Select(i => new
                    {
                        i.NoteItemId,
                        i.RetailQuantity,
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

        public int DeleteDeliveryNote(string drugStoreCode, int noteId)
        {
            var noteTypeId = 0;
            if (noteId <= 0) return noteTypeId;

            try
            {
                var validItems = _dataFilterService.GetValidDeliveryNotes(drugStoreCode);
                var currNote = validItems.Where(i => i.MaPhieuXuat == noteId).Select(i => new
                {
                    NoteDate = i.NgayXuat,
                    NoteType = i.MaLoaiXuatNhap
                }).FirstOrDefault();
                if (currNote == null) return noteTypeId;

                noteTypeId = currNote.NoteType;
                var currNoteDate = currNote.NoteDate.Value.AbsoluteStart();
                var effectedNoteIds = validItems.Where(i => i.NgayXuat >= currNoteDate && i.MaLoaiXuatNhap == currNote.NoteType)
                    .Select(i => i.MaPhieuXuat).ToList();
                effectedNoteIds.Add(noteId);
                var deliveryItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
                var drugIds = deliveryItemRepo.GetAll().Where(i => i.PhieuXuat_MaPhieuXuat == noteId)
                    .Select(i => i.Thuoc_ThuocId.Value).Distinct().ToArray();
                using (var tran = TransactionScopeHelper.CreateReadCommittedForWrite())
                {
                    deliveryItemRepo.UpdateMany(i => effectedNoteIds.Contains(i.PhieuXuat_MaPhieuXuat.Value),
                    i => new PhieuXuatChiTiet()
                    {
                        IsModified = true
                    });

                    var deliveryRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuat>>();
                    deliveryRepo.UpdateMany(i => i.MaPhieuXuat == noteId, i => new PhieuXuat()
                    {
                        Xoa = true
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

        public long GetNewDeliveryNoteNumber(string drugStoreCode)
        {           
            var deliveryNotes = _dataFilterService.GetValidDeliveryNotes(drugStoreCode);
            var maxDeliveryNumber = deliveryNotes.Any() ? deliveryNotes.Max(i => i.SoPhieuXuat):0;

            return maxDeliveryNumber + 1;
        }

        public DrugDeliveryItem GetDrugDeliveryItem(string drugStoreCode, int drugId, string barcode)
        {
            DrugDeliveryItem retVal = null;
            var drugs = _dataFilterService.GetValidDrugs(drugStoreCode, null, false);
            if (drugId > 0)
            {
                drugs = drugs.Where(i => i.ThuocId == drugId);
            }
            if (!string.IsNullOrWhiteSpace(barcode))
            {
                drugs = drugs.Where(i => i.BarCode == barcode);
            }
            retVal = drugs
                .Select(i => new DrugDeliveryItem()
                {
                    DrugId = i.ThuocId,
                    DrugCode = i.MaThuoc,
                    DrugName = i.TenThuoc,
                    RetailPrice = (double)i.GiaBanLe,
                    RetailUnitId = i.DonViXuatLe_MaDonViTinh.HasValue ? i.DonViXuatLe_MaDonViTinh.Value : 0,
                    UnitId = i.DonViThuNguyen_MaDonViTinh.HasValue ? i.DonViThuNguyen_MaDonViTinh.Value : 0,
                    Factors = i.HeSo,
                    Quantity = 1
                }).FirstOrDefault();
            if (retVal != null)
            {
                retVal.SelectedUnitId = retVal.RetailUnitId;
                retVal.TotalAmount = retVal.Quantity * retVal.RetailPrice;
                retVal.Price = retVal.RetailPrice;
                var unitIds = new List<int>();
                if (retVal.RetailUnitId > 0)
                {
                    unitIds.Add(retVal.RetailUnitId);
                }
                if (retVal.UnitId > 0 && retVal.UnitId != retVal.RetailUnitId)
                {
                    unitIds.Add(retVal.UnitId);
                }
                if (unitIds.Any())
                {
                    var units = _dataFilterService.GetValidUnits(drugStoreCode).Where(i => unitIds.Contains(i.MaDonViTinh))
                        .Select(i => new DrugUnit()
                        {
                            UnitId = i.MaDonViTinh,
                            UnitName = i.TenDonViTinh,
                            Factors = 1
                        }).ToList();
                    units.ForEach(i =>
                    {
                        if (i.UnitId == retVal.UnitId)
                        {
                            i.Factors = retVal.Factors;
                        }
                    });
                    retVal.DrugUnits = units.ToArray();
                }

                var invService = IoC.Container.Resolve<IInventoryService>();
                var lastInvetories = invService.GetLastInventoryQuantities(drugStoreCode, true, retVal.DrugId);
                if (lastInvetories.ContainsKey(retVal.DrugId))
                {
                    retVal.LastInventoryQuantity = lastInvetories[retVal.DrugId];
                }
            }

            return retVal;
        }

        public int SaveDeliveryNote(string drugStoreCode, int userId, List<DrugDeliveryItem> deliveryItems, double paymentAmount, int noteNumber,
            DateTime? noteDate, int? customerId, int? doctorId, string description)
        {
            if (deliveryItems == null || !deliveryItems.Any() || noteDate == null || noteNumber < 1) return 0;
            int retVal = 0;
            try
            {
                var dateTimeNow = DateTime.Now;
                var newNoteDate = new DateTime(noteDate.Value.Year, noteDate.Value.Month, noteDate.Value.Day, dateTimeNow.Hour, dateTimeNow.Minute, dateTimeNow.Second);
                var totalAmmount = deliveryItems.Sum(i => i.TotalAmount);
                //using (var tran = TransactionScopeHelper.CreateReadCommittedForWrite())
                {
                    var deliveryRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuat>>();
                    var realPayment = paymentAmount;
                    if (Math.Abs(totalAmmount - paymentAmount) < MedConstants.EspAmount)
                    {
                        realPayment = totalAmmount;
                    }
                    if (paymentAmount > totalAmmount)
                    {
                        realPayment = totalAmmount;
                    } 
                    var deliNote = new PhieuXuat()
                    {
                        Created = DateTime.Now,
                        CreatedBy_UserId = userId,
                        NhaThuoc_MaNhaThuoc = drugStoreCode,
                        TongTien = (decimal)totalAmmount,
                        DaTra = (decimal)realPayment,
                        DienGiai = description,
                        MaLoaiXuatNhap = (int)NoteInOutType.Delivery,
                        NgayXuat = newNoteDate,

                        SoPhieuXuat = GetNewDeliveryNoteNumber(drugStoreCode)
                    };
                    deliNote.IsDebt = (double)Math.Abs(deliNote.TongTien - deliNote.DaTra) > MedConstants.EspAmount;

                    if (customerId > 0)
                    {
                        deliNote.KhachHang_MaKhachHang = customerId;
                    }
                    if (doctorId > 0)
                    {
                        deliNote.BacSy_MaBacSy = doctorId;
                    }
                    deliveryRepo.Add(deliNote);
                    deliveryRepo.Commit();

                    var deliNoteItems = deliveryItems.Select(i => new PhieuXuatChiTiet()
                    {
                        Thuoc_ThuocId = i.DrugId,
                        DonViTinh_MaDonViTinh = i.SelectedUnitId,
                        GiaXuat = (decimal)i.Price,
                        IsModified = true,
                        NhaThuoc_MaNhaThuoc = drugStoreCode,
                        PhieuXuat_MaPhieuXuat = deliNote.MaPhieuXuat,
                        SoLuong = (decimal)i.Quantity,
                        ChietKhau = 0
                    }).ToList();
                    var deliveryItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
                    deliveryItemRepo.InsertMany(deliNoteItems, true);

                    //tran.Complete();
                    retVal = deliNote.MaPhieuXuat;
                }
            }
            catch (Exception ex)
            {
                retVal = 0;
                FaultHandler.Instance.Handle(ex, this);
            }

            return retVal;
        }
        public bool CanTransitWarehouse(string drugStoreCode, int noteId)
        {
            var result = true;
            var whTransitRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, WarehouseTransition>>();
            result = !whTransitRepo.GetAll().Any(i => i.DeliveryNoteId == noteId && i.RecordStatusId == (int)RecordStatus.Activated);
            if (result)
            {
                var dsService = IoC.Container.Resolve<IDrugStoreService>();
                result = dsService.GetRelatedDrugStores(drugStoreCode, true).Any();
            }

            return result;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
