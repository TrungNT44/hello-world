using App.Common.DI;
using Hangfire;
using Med.Service.Background;
using Med.Service.Caching;
using Med.Service.Drug;
using Med.Service.Log;
using Med.Web.Data.Session;
using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Collections.Generic;
using Med.Common.Enums;
using Med.Common;
using App.Constants.Enums;
using Med.DbContext;
using Med.Entity;
using App.Common.Data;
using App.Common.Helpers;

namespace Med.Web.Helpers
{
    public static class NoteHelper
    {
        public static void ApplyAdditionalInfos(string drugStoreCode, params sThuoc.Models.PhieuXuatChiTiet[] noteItems)
        {
            if (noteItems == null || !noteItems.Any()) return;

            var drugIds = noteItems.Where(i => i.Thuoc_ThuocId > 0).Select(i => i.Thuoc_ThuocId.Value).Distinct().ToArray();
            var drugs = MedCacheManager.Instance.GetCacheDrugs(drugStoreCode, drugIds)
                .ToDictionary(i => i.DrugId, i => i);

            foreach (var item in noteItems)
            {
                if (item.RecordStatusID == (byte)RecordStatus.Deleted)
                {
                    item.IsModified = true;
                    item.NeedUpdate = item.MaPhieuXuatCt > 0;
                    continue;
                }
                if (item.Original != null)
                {
                    var diffQuantity = Math.Abs((double)(item.Original.SoLuong - item.SoLuong));
                    var diffPrice = Math.Abs((double)(item.Original.GiaXuat - item.GiaXuat));
                    var diffDiscount = Math.Abs((double)(item.Original.ChietKhau - item.ChietKhau));
                    var isNotDiffUnit = item.Original.DonViTinh_MaDonViTinh == item.DonViTinh_MaDonViTinh;
                    if (diffQuantity < MedConstants.EspQuantity
                        && diffPrice < MedConstants.EspPrice
                        && diffDiscount < MedConstants.EspDiscount
                        && isNotDiffUnit) continue;
                }
                if (drugs.ContainsKey(item.Thuoc_ThuocId.Value))
                {
                    var drugItem = drugs[item.Thuoc_ThuocId.Value];
                    item.RetailQuantity = (double)item.SoLuong;
                    item.RetailPrice = (double)item.GiaXuat;
                    var factors = 1.0;
                    if (drugItem.RetailUnitId != item.DonViTinh_MaDonViTinh && drugItem.Factors > 0)
                    {
                        factors = drugItem.Factors;
                    }
                    item.RetailQuantity = item.RetailQuantity * factors;
                    item.RetailPrice = item.RetailPrice / factors;
                    item.IsModified = true;
                    item.ReduceNoteItemIds = string.Empty;
                    item.ReduceQuantity = 0;
                    item.HandledStatusId = (int)NoteItemHandledStatus.None;
                    item.NeedUpdate = item.MaPhieuXuatCt > 0;
                }
            }
        }
        public static void ApplyAdditionalInfos(string drugStoreCode, params sThuoc.Models.PhieuNhapChiTiet[] noteItems)
        {
            if (noteItems == null || !noteItems.Any()) return;

            var drugIds = noteItems.Where(i => i.Thuoc_ThuocId > 0).Select(i => i.Thuoc_ThuocId.Value).Distinct().ToArray();
            var drugs = MedCacheManager.Instance.GetCacheDrugs(drugStoreCode, drugIds)
                .ToDictionary(i => i.DrugId, i => i);

            foreach (var item in noteItems)
            {
                if (item.RecordStatusID == (byte)RecordStatus.Deleted)
                {
                    item.IsModified = true;
                    item.NeedUpdate = item.MaPhieuNhapCt > 0;
                    continue;
                }
                if (item.Original != null)
                {
                    var diffQuantity = Math.Abs((double)(item.Original.SoLuong - item.SoLuong));
                    var diffPrice = Math.Abs((double)(item.Original.GiaNhap - item.GiaNhap));
                    var diffDiscount = Math.Abs((double)(item.Original.ChietKhau - item.ChietKhau));
                    var diffOutPrice = Math.Abs((double)(item.Original.GiaBanLe - item.GiaBanLe));
                    var isNotDiffUnit = item.Original.DonViTinh_MaDonViTinh == item.DonViTinh_MaDonViTinh;
                    if (diffQuantity < MedConstants.EspQuantity
                        && diffPrice < MedConstants.EspPrice
                        && diffOutPrice < MedConstants.EspPrice
                        && diffDiscount < MedConstants.EspDiscount
                        && isNotDiffUnit) continue;
                }
                if (drugs.ContainsKey(item.Thuoc_ThuocId.Value))
                {
                    var drugItem = drugs[item.Thuoc_ThuocId.Value];
                    item.RetailQuantity = (double)item.SoLuong;
                    item.RetailPrice = (double)item.GiaNhap;
                    var factors = 1.0;
                    if (drugItem.RetailUnitId != item.DonViTinh_MaDonViTinh && drugItem.Factors > 0)
                    {
                        factors = drugItem.Factors;
                    }
                    item.RetailQuantity = item.RetailQuantity * factors;
                    item.RetailPrice = item.RetailPrice / factors;
                    item.IsModified = true;
                    item.ReduceNoteItemIds = string.Empty;
                    item.ReduceQuantity = 0;
                    item.RemainRefQuantity = item.RetailQuantity;
                    item.HandledStatusId = (int)NoteItemHandledStatus.None;
                    item.RetailOutPrice = (double)item.GiaBanLe / factors;
                    item.NeedUpdate = item.MaPhieuNhapCt > 0;
                }
            }
        }

        public static void UpdateNoteItems(params sThuoc.Models.PhieuNhapChiTiet[] noteItems)
        {
            if (noteItems == null || !noteItems.Any()) return;
            
            var noteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();      
           
            var updateItems = noteItems.Where(i => i.RecordStatusID == (byte)RecordStatus.Activated).Select(i => new PhieuNhapChiTiet()
            {
                MaPhieuNhapCt = i.MaPhieuNhapCt,
                SoLuong = i.SoLuong,
                GiaNhap = i.GiaNhap,
                SoLo = i.SoLo,
                ChietKhau = i.ChietKhau,
                HanDung = i.HanDung,
                DonViTinh_MaDonViTinh = i.DonViTinh_MaDonViTinh,
                RetailQuantity = i.RetailQuantity,
                RetailPrice = i.RetailPrice,
                IsModified = true,
                ReduceNoteItemIds = string.Empty,
                ReduceQuantity = 0,
                HandledStatusId = (int)NoteItemHandledStatus.None,
                RemainRefQuantity = i.RetailQuantity,
                RecordStatusID = i.RecordStatusID,
                RetailOutPrice = i.RetailOutPrice
            });
            var deletedItems = noteItems.Where(i => i.RecordStatusID == (byte)RecordStatus.Deleted).Select(i => new PhieuNhapChiTiet()
            {
                MaPhieuNhapCt = i.MaPhieuNhapCt,
                RecordStatusID = i.RecordStatusID
            });
            
            //using (var trans = TransactionScopeHelper.CreateReadCommittedForWrite())
            {
                noteItemRepo.UpdateMany(updateItems,
                i => i.SoLuong, i => i.SoLo, i => i.GiaNhap, i => i.ChietKhau, i => i.HanDung,
                i => i.DonViTinh_MaDonViTinh, i => i.RetailQuantity, i => i.RetailPrice,
                i => i.IsModified, i => i.ReduceNoteItemIds, i => i.ReduceQuantity,
                i => i.HandledStatusId, i => i.RemainRefQuantity, i => i.RecordStatusID, i => i.RetailOutPrice);
                noteItemRepo.UpdateMany(deletedItems, i => i.RecordStatusID);
            }
        }

        public static void UpdateNoteItems(params sThuoc.Models.PhieuXuatChiTiet[] noteItems)
        {
            if (noteItems == null || !noteItems.Any()) return;

            var noteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();

            var updateItems = noteItems.Where(i => i.RecordStatusID == (byte)RecordStatus.Activated).Select(i => new PhieuXuatChiTiet()
            {
                MaPhieuXuatCt = i.MaPhieuXuatCt,
                SoLuong = i.SoLuong,
                GiaXuat = i.GiaXuat,
                ChietKhau = i.ChietKhau,
                DonViTinh_MaDonViTinh = i.DonViTinh_MaDonViTinh,
                RetailQuantity = i.RetailQuantity,
                RetailPrice = i.RetailPrice,
                IsModified = true,
                ReduceNoteItemIds = string.Empty,
                ReduceQuantity = 0,
                HandledStatusId = (int)NoteItemHandledStatus.None,
                RecordStatusID = i.RecordStatusID
            });
            var deletedItems = noteItems.Where(i => i.RecordStatusID == (byte)RecordStatus.Deleted).Select(i => new PhieuXuatChiTiet()
            {
                MaPhieuXuatCt = i.MaPhieuXuatCt,
                RecordStatusID = i.RecordStatusID
            });
            //using (var trans = TransactionScopeHelper.CreateReadCommittedForWrite())
            {
                noteItemRepo.UpdateMany(updateItems,
                i => i.SoLuong, i => i.GiaXuat, i => i.ChietKhau,
                i => i.DonViTinh_MaDonViTinh, i => i.RetailQuantity, i => i.RetailPrice,
                i => i.IsModified, i => i.ReduceNoteItemIds, i => i.ReduceQuantity,
                i => i.HandledStatusId, i => i.RecordStatusID);
                noteItemRepo.UpdateMany(deletedItems, i => i.RecordStatusID);
            }
        }
    }
}