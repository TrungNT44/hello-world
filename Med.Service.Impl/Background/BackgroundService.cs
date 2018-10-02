using App.Common.Data;
using global::System.Collections.Generic;
using Med.Service.Log;
using Med.Entity.Log;
using Med.DbContext;
using App.Common.DI;
using Med.Service.Base;
using Med.ServiceModel.Log;
using global::System;
using Newtonsoft.Json;
using Med.Entity;
using global::System.Linq;
using App.Common.Helpers;
using Med.Common.Enums;
using Med.Service.Background;
using Med.Service.Report;
using Med.Service.Utilities;
using Med.Common;
using Med.ServiceModel.Common;
using Med.Service.Caching;
using Med.Service.Drug;
using Med.ServiceModel.Drug;
using Med.Entity.Drug;
using App.Constants.Enums;

namespace Med.Service.Impl.Background
{
    public class BackgroundService : MedBaseService, IBackgroundService
    {
        public void UpdateDrugExpiredDate4InitInventory(string drugStoreCode, int drugId)
        {
            var noteRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhap>>();
            var noteItems = noteRepo.GetAll().Where(i => i.NhaThuoc_MaNhaThuoc == drugStoreCode && !i.Xoa);
            var receiptNoteId = noteItems.Where(r => r.LoaiXuatNhap_MaLoaiXuatNhap == (int)NoteInOutType.InitialInventory)
               .Select(i => i.MaPhieuNhap).FirstOrDefault();
            if (receiptNoteId <= 0) return;
            var drugRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Thuoc>>();
            var expiredDate = drugRepo.GetAll().Where(i => i.NhaThuoc_MaNhaThuoc == drugStoreCode && i.ThuocId == drugId)
               .Select(i => i.HanDung).FirstOrDefault();
            var noteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();

            noteItemRepo.UpdateMany(i => i.NhaThuoc_MaNhaThuoc == drugStoreCode
                && i.PhieuNhap_MaPhieuNhap == receiptNoteId
                && i.Thuoc_ThuocId == drugId, i => new PhieuNhapChiTiet() { HanDung = expiredDate, RequestUpdateFromBkgService = true });
        }
        
        public void UpdateLastInventoryQuantity4CacheDrugs(string drugStoreCode, int[] drugIds)
        {
            var invService = IoC.Container.Resolve<IInventoryService>();
            //var forceRecache = !MedCacheManager.Instance.HasRemainQuantityCaching(drugStoreCode);
            var inventories = invService.GenerateInventory4Drugs(drugStoreCode, false, false, drugIds);
        }
        public void UpdateExtraInfo4DeliveryNotes(string drugStoreCode, params int[] noteIds)
        {
            LogHelper.Debug("Update real quantity of delivery notes: {0}.", string.Join(",", noteIds));
            IBaseRepository<PhieuXuat> noteRepo = null;
            var notes = _dataFilterService.GetValidDeliveryNotes(drugStoreCode, out noteRepo);
            var noteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
            var drugs = _dataFilterService.GetValidDrugs(drugStoreCode);
            var noteItems = (from n in notes
                                 join ni in noteItemRepo.GetAll() on n.MaPhieuXuat equals ni.PhieuXuat_MaPhieuXuat
                                 join dr in drugs on ni.Thuoc_ThuocId equals dr.ThuocId
                                 where ni.SoLuong > 0 && ni.NhaThuoc_MaNhaThuoc == drugStoreCode
                                    && noteIds.Contains(n.MaPhieuXuat)
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
                                     ReduceQuantity = ni.ReduceQuantity ?? 0
                                 }).ToList();
            if (!noteItems.Any()) return;

            var updateCands = new List<PhieuXuatChiTiet>();
            foreach (var item in noteItems)
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
                    HandledStatusId = (int)NoteItemHandledStatus.None,
                    IsModified = true
                });
            }

            noteItemRepo.UpdateMany(updateCands, d => d.MaPhieuXuatCt, d => d.RetailQuantity,
                d => d.RetailPrice, d => d.ReduceQuantity, d => d.ReduceNoteItemIds, d => d.HandledStatusId,
                d => d.IsModified);

            // Update last delivery price of drugs
            var invRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Inventory>>();
            var drugPrices = noteItems.GroupBy(i => i.DrugId).ToDictionary(i => i.Key,
                i => i.Select(d => d.RetailPrice).OrderByDescending(d => d).First());

            var drugIds = drugPrices.Keys;
            var updateInvs = invRepo.Table.Where(i => i.DrugStoreID == drugStoreCode
                && drugIds.Contains(i.DrugID) && i.RecordStatusID == (byte)RecordStatus.Activated).ToList();
            updateInvs.ForEach(i =>
            {
                if (drugPrices.ContainsKey(i.DrugID))
                {
                    i.LastOutPrice = drugPrices[i.DrugID];
                }
            });
            invRepo.UpdateMany(updateInvs);
            invRepo.Commit();
        }
        public void UpdateExtraInfo4ReceiptNotes(string drugStoreCode, params int[] noteIds)
        {
            LogHelper.Debug("Update real quantity of receipt notes: {0}.", string.Join(",", noteIds));
            IBaseRepository<PhieuNhap> noteRepo = null;
            var notes = _dataFilterService.GetValidReceiptNotes(drugStoreCode, out noteRepo);
            var noteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            var drugs = _dataFilterService.GetValidDrugs(drugStoreCode);
            var noteItems = (from n in notes
                             join ni in noteItemRepo.GetAll() on n.MaPhieuNhap equals ni.PhieuNhap_MaPhieuNhap
                             join dr in drugs on ni.Thuoc_ThuocId equals dr.ThuocId
                             where ni.SoLuong > 0 && ni.NhaThuoc_MaNhaThuoc == drugStoreCode
                                && noteIds.Contains(n.MaPhieuNhap)
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
                             }).ToList();
            if (!noteItems.Any()) return;

            var updateCands = new List<PhieuNhapChiTiet>();
            foreach (var item in noteItems)
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

                updateCands.Add(new PhieuNhapChiTiet()
                {
                    MaPhieuNhapCt = item.NoteItemId,
                    RetailQuantity = item.RetailQuantity,
                    RetailPrice = item.RetailPrice,
                    ReduceQuantity = 0,
                    ReduceNoteItemIds = string.Empty,
                    HandledStatusId = (int)NoteItemHandledStatus.None,
                    IsModified = true,          
                });
            }

            noteItemRepo.UpdateMany(updateCands, d => d.MaPhieuNhapCt, d => d.RetailQuantity,
                d => d.RetailPrice, d => d.ReduceQuantity, d => d.ReduceNoteItemIds, d => d.HandledStatusId,
                d => d.IsModified);

            // Update last receipt price of drugs
            var drugRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Thuoc>>();
            var invRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Inventory>>();
            var drugPrices = noteItems.GroupBy(i => i.DrugId).ToDictionary(i => i.Key, 
                i => i.OrderByDescending(d => d.NoteDate).First());

            var updateDrugs = new List<Thuoc>();
            var drugIds = drugPrices.Keys;
            var updateInvs = invRepo.Table.Where(i => i.DrugStoreID == drugStoreCode
                && drugIds.Contains(i.DrugID) && i.RecordStatusID == (byte)RecordStatus.Activated).ToList();
            updateInvs.ForEach(i =>
            {
                if (drugPrices.ContainsKey(i.DrugID))
                {
                    var drugPrice = drugPrices[i.DrugID];
                    i.LastInPrice = drugPrice.RetailPrice;
                    i.RetailOutPrice = drugPrice.RetailOutPrice;
                    i.RetailBatchOutPrice = drugPrice.RetailBatchOutPrice;
                    updateDrugs.Add(new Thuoc()
                    {
                        ThuocId = i.DrugID,
                        GiaBanLe = (decimal)drugPrice.RetailOutPrice,                      
                        GiaNhap = (decimal)drugPrice.RetailPrice
                    });
                }
            });
            invRepo.UpdateMany(updateInvs);
            invRepo.Commit();
            var dsSettings = IoC.Container.Resolve<IUtilitiesService>().GetDrugStoreSetting(drugStoreCode);
            if (dsSettings != null && dsSettings.AutoUpdateInOutPriceOnNote)
            {
                drugRepo.UpdateMany(updateDrugs, i => i.ThuocId, i => i.GiaNhap, i => i.GiaBanLe);
            }
            // Update drug mapping
            var drugMapRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, DrugMapping>>();
            var drugMaps = drugMapRepo.TableAsNoTracking.Where(i => i.DrugStoreID == drugStoreCode
                && drugIds.Contains(i.SlaveDrugID)).ToList();
            var updateDrugMaps = new List<DrugMapping>();
            drugMaps.ForEach(i =>
            {
                if (drugPrices.ContainsKey(i.SlaveDrugID))
                {
                    var prices = drugPrices[i.SlaveDrugID];
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
        }
    }
}
