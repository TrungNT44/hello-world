using App.Common.DI;
using Hangfire;
using Med.Service.Background;
using Med.Service.Caching;
using System;
using System.Linq;
using System.Collections.Generic;
using Med.Entity;
using Med.Common.Enums;

namespace Med.Service.Helpers
{
    public static class NoteServiceHelper
    {
        public static void ApplyAdditionalInfos(params PhieuXuatChiTiet[] noteItems)
        {
            if (noteItems == null || !noteItems.Any()) return;

            var drugStoreCode = noteItems[0].NhaThuoc_MaNhaThuoc;
            var drugIds = noteItems.Select(i => i.Thuoc_ThuocId.Value).Distinct().ToArray();
            var drugs = MedCacheManager.Instance.GetCacheDrugs(drugStoreCode, drugIds)
                .ToDictionary(i => i.DrugId, i => i);

            foreach (var item in noteItems)
            {
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
                }
            }
        }

        public static void ApplyAdditionalInfos(params PhieuNhapChiTiet[] noteItems)
        {
            if (noteItems == null || !noteItems.Any()) return;

            var drugStoreCode = noteItems[0].NhaThuoc_MaNhaThuoc;
            var drugIds = noteItems.Select(i => i.Thuoc_ThuocId.Value).Distinct().ToArray();
            var drugs = MedCacheManager.Instance.GetCacheDrugs(drugStoreCode, drugIds)
                .ToDictionary(i => i.DrugId, i => i);

            foreach (var item in noteItems)
            {
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
                    item.ReduceQuantity = 0;
                    item.ReduceNoteItemIds = string.Empty;
                    item.RemainRefQuantity = item.RetailQuantity;
                    item.HandledStatusId = (int)NoteItemHandledStatus.None;
                    item.RetailOutPrice = (double)item.GiaBanLe / factors;
                }
            }
        }
    }
}