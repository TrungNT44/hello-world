using System;
using System.Collections.Generic;
using System.Linq;
using App.Common.Data;
using App.Common.Utility;
using Med.DbContext;
using Med.Entity;
using Med.Repository.Factory;
using Med.Service.Base;
using Med.Service.Report;
using Med.ServiceModel.Common;
using Med.ServiceModel.Report;
using App.Common.DI;

namespace Med.Service.Impl.Report
{
    public class CustomizeReportService : MedBaseService, ICustomizeReportService
    {
        public CustomizeReportItemResponse GetCustomizeReportItems(string drugStoreCode, FilterObject filter)
        {
            var customizeReportItems = new List<CustomizeReportItem>();
            var customizeReportItemResponse = new CustomizeReportItemResponse();

            IBaseRepository<PhieuXuat> deliveryNoteRepo = null;
            var deliveryNotes =
                _dataFilterService.GetValidDeliveryNotes(drugStoreCode, out deliveryNoteRepo, filter)
                    .Where(x => x.BacSy_MaBacSy != null);

            var deliveryDetailItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
            var drugs = _dataFilterService.GetValidDrugs(drugStoreCode);
            var deliveryDetails = deliveryDetailItemRepo.Find(x => x.NhaThuoc_MaNhaThuoc.Equals(drugStoreCode));
            var customerRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, KhachHang>>();
            var doctorRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, BacSy>>();
            var customers = customerRepo.Find(x => x.MaNhaThuoc.Equals(drugStoreCode));
            var doctors = doctorRepo.Find(x => x.MaNhaThuoc.Equals(drugStoreCode));

            var generalCustomizeDrugItems = new Dictionary<int, CustomizeDrugItem>();
            foreach (var drug in drugs.OrderBy(x => x.TenThuoc))
            {
                if (generalCustomizeDrugItems.ContainsKey(drug.ThuocId)) continue;
                var customizeDrugItem = new CustomizeDrugItem
                {
                    Id = drug.ThuocId,
                    Code = drug.MaThuoc,
                    Name = drug.TenThuoc,
                    Quantity = 0,
                    PriceAfterDiscount = 0
                };
                generalCustomizeDrugItems.Add(drug.ThuocId, customizeDrugItem);
            }

            var duplicateResult = (from px in deliveryNotes
                                   group px by
                                       new
                                       {
                                           px.BacSy_MaBacSy,
                                           px.KhachHang_MaKhachHang,
                                           px.NgayXuat.Value.Year,
                                           px.NgayXuat.Value.Month,
                                           px.NgayXuat.Value.Day
                                       }
                                       into grp
                                   where grp.Count() > 1
                                   select grp.Key);
            foreach (var deiveryNote in deliveryNotes)
            {
                var customizeReportItem = new CustomizeReportItem();
                customizeReportItem.DeliveryId = deiveryNote.MaPhieuXuat;
                customizeReportItem.CustomizeDrugItems = new Dictionary<int, CustomizeDrugItem>();
                customizeReportItem.CustomizeDrugItems = generalCustomizeDrugItems.DeepCopyByExpressionTree();

                customizeReportItem.DoctorId = deiveryNote.BacSy_MaBacSy;
                customizeReportItem.CustomerId = deiveryNote.KhachHang_MaKhachHang;

                var customer = customers.FirstOrDefault(x => x.MaKhachHang == deiveryNote.KhachHang_MaKhachHang);
                customizeReportItem.CustomerName = customer != null ? customer.TenKhachHang : "";
                customizeReportItem.CustomerAddress = customer != null ? customer.DiaChi : "";

                var doctor = doctors.FirstOrDefault(x => x.MaBacSy == deiveryNote.BacSy_MaBacSy);
                customizeReportItem.DoctorName = doctor != null ? doctor.TenBacSy : "";

                customizeReportItem.ExportDate = deiveryNote.NgayXuat;

                bool bNew = false;
                //check duplicate before adding to the list
                var matched = duplicateResult.FirstOrDefault(
                    x =>
                        x.BacSy_MaBacSy == customizeReportItem.DoctorId &&
                        x.KhachHang_MaKhachHang == customizeReportItem.CustomerId &&
                        x.Year == customizeReportItem.ExportDate.Value.Year &&
                        x.Month == customizeReportItem.ExportDate.Value.Month &&
                        x.Day == customizeReportItem.ExportDate.Value.Day);

                if (matched == null)
                {
                    bNew = true;
                }
                else
                {
                    //check existing in report table
                    var existed = customizeReportItems.FirstOrDefault(
                        x =>
                            x.ExportDate != null &&
                            (customizeReportItem.ExportDate != null &&
                             (x.DoctorId == customizeReportItem.DoctorId &&
                              x.CustomerId == customizeReportItem.CustomerId &&
                              x.ExportDate.Value.Year == customizeReportItem.ExportDate.Value.Year &&
                              x.ExportDate.Value.Month == customizeReportItem.ExportDate.Value.Month &&
                              x.ExportDate.Value.Day == customizeReportItem.ExportDate.Value.Day)));
                    if (existed == null)
                    {
                        bNew = true;
                    }
                    else
                    {
                        //summary quantity and price after discount
                        var deliveryDetail = deliveryDetails.Where(x => x.PhieuXuat_MaPhieuXuat == customizeReportItem.DeliveryId);
                        foreach (var itemDetail in deliveryDetail)
                        {
                            if (itemDetail.Thuoc_ThuocId.HasValue)
                            {
                                existed.CustomizeDrugItems[itemDetail.Thuoc_ThuocId.Value].Quantity += itemDetail.SoLuong;
                                existed.CustomizeDrugItems[itemDetail.Thuoc_ThuocId.Value].PriceAfterDiscount += itemDetail.GiaXuat * (1 - itemDetail.ChietKhau / 100);
                            }
                        }
                    }
                }

                if (bNew)
                {
                    var deliveryDetail = deliveryDetails.Where(x => x.PhieuXuat_MaPhieuXuat == customizeReportItem.DeliveryId);
                    foreach (var itemDetail in deliveryDetail)
                    {
                        if (itemDetail.Thuoc_ThuocId.HasValue)
                        {
                            customizeReportItem.CustomizeDrugItems[itemDetail.Thuoc_ThuocId.Value].Quantity += itemDetail.SoLuong;
                            customizeReportItem.CustomizeDrugItems[itemDetail.Thuoc_ThuocId.Value].PriceAfterDiscount += itemDetail.GiaXuat * (1 - itemDetail.ChietKhau / 100);
                        }
                    }
                    customizeReportItems.Add(customizeReportItem);
                }
            }

            foreach (var drugKey in generalCustomizeDrugItems.Keys)
            {
                decimal totalQuantity = 0;
                foreach (var customizeReportItem in customizeReportItems)
                {
                    if (customizeReportItem.CustomizeDrugItems.ContainsKey(drugKey))
                    {
                        totalQuantity += customizeReportItem.CustomizeDrugItems[drugKey].Quantity;
                    }
                }
                if (totalQuantity == 0)
                {
                    //drug should be remove from reports
                    foreach (var customizeReportItem in customizeReportItems)
                    {
                        customizeReportItem.CustomizeDrugItems.Remove(drugKey);
                    }
                }
                else
                    customizeReportItemResponse.DrugNames.Add(generalCustomizeDrugItems[drugKey].Name);
            }

            foreach (var customizeReportItem in customizeReportItems)
            {
                customizeReportItem.DrugQuantityAndPrice = new List<decimal>();
                foreach (var drugKey in customizeReportItem.CustomizeDrugItems.Keys)
                {
                    customizeReportItem.DrugQuantityAndPrice.Add(customizeReportItem.CustomizeDrugItems[drugKey].Quantity);
                    customizeReportItem.DrugQuantityAndPrice.Add(customizeReportItem.CustomizeDrugItems[drugKey].PriceAfterDiscount);
                }
            }

            customizeReportItemResponse.CustomizeReportItems = new List<CustomizeReportItem>();
            customizeReportItemResponse.CustomizeReportItems = customizeReportItems;

            var totalTable = new decimal[customizeReportItemResponse.DrugNames.Count() * 2];
            for (var i = 0; i < totalTable.Count(); i++)
            {
                totalTable[i] = 0;
            }
            foreach (var customizeReportItem in customizeReportItems)
            {
                for (var i = 0; i < customizeReportItem.DrugQuantityAndPrice.Count; i++)
                {
                    totalTable[i] += customizeReportItem.DrugQuantityAndPrice[i];
                }
            }

            customizeReportItemResponse.TotalQuantityAndPrice = totalTable.ToList();

            return customizeReportItemResponse;
        }
    }
}
