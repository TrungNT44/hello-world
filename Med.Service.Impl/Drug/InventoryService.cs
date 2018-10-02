using System.Collections.Generic;
using System.Linq;
using App.Common;
using Med.DbContext;
using Med.Entity;
using Med.Repository.Factory;
using Med.Service.Base;
using Med.Service.Drug;
using App.Common.Validation;
using App.Common.Data;
using App.Common.Helpers;
using System;
using App.Common.FaultHandling;
using App.Common.DI;
using Med.ServiceModel.Common;
using App.Common.Extensions;
using Med.Entity.Drug;
using Med.ServiceModel.Drug;
using Castle.Core.Internal;
using Med.Common.Enums;
using App.Constants.Enums;
using Med.Common;
using Med.Service.Caching;
using Med.ServiceModel.CacheObjects;
using Med.Service.Utilities;

namespace Med.Service.Impl.Drug
{
    public class InventoryService : MedBaseService, IInventoryService
    {
        #region Fields
        #endregion

        #region Interface Implementation
        public void GetNoteItemQuantities(string drugStoreCode, out List<NoteItemQuantity> receiptItemQuantities,
            out List<NoteItemQuantity> deliveryItemQuantities, FilterObject filter)
        {
            receiptItemQuantities = new List<NoteItemQuantity>();
            deliveryItemQuantities = new List<NoteItemQuantity>();

            receiptItemQuantities = _dataFilterService.GetValidReceiptNoteItems(drugStoreCode, filter)
                .Select(i => new NoteItemQuantity()
                {
                    DrugId = i.DrugId,
                    NoteItemId = i.NoteItemId,
                    Quantity = i.Quantity,
                    NoteType = i.NoteType,
                    UnitId = i.UnitId,
                    DrugUnitId = i.DrugUnitId,
                    DrugUnitFactors = i.DrugUnitFactors,
                    NoteDate = i.NoteDate,
                    Price = i.Price
                })
                .ToList();
            receiptItemQuantities.AsParallel().ForAll(item =>
            {
                var factors = 1.0;
                if (item.DrugUnitId.HasValue && item.UnitId == item.DrugUnitId.Value && item.DrugUnitFactors > MedConstants.EspQuantity)
                {
                    factors = item.DrugUnitFactors;
                }
                item.RetailQuantity = item.Quantity * factors;
                item.RetailPrice = item.Price / factors;
            });
            deliveryItemQuantities = _dataFilterService.GetValidDeliveryNoteItems(drugStoreCode, filter)
                .Select(i => new NoteItemQuantity()
                {
                    DrugId = i.DrugId,
                    NoteItemId = i.NoteItemId,
                    Quantity = i.Quantity,
                    NoteType = i.NoteType,
                    UnitId = i.UnitId,
                    DrugUnitId = i.DrugUnitId,
                    DrugUnitFactors = i.DrugUnitFactors,
                    NoteDate = i.NoteDate,
                    Price = i.Price
                })
                .ToList();
            deliveryItemQuantities.AsParallel().ForAll(item =>
            {
                var factors = 1.0;
                if (item.DrugUnitId.HasValue && item.UnitId == item.DrugUnitId.Value && item.DrugUnitFactors > MedConstants.EspQuantity)
                {
                    factors = item.DrugUnitFactors;
                }
                item.RetailQuantity = item.Quantity * factors;
                item.RetailPrice = item.Price / factors;
            });
        }

        public Dictionary<int, DrugInventoryInfo> GetDrugInventoryValues(string drugStoreCode, int[] drugIds,
            DateTime? fromDate = null, DateTime? toDate = null)
        {
            Dictionary<int, DrugInventoryInfo> result = null;
            var filter = new FilterObject()
            {
                DrugIds = drugIds,
                FromDate = null,
                ToDate = toDate ?? MedConstants.MaxProductionDataDate
            };

            var deliveryQuantity = 0.0;
            var receiptQuantity = 0.0;
            var returnedFromCustomerQuantity = 0.0;
            var returnedToSupplyerQuantity = 0.0;
            var initInventoryQuantity = 0.0;
            var hasFromDate = fromDate.HasValue && fromDate.Value > MedConstants.MinProductionDataDate;
            var validDrugs = _dataFilterService.GetValidDrugs(drugStoreCode, filter);
            var isChild = IsChildDrugStore(drugStoreCode);

            var allDrugItems = validDrugs.Select(i => new
            {
                DrugId = i.ThuocId,
                RetailQuantity = isChild ? 0 : (double)i.SoDuDauKy,
                RetailUnitID = i.DonViXuatLe_MaDonViTinh ?? (i.DonViThuNguyen_MaDonViTinh ?? 0),
                InPrice = (double)(i.GiaNhap > 0 ? i.GiaNhap : i.GiaDauKy),
                OutPrice = (double)i.GiaBanLe
            }).ToList();


            var drugItems = allDrugItems.ToDictionary(i => i.DrugId, i => new { i.RetailUnitID, i.RetailQuantity, i.InPrice, i.OutPrice });

            result = drugItems.ToDictionary(i => i.Key, i => new DrugInventoryInfo()
            {
                DrugId = i.Key,
                FirstInventoryQuantity = 0.0,
                LastInventoryQuantity = 0.0,
                DrugUnitId = i.Value.RetailUnitID,
                InPrice = i.Value.InPrice,
                OutPrice = i.Value.OutPrice
            });

            List<NoteItemQuantity> receiptItems = null;
            List<NoteItemQuantity> deliveryItems = null;
            var invService = IoC.Container.Resolve<IInventoryService>();
            invService.GetNoteItemQuantities(drugStoreCode, out receiptItems, out deliveryItems, filter);

            var validReceiptStatus = new int[] { (int)NoteInOutType.Receipt, (int)NoteInOutType.InventoryAdjustment };
            var validDeliveryStatus = new int[] { (int)NoteInOutType.Delivery, (int)NoteInOutType.InventoryAdjustment };
            drugItems.ForEach(d =>
            {
                var drugId = d.Key;
                initInventoryQuantity = drugItems[drugId].RetailQuantity;
                receiptQuantity = receiptItems.Where(i => validReceiptStatus.Contains(i.NoteType) && i.DrugId == drugId)
                    .Sum(i => i.RetailQuantity);
                deliveryQuantity = deliveryItems.Where(i => validDeliveryStatus.Contains(i.NoteType) && i.DrugId == drugId)
                    .Sum(i => i.RetailQuantity);
                returnedFromCustomerQuantity = receiptItems.Where(i => i.NoteType == (int)NoteInOutType.ReturnFromCustomer && i.DrugId == drugId)
                    .Sum(i => i.RetailQuantity);
                returnedToSupplyerQuantity = deliveryItems.Where(i => i.NoteType == (int)NoteInOutType.ReturnToSupplier && i.DrugId == drugId)
                    .Sum(i => i.RetailQuantity);
                result[drugId].LastInventoryQuantity = Math.Round((initInventoryQuantity + (receiptQuantity - returnedToSupplyerQuantity)) - (deliveryQuantity - returnedFromCustomerQuantity), 2);
            });

            if (hasFromDate)
            {
                drugItems.ForEach(d =>
                {
                    var drugId = d.Key;
                    initInventoryQuantity = drugItems[drugId].RetailQuantity;
                    receiptQuantity = receiptItems.Where(i => validReceiptStatus.Contains(i.NoteType) && i.DrugId == drugId && i.NoteDate < fromDate)
                        .Sum(i => i.RetailQuantity);
                    deliveryQuantity = deliveryItems.Where(i => validDeliveryStatus.Contains(i.NoteType) && i.DrugId == drugId && i.NoteDate < fromDate)
                        .Sum(i => i.RetailQuantity);
                    returnedFromCustomerQuantity = receiptItems.Where(i => i.NoteType == (int)NoteInOutType.ReturnFromCustomer && i.DrugId == drugId && i.NoteDate < fromDate)
                        .Sum(i => i.RetailQuantity);
                    returnedToSupplyerQuantity = deliveryItems.Where(i => i.NoteType == (int)NoteInOutType.ReturnToSupplier && i.DrugId == drugId && i.NoteDate < fromDate)
                        .Sum(i => i.RetailQuantity);
                    result[drugId].FirstInventoryQuantity = Math.Round((initInventoryQuantity + (receiptQuantity - returnedToSupplyerQuantity)) - (deliveryQuantity - returnedFromCustomerQuantity), 2);
                });
            }

            return result;
        }
        public List<Inventory> GenerateInventory4Drugs(string drugStoreCode, bool reGen4AllDrugs, bool updatePrices, params int[] drugIds)
        {
            LogHelper.Debug("Generate inventory of drugs for drug store: {0}", drugStoreCode);
            var results = new List<Inventory>();
            var invRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Inventory>>();
            var drugs = _dataFilterService.GetValidDrugs(drugStoreCode);
            if (reGen4AllDrugs)
            {
                drugIds = drugs.Select(i => i.ThuocId).ToArray();
            }
            else if (drugIds == null || !drugIds.Any())
            {
                drugIds = (from d in drugs
                           join i in invRepo.GetAll() on d.ThuocId equals i.DrugID into diGroup
                           from di in diGroup.DefaultIfEmpty()
                           where di == null
                           select d.ThuocId
                          ).Distinct().ToArray();
            }

            if (drugIds == null || !drugIds.Any()) return results;

            var inventories = GetDrugInventoryValues(drugStoreCode, drugIds);
            var updateInvs = invRepo.Table.Where(i => i.DrugStoreID == drugStoreCode
                && drugIds.Contains(i.DrugID) && i.RecordStatusID == (byte)RecordStatus.Activated).ToList();
            var existingInvDrugIds = updateInvs.Select(i => i.DrugID).Distinct().ToArray();
            var newInvDrugIds = drugIds.Except(existingInvDrugIds).ToArray();
            var newInvs = inventories.Where(i => newInvDrugIds.Contains(i.Key))
                .Select(i => new Inventory()
                {
                    DrugStoreID = drugStoreCode,
                    DrugID = i.Key,
                    LastValue = i.Value.LastInventoryQuantity,
                    DrugUnitID = i.Value.DrugUnitId,
                    LastInPrice = i.Value.InPrice,
                    LastOutPrice = i.Value.OutPrice,
                    RecordStatusID = (byte)RecordStatus.Activated
                }).ToList();
            updateInvs.ForEach(i => 
            {
                if (inventories.ContainsKey(i.DrugID))
                {
                    i.LastValue = inventories[i.DrugID].LastInventoryQuantity;
                }
            });
            invRepo.UpdateMany(updateInvs);
            invRepo.InsertMany(newInvs);
            invRepo.Commit();
            results = updateInvs;
            results.AddRange(newInvs);

            if (updatePrices)
            {
                using (var scope = IoC.Container.BeginScope())
                {
                    results = UpdateLastestInventoryPrices(drugStoreCode, drugIds);
                }
            }

            var cacheDrugs = MedCacheManager.Instance.GetAllCacheDrugs(drugStoreCode);
            if (cacheDrugs != null)
            {
                var invDict = results.GroupBy(i => i.DrugID).ToDictionary(i => i.Key, i => i.First());
                cacheDrugs.ForEach(i =>
                {
                    if (invDict.ContainsKey(i.DrugId))
                    {
                        var inv = invDict[i.DrugId];
                        i.LastInventoryQuantity = (double)inv.LastValue;
                        i.LastInPrice = (double)inv.LastInPrice;
                        i.LastOutPrice = (double)inv.LastOutPrice;
                        i.RetailOutPrice = (double)inv.RetailOutPrice;
                        i.RetailBatchOutPrice = (double)inv.RetailBatchOutPrice;
                    }
                });
                MedCacheManager.Instance.UpdateCacheDrugs(drugStoreCode, cacheDrugs);
            }           

            return results;
        }
        public Dictionary<int, double> GetLastInventoryQuantities(string drugStoreCode, bool fromCache, params int[] drugIds)
        {
            Dictionary<int, double> results = null;
            if (fromCache)
            {
                var drugs = MedCacheManager.Instance.GetCacheDrugs(drugStoreCode, drugIds);
                // results = drugs.ToDictionary(i => i.DrugId, i => i.LastInventoryQuantity > 0 ? i.LastInventoryQuantity : 0);
                results = drugs.ToDictionary(i => i.DrugId, i => i.LastInventoryQuantity);
            }
            else
            {
                var inventories = GetDrugInventoryValues(drugStoreCode, drugIds);
                // results = inventories.ToDictionary(i => i.Key, i => i.Value.LastInventoryQuantity > 0 ? i.Value.LastInventoryQuantity : 0.0);
                results = inventories.ToDictionary(i => i.Key, i => i.Value.LastInventoryQuantity);
            }
            var notInventiedDrugIds = drugIds.Except(results.Keys).ToList();
            if (notInventiedDrugIds != null || notInventiedDrugIds.Any())
            {
                results = results.Union(notInventiedDrugIds.ToDictionary(i => i, i => 0.0))
                    .ToDictionary(p => p.Key, p =>p.Value);
            }
            
            return results;
        }
        public void UpdateLastQuantity4CacheDrugs(string drugStoreCode, List<CacheDrug> drugs)
        {
            if (drugs == null || !drugs.Any()) return;

            var drugIds = drugs.Select(i => i.DrugId).ToList();
            var invRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Inventory>>();
            var inventories = invRepo.GetAll().Where(i => i.DrugStoreID == drugStoreCode
                && drugIds.Contains(i.DrugID) && i.RecordStatusID == (byte)RecordStatus.Activated)
                .Select(i => new { i.DrugID, i.LastValue, i.LastInPrice, i.LastOutPrice, i.RetailOutPrice, i.RetailBatchOutPrice }).ToList()
                .GroupBy(i => i.DrugID).ToDictionary(i => i.Key, i => i.First());
            drugs.ForEach(i =>
            {                
                if (inventories.ContainsKey(i.DrugId))
                {
                    var inv = inventories[i.DrugId];
                    // i.LastInventoryQuantity = (double)inv.LastValue > MedConstants.EspQuantity ? (double)inv.LastValue : 0.0;
                    i.LastInventoryQuantity = (double)inv.LastValue;
                    i.LastInPrice = (double)inv.LastInPrice;
                    i.LastOutPrice = (double)inv.LastOutPrice;
                    i.RetailOutPrice = (double)inv.RetailOutPrice;
                    i.RetailBatchOutPrice = (double)inv.RetailBatchOutPrice;
                }
            });
        }
        public List<ItemInfoCandidate> GetLastestDeliveryItemsByDrugs(string drugStoreCode, params int[] drugIds)
        {
            // Get lastest delivery items by drugs
            var filterByDrugs = drugIds != null && drugIds.Any();
            IBaseRepository<PhieuXuat> noteRepo = null;
            var notes = _dataFilterService.GetValidDeliveryNotes(drugStoreCode, out noteRepo);
            var noteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
            var drugs = _dataFilterService.GetValidDrugs(drugStoreCode);
            var deliveryNoteItems = (from n in notes
                                     join ni in noteItemRepo.GetAll() on n.MaPhieuXuat equals ni.PhieuXuat_MaPhieuXuat
                                     join dr in drugs on ni.Thuoc_ThuocId equals dr.ThuocId
                                     where ni.SoLuong > 0 && ni.NhaThuoc_MaNhaThuoc == drugStoreCode
                                        && (!filterByDrugs || drugIds.Contains(dr.ThuocId))
                                     select new ItemInfoCandidate
                                     {
                                         NoteId = n.MaPhieuXuat,
                                         NoteItemId = ni.MaPhieuXuatCt,
                                         Quantity = (double)ni.SoLuong,
                                         DrugId = dr.ThuocId,
                                         ItemUnitId = ni.DonViTinh_MaDonViTinh,
                                         RetailUnitId = dr.DonViXuatLe_MaDonViTinh,
                                         UnitId = dr.DonViThuNguyen_MaDonViTinh,
                                         Factors = dr.HeSo,
                                         NoteType = n.MaLoaiXuatNhap,
                                         NoteDate = n.NgayXuat,
                                         Price = (double)ni.GiaXuat,
                                         VAT = (double)n.VAT,
                                         Discount = (double)ni.ChietKhau,
                                         ReduceQuantity = ni.ReduceQuantity ?? 0,
                                     });
            var lastestDeliveryItems = (from i in deliveryNoteItems
                                        group i by i.DrugId into g
                                        select g.OrderByDescending(gi => gi.NoteDate).FirstOrDefault()).ToList();
            foreach (var item in lastestDeliveryItems)
            {
                var factors = 1.0;
                if (item.UnitId.HasValue && item.ItemUnitId == item.UnitId.Value && item.Factors > MedConstants.EspQuantity)
                {
                    factors = item.Factors;
                }
                item.RetailQuantity = item.Quantity * factors;
                item.RetailPrice = item.Price / factors;
            }            

            return lastestDeliveryItems;
        }
        public List<ItemInfoCandidate> GetLastestReceiptItemsByDrugs(string drugStoreCode, params int[] drugIds)
        {
            // Get lastest receipt items by drugs
            var filterByDrugs = drugIds != null && drugIds.Any();           
            var drugs = _dataFilterService.GetValidDrugs(drugStoreCode);
            IBaseRepository<PhieuNhap> receiptNoteRepo = null;
            var receiptNotes = _dataFilterService.GetValidReceiptNotes(drugStoreCode, out receiptNoteRepo);
            var receiptNoteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            var receiptNoteItems = (from n in receiptNotes
                                    join ni in receiptNoteItemRepo.GetAll() on n.MaPhieuNhap equals ni.PhieuNhap_MaPhieuNhap
                                    join dr in drugs on ni.Thuoc_ThuocId equals dr.ThuocId
                                    where ni.SoLuong > 0 && ni.NhaThuoc_MaNhaThuoc == drugStoreCode
                                       && (!filterByDrugs || drugIds.Contains(dr.ThuocId))
                                    select new ItemInfoCandidate
                                    {
                                        NoteId = n.MaPhieuNhap,
                                        NoteItemId = ni.MaPhieuNhapCt,
                                        Quantity = (double)ni.SoLuong,
                                        DrugId = dr.ThuocId,
                                        ItemUnitId = ni.DonViTinh_MaDonViTinh,
                                        RetailUnitId = dr.DonViXuatLe_MaDonViTinh,
                                        UnitId = dr.DonViThuNguyen_MaDonViTinh,
                                        Factors = dr.HeSo,
                                        NoteType = n.LoaiXuatNhap_MaLoaiXuatNhap ?? 0,
                                        NoteDate = n.NgayNhap,
                                        Price = (double)ni.GiaNhap,
                                        VAT = (double)n.VAT,
                                        Discount = (double)ni.ChietKhau,
                                        ReduceQuantity = ni.ReduceQuantity ?? 0,
                                        OutPrice = (double)ni.GiaBanLe,
                                        RetailBatchOutPrice = (double)dr.GiaBanBuon,
                                        PrevRetailOutPrice = (double)dr.GiaBanLe,                                       
                                    });
            var lastestReceiptItems = (from i in receiptNoteItems
                                       group i by i.DrugId into g
                                       select g.OrderByDescending(gi => gi.NoteDate).FirstOrDefault()).ToList();
            foreach (var item in lastestReceiptItems)
            {
                var factors = 1.0;
                if (item.UnitId.HasValue && item.ItemUnitId == item.UnitId.Value && item.Factors > MedConstants.EspQuantity)
                {
                    factors = item.Factors;
                }
                item.RetailQuantity = item.Quantity * factors;
                item.RetailPrice = item.Price / factors;
                item.RetailOutPrice = item.OutPrice / factors;
                if (item.RetailPrice <= MedConstants.EspPrice)
                {
                    item.RetailPrice = item.PrevRetailOutPrice;
                }
                if (item.RetailOutPrice <= MedConstants.EspPrice)
                {
                    item.RetailOutPrice = item.PrevRetailOutPrice;
                }
            }

            return lastestReceiptItems;
        }
        public List<Inventory> UpdateLastestInventoryPrices(string drugStoreCode, params int[] drugIds)
        {
            // Get lastest delivery items by drugs
            var lastestDeliveryItems = GetLastestDeliveryItemsByDrugs(drugStoreCode, drugIds);
            // Get lastest receipt items by drugs
            var lastestReceiptItems = GetLastestReceiptItemsByDrugs(drugStoreCode, drugIds);
            // Update last delivery/receipt price of drugs            
            var updateDrugs = new List<Thuoc>();
            var drugRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Thuoc>>();
            var lastReceiptPrices = lastestReceiptItems.ToDictionary(i => i.DrugId, i => i);
            var lastDeliveryPrices = lastestDeliveryItems.ToDictionary(i => i.DrugId, i => i);
            var invRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Inventory>>();
            var updateInvs = invRepo.TableAsNoTracking.Where(i => i.DrugStoreID == drugStoreCode
                && drugIds.Contains(i.DrugID) && i.RecordStatusID == (byte)RecordStatus.Activated).ToList();
            updateInvs.ForEach(i =>
            {
                if (lastReceiptPrices.ContainsKey(i.DrugID))
                {
                    var prices = lastReceiptPrices[i.DrugID];
                    i.LastInPrice = prices.RetailPrice;
                    updateDrugs.Add(new Thuoc()
                    {
                        ThuocId = i.DrugID,
                        GiaBanLe = (decimal)prices.RetailOutPrice,
                        GiaBanBuon = (decimal)prices.RetailBatchOutPrice,
                        GiaNhap = (decimal)prices.RetailPrice
                    });
                }
                if (lastDeliveryPrices.ContainsKey(i.DrugID))
                {
                    var prices = lastDeliveryPrices[i.DrugID];
                    i.LastOutPrice = prices.RetailPrice;
                }
            });
            invRepo.UpdateMany(updateInvs);
            invRepo.Commit();
            var dsSettings = IoC.Container.Resolve<IUtilitiesService>().GetDrugStoreSetting(drugStoreCode);
            if (dsSettings != null && dsSettings.AutoUpdateInOutPriceOnNote)
            {
                drugRepo.UpdateMany(updateDrugs, i => i.ThuocId, i => i.GiaNhap, i => i.GiaBanLe, i => i.GiaBanBuon);
            }

            // Update drug mapping
            var drugMapRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, DrugMapping>>();
            var drugMaps = drugMapRepo.TableAsNoTracking.Where(i => i.DrugStoreID == drugStoreCode
                && drugIds.Contains(i.SlaveDrugID)).ToList();
            var updateDrugMaps = new List<DrugMapping>();
            drugMaps.ForEach(i =>
            {
                if (lastReceiptPrices.ContainsKey(i.SlaveDrugID))
                {
                    var prices = lastReceiptPrices[i.SlaveDrugID];
                    if (prices.NoteDate > i.InLastUpdateDate)
                    {
                        var drugMapUpdate = false;
                        if (prices.RetailPrice > MedConstants.EspPrice)
                        {
                            i.InPrice = (decimal)prices.RetailPrice;
                            i.InLastUpdateDate = prices.NoteDate;
                            drugMapUpdate = true;
                        }
                        if (prices.RetailOutPrice > MedConstants.EspPrice)
                        {
                            i.OutPrice = (decimal)prices.RetailOutPrice;
                            i.OutLastUpdateDate = prices.NoteDate;
                            drugMapUpdate = true;
                        }
                        if (drugMapUpdate)
                        {
                            updateDrugMaps.Add(i);
                        }
                    } 
                    else if (prices.NoteDate.Value.Date == i.InLastUpdateDate.Value.Date)
                    {
                        var drugMapUpdate = false;
                        if (prices.RetailPrice > MedConstants.EspPrice
                            && prices.RetailPrice < (double)i.InPrice)
                        {
                            i.InPrice = (decimal)prices.RetailPrice;
                            i.InLastUpdateDate = prices.NoteDate;
                            drugMapUpdate = true;
                        }
                        if (prices.RetailOutPrice > MedConstants.EspPrice
                            && prices.RetailOutPrice < (double)i.InPrice)
                        {
                            i.OutPrice = (decimal)prices.RetailOutPrice;
                            i.OutLastUpdateDate = prices.NoteDate;
                            drugMapUpdate = true;
                        }
                        if (drugMapUpdate)
                        {
                            updateDrugMaps.Add(i);
                        }
                    }
                }
            });
            drugMapRepo.UpdateMany(updateDrugMaps);
            drugMapRepo.Commit();

            return updateInvs;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
