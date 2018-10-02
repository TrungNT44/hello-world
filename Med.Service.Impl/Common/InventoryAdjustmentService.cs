using System;
using System.Collections.Generic;
using System.Linq;
using App.Common.Data;
using App.Common.DI;
using App.Common.Extensions;
using App.Common.FaultHandling;
using App.Constants.Enums;
using Med.Common;
using Med.Common.Enums;
using Med.DbContext;
using Med.Entity;
using Med.Service.Base;
using Med.Service.Common;
using Med.Service.Delivery;
using Med.Service.Drug;
using Med.Service.Receipt;
using Med.Service.Report;
using Med.ServiceModel.Inventory;

//using Med.ServiceModel.PhieuKiemKe;

namespace Med.Service.Impl.Common
{
    public class InventoryAdjustmentService : MedBaseService, IInventoryAdjustmentService
    {
        // lấy danh sách toàn bộ Phiếu Kiểm kê (lọc theo thuốc và ngày tìm kiếm nếu có)
        public InventoryResponseModel GetInventoryList(String maNhaThuoc, int thuocId, DateTime? fromDate, DateTime? toDate)
        {
            var phieuKiemKeRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKe>>().GetAll();
            var phieuKiemKeChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKeChiTiet>>().GetAll();
            var userProfileRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, UserProfile>>().GetAll();

            // cau query de lay danh sach phieu kiem ke theo ma Nha thuoc
            var query = from pkk in phieuKiemKeRepo

                        join up in userProfileRepo
                        on pkk.CreatedBy_UserId equals up.UserId

                        join pkkct in phieuKiemKeChiTietRepo
                        on pkk.MaPhieuKiemKe equals pkkct.PhieuKiemKe_MaPhieuKiemKe

                        where (pkk.NhaThuoc_MaNhaThuoc == maNhaThuoc && pkk.RecordStatusID == (int)RecordStatus.Activated
                        && pkkct.RecordStatusID == (int)RecordStatus.Activated)

                        group new { pkk.Created, pkk.DaCanKho, pkkct.MaPhieuKiemKeCt, up.TenDayDu } by pkk.MaPhieuKiemKe into g

                        select new InventoryDetailModel
                        {
                            Id = g.Key,
                            CreateTime = g.Select(x => x.Created).FirstOrDefault(),
                            DaCanKho = g.Select(x => x.DaCanKho).FirstOrDefault(),
                            FullName = g.Select(x => x.TenDayDu).FirstOrDefault(),
                            DrugQuantity = g.Select(x => x.MaPhieuKiemKeCt).Distinct().Count()
                        };

            // loc theo Id cua thuoc tim kiem
            if (thuocId > 0)
            {
                query = from q in query
                        join pkkct in phieuKiemKeChiTietRepo
                        on q.Id equals pkkct.PhieuKiemKe_MaPhieuKiemKe
                        where (pkkct.Thuoc_ThuocId == thuocId)
                        select q;
            }

            // Lọc theo tham số fromDate và toDate nếu có
            if (fromDate != null && toDate != null)
            {
                query = query.Where(x => x.CreateTime >= fromDate && x.CreateTime < toDate);

            }

            // trả về danh sách phiếu và order theo thời gian tạo phiếu giảm dần
            InventoryResponseModel inventoryResponseModel = new InventoryResponseModel
            {
                InventoryDetailModels = query.OrderByDescending(x => x.CreateTime).ToList()
            };
            return inventoryResponseModel;
        }

        // lấy thông tin chi tiết Phiếu Kiểm kê 
        public InventoryDetailModel GetInventoryDetailInfo(String maNhaThuoc, int? Id)
        {
            var phieuKiemKeRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKe>>().GetAll();
            var phieuKiemKeChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKeChiTiet>>().GetAll();
            var userProfileRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, UserProfile>>().GetAll();
            var dataFilterService = IoC.Container.Resolve<IDataFilterService>();
            var thuocRepo = dataFilterService.GetValidDrugs(maNhaThuoc);
            var nhomThuocRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhomThuoc>>().GetAll();
            var donViTinhRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, DonViTinh>>().GetAll();
            var phieuNhapRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhap>>().GetAll();
            var phieuXuatRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuat>>().GetAll();
            var phieuNhapChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>().GetAll();
            var phieuXuatChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>().GetAll();

            // join bảng PhieuKiemKe và PhieuKiemKeChiTiet để lấy thông tin phiếu
            var inventoryDetailQuery = from pkk in phieuKiemKeRepo

                                       where (pkk.MaPhieuKiemKe == Id && pkk.RecordStatusID == (int)RecordStatus.Activated)

                                       join up in userProfileRepo
                                        on pkk.CreatedBy_UserId equals up.UserId

                                       select new InventoryDetailModel
                                       {
                                           Id = pkk.MaPhieuKiemKe,
                                           FullName = up.TenDayDu,
                                           CreateTime = pkk.Created.Value,
                                           DaCanKho = pkk.DaCanKho,
                                           NhaThuoc_MaNhaThuoc = pkk.NhaThuoc_MaNhaThuoc

                                       };
            InventoryDetailModel inventoryDetailResult = inventoryDetailQuery.FirstOrDefault();
            if (inventoryDetailResult == null)
            {
                return inventoryDetailResult;
            }

            // lấy thông tin thuốc, nhóm thuốc, đơn vị tính
            var medicineDetailQuery = from pkk in phieuKiemKeRepo

                                      join pkkct in phieuKiemKeChiTietRepo
                                      on pkk.MaPhieuKiemKe equals pkkct.PhieuKiemKe_MaPhieuKiemKe

                                      where (pkk.MaPhieuKiemKe == Id && pkk.RecordStatusID == (int)RecordStatus.Activated
                                      && pkkct.RecordStatusID == (int)RecordStatus.Activated)

                                      join t in thuocRepo
                                      on pkkct.Thuoc_ThuocId equals t.ThuocId

                                      join nt in nhomThuocRepo
                                      on t.NhomThuoc_MaNhomThuoc equals nt.MaNhomThuoc

                                      join dvt in donViTinhRepo
                                      on t.DonViXuatLe_MaDonViTinh equals dvt.MaDonViTinh

                                      select new ThuocModel
                                      {
                                          TenNhomThuoc = nt.TenNhomThuoc,
                                          ThuocId = t.ThuocId,
                                          MaThuoc = t.MaThuoc,
                                          TenThuoc = t.TenThuoc,
                                          TonKho = pkkct.TonKho,
                                          ThucTe = pkkct.ThucTe,
                                          TenDonViTinh = dvt.TenDonViTinh,
                                          Gia = pkkct.DonGia,
                                          SoLo = pkkct.SoLo,
                                          HanDung = pkkct.HanDung
                                      };

            inventoryDetailResult.MedicineList = medicineDetailQuery.ToList();

            // TH phiếu đã cân kho, lấy thông tin của các phiếu cân kho sau kiểm kê
            if (inventoryDetailResult.DaCanKho)
            {
                // lấy thông tin phiếu Nhập nếu có
                var phieuNhapQuery = from pkk in phieuKiemKeRepo
                                     where (pkk.MaPhieuKiemKe == Id && pkk.RecordStatusID == (int)RecordStatus.Activated)

                                     join pn in phieuNhapRepo
                                     on pkk.PhieuNhap_MaPhieuNhap equals pn.MaPhieuNhap

                                     join pnct in phieuNhapChiTietRepo
                                     on pn.MaPhieuNhap equals pnct.PhieuNhap_MaPhieuNhap into pngr

                                     select new PhieuCanKhoItem
                                     {
                                         MaPhieu = pn.MaPhieuNhap,
                                         SoPhieu = pn.SoPhieuNhap,
                                         LoaiPhieu = NoteInOutType.Receipt,//"Phiếu Nhập",
                                         SoLuong = pngr.Count(),

                                     };
                var phieuNhapResult = phieuNhapQuery.FirstOrDefault();

                // lấy thông tin phiếu xuất nếu có
                var phieuXuatQuery = from pkk in phieuKiemKeRepo

                                     where (pkk.MaPhieuKiemKe == Id && pkk.RecordStatusID == (int)RecordStatus.Activated)

                                     join px in phieuXuatRepo
                                     on pkk.PhieuXuat_MaPhieuXuat equals px.MaPhieuXuat

                                     join pnct in phieuXuatChiTietRepo
                                     on px.MaPhieuXuat equals pnct.PhieuXuat_MaPhieuXuat into pngr

                                     select new PhieuCanKhoItem
                                     {
                                         MaPhieu = px.MaPhieuXuat, //gr.Key.MaPhieuXuat,
                                         SoPhieu = px.SoPhieuXuat, //gr.Key.SoPhieuXuat,
                                         LoaiPhieu = NoteInOutType.Delivery,//"Phiếu Xuất",
                                         SoLuong = pngr.Distinct().Count(),

                                     };
                var phieuXuatResult = phieuXuatQuery.FirstOrDefault();

                // nếu có phiếu nhập và phiếu xuất, thêm vào phieuCanKhoItems
                var phieuCanKhoItems = new List<PhieuCanKhoItem>();
                if (phieuNhapResult != null)
                {
                    phieuCanKhoItems.Add(phieuNhapResult);
                }

                if (phieuXuatResult != null)
                {
                    phieuCanKhoItems.Add(phieuXuatResult);
                }

                inventoryDetailResult.PhieuCanKhoChiTiet = phieuCanKhoItems;
            }

            // Nếu các thuốc trong phiếu không có thông tin giá/số lô/hạn dùng => lấy thông tin từ phiếu nhập gần nhất
            List<int> drugIdsOfInventory = inventoryDetailResult.MedicineList.Select(x => x.ThuocId).ToList();
            var phieuNhapChiTietQueryable = dataFilterService.GetValidReceiptNoteItems(maNhaThuoc);

            // lấy danh sách phiếu nhập của các thuốc có trong phiếu đã tạo
            var phieuNhapChiTietQuery = from pnct in phieuNhapChiTietQueryable
                                        where drugIdsOfInventory.Contains(pnct.DrugId)
                                        orderby pnct.NoteDate descending

                                        select new
                                        {
                                            pnct.DrugId,
                                            pnct.Price,
                                            pnct.SerialNumber,
                                            pnct.ExpiredDate
                                        };
            var phieuNhapChiTietQueryResult = phieuNhapChiTietQuery.ToList();
            inventoryDetailResult.MedicineList.ForEach(x =>
            {
                if (phieuNhapChiTietQueryResult != null)
                {
                    var pnct = phieuNhapChiTietQueryResult.Where(a => a.DrugId == x.ThuocId).FirstOrDefault();
                    x.Gia = x.Gia == 0 ? (decimal)pnct.Price : x.Gia;
                    x.SoLo = x.SoLo ?? pnct.SerialNumber;
                    x.HanDung = !x.HanDung.HasValue ? pnct.ExpiredDate : x.HanDung;
                }
                // nếu hạn dùng của thuốc < min date quy đinh thì không hiển thị hạn dùng
                x.HanDung = x.HanDung <= MedConstants.MinProductionDataDate ? null : x.HanDung;
            });

            return inventoryDetailResult;
        }

        // xóa Phiếu Kiểm kê theo Mã phiếu
        public bool DeleteInventory(String maNhaThuoc, int userId, int Id)
        {
            bool result = false;
            var phieuKiemKeRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKe>>();
            var phieuKiemKeChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKeChiTiet>>();
            //var userProfileRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, UserProfile>>().GetAll();
            //var thuocRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Thuoc>>().GetAll();
            var nhomThuocRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhomThuoc>>().GetAll();
            //var donViTinhRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, DonViTinh>>().GetAll();
            var phieuNhapRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhap>>();
            var phieuXuatRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuat>>();
            var phieuNhapChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>().GetAll();
            var phieuXuatChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>().GetAll();

            // lấy thông tin phiếu
            var inventory = GetInventoryDetailInfo(maNhaThuoc, Id);
            if (inventory == null)
            {
                return false;
            }

            try
            {
                // TH phiếu đã cân kho, đổi trạng thái phiếu nhập và phiếu xuất (nếu có) sang đã xóa
                if (inventory.DaCanKho && inventory.PhieuCanKhoChiTiet.Count() > 0)
                {
                    var receiptService = IoC.Container.Resolve<IReceiptNoteService>();
                    var deliveryService = IoC.Container.Resolve<IDeliveryNoteService>();
                    foreach (var phieuCanKho in inventory.PhieuCanKhoChiTiet)
                    {
                        if (phieuCanKho.LoaiPhieu == NoteInOutType.Receipt)
                        {
                            receiptService.DeleteReceiptNote(maNhaThuoc, phieuCanKho.MaPhieu);
                        }
                        if (phieuCanKho.LoaiPhieu == NoteInOutType.Delivery)
                        {
                            deliveryService.DeleteDeliveryNote(maNhaThuoc, phieuCanKho.MaPhieu);
                        }
                    }
                }
                // xóa mềm dữ liệu ở 2 bảng PhieuKiemKes và PhieuKiemKeChiTiets (update RecordStatusID sang RecordStatus.Deleted)
                //phieuKiemKeRepo.Delete(i => i.MaPhieuKiemKe == Id);
                var pkkQuery = from pkk in phieuKiemKeRepo.GetAll()
                               where (pkk.MaPhieuKiemKe == Id)
                               select pkk;

                var phieuKiemKe = pkkQuery.FirstOrDefault();
                phieuKiemKe.Modified = DateTime.Now;
                phieuKiemKe.ModifiedBy_UserId = userId;
                phieuKiemKe.RecordStatusID = (int)RecordStatus.Deleted;

                phieuKiemKeRepo.Update(phieuKiemKe);
                phieuKiemKeRepo.Commit();

                //phieuKiemKeChiTietRepo.Delete(i => i.PhieuKiemKe_MaPhieuKiemKe == Id);
                var pkkctQuery = from pkkct in phieuKiemKeChiTietRepo.GetAll()
                                 where (pkkct.PhieuKiemKe_MaPhieuKiemKe == Id)
                                 select pkkct;

                var phieuKiemKeChiTiet = pkkctQuery.ToList();
                phieuKiemKeChiTiet.ForEach(x =>
                {
                    x.RecordStatusID = (int)RecordStatus.Deleted;
                });

                phieuKiemKeChiTietRepo.UpdateMany(phieuKiemKeChiTiet);
                phieuKiemKeChiTietRepo.Commit();
                result = true;

            }
            catch (Exception ex)
            {
                FaultHandler.Instance.Handle(ex, this, string.Format("DeleteInventoryItem - inventoryId: {0}", Id));
            }

            return result;
        }

        // luu thong tin Phieu Kiem Ke (ca truong hop tao moi + update)
        public int SaveInventory(String maNhaThuoc, int userId, InventoryDetailModel model)
        {
            var phieuKiemKeRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKe>>();
            var phieuKiemKeChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKeChiTiet>>();
            var phieuNhapRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhap>>();
            var phieuXuatRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuat>>();
            var phieuNhapChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            var phieuXuatChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
            var dataFilterService = IoC.Container.Resolve<IDataFilterService>();
            var validDrugRepo = dataFilterService.GetValidDrugs(maNhaThuoc);
            

            bool canKho = model.DaCanKho;
            // add thêm giá trị giờ phút vào ngày tạo phiếu
            var currentDate = DateTime.Now;
            model.CreateTime = model.CreateTime.Value.AddHours(currentDate.Hour).AddMinutes(currentDate.Minute).AddSeconds(currentDate.Second);

            // return value = mã phiếu nếu lưu phiếu thành công
            int retval = 0;
            var inventoryDrugCodes = model.MedicineList.Select(i => i.MaThuoc.ToLower()).ToList();
            var inventoryDrugQuery = from d in validDrugRepo
                        where (inventoryDrugCodes.Contains(d.MaThuoc.ToLower()))
                        select new
                        {
                            d.MaThuoc,
                            d.ThuocId,
                            d.DonViXuatLe_MaDonViTinh,
                            d.GiaNhap
                        };

            var inventoryDrugQueryResult = inventoryDrugQuery.ToDictionary(x => x.MaThuoc.ToLower(), x => new
            {
                x.ThuocId,
                x.DonViXuatLe_MaDonViTinh,
                x.GiaNhap
            });

            // tạo 1 đối tượng phiếu kiểm kê và lưu/cập nhật đối tượng này vào bảng PhieuKiemKes trong DB
            PhieuKiemKe phieuKiemKe = null;
            // TH tạo phiếu kiểm kê mới
            if (model.Id == 0)
            {
                phieuKiemKe = new PhieuKiemKe
                {
                    NhaThuoc_MaNhaThuoc = maNhaThuoc,
                    CreatedBy_UserId = userId,
                    Created = model.CreateTime,
                    DaCanKho = canKho,
                    RecordStatusID = (int)RecordStatus.Activated
                };
                phieuKiemKeRepo.Insert(phieuKiemKe);
                phieuKiemKeRepo.Commit();
            }
            // TH update phiếu đã tạo
            else
            {
                var pkkQuery = from pkk in phieuKiemKeRepo.GetAll()
                               where (pkk.MaPhieuKiemKe == model.Id)
                               select pkk;

                phieuKiemKe = pkkQuery.FirstOrDefault();
                phieuKiemKe.Modified = DateTime.Now;
                phieuKiemKe.DaCanKho = canKho;
                phieuKiemKe.ModifiedBy_UserId = userId;

                phieuKiemKeRepo.Update(phieuKiemKe);
                phieuKiemKeRepo.Commit();
            }
            retval = phieuKiemKe.MaPhieuKiemKe;

            // TH chọn cân kho, tạo phiếu điều chỉnh sau kiểm kê
            if (canKho)
            {
                // tạo phiếu nhập mới
                var nhaCungCap = EnsureNhaCungCapKiemKe(maNhaThuoc, userId);
                var loaiKiemKe = EnsureLoaiXuatNhapKiemKe();
                var receiptService = IoC.Container.Resolve<IReceiptNoteService>();
                var deliveryService = IoC.Container.Resolve<IDeliveryNoteService>();

                var phieuNhap = new PhieuNhap()
                {
                    Created = DateTime.Now,
                    CreatedBy_UserId = userId,
                    NhaCungCap_MaNhaCungCap = nhaCungCap.MaNhaCungCap,
                    NhaThuoc_MaNhaThuoc = maNhaThuoc,
                    SoPhieuNhap = receiptService.GetNewReceiptNoteNumber(maNhaThuoc),
                    NgayNhap = phieuKiemKe.Created,
                    LoaiXuatNhap_MaLoaiXuatNhap = loaiKiemKe.MaLoaiXuatNhap
                };
                phieuNhapRepo.Insert(phieuNhap);
                //phieuNhapRepo.Commit();

                // tạo phiếu xuất mới
                var phieuXuat = new PhieuXuat()
                {
                    Created = DateTime.Now,
                    CreatedBy_UserId = userId,
                    NhaCungCap_MaNhaCungCap = nhaCungCap.MaNhaCungCap,
                    NhaThuoc_MaNhaThuoc = maNhaThuoc,
                    SoPhieuXuat = deliveryService.GetNewDeliveryNoteNumber(maNhaThuoc),
                    NgayXuat = phieuKiemKe.Created,
                    MaLoaiXuatNhap = loaiKiemKe.MaLoaiXuatNhap
                };

                phieuXuatRepo.Insert(phieuXuat);
                //phieuXuatRepo.Commit();

                var phieuXuatChiTiets = new List<PhieuXuatChiTiet>();
                var phieuNhapChiTiets = new List<PhieuNhapChiTiet>();
                model.MedicineList.ForEach(e =>
                {

                    var chenhLech = e.ThucTe.HasValue ? e.TonKho - e.ThucTe : e.TonKho;
                    var drugId = inventoryDrugQueryResult[e.MaThuoc.ToLower()].ThuocId;
                    var maDonViTinh = inventoryDrugQueryResult[e.MaThuoc.ToLower()].DonViXuatLe_MaDonViTinh;
                    var gia = inventoryDrugQueryResult[e.MaThuoc.ToLower()].GiaNhap;
                    if (chenhLech > 0)
                    {
                        // tao phieu xuat dieu chinh kiem ke

                        var dItem = new PhieuXuatChiTiet()
                        {
                            DonViTinh_MaDonViTinh = maDonViTinh.Value,
                            NhaThuoc_MaNhaThuoc = maNhaThuoc,
                            SoLuong = chenhLech.Value,
                            Thuoc_ThuocId = drugId,
                            GiaXuat = e.Gia != 0 ? e.Gia : gia,
                        };
                        phieuXuatChiTiets.Add(dItem);

                    }
                    else if (chenhLech < 0)
                    {
                        // tao phieu nhap dieu chinh kiem ke
                        var rItem = new PhieuNhapChiTiet()
                        {
                            DonViTinh_MaDonViTinh = maDonViTinh,
                            NhaThuoc_MaNhaThuoc = maNhaThuoc,
                            SoLuong = chenhLech.Value * -1,
                            Thuoc_ThuocId = drugId,
                            GiaNhap = e.Gia != 0 ? e.Gia : gia,
                            SoLo = e.SoLo,
                            HanDung = e.HanDung,
                        };
                        phieuNhapChiTiets.Add(rItem);
                    }
                });

                phieuNhap.TongTien = phieuNhapChiTiets.Sum(a => a.SoLuong * a.GiaNhap);
                phieuNhap.DaTra = phieuNhap.TongTien;
                phieuXuat.TongTien = phieuXuatChiTiets.Sum(a => a.SoLuong * a.GiaXuat);
                phieuXuat.DaTra = phieuXuat.TongTien;
                phieuNhapRepo.Commit();
                phieuXuatRepo.Commit();

                phieuNhapChiTiets.ForEach(i =>
                {
                    i.PhieuNhap_MaPhieuNhap = phieuNhap.MaPhieuNhap;
                    i.IsModified = true;
                });
                phieuXuatChiTiets.ForEach(i =>
                    {
                        i.PhieuXuat_MaPhieuXuat = phieuXuat.MaPhieuXuat;
                        i.IsModified = true;
                    });

                phieuNhapChiTietRepo.InsertMany(phieuNhapChiTiets);
                phieuNhapChiTietRepo.Commit();
                phieuXuatChiTietRepo.InsertMany(phieuXuatChiTiets);
                phieuXuatChiTietRepo.Commit();

                phieuKiemKeRepo.UpdateMany(i => i.MaPhieuKiemKe == retval, i => new PhieuKiemKe()
                {
                    PhieuNhap_MaPhieuNhap = phieuNhap.MaPhieuNhap,
                    PhieuXuat_MaPhieuXuat = phieuXuat.MaPhieuXuat
                });
                phieuKiemKeRepo.Commit();
            }


            // tạo 1 đối tượng PhieuKiemKeChiTiet
            // update table PhieuKiemKeChiTiets
            if (model.Id > 0)
            {
                phieuKiemKeChiTietRepo.Delete(i => i.PhieuKiemKe_MaPhieuKiemKe == model.Id);
                phieuKiemKeChiTietRepo.Commit();
            }

            List<PhieuKiemKeChiTiet> phieuKiemKeChiTiets = new List<PhieuKiemKeChiTiet>();
            foreach (var thuoc in model.MedicineList)
            {
                phieuKiemKeChiTiets.Add(new PhieuKiemKeChiTiet
                {
                    PhieuKiemKe_MaPhieuKiemKe = retval,
                    Thuoc_ThuocId = inventoryDrugQueryResult[thuoc.MaThuoc.ToLower()].ThuocId,
                    TonKho = thuoc.TonKho,
                    ThucTe = thuoc.ThucTe,
                    DonGia = thuoc.Gia,
                    SoLo = thuoc.SoLo,
                    HanDung = thuoc.HanDung,
                    RecordStatusID = (int)RecordStatus.Activated

                });
            }
            phieuKiemKeChiTietRepo.InsertMany(phieuKiemKeChiTiets);
            phieuKiemKeChiTietRepo.Commit();
            return retval;
        }

        public NhaCungCap EnsureNhaCungCapKiemKe(String maNhaThuoc, int userId)
        {
            var nhaCungCapRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhaCungCap>>();
            var nhomNhaCungCapRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhomNhaCungCap>>().GetAll();
            var nccQuery = from ncc in nhaCungCapRepo.GetAll()
                           where (ncc.MaNhaThuoc == maNhaThuoc && ncc.TenNhaCungCap == "Điều chỉnh sau kiểm kê")
                           select ncc;

            var nhaCungCapKiemKe = nccQuery.FirstOrDefault();
            if (nhaCungCapKiemKe == null)
            {
                var nhomNhaCungCapQuery = from nncc in nhomNhaCungCapRepo
                                          where (nncc.MaNhaThuoc == maNhaThuoc)
                                          orderby nncc.MaNhomNhaCungCap
                                          select nncc.MaNhomNhaCungCap;

                var maNhomNhaCungCap = nhomNhaCungCapQuery.First();

                nhaCungCapKiemKe = new NhaCungCap()
                {
                    TenNhaCungCap = "Điều chỉnh sau kiểm kê",
                    Created = DateTime.Now,
                    CreatedBy_UserId = userId,
                    MaNhaThuoc = maNhaThuoc,
                    MaNhomNhaCungCap = maNhomNhaCungCap,
                    SupplierTypeId = (int)SupplierType.InventoryAdjustment
                };
                nhaCungCapRepo.Insert(nhaCungCapKiemKe);
                nhaCungCapRepo.Commit();
            }

            return nhaCungCapKiemKe;
        }

        public LoaiXuatNhap EnsureLoaiXuatNhapKiemKe()
        {
            var loaiXuatNhapRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, LoaiXuatNhap>>();
            var query = from lxn in loaiXuatNhapRepo.GetAll()
                        where (lxn.TenLoaiXuatNhap == "Điều chỉnh kiểm kê")
                        orderby lxn.MaLoaiXuatNhap
                        select lxn;

            var loaiXuatNhapKiemKe = query.FirstOrDefault();
            if (loaiXuatNhapKiemKe == null)
            {
                loaiXuatNhapKiemKe = new LoaiXuatNhap()
                {
                    TenLoaiXuatNhap = "Điều chỉnh kiểm kê"
                };
                loaiXuatNhapRepo.Insert(loaiXuatNhapKiemKe);
                loaiXuatNhapRepo.Commit();
            }
            return loaiXuatNhapKiemKe;
        }

        // lấy danh sách thuốc chưa được Kiêm kê
        public InventoryResponseModel GetDrugsHaveNotInventoried(String maNhaThuoc, DateTime? fromDate, DateTime? toDate)
        {
            var phieuKiemKeRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKe>>().GetAll();
            var phieuKiemKeChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKeChiTiet>>().GetAll();
            var drugRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Thuoc>>().GetAll();
            var donViTinhRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, DonViTinh>>().GetAll();
            var nhomThuocRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhomThuoc>>().GetAll();

            // lấy danh sách thuốc thuộc các phiếu Kiêm kê đã tạo, lọc theo thời gian tạo phiếu (nếu có)
            var thuocDaKiemKeQuery = from pkk in phieuKiemKeRepo
                                     join pkkct in phieuKiemKeChiTietRepo
                                     on pkk.MaPhieuKiemKe equals pkkct.PhieuKiemKe_MaPhieuKiemKe

                                     where (pkk.NhaThuoc_MaNhaThuoc == maNhaThuoc && pkk.RecordStatusID == (int)RecordStatus.Activated
                                      && pkkct.RecordStatusID == (int)RecordStatus.Activated && ((fromDate == null && toDate == null)
                                     || (pkk.Created >= fromDate && pkk.Created < toDate)
                                     ))

                                     select pkkct.Thuoc_ThuocId;

            // danh sách thuốc thuộc các phiếu Kiêm kê đã tạo
            var thuocDaKiemKes = thuocDaKiemKeQuery.Distinct().ToList();

            // Thuốc chưa kiểm kê là những thuốc KHÔNG thuộc danh sách trên
            var query = from dr in drugRepo

                        join dvt in donViTinhRepo
                        on dr.DonViXuatLe_MaDonViTinh equals dvt.MaDonViTinh

                        join nt in nhomThuocRepo
                        on dr.NhomThuoc_MaNhomThuoc equals nt.MaNhomThuoc

                        where (dr.NhaThuoc_MaNhaThuoc == maNhaThuoc
                        && dr.HoatDong && !thuocDaKiemKes.Contains(dr.ThuocId))

                        select new ThuocModel
                        {
                            ThuocId = dr.ThuocId,
                            TenNhomThuoc = nt.TenNhomThuoc,
                            MaThuoc = dr.MaThuoc,
                            TenThuoc = dr.TenThuoc,
                            TenDonViTinh = dvt.TenDonViTinh,
                        };


            InventoryResponseModel inventoryResponseModel = new InventoryResponseModel
            {
                ThuocModels = query.ToList()
            };

            return inventoryResponseModel;
        }

        // lấy thông tin chi tiết của thuốc theo mã nhóm thuốc, mã thuốc hoặc barcode
        public List<ThuocModel> GetDrugInfo(string maNhaThuoc, int? maNhomThuoc, int?[] drugIds, string ngayTao, string barcode = "")
        {
            var phieuKiemKeRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKe>>().GetAll();
            var phieuKiemKeChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKeChiTiet>>().GetAll();
            var dataFilterService = IoC.Container.Resolve<IDataFilterService>();
            var thuocs = dataFilterService.GetValidDrugs(maNhaThuoc);
            var nhomThuocRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Med.Entity.NhomThuoc>>().GetAll();
            var donViTinhRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Med.Entity.DonViTinh>>().GetAll();
            var rpService = IoC.Container.Resolve<IReportService>();

            DateTime toDate = Convert.ToDateTime(ngayTao);
            toDate = toDate.AbsoluteEnd();

            // lọc theo mã nhóm thuốc
            if (maNhomThuoc > 0)
            {
                thuocs = thuocs.Where(i => i.NhomThuoc_MaNhomThuoc == maNhomThuoc);
            }
            // lọc theo Id thuốc
            else if (drugIds != null && drugIds.Count() > 0)
            {
                thuocs = thuocs.Where(i => drugIds.Contains(i.ThuocId));
            }
            // lọc theo mã vạch
            else if (!string.IsNullOrWhiteSpace(barcode))
            {
                thuocs = thuocs.Where(i => i.BarCode == barcode);
            }

            var thuocQuery = from t in thuocs
                             join nt in nhomThuocRepo on t.NhomThuoc_MaNhomThuoc equals nt.MaNhomThuoc
                             join dvt in donViTinhRepo on t.DonViXuatLe_MaDonViTinh equals dvt.MaDonViTinh

                             where (t.NhaThuoc_MaNhaThuoc == maNhaThuoc && nt.MaNhaThuoc == maNhaThuoc && dvt.MaNhaThuoc == maNhaThuoc)

                             select new ThuocModel()
                             {
                                 ThuocId = t.ThuocId,
                                 TenNhomThuoc = nt.TenNhomThuoc,
                                 MaThuoc = t.MaThuoc,
                                 TenThuoc = t.TenThuoc,
                                 TenDonViTinh = dvt.TenDonViTinh,
                                 Gia = t.GiaBanLe,
                             };
            var thuocModels = thuocQuery.ToList();
            var thuocIds = thuocModels.Select(i => i.ThuocId).Distinct().ToArray();

            // lấy danh sách phiếu Kiêm kê chưa cân kho để xem thuốc đã được kiểm kê chưa
            var pkkChuaCanKhoQuery = from pkk in phieuKiemKeRepo
                                     join pkkct in phieuKiemKeChiTietRepo
                                     on pkk.MaPhieuKiemKe equals pkkct.PhieuKiemKe_MaPhieuKiemKe

                                     where (pkk.NhaThuoc_MaNhaThuoc == maNhaThuoc &&
                                     thuocIds.Contains(pkkct.Thuoc_ThuocId.Value) &&
                                     pkk.DaCanKho == false && pkk.RecordStatusID == (int)RecordStatus.Activated
                                      && pkkct.RecordStatusID == (int)RecordStatus.Activated)

                                     select new
                                     {
                                         pkk.MaPhieuKiemKe,
                                         pkkct.Thuoc_ThuocId
                                     };
            var invService = IoC.Container.Resolve<IInventoryService>();
            var drugQuantityAndPrice = invService.GetDrugInventoryValues(maNhaThuoc,
                thuocIds, MedConstants.MinProductionDataDate, toDate);
            var phieuNhapChiTietQueryable = dataFilterService.GetValidReceiptNoteItems(maNhaThuoc);
            var phieuNhapChiTietQuery = from pnct in phieuNhapChiTietQueryable
                                        join t in thuocs
                                        on pnct.DrugId equals t.ThuocId
                                        orderby pnct.NoteDate descending

                                        select new
                                        {
                                            t.ThuocId,
                                            pnct.Price,
                                            pnct.SerialNumber,
                                            pnct.ExpiredDate
                                        };

            var phieuNhapChiTietResult = phieuNhapChiTietQuery.ToList();
            var pkkChuaCanKhoResult = pkkChuaCanKhoQuery.ToList();

            // duyệt danh sách thuốc để update giá trị giá/số tồn/lô/hạn dùng/mã phiếu kk chưa cân kho tồn tại
            thuocModels.ForEach(t =>
                {
                    if (drugQuantityAndPrice.ContainsKey(t.ThuocId))
                    {
                        t.TonKho = (decimal)drugQuantityAndPrice[t.ThuocId].LastInventoryQuantity;
                        t.Gia = (decimal)drugQuantityAndPrice[t.ThuocId].InPrice;
                    }
                    var pnct = phieuNhapChiTietResult.Where(x => x.ThuocId == t.ThuocId).FirstOrDefault();
                    // lấy thông tin giá, số lô, hạn dùng
                    if (pnct != null)
                    {
                        //t.Gia = (decimal)pnct.Price;
                        t.SoLo = pnct.SerialNumber;
                        t.HanDung = pnct.ExpiredDate;
                    }

                    // nếu thuốc này đã có trong phiếu Kiểm kê khác, gán mã phiếu đó vào MaPhieuKiemKeTonTai
                    var pkk = pkkChuaCanKhoResult.Where(x => x.Thuoc_ThuocId == t.ThuocId).FirstOrDefault();
                    if (pkk != null)
                    {
                        t.MaPhieuKiemKeTonTai = pkk.MaPhieuKiemKe;
                    }

                    // TH HanDung nho hon MinTime, update lai HanDung
                    t.HanDung = (t.HanDung == null || t.HanDung <= MedConstants.MinProductionDataDate) ? null : t.HanDung;
                }
            );
            return thuocModels;
        }

        // cap nhat gia/lo/han dung cho tung thuoc o trong Phieu kiem ke
        public void UpdateDrugSerialNoAndExpDate(String maNhaThuoc, InventoryEditModel inventoryEditModel)
        {
            int inventoryId = inventoryEditModel.InventoryId;
            // return khi ma phieu kiem ke khong hop le
            if (inventoryId <= 0) return;

            bool canKho = inventoryEditModel.DaCanKho;
            decimal gia = inventoryEditModel.Gia;
            string soLo = inventoryEditModel.SoLo;
            DateTime? hanDung = inventoryEditModel.HanDung;

            var phieuKiemKeRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKe>>().GetAll();
            var phieuKiemKeChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKeChiTiet>>();
            var phieuNhapRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhap>>();
            var phieuXuatRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuat>>();
            var phieuNhapChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            var phieuXuatChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();

            InventoryDetailModel inventoryDetailModel = GetInventoryDetailInfo(maNhaThuoc, inventoryId);
            var thuocId = inventoryDetailModel.MedicineList.Where(x => x.MaThuoc == inventoryEditModel.MaThuoc).FirstOrDefault().ThuocId;
            // lay thong tin phieu kiem ke chi tiet theo PhieuKiemKe_MaPhieuKiemKe va Thuoc_ThuocId
            var pkkctQuery = from pkkct in phieuKiemKeChiTietRepo.GetAll()
                             where (pkkct.PhieuKiemKe_MaPhieuKiemKe == inventoryId && pkkct.RecordStatusID == (int)RecordStatus.Activated
                             && pkkct.Thuoc_ThuocId == thuocId)

                             select pkkct;

            var pkkctQueryResult = pkkctQuery.FirstOrDefault();
            PhieuKiemKeChiTiet phieuKiemKeChiTietEntity = pkkctQueryResult;
            phieuKiemKeChiTietEntity.DonGia = gia;
            phieuKiemKeChiTietEntity.SoLo = soLo;
            phieuKiemKeChiTietEntity.HanDung = hanDung;

            // cap nhat gia/lo/han vao bang PhieuKiemKeChiTiets
            phieuKiemKeChiTietRepo.Update(phieuKiemKeChiTietEntity);
            phieuKiemKeChiTietRepo.Commit();

            // TH phieu da can kho, cap nhat gia/lo/han trong phieu Nhap/xuat chi tiet
            if (canKho)
            {
                inventoryDetailModel.PhieuCanKhoChiTiet.ForEach(p =>
                {
                    // TH Phieu Nhap
                    if (p.LoaiPhieu == NoteInOutType.Receipt && p.SoLuong > 0)
                    {
                        // tim Phieu Nhap Chi Tiet theo MaPhieu va thuocId
                        var pnctQuery = from pnct in phieuNhapChiTietRepo.GetAll()
                                        where (pnct.PhieuNhap_MaPhieuNhap == p.MaPhieu
                                        && pnct.Thuoc_ThuocId == thuocId)

                                        select pnct;

                        var pnctQueryResult = pnctQuery.FirstOrDefault();
                        if (pnctQueryResult != null)
                        {
                            PhieuNhapChiTiet phieuNhapChiTietEntity = pnctQueryResult;
                            phieuNhapChiTietEntity.GiaNhap = gia;
                            phieuNhapChiTietEntity.SoLo = soLo;
                            phieuNhapChiTietEntity.HanDung = hanDung;

                            phieuNhapChiTietRepo.Update(phieuNhapChiTietEntity);
                            phieuNhapChiTietRepo.Commit();
                        }


                    }
                    // TH Phieu Xuat
                    if (p.LoaiPhieu == NoteInOutType.Delivery && p.SoLuong > 0)
                    {
                        // tim Phieu Xuat Chi Tiet theo MaPhieu va thuocId
                        var pxctQuery = from pxct in phieuXuatChiTietRepo.GetAll()
                                        where (pxct.PhieuXuat_MaPhieuXuat == p.MaPhieu
                                        && pxct.Thuoc_ThuocId == thuocId)

                                        select pxct;
                        var pxctQueryResult = pxctQuery.FirstOrDefault();
                        if (pxctQueryResult != null)
                        {
                            PhieuXuatChiTiet phieuXuatChiTietEntity = pxctQueryResult;
                            phieuXuatChiTietEntity.GiaXuat = gia;

                            phieuXuatChiTietRepo.Update(phieuXuatChiTietEntity);
                            phieuXuatChiTietRepo.Commit();
                        }

                    }
                });
            }
        }
    }
}
