using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using App.Common;
using App.Common.Data;
using App.Common.DI;
using Med.DbContext;
using Med.Entity;
using Med.Repository.Factory;
using Med.Service.Drug;
using Microsoft.Ajax.Utilities;
using PagedList;
using sThuoc.DAL;
using Med.Web.Extensions;
using sThuoc.Filter;
using sThuoc.Models;
using WebGrease.Css.Extensions;
using Microsoft.Reporting.WebForms;
using System.IO;
using System.Data;
using App.Common.MVC;
using Med.Service.Delivery;
using Med.Service.Receipt;
using Med.Web.Data.Session;
using Constants = MedMan.App_Start.Constants;
using KhachHang = sThuoc.Models.KhachHang;
using LoaiXuatNhap = sThuoc.Models.LoaiXuatNhap;
using NhaCungCap = sThuoc.Models.NhaCungCap;
using PhieuKiemKe = sThuoc.Models.PhieuKiemKe;
using PhieuKiemKeChiTiet = sThuoc.Models.PhieuKiemKeChiTiet;
using PhieuNhap = sThuoc.Models.PhieuNhap;
using PhieuNhapChiTiet = sThuoc.Models.PhieuNhapChiTiet;
using PhieuXuat = sThuoc.Models.PhieuXuat;
using PhieuXuatChiTiet = sThuoc.Models.PhieuXuatChiTiet;
using Thuoc = sThuoc.Models.Thuoc;
using UnitOfWork = sThuoc.Repositories.UnitOfWork;
using Med.Web.Filter;
using Med.Common.Enums;
using Med.Service.Report;
using Med.Common;
using Med.Web.Helpers;
using Med.Service.Helpers;
using App.Constants.Enums;

namespace Med.Web.Controllers
{
    public class PhieuKiemKesController : BaseController
    {
        private SecurityContext db = new SecurityContext();
        //private readonly string this.GetNhaThuoc().MaNhaThuoc = UserService.GetMaNhaThuoc();
        private readonly UnitOfWork unitOfWork = new UnitOfWork();
        // GET: PhieuKiemKes
        [SimpleAuthorize("Admin")]
        public ActionResult Index(string searchTen, int? page)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            ViewBag.searchTen = searchTen;
            var phieuKiemKes =
                db.PhieuKiemKes.Where(
                    x =>
                        (string.IsNullOrEmpty(searchTen) || x.PhieuKiemKeChiTiets.Any(f => f.Thuoc.TenThuoc.ToLower().Contains(searchTen.ToLower()))) &&
                        x.NhaThuoc.MaNhaThuoc == maNhaThuoc)
                    .OrderByDescending(m => m.Created)
                    .Include(p => p.CreatedBy)
                    .Include(p => p.NhaThuoc);
            const int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(phieuKiemKes.ToPagedList(pageNumber, pageSize));
        }

        //GET: PhieuKiemKes/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var phieuKiemKe =
                await
                    unitOfWork.PhieuKiemKeRepository.GetMany(
                        e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuKiemKe == id).FirstOrDefaultAsync();
            if (phieuKiemKe == null)
            {
                return HttpNotFound();
            }
            if (phieuKiemKe.DaCanKho)
            {
                //    phieuKiemKe.PhieuKiemKeChiTiets.ForEach(e =>
                //    {
                //        e.TonKho = calculateAvailableQuantity(e.Thuoc);
                //    });
                //}
                //else
                //{
                var list = new List<PhieuCanKhoItem>();
                if (phieuKiemKe.PhieuNhap != null)
                {
                    var item = new PhieuCanKhoItem();
                    item.MaPhieu = phieuKiemKe.PhieuNhap.MaPhieuNhap;
                    item.SoPhieu = phieuKiemKe.PhieuNhap.SoPhieuNhap;
                    item.LoaiPhieu = Constants.LoaiPhieu.PhieuNhap;//phieuKiemKe.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap;                    
                    item.SoLuong = phieuKiemKe.PhieuNhap.PhieuNhapChiTiets.Count();
                    list.Add(item);
                }

                if (phieuKiemKe.PhieuXuat != null)
                {
                    var item = new PhieuCanKhoItem();
                    item.MaPhieu = phieuKiemKe.PhieuXuat.MaPhieuXuat;
                    item.SoPhieu = phieuKiemKe.PhieuXuat.SoPhieuXuat;
                    item.LoaiPhieu = Constants.LoaiPhieu.PhieuXuat; //phieuKiemKe.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap;                    
                    item.SoLuong = phieuKiemKe.PhieuXuat.PhieuXuatChiTiets.Count();
                    list.Add(item);
                }


                ViewBag.CanKho = list;
            }

            return View(phieuKiemKe);
        }

        // GET: PhieuKiemKes/Create
        [SimpleAuthorize("Admin")]
        public ActionResult Create()
        {
            BackgroundJobHelper.EnqueueUpdateNewestInventories(null);
            var nhaThuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhaThuoc.MaNhaThuoc;
            var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            var list = db.NhomThuocs.Where(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc || x.NhaThuoc.MaNhaThuoc == maNhaThuocCha ).OrderBy(c => c.TenNhomThuoc).ToList();
            ViewBag.MaNhomThuoc = new SelectList(list, "MaNhomThuoc", "TenNhomThuoc");
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            ViewBag.SoPhieu = _generateAvaliableSoPhieu();
            return View();
        }
        [SimpleAuthorize("Admin")]
        public ActionResult Edit(int? id)
        {
            if (id <= 0)
            {
                return HttpNotFound();
            }
            BackgroundJobHelper.EnqueueUpdateNewestInventories(null);
            var nhaThuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhaThuoc.MaNhaThuoc;
            var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            var item = unitOfWork.PhieuKiemKeRepository.GetMany(e => e.MaPhieuKiemKe == id && e.NhaThuoc.MaNhaThuoc == maNhaThuoc).FirstOrDefault();

            if (item == null)
                return HttpNotFound();
            if (item.DaCanKho)
            {
                ViewBag.Message = "Không thể sửa phiếu đã cân kho";
                return View("Error");
            }
            var model = new PhieuKiemKeEditModel(item);
           
            var list = db.NhomThuocs.Where(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc || x.NhaThuoc.MaNhaThuoc == maNhaThuocCha).OrderBy(c => c.TenNhomThuoc).ToList();
            ViewBag.MaNhomThuoc = new SelectList(list, "MaNhomThuoc", "TenNhomThuoc");
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.MaNhaThuoc = maNhaThuoc;
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public ActionResult Edit(int? id, PhieuKiemKeEditModel phieuKiemKes)
        {
            var maPhieuKiemKe = 0;
            var nhaThuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhaThuoc.MaNhaThuoc;
            var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;

            var isValid = validateForm(phieuKiemKes);
            if (isValid)
            {
                switch (Request["action"].ToLower())
                {
                    case "cân kho":
                        maPhieuKiemKe = PerformInventories(phieuKiemKes, false, true);
                        break;

                    case "lưu phiếu":                      
                    default:
                        maPhieuKiemKe = PerformInventories(phieuKiemKes, false, false);
                        break;
                }
            }

            var list = db.NhomThuocs.Where(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc || x.NhaThuoc.MaNhaThuoc == maNhaThuocCha).OrderBy(c => c.TenNhomThuoc).ToList();
            ViewBag.MaNhomThuoc = new SelectList(list, "MaNhomThuoc", "TenNhomThuoc");
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            if (isValid)
                return RedirectToAction("Details", "PhieuKiemKes", new { id = maPhieuKiemKe });
            else
            {
                return View("Create");
            }
        }

        //[SimpleAuthorize("Admin")]
        public ActionResult In(int? id)
        {
            var nhathuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhathuoc.MaNhaThuoc;
            var result = unitOfWork.PhieuKiemKeRepository.Get(c => c.MaPhieuKiemKe == id).FirstOrDefault();
            var phieuKiemKeChiTiet = result.PhieuKiemKeChiTiets.OrderBy(x => x.Thuoc.NhomThuoc.TenNhomThuoc);
            if (result == null)
            {
                ViewBag.Message = "Phiếu kiểm kê không tồn tại #" + id;
                return View("Error");
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("STT");
            dt.Columns.Add("MaThuoc");
            dt.Columns.Add("TenNhom");
            dt.Columns.Add("TenHang");
            dt.Columns.Add("DVT");
            dt.Columns.Add("SoLuong");
            dt.Columns.Add("SoLuongThuc");
            string reportPath = "~/Reports/RptPhieuKiemKe_ChuaCan.rdlc";

            if (result.DaCanKho)
            {
                reportPath = "~/Reports/RptPhieuKiemKe_DaCan.rdlc";
                dt.Columns.Add("SoLuongChenhLech");
            }


            int i = 1;
            decimal tongtienhang = 0;
            decimal tienhang = 0;
            foreach (var item in phieuKiemKeChiTiet)
            {
                DataRow dr = dt.NewRow();
                dr["STT"] = i;
                dr["MaThuoc"] = item.Thuoc.MaThuoc;
                dr["TenNhom"] = item.Thuoc.NhomThuoc != null ? item.Thuoc.NhomThuoc.TenNhomThuoc : string.Empty;
                dr["TenHang"] = item.Thuoc.TenThuoc;
                dr["DVT"] = item.Thuoc.DonViXuatLe.TenDonViTinh;
                dr["SoLuong"] = item.TonKho.ToString("#,##0");
                dr["SoLuongThuc"] = item.ThucTe.HasValue ? item.ThucTe.Value.ToString("#,##0") : string.Empty;
                if (result.DaCanKho)
                {
                    dr["SoLuongChenhLech"] = item.ThucTe.HasValue ? (item.TonKho - item.ThucTe.Value).ToString() : string.Empty;
                }

                dt.Rows.Add(dr);

                tongtienhang += tienhang;
                i++;
            }
            
            List<ReportParameter> listParam = new List<ReportParameter>(){
                new ReportParameter("pNhaThuoc",nhathuoc.TenNhaThuoc),
                new ReportParameter("pDiaChiNhaThuoc",nhathuoc.DiaChi),
                new ReportParameter("pSDTNhaThuoc",nhathuoc.DienThoai),
                new ReportParameter("pNgayXuat",result.Created.Value.ToString("dd/MM/yyyy")),
                new ReportParameter("pPhieuXuat",result.MaPhieuKiemKe.ToString()),             
                new ReportParameter("pNhanVien",result.CreatedBy.TenDayDu)               
            };


            ReportViewer viewer = new ReportViewer();
            viewer.ProcessingMode = ProcessingMode.Local;
            viewer.LocalReport.ReportPath = Server.MapPath(reportPath);
            viewer.LocalReport.DataSources.Add(new ReportDataSource("DSPhieuXuat", dt));
            viewer.LocalReport.SetParameters(listParam);
            byte[] bytes = viewer.LocalReport.Render("PDF");
            Stream stream = new MemoryStream(bytes);

            return File(stream, "application/pdf");
        }

        private decimal calculateAvailableQuantity(Thuoc th)
        {
            //decimal quantity = 0;
            //var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            //var phieuXuatChiTiets =
            //    unitOfWork.PhieuXuatChiTietRepository.GetMany(
            //        e =>
            //            e.PhieuXuat.NhaThuoc.MaNhaThuoc == maNhaThuoc && !e.PhieuXuat.Xoa &&
            //            e.Thuoc.MaThuoc == thuoc.MaThuoc).AsEnumerable();
            //var phieuNhapChiTiets =
            //    unitOfWork.PhieuNhapChiTietRepository.GetMany(
            //        e =>
            //            e.PhieuNhap.NhaThuoc.MaNhaThuoc == maNhaThuoc && !e.PhieuNhap.Xoa &&
            //            e.Thuoc.MaThuoc == thuoc.MaThuoc).AsEnumerable();
            //// tinh tong so luong da nhap
            //phieuNhapChiTiets.ForEach(e =>
            //{
            //    if (thuoc.DonViThuNguyen != null && e.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)
            //    {
            //        quantity += e.SoLuong * thuoc.HeSo;
            //    }
            //    else
            //    {
            //        quantity += e.SoLuong;
            //    }
            //});
            //// tinh tong so luong da xuat
            //phieuXuatChiTiets.ForEach(e =>
            //{
            //    if (thuoc.DonViThuNguyen != null && e.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)
            //    {
            //        quantity -= e.SoLuong * thuoc.HeSo;
            //    }
            //    else
            //    {
            //        quantity -= e.SoLuong;
            //    }
            //});

            //return quantity;
            var service = IoC.Container.Resolve<IInventoryService>();
            var drugIds = new int[] { th.ThuocId };
            var drugQuantities = service.GetLastInventoryQuantities(WebSessionManager.Instance.CurrentDrugStoreCode,
                true, drugIds);
            double quanties = 0.0;
            if (drugQuantities.ContainsKey(th.ThuocId))
            {
                quanties = drugQuantities[th.ThuocId];
            }

            return (decimal)quanties;
        }
        // POST: PhieuKiemKes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public async Task<ActionResult> Create(PhieuKiemKeEditModel phieuKiemKes)
        {
            var maPhieuKiemKe = 0;
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var isValid = validateForm(phieuKiemKes);
            if (isValid)
            {
                switch (Request["action"].ToLower())
                {
                    case "cân kho":
                        maPhieuKiemKe = PerformInventories(phieuKiemKes, true, true);
                        break;

                    case "lưu phiếu":
                    default:
                        maPhieuKiemKe = PerformInventories(phieuKiemKes, true, false);
                        break;
                }
            }

            ViewBag.MaNhomThuoc = new SelectList(db.NhomThuocs, "MaNhomThuoc", "TenNhomThuoc");
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            if (isValid)
                return RedirectToAction("Details", "PhieuKiemKes", new { id = maPhieuKiemKe });
            else
            {
                return View("Create");
            }
        }

        private int PerformInventories(PhieuKiemKeEditModel phieuKiemKes, bool createNew, bool balancingInventory)
        {
            var maPhieuKiemKe = 0;
            var nhaThuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhaThuoc.MaNhaThuoc;
            var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            var receiptRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Entity.PhieuNhap>>();
            var deliveryRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Entity.PhieuXuat>>();
            var receiptItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Entity.PhieuNhapChiTiet>>();
            var deliveryItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Entity.PhieuXuatChiTiet>>();
            var inventoryRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Entity.PhieuKiemKe>>();
            var inventoryItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Entity.PhieuKiemKeChiTiet>>();
            var drugRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Entity.Thuoc>>();
            var drugService = IoC.Container.Resolve<IDrugManagementService>();
            var inventoryItems = new List<Entity.PhieuKiemKeChiTiet>();

            var drugCodes = phieuKiemKes.Items.Select(i => i.MaThuoc.ToLower()).ToList();
            var drugs = drugRepo.GetAll().Where(i => drugCodes.Contains(i.MaThuoc.ToLower()) && (i.NhaThuoc_MaNhaThuoc == maNhaThuoc || i.NhaThuoc_MaNhaThuoc == maNhaThuocCha))
                .ToDictionary(i => i.MaThuoc.ToLower(), i => new { i.ThuocId, i.DonViXuatLe_MaDonViTinh, i.GiaNhap });
            var drugIds = drugs.Select(i => i.Value.ThuocId).Distinct().ToArray();
            Entity.PhieuKiemKe phieuKiemKe = null;
            if (!createNew)
            {
                phieuKiemKe = inventoryRepo.GetAll().Where(i => i.MaPhieuKiemKe == phieuKiemKes.MaPhieuKiemKe).FirstOrDefault();
                inventoryItemRepo.Delete(i => i.PhieuKiemKe_MaPhieuKiemKe == phieuKiemKe.MaPhieuKiemKe);
                deliveryRepo.UpdateMany(i => i.MaPhieuXuat == phieuKiemKe.PhieuXuat_MaPhieuXuat, i => new Entity.PhieuXuat()
                {
                    RecordStatusID = (byte)RecordStatus.Deleted
                });
                receiptRepo.UpdateMany(i => i.MaPhieuNhap == phieuKiemKe.PhieuNhap_MaPhieuNhap, i => new Entity.PhieuNhap()
                {
                    RecordStatusID = (byte)RecordStatus.Deleted
                });
            }

            if (balancingInventory)
            {
                var nhaCungCap = EnsureNhaCungCapKiemKe(maNhaThuoc);
                var loaiKiemKe = EnsureLoaiXuatNhapKiemKe();

                var phieuXuat = new Entity.PhieuXuat()
                {
                    Created = createNew? DateTime.Now : phieuKiemKes.NgayTao,
                    CreatedBy_UserId = WebSecurity.GetCurrentUserId,
                    NhaCungCap_MaNhaCungCap = nhaCungCap.MaNhaCungCap,
                    NhaThuoc_MaNhaThuoc = maNhaThuoc,
                    SoPhieuXuat = _generateAvaliableSoPhieuXuat(),
                    NgayXuat = phieuKiemKes.NgayTao,
                    MaLoaiXuatNhap = loaiKiemKe.MaLoaiXuatNhap,
                    PreNoteDate = phieuKiemKes.NgayTao
                };
                
                deliveryRepo.Insert(phieuXuat);
                // kiem tra xem da co nha cung cap dieu chinh sau kiem ke chua? 

                var phieuNhap = new Entity.PhieuNhap()
                {
                    Created = createNew ? DateTime.Now : phieuKiemKes.NgayTao,
                    CreatedBy_UserId = WebSecurity.GetCurrentUserId,
                    NhaCungCap_MaNhaCungCap = nhaCungCap.MaNhaCungCap,
                    NhaThuoc_MaNhaThuoc = maNhaThuoc,
                    SoPhieuNhap = _generateAvaliableSoPhieuNhap(),
                    NgayNhap = phieuKiemKes.NgayTao,
                    LoaiXuatNhap_MaLoaiXuatNhap = loaiKiemKe.MaLoaiXuatNhap,
                    PreNoteDate = phieuKiemKes.NgayTao
                };
                receiptRepo.Insert(phieuNhap);

                if (createNew)
                {
                    phieuKiemKe = new Entity.PhieuKiemKe()
                    {
                        Created = phieuKiemKes.NgayTao,
                        CreatedBy_UserId = WebSecurity.GetCurrentUserId,
                        NhaThuoc_MaNhaThuoc = maNhaThuoc,
                        DaCanKho = true,
                        SoPhieu = _generateAvaliableSoPhieu()
                    };
                    inventoryRepo.Insert(phieuKiemKe);
                }
                else
                {
                    phieuXuat.Created = phieuKiemKe.Created;
                    phieuXuat.CreatedBy_UserId = phieuKiemKe.CreatedBy_UserId;
                    phieuXuat.Modified = DateTime.Now;
                    phieuXuat.ModifiedBy_UserId = WebSecurity.GetCurrentUserId;

                    phieuNhap.Created = phieuKiemKe.Created;
                    phieuNhap.CreatedBy_UserId = phieuKiemKe.CreatedBy_UserId;
                    phieuNhap.Modified = DateTime.Now;
                    phieuNhap.ModifiedBy_UserId = WebSecurity.GetCurrentUserId;

                    phieuKiemKe.Created = phieuKiemKes.NgayTao;
                    phieuKiemKe.Modified = DateTime.Now;
                    phieuKiemKe.ModifiedBy_UserId = WebSecurity.GetCurrentUserId;

                }
                phieuKiemKe.DaCanKho = true;

                var deliveryItems = new List<Entity.PhieuXuatChiTiet>();
                var receiptItems = new List<Entity.PhieuNhapChiTiet>();
                
                var lastReceiptDrugPrices = drugService.GetLastDrugPriceOnReceiptNotes(maNhaThuoc, drugIds);
                var lastDeliveryDrugPrices = drugService.GetLastDrugPriceOnDeliveryNotes(maNhaThuoc, drugIds);
                phieuKiemKes.Items.ForEach(e =>
                {
                    if (drugs.ContainsKey(e.MaThuoc.ToLower()))
                    {
                        var drugId = drugs[e.MaThuoc.ToLower()].ThuocId;
                        var retailUnit = drugs[e.MaThuoc.ToLower()].DonViXuatLe_MaDonViTinh;
                        var retailPrice = drugs[e.MaThuoc.ToLower()].GiaNhap;
                        inventoryItems.Add(new Entity.PhieuKiemKeChiTiet()
                        {
                            Thuoc_ThuocId = drugId,
                            ThucTe = e.SoLuongThucTe,
                            TonKho = e.SoLuongHeThong
                        });

                        var chenhLech = e.SoLuongThucTe.HasValue ? e.SoLuongHeThong - e.SoLuongThucTe : e.SoLuongHeThong;

                        if (chenhLech > 0)
                        {
                            // tao phieu xuat dieu chinh kiem ke   
                            var dItem = new Entity.PhieuXuatChiTiet()
                            {
                                DonViTinh_MaDonViTinh = retailUnit.Value,
                                NhaThuoc_MaNhaThuoc = maNhaThuoc,
                                SoLuong = chenhLech.Value,
                                Thuoc_ThuocId = drugId,
                                GiaXuat = retailPrice
                            };
                            deliveryItems.Add(dItem);
                            if (lastReceiptDrugPrices.ContainsKey(drugId))
                            {
                                //Tôi comment lại câu lệnh này để ông xem lại giá nhập gần nhất
                                dItem.GiaXuat = (Decimal)lastReceiptDrugPrices[drugId];
                            }

                        }
                        else if (chenhLech < 0)
                        {
                            // tao phieu nhap dieu chinh kiem ke        
                            var rItem = new Entity.PhieuNhapChiTiet()
                            {
                                DonViTinh_MaDonViTinh = retailUnit,
                                NhaThuoc_MaNhaThuoc = maNhaThuoc,
                                SoLuong = chenhLech.Value*-1,
                                Thuoc_ThuocId = drugId,
                                GiaNhap = retailPrice,
                                HanDung = null
                            };
                            receiptItems.Add(rItem);
                            if (lastReceiptDrugPrices.ContainsKey(drugId))
                            {
                                //Tôi comment lại câu lệnh này để ông check lại phần giá nhập gần nhất
                                rItem.GiaNhap = (Decimal)lastReceiptDrugPrices[drugId];
                            }
                        }
                    }
                });
                phieuNhap.TongTien = receiptItems.Sum(e => e.SoLuong * e.GiaNhap);
                phieuNhap.DaTra = phieuNhap.TongTien;
                phieuXuat.TongTien = deliveryItems.Sum(e => e.SoLuong * e.GiaXuat);
                phieuXuat.DaTra = phieuXuat.TongTien;
                receiptRepo.Commit();
                
                maPhieuKiemKe = createNew ? phieuKiemKe.MaPhieuKiemKe : phieuKiemKes.MaPhieuKiemKe;
                inventoryItems.ForEach(i =>
                {
                    i.PhieuKiemKe_MaPhieuKiemKe = maPhieuKiemKe;
                });
                deliveryItems.ForEach(i =>
                {
                    i.PhieuXuat_MaPhieuXuat = phieuXuat.MaPhieuXuat;
                    i.IsModified = true;
                });
                receiptItems.ForEach(i =>
                {
                    i.PhieuNhap_MaPhieuNhap = phieuNhap.MaPhieuNhap;
                    i.IsModified = true;
                });

                maPhieuKiemKe = phieuKiemKe.MaPhieuKiemKe;
                NoteServiceHelper.ApplyAdditionalInfos(deliveryItems.ToArray());
                NoteServiceHelper.ApplyAdditionalInfos(receiptItems.ToArray());
                deliveryItemRepo.InsertMany(deliveryItems);
                receiptItemRepo.InsertMany(receiptItems);
                inventoryItemRepo.InsertMany(inventoryItems);
                inventoryRepo.UpdateMany(i => i.MaPhieuKiemKe == maPhieuKiemKe, i => new Entity.PhieuKiemKe()
                {
                    PhieuNhap_MaPhieuNhap = phieuNhap.MaPhieuNhap,
                    PhieuXuat_MaPhieuXuat = phieuXuat.MaPhieuXuat
                });
                BackgroundJobHelper.EnqueueMakeAffectedChangesRelatedDeliveryNotes(phieuXuat.MaPhieuXuat);
                BackgroundJobHelper.EnqueueMakeAffectedChangesRelatedReceiptNotes(phieuNhap.MaPhieuNhap);
            }
            else
            {
                if (createNew)
                {
                    phieuKiemKe = new Entity.PhieuKiemKe()
                    {
                        Created = phieuKiemKes.NgayTao,
                        CreatedBy_UserId = WebSecurity.GetCurrentUserId,
                        NhaThuoc_MaNhaThuoc = maNhaThuoc,
                        DaCanKho = false,
                        SoPhieu = phieuKiemKes.SoPhieu
                    };
                    inventoryRepo.Insert(phieuKiemKe);
                    inventoryRepo.Commit();
                }                

                maPhieuKiemKe = createNew ? phieuKiemKe.MaPhieuKiemKe : phieuKiemKes.MaPhieuKiemKe;
                phieuKiemKes.Items.ForEach(e =>
                {
                    if (drugs.ContainsKey(e.MaThuoc.ToLower()))
                    {
                        var drugId = drugs[e.MaThuoc.ToLower()].ThuocId;
                        inventoryItems.Add(new Entity.PhieuKiemKeChiTiet()
                        {
                            Thuoc_ThuocId = drugId,
                            ThucTe = e.SoLuongThucTe,
                            TonKho = e.SoLuongHeThong,
                            PhieuKiemKe_MaPhieuKiemKe = maPhieuKiemKe
                        });
                    }
                });
                inventoryItemRepo.InsertMany(inventoryItems);
            }
            if (maPhieuKiemKe > 0 && balancingInventory)
            {
                inventoryRepo.UpdateMany(i => i.MaPhieuKiemKe == maPhieuKiemKe, i => new Med.Entity.PhieuKiemKe()
                {
                    DaCanKho = true
                });
            }           
            

            return maPhieuKiemKe;
        }

        private bool validateForm(PhieuKiemKeEditModel phieuKiemke)
        {
            if (phieuKiemke.Items == null || !phieuKiemke.Items.Any())
            {
                ViewBag.ErrorMessage = "Không có mã thuốc nào trong danh sách";
                return false;
            }
            return true;
        }

        private LoaiXuatNhap EnsureLoaiXuatNhapKiemKe()
        {
            var loaiXuatNhapKiemKe = unitOfWork.LoaiXuatNhapRepository.GetMany(
                e => e.TenLoaiXuatNhap == Constants.Default.ConstantEntities.LoaiXuatNhapKiemKe)
                .OrderBy(e => e.MaLoaiXuatNhap)
                .FirstOrDefault();
            if (loaiXuatNhapKiemKe == null)
            {
                loaiXuatNhapKiemKe = new LoaiXuatNhap()
                {
                    TenLoaiXuatNhap = Constants.Default.ConstantEntities.LoaiXuatNhapKiemKe
                };
                unitOfWork.LoaiXuatNhapRepository.Insert(loaiXuatNhapKiemKe);
            }
            return loaiXuatNhapKiemKe;
        }
        
        private long _generateAvaliableSoPhieuNhap()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var maxSoPhieu = unitOfWork.PhieuNhapRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).Any() ? unitOfWork.PhieuNhapRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).Max(e => e.SoPhieuNhap) : 0;
            return maxSoPhieu + 1;
        }
        private long _generateAvaliableSoPhieuXuat()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var maxSoPhieu = unitOfWork.PhieuXuatRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).Any() ? unitOfWork.PhieuXuatRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).Max(e => e.SoPhieuXuat) : 0;
            return maxSoPhieu + 1;
        }
        private NhaCungCap EnsureNhaCungCapKiemKe(string maNhaThuoc)
        {
            var nhaCungCapKiemKe =
                unitOfWork.NhaCungCapRespository.GetMany(
                    e =>
                        e.NhaThuoc.MaNhaThuoc == maNhaThuoc &&
                        e.TenNhaCungCap == Constants.Default.ConstantEntities.NhaCungCapKiemKe).FirstOrDefault();
            if (nhaCungCapKiemKe == null)
            {

                nhaCungCapKiemKe = new NhaCungCap()
                {
                    TenNhaCungCap = Constants.Default.ConstantEntities.KhachHangKiemKe,
                    Created = DateTime.Now,
                    CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId),
                    NhaThuoc = unitOfWork.NhaThuocRepository.GetById(maNhaThuoc),
                    NhomNhaCungCap = unitOfWork.NhomNhaCungCapRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).OrderBy(e => e.MaNhomNhaCungCap).First(),
                    SupplierTypeId = (int)SupplierType.InventoryAdjustment
                };
                unitOfWork.NhaCungCapRespository.Insert(nhaCungCapKiemKe);
            }
            return nhaCungCapKiemKe;
        }

        private KhachHang EnsureKhachHangKiemKe(string maNhaThuoc)
        {
            var khachHangKiemKe =
                unitOfWork.KhachHangRepository.GetMany(
                    e =>
                        e.NhaThuoc.MaNhaThuoc == maNhaThuoc &&
                        e.TenKhachHang == Constants.Default.ConstantEntities.KhachHangKiemKe).FirstOrDefault();
            if (khachHangKiemKe == null)
            {

                khachHangKiemKe = new KhachHang()
                {
                    TenKhachHang = Constants.Default.ConstantEntities.KhachHangKiemKe,
                    Created = DateTime.Now,
                    CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId),
                    NhaThuoc = unitOfWork.NhaThuocRepository.GetById(maNhaThuoc),
                    NhomKhachHang = unitOfWork.NhomKhachHangRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).OrderBy(e => e.MaNhomKhachHang).First(),
                    CustomerTypeId = (int)CustomerType.InventoryAdjustment
                };
                unitOfWork.KhachHangRepository.Insert(khachHangKiemKe);
            }
            return khachHangKiemKe;
        }

        // GET: PhieuKiemKes/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    PhieuKiemKe phieuKiemKe = await db.PhieuKiemKes.FindAsync(id);
        //    if (phieuKiemKe == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.MaNhomThuoc = new SelectList(db.NhomThuocs, "MaNhomThuoc", "TenNhomThuoc");
        //    ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
        //    ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
        //    return View(phieuKiemKe);
        //}

        //// POST: PhieuKiemKes/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "MaPhieuKiemKe,NgayTao,MaNguoiTao,MaPhieuNhap,MaPhieuXuat,MaNhaThuoc,PhieuKiemKeChiTiets")] PhieuKiemKe phieuKiemKe)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (phieuKiemKe.MaPhieuNhap == 0 || phieuKiemKe.MaPhieuXuat == 0)
        //        {
        //            var lstNhap = phieuKiemKe.PhieuKiemKeChiTiets.Where(x => x.ThucTe - x.TonKho > 0);
        //            var lstXuat = phieuKiemKe.PhieuKiemKeChiTiets.Where(x => x.ThucTe - x.TonKho < 0);
        //            if (!lstNhap.Any())
        //                phieuKiemKe.MaPhieuNhap = null;
        //            if (!lstXuat.Any())
        //                phieuKiemKe.MaPhieuXuat = null;

        //            //Tao list Nhap
        //            var phieuKiemKeChiTiets = lstNhap as IList<PhieuKiemKeChiTiet> ?? lstNhap.ToList();
        //            if (phieuKiemKeChiTiets.Any())
        //            {
        //                phieuKiemKe.MaPhieuNhap = GeneratePhieuNhap(phieuKiemKeChiTiets);

        //            }
        //            //Tao list Xuat                    
        //            var kiemKeChiTiets = lstXuat as IList<PhieuKiemKeChiTiet> ?? lstXuat.ToList();
        //            if (kiemKeChiTiets.Any())
        //            {
        //                phieuKiemKe.MaPhieuXuat = GeneratePhieuXuat(kiemKeChiTiets);
        //            }
        //        }
        //        db.Entry(phieuKiemKe).State = EntityState.Modified;
        //        foreach (var phieu in phieuKiemKe.PhieuKiemKeChiTiets)
        //        {
        //            db.Entry(phieu).State = EntityState.Modified;
        //        }

        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Edit", new { id = phieuKiemKe.MaPhieuKiemKe });
        //    }
        //    ViewBag.MaNhomThuoc = new SelectList(db.NhomThuocs, "MaNhomThuoc", "TenNhomThuoc");
        //    ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
        //    ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
        //    return View(phieuKiemKe);
        //}

        // GET: PhieuKiemKes/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var phieuKiemKe =
                await
                    unitOfWork.PhieuKiemKeRepository.GetMany(
                        e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuKiemKe == id).FirstOrDefaultAsync();
            if (phieuKiemKe == null)
            {
                return HttpNotFound();
            }
            //if (!phieuKiemKe.DaCanKho)
            //{
            //    phieuKiemKe.PhieuKiemKeChiTiets.ForEach(e =>
            //    {
            //        e.TonKho = calculateAvailableQuantity(e.Thuoc);
            //    });
            //}
            return View(phieuKiemKe);
        }

        [HttpPost]
        // [Audit]
        public async Task<ActionResult> Delete(PhieuKiemKe pkk)
        {
            if (pkk == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var phieuKiemKe =
                await
                    unitOfWork.PhieuKiemKeRepository.GetMany(
                        e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuKiemKe == pkk.MaPhieuKiemKe).FirstOrDefaultAsync();
            if (phieuKiemKe == null)
            {
                return HttpNotFound();
            }
            // delete 
            if (phieuKiemKe.PhieuNhap != null)
            {
                var service = IoC.Container.Resolve<IReceiptNoteService>();
                var noteTypeId = service.DeleteReceiptNote(MedSessionManager.CurrentDrugStoreCode, phieuKiemKe.PhieuNhap.MaPhieuNhap, MedSessionManager.CurrentUserId);
               
            }
                //unitOfWork.PhieuNhapRepository.Delete(phieuKiemKe.PhieuNhap);
            if (phieuKiemKe.PhieuXuat != null)
            {
                var service = IoC.Container.Resolve<IDeliveryNoteService>();
                var noteTypeId = service.DeleteDeliveryNote(MedSessionManager.CurrentDrugStoreCode, phieuKiemKe.PhieuXuat.MaPhieuXuat, MedSessionManager.CurrentUserId);
                
            }

            unitOfWork.PhieuKiemKeRepository.Delete(phieuKiemKe);
            unitOfWork.Save();
            return RedirectToAction("index");
        }

        //// POST: PhieuKiemKes/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(int id)
        //{
        //    PhieuKiemKe phieuKiemKe = await db.PhieuKiemKes.FindAsync(id);
        //    db.PhieuKiemKes.Remove(phieuKiemKe);
        //    await db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}

        [HttpPost]
        public JsonResult CanKho(List<KiemKePostModel> kiemKePostModels)
        {
            var phieuNhapId = 0;
            var phieuXuatId = 0;
            var result = new { phieuNhap = phieuNhapId, phieuXuat = phieuXuatId };

            return new JsonResult() { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        #region Util
        private int _generateAvaliableSoPhieu()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var maxSoPhieu = unitOfWork.PhieuKiemKeRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).Any() ? unitOfWork.PhieuKiemKeRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).Max(e => e.SoPhieu) : 0;
            return maxSoPhieu + 1;
        }
        /// <summary>
        /// Generate PhieuNhap based on PhieuKiemKe
        /// </summary>
        /// <param name="lstItems"></param>
        /// <returns></returns>
        private int GeneratePhieuNhap(IEnumerable<PhieuKiemKeChiTiet> lstItems)
        {
            var phieuNhap = new PhieuNhap
            {
                MaPhieuNhap = 0,
                //MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc,
                //MaNguoiTao = WebSecurity.GetCurrentUserId,
                //MaLoaiXuatNhap = 3,
                //MaKhachHang = 1,
                //NgayTao = DateTime.Today,
                NgayNhap = DateTime.Today,
                DaTra = 0,
                VAT = 0,
                SoPhieuNhap = db.PhieuNhaps.Where(x => x.NhaThuoc.MaNhaThuoc == this.GetNhaThuoc().MaNhaThuoc).OrderByDescending(x => x.SoPhieuNhap).FirstOrDefault().SoPhieuNhap,
                PhieuNhapChiTiets = new List<PhieuNhapChiTiet>()
            };
            foreach (var nhap in lstItems)
            {
                phieuNhap.PhieuNhapChiTiets.Add(new PhieuNhapChiTiet()
                {
                    //ThuocId = nhap.ThuocId,
                    //MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc,
                    // MaDonViTinh = db.Thuocs.FirstOrDefault(x => x.ThuocId == nhap.ThuocId).DonViXuatLe.MaDonViTinh,
                    SoLuong = nhap.ThucTe.HasValue ? nhap.ThucTe.Value - nhap.TonKho : nhap.TonKho * -1,
                    //HanDung = db.PhieuNhapChiTiets.Where(x => x.MaNhaThuoc == this.GetNhaThuoc().MaNhaThuoc).OrderByDescending(x => x.HanDung).FirstOrDefault().HanDung,
                    MaPhieuNhapCt = 0
                });
            }
            db.PhieuNhaps.Add(phieuNhap);
            db.SaveChanges();

            return phieuNhap.MaPhieuNhap;
        }

        /// <summary>
        /// Generate PhieuXuat based on PhieuKiemKe
        /// </summary>
        /// <param name="lstItems"></param>
        /// <returns></returns>
        private int GeneratePhieuXuat(IEnumerable<PhieuKiemKeChiTiet> lstItems)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var phieuXuat = new PhieuXuat
            {
                MaPhieuXuat = 0,
                NhaThuoc = unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc),
                CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId),
                MaLoaiXuatNhap = 3,
                // KhachHang = 1,
                // NgayTao = DateTime.Today,
                NgayXuat = DateTime.Today,
                DaTra = 0,
                VAT = 0,
                SoPhieuXuat = db.PhieuXuats.Where(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc).OrderByDescending(x => x.SoPhieuXuat).FirstOrDefault().SoPhieuXuat,
                PhieuXuatChiTiets = new List<PhieuXuatChiTiet>()
            };
            var thuocsUtil = new ThuocsUtil(db, this.GetNhaThuoc().MaNhaThuoc);
            foreach (var xuat in lstItems)
            {
                //var lstThuoc = thuocsUtil.GetThuocsLeft(xuat.ThuocId, Math.Abs((int)(xuat.TonKho - xuat.ThucTe))).ToList();
                var maDonViTinh =
                    db.Thuocs.FirstOrDefault(x => x.ThuocId == xuat.Thuoc.ThuocId)
                        .DonViXuatLe.MaDonViTinh;
                //foreach (var thuoc in lstThuoc)
                //{
                phieuXuat.PhieuXuatChiTiets.Add(new PhieuXuatChiTiet()
                {
                    //ThuocId = xuat.ThuocId,
                    // MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc,
                    // MaDonViTinh = maDonViTinh,
                    //SoLuong = thuoc.SoLuong,
                    SoLuong = Math.Abs((int)(xuat.TonKho - xuat.ThucTe)),
                    //HanDung = thuoc.HanDung,
                    MaPhieuXuatCt = 0
                });
                //}
                //if (lstThuoc.Count > 0 && lstThuoc.ElementAt(0).Message != null)
                //{
                //    phieuXuat.PhieuXuatChiTiets.Add(new PhieuXuatChiTiet()
                //    {
                //        ThuocId = xuat.ThuocId,
                //        MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc,
                //        MaDonViTinh = maDonViTinh,
                //        SoLuong = int.Parse(lstThuoc.ElementAt(0).Message),
                //        //HanDung =
                //        //    db.PhieuXuatChiTiets.Where(x => x.ThuocId == xuat.ThuocId)
                //        //        .OrderByDescending(x => x.HanDung)
                //        //        .FirstOrDefault()
                //        //        .HanDung,
                //        MaPhieuXuatCt = 0
                //    });
                //}

            }
            db.PhieuXuats.Add(phieuXuat);
            db.SaveChanges();

            return phieuXuat.MaPhieuXuat;
        }

        //private IEnumerable<CurrentThuoc> GetThuocTonKho(int thuocId, int soLuong)
        //{
        //    var qry =
        //       db.PhieuNhapChiTiets.Where(x => x.MaNhaThuoc == this.GetNhaThuoc().MaNhaThuoc && x.ThuocId == thuocId)
        //           .GroupBy(y => y.HanDung).OrderBy(x => x.Key).Select(z => new CurrentThuoc
        //           {
        //               HanDung = z.Key,
        //               MaPhieuNhapCt = z.Select(a => a.MaPhieuNhapCt),
        //               SoLuong = z.Sum(a => a.SoLuong)
        //           }).ToList();

        //    var currentTake = 0;
        //    var result = new List<CurrentThuoc>();
        //    foreach (var currentThuoc in qry)
        //    {

        //        var currentSoLuong = 0;
        //        var thuoc = currentThuoc;
        //        var thuocSold = db.PhieuXuatChiTiets.Where(
        //            x => x.MaNhaThuoc == this.GetNhaThuoc().MaNhaThuoc && x.ThuocId == thuocId && x.HanDung == thuoc.HanDung).Select(x => x.SoLuong).DefaultIfEmpty(0).Sum();

        //        currentSoLuong = thuoc.SoLuong - thuocSold;
        //        if (currentSoLuong <= 0)
        //            break;
        //        if (currentSoLuong >= soLuong - currentTake)
        //        {
        //            result.Add(new CurrentThuoc() { HanDung = currentThuoc.HanDung, SoLuong = soLuong - currentTake });
        //            currentTake += soLuong - currentTake;
        //            break;
        //        }
        //        result.Add(new CurrentThuoc() { HanDung = currentThuoc.HanDung, SoLuong = currentSoLuong });

        //        currentTake += currentSoLuong;
        //    }
        //    if (currentTake < soLuong && result.Count > 0)
        //    {
        //        result[0].Message = (soLuong - currentTake).ToString(CultureInfo.InvariantCulture);
        //    }
        //    return result;
        //}
    }
        #endregion
}

public class KiemKePostModel
{
    public int ThuocId { get; set; }
    public int TonKho { get; set; }
    public int ThucTe { get; set; }
}