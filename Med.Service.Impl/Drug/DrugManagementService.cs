using System.Collections.Generic;
using System.Linq;
using System.Text;
using App.Common;
using App.Common.FaultHandling;
using App.Common.Helpers;
using Med.Common.Enums;
using Med.DbContext;
using Med.Entity;
using Med.Repository.Factory;
using Med.Service.Base;
using Med.Service.Drug;
using App.Common.Validation;
using App.Common.Data;
using Med.ServiceModel.CacheObjects;
using Med.ServiceModel.Drug;
using System;
using Med.ServiceModel.Common;
using Med.Service.Impl.Report;
using Med.Common;
using Med.ServiceModel.Delivery;
using Castle.Core.Internal;
using App.Common.DI;
using Med.Entity.Report;
using System.Data.Entity;
using Med.Service.System;

namespace Med.Service.Impl.Drug
{
    public class DrugManagementService : MedBaseService, IDrugManagementService
    {
        #region Fields
        #endregion

        #region Interface Implementation

        private string GetDeleteDrugsCommands(string drugStoreCode, params int[] drugIds)
        {
            if (drugIds == null || !drugIds.Any()) return string.Empty;

            var sqlBuilder = new StringBuilder();
            //sqlBuilder.AppendFormat("DELETE FROM ReceiptDrugPriceRef WHERE DrugId IN ({0});", string.Join(",", drugIds));
            sqlBuilder.AppendFormat("DELETE FROM PhieuNhapChiTiets WHERE Thuoc_ThuocId IN ({0}) AND NhaThuoc_MaNhaThuoc = {1};", string.Join(",", drugIds), drugStoreCode);
            sqlBuilder.AppendFormat("DELETE FROM PhieuXuatChiTiets WHERE Thuoc_ThuocId IN ({0}) AND NhaThuoc_MaNhaThuoc = {1};", string.Join(",", drugIds), drugStoreCode);
            sqlBuilder.AppendFormat("DELETE FROM PhieuKiemKeChiTiets WHERE Thuoc_ThuocId IN ({0});", string.Join(",", drugIds), drugStoreCode);
            sqlBuilder.AppendFormat("DELETE FROM TongKetKyChiTiets WHERE Thuoc_ThuocId IN ({0}) AND NhaThuoc_MaNhaThuoc = {1};", string.Join(",", drugIds), drugStoreCode);
            sqlBuilder.AppendFormat("DELETE FROM TongKetKies WHERE Thuoc_ThuocId IN ({0}) AND NhaThuoc_MaNhaThuoc = {1};", string.Join(",", drugIds), drugStoreCode);
            sqlBuilder.AppendFormat("DELETE FROM Thuocs WHERE ThuocId IN ({0}) AND NhaThuoc_MaNhaThuoc = {1};", string.Join(",", drugIds), drugStoreCode);

            return sqlBuilder.ToString();
        }

        private bool CheckDrugsHasDeals(string drugStoreCode, params int[] drugIds)
        {
            if (drugIds == null || !drugIds.Any()) return false;
            
            var retVal = false;

            var receiptNotes = _dataFilterService.GetValidReceiptNotes(drugStoreCode);
            var receiptNoteItems = _dataFilterService.GetValidReceiptItems(drugStoreCode);
            var deliveryNotes = _dataFilterService.GetValidDeliveryNotes(drugStoreCode);
            var deliveryNoteItems = _dataFilterService.GetValidDeliveryItems(drugStoreCode);

            var receiptDrugs = (from ri in receiptNoteItems
                                join r in receiptNotes on ri.PhieuNhap_MaPhieuNhap equals r.MaPhieuNhap
                                where r.LoaiXuatNhap_MaLoaiXuatNhap == (int)NoteInOutType.Receipt
                                      && drugIds.Contains(ri.Thuoc_ThuocId.Value)
                                select ri);
            var deliveryDrugs = (from di in deliveryNoteItems
                                 join d in deliveryNotes on di.PhieuXuat_MaPhieuXuat equals d.MaPhieuXuat
                                 where d.MaLoaiXuatNhap == (int)NoteInOutType.Delivery
                                       && drugIds.Contains(di.Thuoc_ThuocId.Value)
                                 select di);

            retVal = receiptDrugs.Any() || deliveryDrugs.Any();

            return retVal;
        }

        public bool DeleteDrugs(string drugStoreCode, params int[] drugIds)
        {
            if (!drugIds.Any()) return false;

            var retVal = true;
            try
            {
                using (var tran = TransactionScopeHelper.CreateLockAllForWrite())               
                {
                    if (CheckDrugsHasDeals(drugStoreCode, drugIds))
                    {
                        tran.Complete();
                        return false;
                    }
                    var drugRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Thuoc>>();
                    var sqlCommands = GetDeleteDrugsCommands(drugStoreCode, drugIds);
                    drugRepo.ExecuteSqlCommand(sqlCommands);

                    tran.Complete();
                }
            }
            catch (Exception ex)
            {
                retVal = false;
                FaultHandler.Instance.Handle(ex, this);
            }

            return retVal;
        }
        public bool DeleteDrugGroups(string drugStoreCode, params int[] drugGroupIds)
        {
            if (!drugGroupIds.Any()) return false;

            var retVal = true;
            try
            {
                var drugRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Thuoc>>();
                var drugIds = drugRepo.GetAll().Where(i => drugGroupIds.Contains(i.NhomThuoc_MaNhomThuoc))
                  .Select(i => i.ThuocId).Distinct().ToList();
               
                //using (var tran = TransactionScopeHelper.CreateLockAllForWrite())
                {
                    if (CheckDrugsHasDeals(drugStoreCode, drugIds.ToArray()))
                    {
                        //tran.Complete();
                        return false;
                    }
                    var drugIdsString = string.Join(",", drugIds);
                    var sqlBuilder = new StringBuilder();
                    sqlBuilder.Append(GetDeleteDrugsCommands(drugStoreCode, drugIds.ToArray()));
                    sqlBuilder.AppendFormat("DELETE FROM NhomThuocs WHERE MaNhomThuoc IN ({0});",
                        string.Join(",", drugGroupIds));
                    drugRepo.ExecuteSqlCommand(sqlBuilder.ToString());

                    if (drugIds != null && drugIds.Any())
                    {
                        var priceRefRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, ReceiptDrugPriceRef>>();
                        sqlBuilder.Length = 0;
                        sqlBuilder.AppendFormat("DELETE FROM ReceiptDrugPriceRef WHERE DrugId IN ({0}) AND DrugStoreCode = {1};", drugIdsString, drugStoreCode);
                        sqlBuilder.AppendFormat("DELETE FROM DeliveryNoteItemSnapshotInfo WHERE DrugId IN ({0}) AND DrugStoreCode = {1};", drugIdsString, drugStoreCode);
                         
                        priceRefRepo.ExecuteSqlCommand(sqlBuilder.ToString());
                    }

                    //tran.Complete();
                }
            }
            catch (Exception ex)
            {
                retVal = false;
                FaultHandler.Instance.Handle(ex, this);
            }

            return retVal;
        }
        public void UpdateDrugPrice(string drugCode, string drugStoreCode, double inPrice, double outPriceL, double outPriceB, int unitCode, DateTime? dateModify)
        {
            ValidateUpdateRequest(null, null);
            IBaseRepository<Thuoc> drugRepo = null;
            var drugs = _dataFilterService.GetValidDrugs(drugStoreCode, null, false, out drugRepo);
            var drug = drugs.Where(d => d.MaThuoc == drugCode).FirstOrDefault();
            if (drug == null)
            {
                throw new ValidationException("drugManagement.validation.itemNotExist");
            }

            if (drug.DonViThuNguyen_MaDonViTinh == unitCode && drug.HeSo != 0)
            {
                if (inPrice != 0)
                {
                    drug.GiaNhap = (decimal)Math.Round(inPrice / drug.HeSo, MidpointRounding.AwayFromZero);
                }                
                drug.GiaBanLe = (decimal)Math.Round(outPriceL / drug.HeSo, MidpointRounding.AwayFromZero);
                drug.GiaBanBuon = (decimal)Math.Round(outPriceB / drug.HeSo, MidpointRounding.AwayFromZero);
            }
            else
            {
                if (inPrice != 0)
                {
                    drug.GiaNhap = (decimal)inPrice;
                }
                
                drug.GiaBanLe = (decimal)outPriceL;
                drug.GiaBanBuon = (decimal)outPriceB;
            }
            drugRepo.Update(drug);
            drugRepo.Commit();
        }

        public void UpdateOutDrugPrice(string drugCode, string drugStoreCode, double price, int unitCode)
        {
            ValidateUpdateRequest(null, null);
            IBaseRepository<Thuoc> drugRepo = null;
            var drugs = _dataFilterService.GetValidDrugs(drugStoreCode, null, false, out drugRepo);
            var drug = drugs.Where(d => d.MaThuoc == drugCode).FirstOrDefault();
            if (drug == null)
            {
                throw new ValidationException("drugManagement.validation.itemNotExist");
            }

            if (drug.DonViThuNguyen_MaDonViTinh == unitCode && drug.HeSo != 0)
            {
                drug.GiaBanLe = (decimal)Math.Round(price / drug.HeSo, MidpointRounding.AwayFromZero);
            }
            else
            {
                drug.GiaBanLe = (decimal)price;
            }

            drugRepo.Update(drug);
            drugRepo.Commit();
        }

        public void UpdateExpiredDateDrug(int noteItemId, string batchNumber, DateTime? expiredDate, string drugStoreCode, DrugStoreSetting setting)
        {            
            ValidateUpdateRequest(null, null);
            var receiptItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            receiptItemRepo.UpdateMany(i => i.MaPhieuNhapCt == noteItemId, i => new PhieuNhapChiTiet()
            {
                SoLo = batchNumber,
                HanDung = expiredDate
            });

            var sysService = IoC.Container.Resolve<ISystemService>();
            sysService.GenerateDrugExpiredDateWarning(drugStoreCode, setting);
        }

        public DrugInfo GetDrugInfo(string drugCode, string drugStoreCode, int drugUnit)
        {
            var drugInfo = new DrugInfo();
            IBaseRepository<Thuoc> drugRepo = null;
            var drugs = _dataFilterService.GetValidDrugs(drugStoreCode, null, false, out drugRepo);
            var drug = drugs.Where(d => d.MaThuoc == drugCode).FirstOrDefault();
            if (drug == null)
            {
                throw new ValidationException("drugManagement.validation.itemNotExist");
            }

            if (drug.DonViXuatLe_MaDonViTinh == drugUnit)
            {
                drugInfo.InPrice = drug.GiaNhap;
                drugInfo.OutPrice = drug.GiaBanLe;
                drugInfo.OutPriceB = drug.GiaBanBuon;
                drugInfo.UnitCode = drugUnit;
                //drugInfo.UnitName = drug.DonViTinh1.TenDonViTinh;
            }
            else
            {
                drugInfo.InPrice = drug.GiaNhap * drug.HeSo;
                drugInfo.OutPrice = drug.GiaBanLe * drug.HeSo;
                drugInfo.OutPriceB = drug.GiaBanBuon * drug.HeSo;
                drugInfo.UnitCode = drugUnit;
                //drugInfo.UnitName = drug.DonViTinh.TenDonViTinh;
            }

            return drugInfo;
        }

        public Dictionary<string, List<CacheDrug>> GetAllCacheDrugs()
        {
            var results = new Dictionary<string, List<CacheDrug>>();
            var drugs = _dataFilterService.GetValidDrugs(string.Empty).Select(i => new
            {
                DrugId = i.ThuocId,
                DrugCode = i.MaThuoc,
                DrugName = i.TenThuoc,
                ExtraInfo = i.ThongTin,
                DrugStoreCode = i.NhaThuoc_MaNhaThuoc,
                RetailUnitId = i.DonViXuatLe_MaDonViTinh,
                UnitId = i.DonViThuNguyen_MaDonViTinh,
                Factors = i.HeSo
            }).ToList();

            results = drugs.GroupBy(i => i.DrugStoreCode).ToDictionary(i => i.Key, i => i.Select(ii => new CacheDrug()
            {
                DrugId = ii.DrugId,
                DrugCode = ii.DrugCode,
                ExtraInfo = ii.ExtraInfo,
                DrugBarcode = ii.DrugName,
                RetailUnitId = ii.RetailUnitId,
                UnitId = ii.UnitId,
                Factors = ii.Factors
            }).ToList());

            var unitIds = drugs.Where(i => i.UnitId > 0).Select(i => i.UnitId).ToList();
            var retailUnitIds = drugs.Where(i => i.RetailUnitId > 0).Select(i => i.RetailUnitId).ToList();
            unitIds.AddRange(retailUnitIds);
            var units = _dataFilterService.GetValidUnits(string.Empty).Where(i => unitIds.Contains(i.MaDonViTinh))
                .Select(i => new DrugUnit()
                {
                    UnitId = i.MaDonViTinh,
                    UnitName = i.TenDonViTinh,
                    Factors = 1
                }).ToList();
            foreach (var item in results)
            {
                item.Value.ForEach(i =>
                {
                    i.Units = units.Where(u => u.UnitId == i.UnitId || u.UnitId == i.RetailUnitId).ToList();
                });
            }

            return results;
        }

        public List<CacheDrug> GetCacheDrugs(string drugStoreCode, params int[] drugIds)
        {
            var results = new List<CacheDrug>();

            var drugs = _dataFilterService.GetValidDrugs(drugStoreCode).Select(i => new CacheDrug()
            {
                DrugId = i.ThuocId,
                DrugCode = i.MaThuoc,
                DrugName = i.TenThuoc,
                ExtraInfo = i.ThongTin,
                DrugBarcode = i.BarCode,
                RetailUnitId = i.DonViXuatLe_MaDonViTinh,
                UnitId = i.DonViThuNguyen_MaDonViTinh,
                Factors = i.HeSo,
                RetailInPrice = (double)i.GiaNhap,
                RetailOutPrice = (double)i.GiaBanLe,
                CreatedDateTime = i.Created,
                ExpiredDateTime = i.HanDung
            });
            if (drugIds != null && drugIds.Length > 0)
            {
                drugs = drugs.Where(i => drugIds.Contains(i.DrugId));
            }
            results = drugs.ToList();
            if (!results.Any()) return results;

            var unitIds = results.Where(i => i.UnitId > 0).Select(i => i.UnitId).ToList();
            var retailUnitIds = results.Where(i => i.RetailUnitId > 0).Select(i => i.RetailUnitId).ToList();
            unitIds.AddRange(retailUnitIds);
            var units = _dataFilterService.GetValidUnits(drugStoreCode).Where(i => unitIds.Contains(i.MaDonViTinh))
                .Select(i => new DrugUnit()
                {
                    UnitId = i.MaDonViTinh,
                    UnitName = i.TenDonViTinh,
                    Factors = 1
                }).ToList();
            results.ForEach(i =>
            {
                i.Units = units.Where(u => u.UnitId == i.UnitId || u.UnitId == i.RetailUnitId).ToList();
            });
            var invService = IoC.Container.Resolve<IInventoryService>();
            invService.UpdateLastQuantity4CacheDrugs(drugStoreCode, results);

            return results;
        }
        public List<CreateReserveItem> GetListDrugForCreateReserve(string drugStoreCode, int type, int provider, int group_drug, string name_drug, bool get_drug_empty)
        {
            List<CreateReserveItem> lstItem = new List<CreateReserveItem>();
            CreateReserveItem item;
            GenerateReportData(drugStoreCode);
            var temp = _dataFilterService.GetValidDrugs(drugStoreCode).Where(x => x.HoatDong == true && x.GioiHan.HasValue);

            switch (type)
            {
                case 1:
                    {
                        var pnVali = _dataFilterService.GetValidReceiptNotes(drugStoreCode);
                        var pnctVali = _dataFilterService.GetValidReceiptItems(drugStoreCode);
                        var arrValiDrugs = (from pn in pnVali
                                            join pnct in pnctVali on pn.MaPhieuNhap equals pnct.PhieuNhap_MaPhieuNhap
                                            where pn.NhaCungCap_MaNhaCungCap == provider
                                            select pnct.Thuoc_ThuocId
                                            ).Distinct().ToList();
                        temp = temp.Where(x => arrValiDrugs.Contains(x.ThuocId));
                    }
                    break;
                case 2:
                    {
                        temp = temp.Where(x => x.NhomThuoc_MaNhomThuoc == group_drug);
                    }
                    break;
                case 3:
                    {
                        temp = temp.Where(x => x.MaThuoc == name_drug.Trim());
                    }
                    break;
            }
            var drugStore = temp.ToList();
            var drugGroupIds = drugStore.Select(i => i.NhomThuoc_MaNhomThuoc).Distinct().ToList();
            var drugGroups = _dataFilterService.GetValidDrugGroups(drugStoreCode)
                .Where(i => drugGroupIds.Contains(i.MaNhomThuoc))
                .Select(i => new { i.MaNhomThuoc, i.TenNhomThuoc })
                .ToDictionary(i => i.MaNhomThuoc, i => i.TenNhomThuoc);

            var units = _dataFilterService.GetValidUnits(drugStoreCode)
                .Select(i => new { i.MaDonViTinh, i.TenDonViTinh })
                .ToDictionary(i => i.MaDonViTinh, i => i.TenDonViTinh);

            var stt = 0;
            ReportService rpService = new ReportService();
            var Filter = new FilterObject()
            {
                DrugIds = drugStore.Select(x => x.ThuocId).ToArray(),
                ToDate = DateTime.Now
            };

            var drugWarehouses = rpService.GetDrugWarehouseSyntheises(drugStoreCode, Filter);
            drugWarehouses.ForEach(itemWH =>
            {
                //order++;
                var cand = itemWH.Value;
                var drug = drugStore.Where(x => x.ThuocId == cand.DrugId).FirstOrDefault();
                var drugUnitName = string.Empty;
                var drugUnitNameNguyen = string.Empty;
                var drugGroupName = string.Empty;
                if (drugGroups.ContainsKey(drug.NhomThuoc_MaNhomThuoc))
                {
                    drugGroupName = drugGroups[drug.NhomThuoc_MaNhomThuoc];
                }
                if (drug.DonViXuatLe_MaDonViTinh > 0 && units.ContainsKey(drug.DonViXuatLe_MaDonViTinh.Value))
                {
                    drugUnitName = units[drug.DonViXuatLe_MaDonViTinh.Value];
                    //Gán tam don vi nguyen = don vi le neu don vị nguyên không tồn tại
                    drugUnitNameNguyen = drugUnitName;
                }
                if (drug.DonViThuNguyen_MaDonViTinh > 0 && units.ContainsKey(drug.DonViThuNguyen_MaDonViTinh.Value))
                {
                    //Gán lại đơn vị nguyên
                    drugUnitNameNguyen = units[drug.DonViThuNguyen_MaDonViTinh.Value];
                }
                
                if (cand.LastInventoryQuantity <= drug.GioiHan && get_drug_empty)
                {
                    item = new CreateReserveItem()
                    {
                        STT = ++stt,
                        ThuocId = drug.ThuocId,
                        MaThuoc = drug.MaThuoc,
                        TenNhomThuoc = drugGroupName,
                        TenThuoc = drug.TenThuoc,
                        SoLuongCanhBao = drug.GioiHan.Value,
                        TonKho = cand.LastInventoryQuantity,
                        DonGia = drug.HeSo != 0 ? (drug.GiaNhap * drug.HeSo) : drug.GiaNhap,
                        DonViLe = drugUnitName,
                        DonViNguyen = drugUnitNameNguyen,
                        DuTru = "0",
                        HeSo = drug.HeSo,
                        ThanhTien = string.Empty
                    };
                    item.DonGia_view = item.DonGia.ToString("#,##0");
                    lstItem.Add(item);
                }
                else if (!get_drug_empty)
                {
                    item = new CreateReserveItem()
                    {
                        STT = ++stt,
                        ThuocId = drug.ThuocId,
                        MaThuoc = drug.MaThuoc,
                        TenThuoc = drug.TenThuoc,
                        TenNhomThuoc = drugGroupName,
                        SoLuongCanhBao = drug.GioiHan.Value,
                        TonKho = cand.LastInventoryQuantity,
                        DonGia = drug.HeSo != 0 ? (drug.GiaNhap * drug.HeSo) : drug.GiaNhap,
                        DonViLe = drugUnitName,
                        DonViNguyen = drugUnitNameNguyen,
                        DuTru = "0",
                        HeSo = drug.HeSo,
                        ThanhTien = string.Empty
                    };
                    item.DonGia_view = item.DonGia.ToString("#,##0");
                    lstItem.Add(item);
                }

            });

            //Lấy giá thuốc ở đây để tăng hiệu năng
            //var drugPrice = GetDrugPriceNewest(lstItem.Select(x => x.ThuocId).ToArray(), drugStoreCode);
            //DrugInfo drug_info;
            //foreach (var drug in lstItem)
            //{
            //    drug_info = drugPrice.Where(x => x.DrugId == drug.ThuocId).FirstOrDefault();
            //    if (drug_info != null)
            //    {
            //        drug.DonGia = (drug.HeSo != 0 ? (drug_info.InPrice * drug.HeSo) : drug_info.InPrice);
            //        drug.DonGia_view = drug.DonGia.ToString("#,##0");
            //    }
            //}

            return lstItem;
        }
        private List<DrugInfo> GetDrugPriceNewest(int[] drugIds, string drugStoreCode)
        {
            var pnctVali = _dataFilterService.GetValidReceiptItems(drugStoreCode);
            var pnVali = _dataFilterService.GetValidReceiptNotes(drugStoreCode);
            var maxPrices = from rep in (
                   from pn in pnVali
                   join pnct in pnctVali on pn.MaPhieuNhap equals pnct.PhieuNhap_MaPhieuNhap
                   where drugIds.Contains(pnct.Thuoc_ThuocId.Value)
                   select new
                   {
                       thuoc_id = pnct.Thuoc_ThuocId,
                       ngay_nhap = pn.NgayNhap,
                       gia_nhap = pnct.GiaNhap
                   })
                            group rep by rep.thuoc_id
                into g
                            select new DrugInfo
                            {
                                DrugId = g.Key.Value,
                                InPrice = g.OrderByDescending(x => x.ngay_nhap).FirstOrDefault().gia_nhap
                            };
            return maxPrices.ToList();
        }
        public List<ProviderInfo> GetListProvider(string drugStoreCode)
        {
            var tblProvider = _dataFilterService.GetValidSupplyers(drugStoreCode);
            var res = (from pr in tblProvider
                       select (new ProviderInfo()
                       {
                           MaNhaCungCap = pr.MaNhaCungCap,
                           TenNhaCungCap = pr.TenNhaCungCap,
                           MaNhaThuoc = pr.MaNhaThuoc,
                           MaNhomNhaCungCap = pr.MaNhomNhaCungCap
                       })).ToList();
            res.Remove(res.Where(x => x.TenNhaCungCap == "Điều chỉnh sau kiểm kê").FirstOrDefault());
            return res;
        }

        public List<GroupDrugInfo> GetListGroupDrug(string drugStoreCode)
        {
            var tblProvider = _dataFilterService.GetValidDrugGroups(drugStoreCode);
            var res = (from pr in tblProvider
                       select (new GroupDrugInfo()
                       {
                           MaNhomThuoc = pr.MaNhomThuoc,
                           TenNhomThuoc = pr.TenNhomThuoc
                       })).ToList();
            return res;
        }

        public Dictionary<int, double> GetLastDrugPriceOnReceiptNotes(string drugStoreCode, params int[] drugIds)
        {
            var filter = new FilterObject()
            {
                DrugIds = drugIds
            };

            var receiptItemsQueyrable = (from it in _dataFilterService.GetValidReceiptItems(drugStoreCode, filter, new int[] { (int)NoteInOutType.Receipt })
                                         join ha in _dataFilterService.GetValidReceiptNotes(drugStoreCode) on it.PhieuNhap_MaPhieuNhap equals ha.MaPhieuNhap
                                         join dr in _dataFilterService.GetValidDrugs(drugStoreCode)
                                         on it.Thuoc_ThuocId equals dr.ThuocId
                                         select new
                                         {
                                             dr.ThuocId,
                                             ha.NgayNhap,
                                             ha.VAT,
                                             it.ChietKhau,
                                             it.SoLo,
                                             it.HanDung,
                                             GiaNhap = (dr.DonViXuatLe_MaDonViTinh.Value == it.DonViTinh_MaDonViTinh.Value ? it.GiaNhap * (1 - it.ChietKhau/100) * (1 + (decimal)ha.VAT/100) : it.GiaNhap * (1 - it.ChietKhau / 100) * (1 + (decimal)ha.VAT / 100) / dr.HeSo)
                                         });

            var lastPrices = receiptItemsQueyrable.GroupBy(i => i.ThuocId).Select(i => 
                new { DrugId = i.Key, Price = (double) i.OrderByDescending(ii => ii.NgayNhap).FirstOrDefault().GiaNhap }).ToList();
            if (lastPrices.Count > 0)
            {
                //Trở lại giá nhập gần nhất và quy đổi sang đơn vị nhập lẻ

                return lastPrices.ToDictionary(i => i.DrugId, i => i.Price);
            }
            else
            {
                //Trả lại giá nhập trên thuốc
                IBaseRepository<Thuoc> drugRepo = null;
                var drugs = _dataFilterService.GetValidDrugs(drugStoreCode, filter, false, out drugRepo).ToList();

                var lastPriceOnDrugs = drugs.ToDictionary(i => i.ThuocId, i => (double)i.GiaNhap);
                return lastPriceOnDrugs;
            }
            
        }

        public Dictionary<int, double> GetLastDrugPriceOnDeliveryNotes(string drugStoreCode, params int[] drugIds)
        {
            var filter = new FilterObject()
            {
                DrugIds = drugIds
            };
            
            //
            var deliveryItems = _dataFilterService.GetValidDeliveryItems(drugStoreCode, filter, new int[] { (int)NoteInOutType.Delivery })
                .Where(i => i.RetailPrice > 0).Select(i => new { i.Thuoc_ThuocId, i.PreNoteItemDate, i.RetailPrice }).ToList();
            var lastPrices = deliveryItems.GroupBy(i => i.Thuoc_ThuocId).Select(i => 
                new {DrugId = i.Key, Price = i.OrderByDescending(ii => ii.PreNoteItemDate).FirstOrDefault().RetailPrice}).ToList();
            //deliveryItems.ToList().Groupby();
            if (lastPrices.Count > 0)
            {
                //Trả về giá bán gần nhất
                return lastPrices.ToDictionary(i => i.DrugId.Value, i => i.Price);
            }
            else
            {
                //Trả về giá bán lẻ trên thuốc
                IBaseRepository<Thuoc> drugRepo = null;
                var drugs = _dataFilterService.GetValidDrugs(drugStoreCode, filter, false, out drugRepo).ToList();

                var lastPriceOnDrugs = drugs.ToDictionary(i => i.ThuocId, i => (double)i.GiaBanLe);
                return lastPriceOnDrugs;
            }
        }

        public Dictionary<int, double> GetAvailableDrugQuantities(string drugStoreCode, DateTime? toDate, params int[] drugIds)
        {
            var result = new Dictionary<int, double>();
            var filter = new FilterObject()
            {
                DrugIds = drugIds,
                ToDate = toDate
            };
            var receiptItems = _dataFilterService.GetValidReceiptNoteItems(drugStoreCode, filter);
            receiptItems = receiptItems.Where(i => i.NoteType != (int)NoteInOutType.InitialInventory && i.RemainRefQuantity > MedConstants.EspQuantity);
            var drugQuantities = receiptItems.Select(i => new { i.RemainRefQuantity, i.DrugId }).ToList()
                .GroupBy(i => i.DrugId).ToDictionary(i => i.Key, i => i.Sum(ii => ii.RemainRefQuantity));           

            return drugQuantities;
        }

        public List<DrugPriceModel> GetLastestDrugMinPrices(params int[] drugIds)
        {
            var mapRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, DrugMapping>>();
            //Lấy min giá nhập
            var drugMappings = mapRepo.TableAsNoTracking.Where(i => drugIds.Contains(i.MasterDrugID))
                .Select(i => new { InMappingDate = DbFunctions.TruncateTime(i.InLastUpdateDate), OutMappingDate = DbFunctions.TruncateTime(i.OutLastUpdateDate), i.MasterDrugID, i.SlaveDrugID, i.InPrice, i.OutPrice });
            var maxDateMappingsInPrices = drugMappings.Where(i =>i.InPrice > 0).GroupBy(i => i.MasterDrugID)
                .Select(i => new { DrugId = i.Key, LastDate = i.Max(j => j.InMappingDate) });
            var minInPrices = (from m in drugMappings.Where(i => i.InPrice > 0)
                               join max in maxDateMappingsInPrices
                             on new { DrugId = m.MasterDrugID, MappingDate = m.InMappingDate } equals new { DrugId = max.DrugId, MappingDate = max.LastDate }
                               group m by new { m.InMappingDate, m.MasterDrugID } into g
                               select new { DrugId = g.Key.MasterDrugID, MinInPrice = g.Min(i => i.InPrice), InPriceLastUpdate = g.Key.InMappingDate })
                             .ToList().GroupBy(i => i.DrugId).ToDictionary(i => i.Key, i => i.FirstOrDefault());

            //Min giá bán          
            var maxDateMappingsOutPrices = drugMappings.Where(i => i.OutPrice > 0).GroupBy(i => i.MasterDrugID)
                .Select(i => new { DrugId = i.Key, LastDate = i.Max(j => j.OutMappingDate) });
            var minOutPrices = (from m in drugMappings.Where(i => i.OutPrice > 0)
                                join max in maxDateMappingsOutPrices
                              on new { DrugId = m.MasterDrugID, MappingDate = m.OutMappingDate } equals new { DrugId = max.DrugId, MappingDate = max.LastDate }
                                group m by new { m.OutMappingDate, m.MasterDrugID } into g
                                select new { DrugId = g.Key.MasterDrugID, MinOutPrice = g.Min(i => i.OutPrice), OutPriceLastUpdate = g.Key.OutMappingDate })
                                .ToList().GroupBy(i => i.DrugId).ToDictionary(i => i.Key, i => i.FirstOrDefault());
            var priceMappingModels = minInPrices.Select(i => new DrugPriceModel()
            {
                DrugID = i.Key,
                InPrice = i.Value.MinInPrice,
                InLastUpdateDate = i.Value.InPriceLastUpdate                
            }).ToDictionary(i => i.DrugID, i => i);
            minOutPrices.ForEach(i =>
            {
                DrugPriceModel priceModel = null;
                if (priceMappingModels.ContainsKey(i.Key))
                {
                    priceModel = priceMappingModels[i.Key];
                }
                else
                {
                    priceModel = new DrugPriceModel();
                    priceMappingModels.Add(i.Key, priceModel);
                }
                priceModel.DrugID = i.Key;
                priceModel.OutPrice = i.Value.MinOutPrice;
                priceModel.OutLastUpdateDate = i.Value.OutPriceLastUpdate;
            });

            return priceMappingModels.Values.ToList();
        }
        #endregion

        #region Private Methods

        #endregion
    }
}
