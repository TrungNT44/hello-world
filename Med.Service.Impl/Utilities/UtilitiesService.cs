using Med.Service.Base;
using Med.Service.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Med.Entity;
using Med.ServiceModel.Response;
using Med.ServiceModel.Utilities;
using App.Common.Data;
using Med.DbContext;
using Med.Repository.Factory;
using Med.ServiceModel.Common;
using Med.ServiceModel.Report;
using Med.Common.Enums;
using App.Common.Extensions;
using Med.Service.Delivery;
using App.Common.DI;
using Castle.Core.Internal;
using Med.Common;
using Med.Service.Drug;
using App.Common.Helpers;

namespace Med.Service.Impl.Utilities
{
    public class UtilitiesService : MedBaseService, IUtilitiesService
    {
        public CanhBaoHetHanResponse CanhBaoHangHetHan(string sMaNhaThuoc,string sKeySettingNumExpireDay, string sKeySettingNumDayNoTrans ,string sType, int? sNhomThuocId, string sMaThuoc)
        {
            List<CanhBaoHetHanItem> lstCanhBaoHetHan = new List<CanhBaoHetHanItem>();
            CanhBaoHetHanResponse oRes = new CanhBaoHetHanResponse()
            {
                Total = 0
            };
            var dugsQ = _dataFilterService.GetValidDrugs(sMaNhaThuoc);
            switch (sType)
            {
                case "2":
                    {
                        dugsQ = dugsQ.Where(x => x.NhomThuoc_MaNhomThuoc == sNhomThuocId.Value);
                    }
                    break;
                case "3":
                    {
                        dugsQ = dugsQ.Where(x => x.MaThuoc == sMaThuoc);
                    }
                    break;
            }
            var lstdrugInfo = dugsQ.Select(x => new
            {
                x.ThuocId,
                x.MaThuoc,
                x.TenThuoc,
                //x.DonViTinh1.TenDonViTinh
            }).ToList();
            var arrDrugId = lstdrugInfo.Select(x => x.ThuocId).ToArray();
            var receiptItemsQ = _dataFilterService.GetValidReceiptNoteItems(sMaNhaThuoc);
            
            var receiptItemsVali = (from ri in receiptItemsQ
                                    where ri.RemainRefQuantity > 0 && arrDrugId.Contains(ri.DrugId) && (!ri.ExpiredDate.HasValue || (ri.ExpiredDate.HasValue && ri.ExpiredDate > Med.Common.MedConstants.MinProductionDataDate))
                                    // group ri by new { ri.Thuoc_ThuocId, ri.HanDung }
                                    //into g
                                    // orderby g.Key
                                    select new
                                    {
                                        DrugId = ri.DrugId,
                                        MaxHD = ri.ExpiredDate,
                                        tonkho = ri.RemainRefQuantity,
                                        maphieunhap = ri.NoteId,
                                        ngayphieunhap = ri.NoteDate,
                                        sophieunhap = ri.NoteNumber,
                                        idphieunhap = ri.NoteItemId,
                                        solo = ri.SerialNumber
                                    }).ToList();
            if (receiptItemsVali.Count > 0)
            {
                //Kiểm tra các thuốc xem thuốc nào ít giao dịch
                //Ít giao dịch thì hiển thị (ko quan tâm hạn dùng).
                //Hạn dùng thuộc diện cảnh báo hiển thị (kể cả ít giao dịch).
                var arrDrugIdVali = receiptItemsVali.Select(x => x.DrugId).Distinct().ToArray();
                var deliveryItemQ = _dataFilterService.GetValidDeliveryNoteItems(sMaNhaThuoc, new FilterObject() { DrugIds = arrDrugIdVali });
                var newDateTrans = (from pn in (from ri in receiptItemsQ
                                                where arrDrugIdVali.Contains(ri.DrugId)
                                                group ri by ri.DrugId
                                           into g
                                                orderby g.Key
                                                select new
                                                {
                                                    drugId = g.Key,
                                                    MaxNgayNhap = g.Max(x => x.NoteDate)
                                                })
                                    join px in (
                                     from deli in deliveryItemQ
                                     where arrDrugIdVali.Contains(deli.DrugId)
                                     group deli by deli.DrugId
                                     into g
                                     orderby g.Key
                                     select new
                                     {
                                         drugId = g.Key,
                                         MaxNgayXuat = g.Max(x => x.NoteDate)
                                     }
                                ) on pn.drugId equals px.drugId into pnpx_join
                                    from pnpx in pnpx_join.DefaultIfEmpty()
                                    select new
                                    {
                                        drugId = pn.drugId,
                                        newInputDate = pn.MaxNgayNhap,
                                        newOutputDate = pnpx.MaxNgayXuat
                                    }).ToList();
                var numDayNoTrans = int.Parse("0" + _dataFilterService.GetValidSetting(sMaNhaThuoc, sKeySettingNumDayNoTrans).FirstOrDefault().Value);
                double diffInput, diffOutput;
                List<int> drugIdLittleTrans = new List<int>();
                foreach (var item in receiptItemsVali)
                {
                    var tempItem = newDateTrans.Where(x => x.drugId == item.DrugId).FirstOrDefault();
                    if (tempItem != null)
                    {
                        diffInput = Math.Abs((DateTime.Now.Date - tempItem.newInputDate.Value.Date).TotalDays);
                        diffOutput = !tempItem.newOutputDate.HasValue ? -1 : Math.Abs((DateTime.Now.Date - tempItem.newOutputDate.Value.Date).TotalDays);
                        //if (diffInput >= numDayNoTrans || diffOutput >= numDayNoTrans)
                        if (diffInput >= numDayNoTrans)
                        {
                            drugIdLittleTrans.Add(item.DrugId);
                        }
                    }
                }
                var lstSetting = _dataFilterService.GetValidSetting(sMaNhaThuoc, sKeySettingNumExpireDay).FirstOrDefault();
                if (lstSetting != null)
                {
                    int numExpireDay = int.Parse("0" + lstSetting.Value.Trim());
                    var oCanhBaoHetHanItem = new CanhBaoHetHanItem();
                    //int stt = 1;
                    foreach (var pnct in receiptItemsVali)
                    {

                        if (!pnct.MaxHD.HasValue)
                        {
                            //Nếu ko là hàng ít giao dịch thì bo qua
                            if (!drugIdLittleTrans.Contains(pnct.DrugId))
                            {
                                continue;
                            }

                        }
                        else
                        {
                            //Nếu ko là hàng ít giao dịch thì xét quá hạn
                            double Totaldays = (pnct.MaxHD.Value - DateTime.Now.Date).TotalDays;
                            if (Totaldays > numExpireDay)
                            {
                                continue;
                            }

                        }
                       
                        var dug = lstdrugInfo.Where(x => x.ThuocId == pnct.DrugId).FirstOrDefault();
                        double diffNumber = Math.Abs((DateTime.Now.Date - pnct.ngayphieunhap.Value.Date).TotalDays);
                        oCanhBaoHetHanItem = new CanhBaoHetHanItem()
                        {
                            ThuocId = pnct.DrugId,
                            MaThuoc = dug.MaThuoc,
                            TenThuoc = dug.TenThuoc,
                            HangItGiaoDich = drugIdLittleTrans.Contains(pnct.DrugId) ? diffNumber.ToString("#,##0") : "",
                            SoLuong = pnct.tonkho.ToString("#,##0"),
                            //DonVi = dug.TenDonViTinh,
                            Han = pnct.MaxHD.HasValue ? pnct.MaxHD.Value.ToString("dd/MM/yyyy") : "",
                            MaPhieuNhap = pnct.maphieunhap.ToString(),
                            SoPhieuNhap = pnct.sophieunhap.ToString(),
                            NgayPhieuNhap = pnct.ngayphieunhap.Value.ToString("dd/MM/yyyy"),
                            Solo = pnct.solo,
                            IdPhieuNhap = pnct.idphieunhap.ToString()
                            //NgayPhieuNhap = pnct.ngayphieunhap.HasValue ? pnct.ngayphieunhap.Value.ToString("dd/MM/yyyy") : ""
                        };
                        lstCanhBaoHetHan.Add(oCanhBaoHetHanItem);
                    }
                }
            }
            lstCanhBaoHetHan = lstCanhBaoHetHan.OrderBy(x => x.TenThuoc).ToList();
            for(int i= 0;i< lstCanhBaoHetHan.Count;i++)
            {
                lstCanhBaoHetHan[i].STT = i + 1;
            }
            oRes.PagingResultModel = new PagingResultModel<CanhBaoHetHanItem>(lstCanhBaoHetHan, lstCanhBaoHetHan.Count);
            return oRes;
        }

        protected class NegativeRevenueInfo
        {
            public int DrugId { get; set; }
            public int DeliveryNoteItemId { get; set; }
            public int ReceiptNoteItemId { get; set; }
            public double Revenue { get; set; }
            public int ReceiptNoteNumber { get; set; }
            public int DeliveryNoteNumber { get; set; }
            public int ReceiptNoteId { get; set; }
            public int DeliveryNoteId { get; set; }
        }

        public NegativeRevenueResponse GetNegativeRevenueWarningData(string drugStoreCode, FilterObject filter)
        {
            GenerateReportData(drugStoreCode);
            var result = new NegativeRevenueResponse();
            var resultItems = new List<NegativeRevenueItem>();
            var totalCount = 0;
            using (var trans = TransactionScopeHelper.CreateReadUncommitted())
            {
                var deliverySnapshots = _dataFilterService.GetValidDeliveryItemSnapshots(drugStoreCode, filter);
                var deliveryItems = _dataFilterService.GetValidDeliveryNoteItems(drugStoreCode, null);
                var receiptItems = _dataFilterService.GetValidReceiptNoteItems(drugStoreCode, null);
                var priceRefItems = _dataFilterService.GetValidReceiptDrugPriceRefs(drugStoreCode, null);
                var drugs = _dataFilterService.GetValidDrugs(drugStoreCode, filter, false);
                var negCands = (from s in deliverySnapshots
                                join p in priceRefItems on s.DeliveryNoteItemId equals p.DeliveryNoteItemId
                                where s.NegativeRevenue == true
                                select
                                new NegativeRevenueInfo
                                {
                                    DrugId = s.DrugId,
                                    DeliveryNoteItemId = s.DeliveryNoteItemId,
                                    ReceiptNoteItemId = p.ReceiptNoteItemId,
                                    Revenue = s.Revenue
                                }).ToList();
                var cands = negCands.GroupBy(i => i.DrugId).ToDictionary(i => i.Key, i => i.ToList());
                var deliveryDrugIds = cands.Select(i => i.Key).ToList();
                totalCount = deliveryDrugIds.Count();
                if (totalCount < 1)
                {
                    trans.Complete();
                    result.PagingResultModel = new PagingResultModel<NegativeRevenueItem>(resultItems, totalCount);
                    return result;
                }
                var deliveryNoteItemIds = negCands.Select(i => i.DeliveryNoteItemId).Distinct().ToList();
                var receiptNoteItemIds = negCands.Select(i => i.ReceiptNoteItemId).Distinct().ToList();

                var deliveryItemInfos = deliveryItems.Where(i => deliveryNoteItemIds.Contains(i.NoteItemId))
                    .Select(i => new { i.NoteId, i.NoteNumber, i.NoteItemId })
                    .ToList().GroupBy(i => i.NoteItemId).ToDictionary(i => i.Key, i => i.FirstOrDefault());
                var receiptItemInfos = receiptItems.Where(i => receiptNoteItemIds.Contains(i.NoteItemId))
                    .Select(i => new { i.NoteId, i.NoteNumber, i.NoteItemId })
                    .ToList().GroupBy(i => i.NoteItemId).ToDictionary(i => i.Key, i => i.FirstOrDefault());
                negCands.ForEach(i =>
                {
                    if (deliveryItemInfos.ContainsKey(i.DeliveryNoteItemId))
                    {
                        i.DeliveryNoteId = deliveryItemInfos[i.DeliveryNoteItemId].NoteId;
                        i.DeliveryNoteNumber = (int)deliveryItemInfos[i.DeliveryNoteItemId].NoteNumber;
                    }
                    if (receiptItemInfos.ContainsKey(i.ReceiptNoteItemId))
                    {
                        i.ReceiptNoteId = receiptItemInfos[i.ReceiptNoteItemId].NoteId;
                        i.ReceiptNoteNumber = (int)receiptItemInfos[i.ReceiptNoteItemId].NoteNumber;
                    }
                });
                var drugInfos = drugs.Where(i => deliveryDrugIds.Contains(i.ThuocId))
                    .Select(i => new { DrugId = i.ThuocId, DrugCode = i.MaThuoc, DrugName = i.TenThuoc })
                    .ToDictionary(i => i.DrugId, i => i);
                trans.Complete();

                deliveryDrugIds.ForEach(i =>
                {
                    if (drugInfos.ContainsKey(i))
                    {
                        var subCands = cands[i];
                        var drug = drugInfos[i];
                        var firstCand = subCands.FirstOrDefault();
                        var drugDeliveryNoteItemIds = subCands.Select(ii => ii.DeliveryNoteItemId).ToList();
                        var deliveryNumberPairs = deliveryItemInfos.Where(ii => drugDeliveryNoteItemIds.Contains(ii.Key))
                            .Select(ii => new NoteNumberIdPair()
                            {
                                NoteId = ii.Value.NoteId,
                                NoteNumber = ii.Value.NoteNumber,
                            }).DistinctBy(ii => ii.NoteId).ToList();
                        var drugReceiptNoteItemIds = subCands.Select(ii => ii.ReceiptNoteItemId).ToList();
                        var receiptNoteNumbers = receiptItemInfos.Where(ii => drugReceiptNoteItemIds.Contains(ii.Key))
                            .Select(ii => new NoteNumberIdPair()
                            {
                                NoteId = ii.Value.NoteId,
                                NoteNumber = ii.Value.NoteNumber
                            }).DistinctBy(ii => ii.NoteId).ToList();
                        var deliveryNoteNumbers = deliveryNumberPairs.Select(ii =>
                            new DeliveryNoteNumberPair()
                            {
                                DeliveryNumber = ii
                            }).ToList();
                        deliveryNoteNumbers.ForEach(ii =>
                        {
                            var refReceiptNoteIds = subCands.Where(c => c.DeliveryNoteId == ii.DeliveryNumber.NoteId)
                                .Select(c => c.ReceiptNoteId).Distinct().ToList();
                            if (refReceiptNoteIds.Any())
                            {
                                ii.RefReceiptNumbers = receiptNoteNumbers.Where(c => refReceiptNoteIds.Contains(c.NoteId))
                                    .ToList();
                            }
                        });
                        var revItem = new NegativeRevenueItem()
                        {
                            ItemCode = drug.DrugCode,
                            ItemName = drug.DrugName,
                            Amount = firstCand.Revenue,
                            DeliveryNoteNumbers = deliveryNoteNumbers
                        };
                        resultItems.Add(revItem);
                    }
                });
            }
            result.PagingResultModel = new PagingResultModel<NegativeRevenueItem>(resultItems, totalCount);

            return result;
        }

        public List<RemainQuantityReceiptDrugItem> GetRemainQuantityReceiptDrugItems(string drugStoreCode, DrugStoreSetting setting)
        {            
            var noteTypeIds = new int[] { (int)NoteInOutType.Receipt, (int)NoteInOutType.InitialInventory };
            var receiptItems = _dataFilterService.GetValidReceiptNoteItems(drugStoreCode, null, noteTypeIds);
            var drugs = _dataFilterService.GetValidDrugs(drugStoreCode, null, false);

            var remainQuantityReceiptItems = (from ri in receiptItems
                                              join d in drugs on ri.DrugId equals d.ThuocId
                                              where ri.RemainRefQuantity > MedConstants.EspQuantity
                                              select new
                                              {
                                                  ri,
                                                  d.Created
                                              });

            var receiptNoteTypeId = (int)NoteInOutType.Receipt;
            var candidates = (from ri in remainQuantityReceiptItems
                              select
                              new RemainQuantityReceiptDrugItem
                              {
                                  NoteId = ri.ri.NoteType == receiptNoteTypeId ? ri.ri.NoteId : 0,
                                  NoteItemId = ri.ri.NoteItemId,
                                  SerialNumber = ri.ri.SerialNumber ?? string.Empty,
                                  ItemId = ri.ri.DrugId,
                                  ItemName = ri.ri.DrugName,
                                  ItemCode = ri.ri.DrugCode,
                                  ItemDate = ri.ri.NoteDate == MedConstants.MinProductionDataDate ? ri.Created : ri.ri.NoteDate,
                                  ItemNumber = ri.ri.NoteNumber.ToString(),
                                  Quantity = ri.ri.RemainRefQuantity,
                                  ExpiredDate = ri.ri.ExpiredDate,
                                  UnitName = ri.ri.UnitName,
                                  NoteTypeID = ri.ri.NoteType
                              }).ToList();
            var currDate = DateTime.Now;
            var numDaysNoTrans = setting.NumberDaysNoTransaction;
            var numExpiredDays = setting.NumberDaysNearExpiredDate;
            candidates.ForEach(i =>
            {                
                i.NonTransNumDays = Math.Abs((currDate - i.ItemDate.Value.Date).TotalDays);
                if (i.NonTransNumDays >= numDaysNoTrans && !i.ExpiredDate.HasValue)
                {
                    i.IsLittleTrans = true;
                }
                if (i.ExpiredDate.HasValue)
                {
                    i.ExpiredNumDays = (i.ExpiredDate.Value.Date - currDate).TotalDays;
                    i.IsExpired = (i.ExpiredDate.Value.Date - currDate).TotalDays < numExpiredDays;
                }               
            });

            return candidates;
        }
        public NearExpiredDrugResponse GetNearExpiredDrugWarningData(string drugStoreCode, DrugStoreSetting setting, FilterObject filter, int expiredOption = (int)ExpiredFilterType.OnlyExpired)
        {
            GenerateReportData(drugStoreCode);
            var result = new NearExpiredDrugResponse();
            using (var trans = TransactionScopeHelper.CreateReadUncommitted())
            {
                var resultItems = GetRemainQuantityReceiptDrugItems(drugStoreCode, setting);
                var totalCount = resultItems.Count;
                if (totalCount < 1)
                {
                    trans.Complete();
                    result.PagingResultModel = new PagingResultModel<RemainQuantityReceiptDrugItem>(resultItems, totalCount);
                    return result;
                }
                var drugIds = resultItems.Select(i => i.ItemId).Distinct().ToArray();
                var drugService = IoC.Container.Resolve<IDrugManagementService>();
                var drugsDict = drugService.GetCacheDrugs(drugStoreCode, drugIds).ToDictionary(i => i.DrugId, i => i);
                trans.Complete();

                resultItems.ForEach(i =>
                {
                    if (drugsDict.ContainsKey(i.ItemId))
                    {
                        var drugItem = drugsDict[i.ItemId];
                        var retailUnit = drugItem.Units.Where(u => u.UnitId == drugItem.RetailUnitId).FirstOrDefault();
                        if (retailUnit != null)
                        {
                            i.UnitName = retailUnit.UnitName;
                        }
                        if (i.NoteTypeID == (int)NoteInOutType.InitialInventory)
                        {
                            i.ItemDate = drugItem.CreatedDateTime;
                        }
                    }
                });

                if (expiredOption == (int)ExpiredFilterType.OnlyExpired)
                {
                    resultItems = resultItems.Where(i => i.IsExpired || i.IsLittleTrans).ToList();
                }
                else if (expiredOption == (int)ExpiredFilterType.All)
                {
                    resultItems = resultItems.Where(i => i.ExpiredDate.HasValue).ToList();
                }

                totalCount = resultItems.Count();

                result.PagingResultModel = new PagingResultModel<RemainQuantityReceiptDrugItem>(resultItems, totalCount);
            }            

            return result;
        }
        public DrugStoreSetting GetDrugStoreSetting(string drugStoreCode)
        {
            var retVal = new DrugStoreSetting();
            var settings = _dataFilterService.GetValidSetting(drugStoreCode).ToList();
            var oneSetting = settings.FirstOrDefault(i => i.Key == MedSettingKey.AutoCreateBarcodeKey);
            if (oneSetting != null)
            {
                var autoCreateBarcode = false;
                if (oneSetting.Value.ToUpper() == MedConstants.YesValue)
                {
                    autoCreateBarcode = true;
                }
                //bool.TryParse(oneSetting.Value, out autoCreateBarcode);
                retVal.AutoCreateBarcode = autoCreateBarcode;
            }

            oneSetting = settings.FirstOrDefault(i => i.Key == MedSettingKey.AutoCreateDrugCodeKey);
            if (oneSetting != null)
            {
                var autoCreateDrugCode = false;
                if (oneSetting.Value.ToUpper() == MedConstants.YesValue)
                {
                    autoCreateDrugCode = true;
                }
                // bool.TryParse(oneSetting.Value, out autoCreateDrugCode);
                retVal.AutoCreateDrugCode = autoCreateDrugCode;
            }

            oneSetting = settings.FirstOrDefault(i => i.Key == MedSettingKey.AutoUpdateInOutPriceOnNoteKey);
            if (oneSetting != null)
            {
                var autoUpdateInOutPriceOnNote = false;
                if (oneSetting.Value.ToUpper() == MedConstants.YesValue)
                {
                    autoUpdateInOutPriceOnNote = true;
                }
                //bool.TryParse(oneSetting.Value, out autoUpdateInOutPriceOnNote);
                retVal.AutoUpdateInOutPriceOnNote = autoUpdateInOutPriceOnNote;
            }

            oneSetting = settings.FirstOrDefault(i => i.Key == MedSettingKey.NumberDaysNearExpiredDateKey);
            if (oneSetting != null)
            {
                var numberDaysNearExpiredDate = 0;
                int.TryParse(oneSetting.Value, out numberDaysNearExpiredDate);
                retVal.NumberDaysNearExpiredDate = numberDaysNearExpiredDate;
            }

            oneSetting = settings.FirstOrDefault(i => i.Key == MedSettingKey.NumberDaysNoTransactionKey);
            if (oneSetting != null)
            {
                var numberDaysNoTransaction = 0;
                int.TryParse(oneSetting.Value, out numberDaysNoTransaction);
                retVal.NumberDaysNoTransaction = numberDaysNoTransaction;
            }

            oneSetting = settings.FirstOrDefault(i => i.Key == MedSettingKey.AllowToChangeTotalAmountInDeliveryNoteKey);
            if (oneSetting != null)
            {
                bool allowToChangeTotalAmount = false;
                if (oneSetting.Value.ToUpper() == MedConstants.YesValue)
                {
                    allowToChangeTotalAmount = true;
                }
                //bool.TryParse(oneSetting.Value, out allowToChangeTotalAmount);
                retVal.AllowToChangeTotalAmount = allowToChangeTotalAmount;
            }

            return retVal;
        }
    }  
}
