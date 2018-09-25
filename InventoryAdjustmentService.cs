using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
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
using Med.ServiceModel;
using Med.ServiceModel.Inventory;

//using Med.ServiceModel.PhieuKiemKe;

namespace Med.Service.Impl.Common
{
    public class InventoryAdjustmentService : MedBaseService, IInventoryAdjustmentService
    {
        // lấy danh sách toàn bộ Phiếu Kiểm kê
        public List<InventoryDetailModel> GetInventoryList(String maNhaThuoc, String searchTen, DateTime? fromDate, DateTime? toDate)
        {
            var phieuKiemKeRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKe>>().GetAll();
            var phieuKiemKeChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKeChiTiet>>().GetAll();
            var userProfileRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, UserProfile>>().GetAll();
            var thuocRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Thuoc>>().GetAll();

            // lọc danh sách phiếu Kiểm kê theo tên thuốc (nếu có)
            // group by MaPhieuKiemKeCt để count số lượng thuốc của từng phiếu
            //var query = from pkk in phieuKiemKeRepo

            //            join up in userProfileRepo
            //            on pkk.CreatedBy_UserId equals up.UserId

            //            join pkkct in phieuKiemKeChiTietRepo
            //            on pkk.MaPhieuKiemKe equals pkkct.PhieuKiemKe_MaPhieuKiemKe

            //            join t in thuocRepo
            //            on pkkct.Thuoc_ThuocId equals t.ThuocId

            //            where (pkk.NhaThuoc_MaNhaThuoc == maNhaThuoc && ((searchTen == null) || (t.TenThuoc.Contains(searchTen))))

            //            join pkkct2 in phieuKiemKeChiTietRepo
            //            on pkk.MaPhieuKiemKe equals pkkct2.PhieuKiemKe_MaPhieuKiemKe

            //            group pkkct2.MaPhieuKiemKeCt by new { pkkct2.PhieuKiemKe_MaPhieuKiemKe, up.TenDayDu, pkk.DaCanKho, pkk.Created } into g

            //            select new InventoryDetailModel
            //            {
            //                Id = g.Key.PhieuKiemKe_MaPhieuKiemKe,
            //                FullName = g.Key.TenDayDu,
            //                IsCompareStore = g.Key.DaCanKho,
            //                DrugQuantity = g.Distinct().Count(),
            //                CreateTime = g.Key.Created,
            //            };

            var query = from pkk in phieuKiemKeRepo

                        join up in userProfileRepo
                        on pkk.CreatedBy_UserId equals up.UserId

                        join pkkct in phieuKiemKeChiTietRepo
                        on pkk.MaPhieuKiemKe equals pkkct.PhieuKiemKe_MaPhieuKiemKe

                        where (pkk.NhaThuoc_MaNhaThuoc == maNhaThuoc && pkk.RecordStatusId == (int)RecordStatus.Activated
                        && pkkct.RecordStatusId == (int)RecordStatus.Activated)

                        group new { pkk.Created, pkk.DaCanKho, pkkct.MaPhieuKiemKeCt, up.TenDayDu } by pkk.MaPhieuKiemKe into g

                        select new InventoryDetailModel
                        {
                            Id = g.Key,
                            CreateTime = g.Select(x => x.Created.Value).FirstOrDefault(),
                            IsCompareStore = g.Select(x => x.DaCanKho).FirstOrDefault(),
                            FullName = g.Select(x => x.TenDayDu).FirstOrDefault(),
                            DrugQuantity = g.Select(x => x.MaPhieuKiemKeCt).Distinct().Count(),

                        };



            if (!String.IsNullOrEmpty(searchTen))
            {
                query = from q in query
                        join pkkct in phieuKiemKeChiTietRepo
                        on q.Id equals pkkct.PhieuKiemKe_MaPhieuKiemKe

                        join t in thuocRepo
                        on pkkct.Thuoc_ThuocId equals t.ThuocId

                        where (t.TenThuoc.Contains(searchTen))

                        select q;
            }

            // Lọc theo tham số fromDate và toDate nếu có
            if (fromDate != null && toDate != null)
            {
                query = query.Where(x => x.CreateTime >= fromDate && x.CreateTime < toDate);

            }
            var result = query.OrderByDescending(x => x.CreateTime).ToList();
            return result;
        }

        // lấy thông tin chi tiết Phiếu Kiểm kê 
        public InventoryDetailModel GetInventoryDetailInfo(String maNhaThuoc, int? Id)
        {
            var phieuKiemKeRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKe>>().GetAll();
            var phieuKiemKeChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKeChiTiet>>().GetAll();
            var userProfileRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, UserProfile>>().GetAll();
            var thuocRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Thuoc>>().GetAll();
            var nhomThuocRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhomThuoc>>().GetAll();
            var donViTinhRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, DonViTinh>>().GetAll();
            var phieuNhapRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhap>>().GetAll();
            var phieuXuatRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuat>>().GetAll();
            var phieuNhapChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>().GetAll();
            var phieuXuatChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>().GetAll();

            // join bảng PhieuKiemKe và PhieuKiemKeChiTiet để lấy thông tin phiếu
            var inventoryDetailQuery = from pkk in phieuKiemKeRepo

                                       where (pkk.MaPhieuKiemKe == Id && pkk.RecordStatusId == (int)RecordStatus.Activated)

                                       join up in userProfileRepo
                                        on pkk.CreatedBy_UserId equals up.UserId

                                       select new InventoryDetailModel
                                       {
                                           Id = pkk.MaPhieuKiemKe,
                                           FullName = up.TenDayDu,
                                           CreateTime = pkk.Created.Value,
                                           IsCompareStore = pkk.DaCanKho,
                                           NhaThuoc_MaNhaThuoc = pkk.NhaThuoc_MaNhaThuoc

                                       };

            // lấy thông tin thuốc, nhóm thuốc, đơn vị tính
            var medicineDetailQuery = from pkk in phieuKiemKeRepo

                                      join pkkct in phieuKiemKeChiTietRepo
                                      on pkk.MaPhieuKiemKe equals pkkct.PhieuKiemKe_MaPhieuKiemKe

                                      where (pkk.MaPhieuKiemKe == Id && pkk.RecordStatusId == (int)RecordStatus.Activated
                                      && pkkct.RecordStatusId == (int)RecordStatus.Activated)

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

            // lấy thông tin phiếu Nhập nếu có
            var phieuNhapQuery = from pkk in phieuKiemKeRepo

                                 where (pkk.MaPhieuKiemKe == Id && pkk.RecordStatusId == (int)RecordStatus.Activated)

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

                                 where (pkk.MaPhieuKiemKe == Id && pkk.RecordStatusId == (int)RecordStatus.Activated)

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

            InventoryDetailModel inventoryDetailResult = inventoryDetailQuery.FirstOrDefault();
            if (inventoryDetailResult == null)
            {
                return inventoryDetailResult;
            }
            inventoryDetailResult.MedicineList = medicineDetailQuery.ToList();

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

            return inventoryDetailResult;
        }

        // xóa Phiếu Kiểm kê theo Mã phiếu
        public bool DeleteInventory(String maNhaThuoc, int userId, int Id)
        {
            bool result = false;
            var phieuKiemKeRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKe>>();
            var phieuKiemKeChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKeChiTiet>>();
            var userProfileRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, UserProfile>>().GetAll();
            var thuocRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Thuoc>>().GetAll();
            var nhomThuocRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhomThuoc>>().GetAll();
            var donViTinhRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, DonViTinh>>().GetAll();
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
                {
                    //var maNhaThuoc = inventory.NhaThuoc_MaNhaThuoc;

                    // TH phiếu đã cân kho, đổi trạng thái phiếu nhập và phiếu xuất (nếu có) sang đã xóa
                    if (inventory.IsCompareStore && inventory.PhieuCanKhoChiTiet.Count() > 0)
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
                    // xóa dữ liệu ở 2 bảng PhieuKiemKes và PhieuKiemKeChiTiets
                    //phieuKiemKeRepo.Delete(i => i.MaPhieuKiemKe == Id);
                    var pkkQuery = from pkk in phieuKiemKeRepo.GetAll()
                                   where (pkk.MaPhieuKiemKe == Id)
                                   select pkk;

                    var phieuKiemKe = pkkQuery.FirstOrDefault();
                    phieuKiemKe.Modified = DateTime.Now;
                    phieuKiemKe.ModifiedBy_UserId = userId;
                    phieuKiemKe.RecordStatusId = (int)RecordStatus.Deleted;

                    phieuKiemKeRepo.Update(phieuKiemKe);
                    phieuKiemKeRepo.Commit();

                    //phieuKiemKeChiTietRepo.Delete(i => i.PhieuKiemKe_MaPhieuKiemKe == Id);
                    var pkkctQuery = from pkkct in phieuKiemKeChiTietRepo.GetAll()
                                   where (pkkct.PhieuKiemKe_MaPhieuKiemKe == Id)
                                   select pkkct;

                    var phieuKiemKeChiTiet = pkkctQuery.FirstOrDefault();
                    phieuKiemKeChiTiet.RecordStatusId = (int)RecordStatus.Deleted;

                    phieuKiemKeChiTietRepo.Update(phieuKiemKeChiTiet);
                    phieuKiemKeChiTietRepo.Commit();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                FaultHandler.Instance.Handle(ex, this, string.Format("DeleteInventoryItem - inventoryId: {0}", Id));
            }

            return result;
        }

        // lấy thông tin các nhóm thuốc, dùng cho chức năng thêm nhóm thuốc khi tạo/edit phiếu kiểm kê
        public List<ThuocModel> GetTenNhomThuoc(string maNhaThuoc, string maNhaThuocCha)
        {

            var nhomThuocRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhomThuoc>>().GetAll();

            var query = from nt in nhomThuocRepo
                        where (nt.MaNhaThuoc == maNhaThuoc || nt.MaNhaThuoc == maNhaThuocCha)
                        orderby nt.TenNhomThuoc
                        select new ThuocModel
                        {
                            TenNhomThuoc = nt.TenNhomThuoc,
                            MaNhomThuoc = nt.MaNhomThuoc,
                        };
            List<ThuocModel> TenNhomThuocResult = query.ToList();
            return TenNhomThuocResult;

        }

        // luu thong tin Phieu Kiem Ke (ca truong hop tao moi + update)
        public int SaveInventory(String maNhaThuoc, String maNhaThuocCha, int userId, InventoryDetailModel model)
        {
            var phieuKiemKeRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKe>>();
            var phieuKiemKeChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKeChiTiet>>();
            var phieuNhapRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhap>>();
            var phieuXuatRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuat>>();
            var phieuNhapChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            var phieuXuatChiTietRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
            var drugRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Thuoc>>();

            bool canKho = model.IsCompareStore;
            var currentDate = DateTime.Now;
            model.CreateTime = model.CreateTime.AddHours(currentDate.Hour).AddMinutes(currentDate.Minute).AddSeconds(currentDate.Second);
            int retval = 0;
            var drugCodes = model.MedicineList.Select(i => i.MaThuoc.ToLower()).ToList();
            var query = from d in drugRepo.GetAll()
                        where (drugCodes.Contains(d.MaThuoc.ToLower()) && (d.NhaThuoc_MaNhaThuoc == maNhaThuoc || d.NhaThuoc_MaNhaThuoc == maNhaThuocCha))
                        select new
                        {
                            d.MaThuoc,
                            d.ThuocId,
                            d.DonViXuatLe_MaDonViTinh,
                            d.GiaNhap
                        };

            var drugs = query.ToDictionary(x => x.MaThuoc.ToLower(), x => new
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
                    //MaPhieuKiemKe = model.Id,
                    NhaThuoc_MaNhaThuoc = maNhaThuoc,
                    CreatedBy_UserId = userId,
                    Created = model.CreateTime,
                    DaCanKho = canKho,
                    RecordStatusId = (int)RecordStatus.Activated
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
                var phieuNhap = new PhieuNhap()
                {
                    Created = DateTime.Now,
                    CreatedBy_UserId = userId,
                    NhaCungCap_MaNhaCungCap = nhaCungCap.MaNhaCungCap,
                    NhaThuoc_MaNhaThuoc = maNhaThuoc,
                    SoPhieuNhap = _generateAvaliableSoPhieuNhap(maNhaThuoc),
                    NgayNhap = phieuKiemKe.Created,
                    LoaiXuatNhap_MaLoaiXuatNhap = loaiKiemKe.MaLoaiXuatNhap
                };
                phieuNhapRepo.Insert(phieuNhap);
                //phieuNhapRepo.Commit();

                // tạo phiếu xuất mới
                var phieuXuat = new Entity.PhieuXuat()
                {
                    Created = DateTime.Now,
                    CreatedBy_UserId = userId,
                    NhaCungCap_MaNhaCungCap = nhaCungCap.MaNhaCungCap,
                    NhaThuoc_MaNhaThuoc = maNhaThuoc,
                    SoPhieuXuat = _generateAvaliableSoPhieuXuat(maNhaThuoc),
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
                    var drugId = drugs[e.MaThuoc.ToLower()].ThuocId;
                    var maDonViTinh = drugs[e.MaThuoc.ToLower()].DonViXuatLe_MaDonViTinh;
                    var gia = drugs[e.MaThuoc.ToLower()].GiaNhap;
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
                            //PhieuXuat_MaPhieuXuat = phieuXuat.MaPhieuXuat
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
                            //PhieuNhap_MaPhieuNhap = phieuNhap.MaPhieuNhap
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
                    Thuoc_ThuocId = drugs[thuoc.MaThuoc.ToLower()].ThuocId,
                    TonKho = thuoc.TonKho,
                    ThucTe = thuoc.ThucTe,
                    DonGia = thuoc.Gia,
                    SoLo = thuoc.SoLo,
                    HanDung = thuoc.HanDung,
                    RecordStatusId = (int)RecordStatus.Activated

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

        private long _generateAvaliableSoPhieuNhap(String maNhaThuoc)
        {
            var phieuNhapRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhap>>();
            var query = from pn in phieuNhapRepo.GetAll()
                        where (pn.NhaThuoc_MaNhaThuoc == maNhaThuoc)
                        orderby pn.SoPhieuNhap descending
                        select pn.SoPhieuNhap;

            var maxSoPhieu = query.First();

            return maxSoPhieu + 1;
        }
        private long _generateAvaliableSoPhieuXuat(String maNhaThuoc)
        {
            var phieuXuatRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuat>>();
            var query = from px in phieuXuatRepo.GetAll()
                        where (px.NhaThuoc_MaNhaThuoc == maNhaThuoc)
                        orderby px.SoPhieuXuat descending
                        select px.SoPhieuXuat;

            var maxSoPhieu = query.First();

            return maxSoPhieu + 1;
        }

        // lấy danh sách thuốc chưa được Kiêm kê
        public List<ThuocModel> GetDrugsHaveNotInventoried(String maNhaThuoc, DateTime? fromDate, DateTime? toDate)
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

                                     where (pkk.NhaThuoc_MaNhaThuoc == maNhaThuoc && pkk.RecordStatusId == (int)RecordStatus.Activated
                                      && pkkct.RecordStatusId == (int)RecordStatus.Activated && ((fromDate == null && toDate == null)
                                     || (pkk.Created >= fromDate && pkk.Created < toDate)
                                     ))

                                     select pkkct.Thuoc_ThuocId;

            // danh sách thuốc thuộc các phiếu Kiêm kê đã tạo
            var thuocDaKiemKes = thuocDaKiemKeQuery.ToList();

            // Thuốc chưa kiểm kê là những thuốc KHÔNG thuộc danh sách trên
            var query = from dr in drugRepo

                        join dvt in donViTinhRepo
                        on dr.DonViXuatLe_MaDonViTinh equals dvt.MaDonViTinh

                        join nt in nhomThuocRepo
                        on dr.NhomThuoc_MaNhomThuoc equals nt.MaNhomThuoc

                        where (!thuocDaKiemKes.Contains(dr.ThuocId) && dr.NhaThuoc_MaNhaThuoc == maNhaThuoc)

                        select new ThuocModel
                        {
                            ThuocId = dr.ThuocId,
                            TenNhomThuoc = nt.TenNhomThuoc,
                            MaThuoc = dr.MaThuoc,
                            TenThuoc = dr.TenThuoc,
                            TenDonViTinh = dvt.TenDonViTinh,
                        };

            var thuocModels = query.ToList();
            return thuocModels;
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


            // nếu maNhomThuoc >= 0 thì lọc theo mã nhóm, ngược lại lọc theo thuocId
            if (maNhomThuoc >= 0)
            {
                thuocs = thuocs.Where(i => i.NhomThuoc_MaNhomThuoc == maNhomThuoc);
            }
            else if (drugIds != null && drugIds.Count() > 0)
            {
                thuocs = thuocs.Where(i => drugIds.Contains(i.ThuocId));
            }
            else if (!string.IsNullOrWhiteSpace(barcode))
            {
                thuocs = thuocs.Where(i => i.BarCode == barcode);
            }

            var thuocQuery = from t in thuocs
                             join nt in nhomThuocRepo on t.NhomThuoc_MaNhomThuoc equals nt.MaNhomThuoc
                             join dvt in donViTinhRepo on t.DonViXuatLe_MaDonViTinh equals dvt.MaDonViTinh

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
                                     pkk.DaCanKho == false && pkk.RecordStatusId == (int)RecordStatus.Activated
                                      && pkkct.RecordStatusId == (int)RecordStatus.Activated)

                                     select new
                                     {
                                         pkk.MaPhieuKiemKe,
                                         pkkct.Thuoc_ThuocId,
                                         pkkct.DonGia,
                                         pkkct.SoLo,
                                         pkkct.HanDung
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
                        //t.HanDungDateString = t.HanDung != null ? t.HanDung.Value.ToString("dd/MM/yyyy") : "";
                    }

                    // nếu thuốc này đã có trong phiếu Kiểm kê khác, gán mã phiếu đó vào MaPhieuKiemKeTonTai
                    var pkk = pkkChuaCanKhoResult.Where(x => x.Thuoc_ThuocId == t.ThuocId).FirstOrDefault();
                    if (pkk != null)
                    {
                        //t.Gia = pkk.DonGia;
                        t.MaPhieuKiemKeTonTai = pkk.MaPhieuKiemKe;
                        //t.SoLo = pkk.SoLo;
                        //t.HanDung = pkk.HanDung;
                        //t.HanDungDateString = t.HanDung != null ? t.HanDung.Value.ToString("dd/MM/yyyy") : "";
                    }



                    // TH HanDung nho hon MinTime, update lai HanDung
                    t.HanDung = t.HanDung <= MedConstants.MinProductionDataDate ? MedConstants.MinProductionDataDate : t.HanDung;
                    t.HanDungDateString = t.HanDung != null ? t.HanDung.Value.ToString("dd/MM/yyyy") : "";
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

            bool canKho = inventoryEditModel.IsCompareStore;
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
                             where (pkkct.PhieuKiemKe_MaPhieuKiemKe == inventoryId && pkkct.RecordStatusId == (int)RecordStatus.Activated
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
