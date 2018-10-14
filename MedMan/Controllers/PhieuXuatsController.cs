using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using App.Common.DI;
using Med.Common;
using Med.Service.Receipt;
using Med.Web.Data.Session;
using Newtonsoft.Json;
using MedMan.App_Start;
using sThuoc.DAL;
using sThuoc.Enums;
using Med.Web.Extensions;
using sThuoc.Filter;
using sThuoc.Models;
using sThuoc.Repositories;
using WebGrease.Css.Extensions;
using sThuoc.Models.ViewModels;
using Excel;
using sThuoc.Utils;
using Microsoft.Reporting.WebForms;
using App.Common.MVC;
using Med.Service.Delivery;
using Med.Web.Filter;
using Med.Service.Drug;
using Med.Web.Helpers;
using App.Common.Helpers;
using App.Constants.Enums;

namespace Med.Web.Controllers
{
    public class PhieuXuatsController : BaseController
    {
        private SecurityContext db = new SecurityContext();
        //private readonly string this.GetNhaThuoc().MaNhaThuoc = UserService.GetMaNhaThuoc();
        public ImportError ImportError = new ImportError();
        public UnitOfWork unitOfWork = new UnitOfWork();

        // GET: PhieuXuats
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Index()
        {
            var phieuXuats = db.PhieuXuats.Include(p => p.BacSy).Include(p => p.KhachHang).Include(p => p.LoaiXuatNhap).Include(p => p.CreatedBy).Include(p => p.NhaThuoc);
            return View(await phieuXuats.ToListAsync());
        }

        // GET: PhieuXuats/Details/5
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            PhieuXuat phieuXuat =
                await
                    unitOfWork.PhieuXuatRepository.GetMany(
                        e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuXuat == id).FirstAsync();
            if (phieuXuat == null)
            {
                return HttpNotFound();
            }
            // convert model to modelView
            var model = new PhieuXuatEditModel(phieuXuat);
            var service = IoC.Container.Resolve<IDeliveryNoteService>();
            var dsService = IoC.Container.Resolve<IDrugStoreService>();
            model.CanTransitWarehouse = service.CanTransitWarehouse(maNhaThuoc, id.Value);
            ViewBag.DrugStores = JsonConvert.SerializeObject(dsService.GetRelatedDrugStores(maNhaThuoc, true));
            ViewBag.SoPhieu = model.SoPhieuXuat;
            ViewBag.LoaiPhieu = model.MaLoaiXuatNhap;
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;

            ViewBag.MaDonViTinh = _getListDonViTinh();
            return View(model);
        }

        // GET: PhieuXuats/Details/5        
        public async Task<ActionResult> InDetails(int? id, EBillType BillType)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            PhieuXuat phieuXuat =
                await
                    unitOfWork.PhieuXuatRepository.GetMany(
                        e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuXuat == id).FirstAsync();
            if (phieuXuat == null)
            {
                return HttpNotFound();
            }
            // convert model to modelView
            //var model = new PhieuXuatEditModel(phieuXuat);

            // convert model to modelView
            var model = new PhieuXuatEditModel(phieuXuat);
            var service = IoC.Container.Resolve<IDeliveryNoteService>();
            var dsService = IoC.Container.Resolve<IDrugStoreService>();
            model.CanTransitWarehouse = service.CanTransitWarehouse(maNhaThuoc, id.Value);
            ViewBag.DrugStores = JsonConvert.SerializeObject(dsService.GetRelatedDrugStores(maNhaThuoc, true));
            //
            ViewBag.SoPhieu = model.SoPhieuXuat;
            ViewBag.LoaiPhieu = model.MaLoaiXuatNhap;
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            ViewBag.BillType = BillType;
            ViewBag.MaDonViTinh = _getListDonViTinh();
            return View(model);
        }
        // GET: PhieuXuats/Create
        [SimpleAuthorize("Admin")]       
        public ActionResult Create(string loaiPhieu,string ngayxuat)
        {
            BackgroundJobHelper.EnqueueUpdateNewestInventories(null);
            ViewBag.MaBacSy = new SelectList(_getListBacSy(), "MaBacSy", "TenBacSy");
            ViewBag.MaKhachHang = new SelectList(_getListKhachHang(), "MaKhachHang", "TenKhachHang");
            if (string.IsNullOrEmpty(loaiPhieu))
            {
                loaiPhieu = "2";
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;            
            ViewBag.SoPhieu = _generateAvaliableSoPhieu();
            ViewBag.LoaiPhieu = loaiPhieu;
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.MaLoaiXuatNhap = new SelectList(db.LoaiXuatNhaps.Where(c => !c.IsHidden.HasValue || !c.IsHidden.Value).ToList(), "MaLoaiXuatNhap", "TenLoaiXuatNhap");
            ViewBag.MaNhaCungCap = new SelectList(_getListNhaCungCap(), "MaNhaCungCap", "TenNhaCungCap");
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            ViewBag.MaNguoiTao = WebSecurity.GetCurrentUserId;
            ViewBag.NgayXuat = ngayxuat == null ? DateTime.Now.ToString("dd/MM/yyyy") : ngayxuat;
            //ViewBag.MaThuoc = new SelectList(db.Thuocs, "MaThuoc", "TenThuoc");
            ViewBag.MaDonViTinh = _getListDonViTinh();
            return View(new PhieuXuatEditModel());
        }
        // POST: PhieuXuats/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public async Task<ActionResult> Create(PhieuXuatEditModel phieuXuat)
        {
            ModelState.Remove("NgayXuat");
            if (!phieuXuat.NgayXuat.HasValue || phieuXuat.NgayXuat.Value < MedConstants.MinProductionDataDate)
            {
                ViewBag.Message = "Ngày xuất hàng không hợp lệ. Ngày xuất hàng không được trống và phải lớn hơn ngày 01-01-2010.";
                return View("Error");
            }

            foreach (var state in ModelState.Keys.ToList().Where(c => c.Contains("ChietKhau")))
            {
                ModelState.Remove(state);
            }

            if (ModelState.IsValid)
            {
                // validate 
                var nhathuoc = unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc);
                var dtNow = DateTime.Now;
                var createDate = dtNow;
                var noteItems = new List<PhieuXuatChiTiet>();
                if (phieuXuat.NgayXuat.HasValue)
                {
                    var noteDate = phieuXuat.NgayXuat.Value;
                    createDate = new DateTime(noteDate.Year, noteDate.Month, noteDate.Day
                        , dtNow.Hour, dtNow.Minute, dtNow.Second); 
                }

                var phieuXuatMoi = new PhieuXuat()
                {
                    Created = DateTime.Now,
                    CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId),
                    NhaThuoc = nhathuoc,
                    DaTra = phieuXuat.DaTra,
                    DienGiai = phieuXuat.DienGiai,
                    LoaiXuatNhap = unitOfWork.LoaiXuatNhapRepository.GetById(phieuXuat.MaLoaiXuatNhap),
                    NgayXuat = createDate,
                    SoPhieuXuat = _generateAvaliableSoPhieu(),
                    VAT = phieuXuat.VAT,
                    RecordStatusID = (byte)RecordStatus.Activated,
                    PreNoteDate = createDate
                };
                if (phieuXuat.MaBacSy > 0)
                    phieuXuatMoi.BacSy = unitOfWork.BacSyRespository.GetById(phieuXuat.MaBacSy);
                if (phieuXuat.MaKhachHang > 0)
                {
                    phieuXuatMoi.KhachHang = unitOfWork.KhachHangRepository.GetById(phieuXuat.MaKhachHang);
                }
                if (phieuXuat.MaNhaCungCap > 0)
                {
                    phieuXuatMoi.NhaCungCap = unitOfWork.NhaCungCapRespository.GetById(phieuXuat.MaNhaCungCap);
                }
                unitOfWork.PhieuXuatRepository.Insert(phieuXuatMoi);
                switch (phieuXuat.BillType)
                {
                    case EBillType.Manual:
                        // details
                        if (phieuXuat.PhieuXuatChiTiets != null)
                        {
                            foreach (var detail in phieuXuat.PhieuXuatChiTiets)
                            {
                                if (detail.SoLuong > 0 && detail.GiaXuat >= 0)
                                {
                                    //  chi them moi item  khi so luong va gia nhap phu hop
                                    var item = new PhieuXuatChiTiet()
                                    {
                                        DonViTinh = unitOfWork.DonViTinhRepository.GetById(detail.MaDonViTinh),
                                        GiaXuat = detail.GiaXuat,
                                        PhieuXuat = phieuXuatMoi,
                                        SoLuong = detail.SoLuong,
                                        Thuoc = unitOfWork.ThuocRepository.GetById(detail.ThuocId),
                                        NhaThuoc = nhathuoc,
                                        NhaThuoc_MaNhaThuoc = nhathuoc.MaNhaThuoc,
                                        DonViTinh_MaDonViTinh = detail.MaDonViTinh,
                                        Thuoc_ThuocId = detail.ThuocId
                                    };
                                    Decimal chietKhau = 0;
                                    Decimal.TryParse(detail.ChietKhau, out chietKhau);
                                    item.ChietKhau = chietKhau;
                                    phieuXuatMoi.TongTien += item.SoLuong * item.GiaXuat;
                                    // - (item.SoLuong* item.GiaNhap*item.ChietKhau)
                                    if (item.ChietKhau > 0)
                                    {
                                        phieuXuatMoi.TongTien -= item.SoLuong * item.GiaXuat * item.ChietKhau / 100;
                                    }
                                    unitOfWork.PhieuXuatChiTietRepository.Insert(item);
                                    noteItems.Add(item);
                                }

                            }
                        }
                        break;
                    case EBillType.Barcode:
                        // details
                        if (!string.IsNullOrEmpty(phieuXuat.JsonDrugOderItems))
                        {
                            var drugOrderItems = JsonConvert.DeserializeObject<List<ShortDrugInfo>>(phieuXuat.JsonDrugOderItems);

                            foreach (var detail in drugOrderItems)
                            {
                                if (detail.Quantity > 0 && detail.TotalPrice > 0)
                                {
                                    //  chi them moi item  khi so luong va gia nhap phu hop
                                    var item = new PhieuXuatChiTiet()
                                    {
                                        ChietKhau = 0,
                                        DonViTinh = unitOfWork.DonViTinhRepository.GetById(detail.SelectedUnit),
                                        GiaXuat = detail.Price,
                                        PhieuXuat = phieuXuatMoi,
                                        SoLuong = (Decimal)detail.Quantity,
                                        Thuoc = unitOfWork.ThuocRepository.GetById(detail.ID),
                                        NhaThuoc = nhathuoc,
                                        NhaThuoc_MaNhaThuoc = nhathuoc.MaNhaThuoc,
                                        DonViTinh_MaDonViTinh = detail.SelectedUnit,
                                        Thuoc_ThuocId = detail.ID
                                    };
                                    phieuXuatMoi.TongTien += item.SoLuong * item.GiaXuat;
                                    // - (item.SoLuong* item.GiaNhap*item.ChietKhau)
                                    if (item.ChietKhau > 0)
                                    {
                                        phieuXuatMoi.TongTien -= item.SoLuong * item.GiaXuat * item.ChietKhau / 100;
                                    }
                                    if (phieuXuatMoi.KhachHang.TenKhachHang == "Khách hàng lẻ") // Khach hang le
                                    {
                                        phieuXuatMoi.DaTra = phieuXuatMoi.TongTien;
                                    }
                                    unitOfWork.PhieuXuatChiTietRepository.Insert(item);
                                    noteItems.Add(item);
                                }

                            }
                        }
                        break;
                }

                if (phieuXuat.VAT > 0)
                {
                    phieuXuatMoi.TongTien += phieuXuatMoi.TongTien * phieuXuatMoi.VAT / 100;
                }
                if (phieuXuatMoi.MaLoaiXuatNhap == 4)
                {
                    // khach hang tra lai, auto fill datra
                    phieuXuatMoi.DaTra = phieuXuatMoi.TongTien;
                }
                phieuXuatMoi.IsDebt = (double)Math.Abs(phieuXuatMoi.TongTien - phieuXuatMoi.DaTra) > MedConstants.EspAmount;
                if (phieuXuatMoi.PhieuXuatChiTiets != null && phieuXuatMoi.PhieuXuatChiTiets.Any())
                {
                    NoteHelper.ApplyAdditionalInfos(nhathuoc.MaNhaThuoc, noteItems.ToArray());
                    unitOfWork.Save();
                                 
                    BackgroundJobHelper.EnqueueMakeAffectedChangesRelatedDeliveryNotes(phieuXuatMoi.MaPhieuXuat);
                    return RedirectToAction("InDetails", new { id = phieuXuatMoi.MaPhieuXuat, BillType = phieuXuat.BillType });
                }
                else
                {
                    ModelState.AddModelError("MaNhaCungCap", "Không có mã thuốc nào được nhập");
                }

            }
            if (phieuXuat.PhieuXuatChiTiets != null && phieuXuat.PhieuXuatChiTiets.Any())
            {
                foreach (var detail in phieuXuat.PhieuXuatChiTiets)
                {
                    var thuoc = unitOfWork.ThuocRepository.GetById(detail.ThuocId);
                    detail.HeSo = thuoc.HeSo;
                    detail.MaDonViTinhLe = thuoc.DonViXuatLe.MaDonViTinh;
                    if (thuoc.DonViThuNguyen != null) detail.MaDonViTinhThuNguyen = thuoc.DonViThuNguyen.MaDonViTinh;
                    detail.TenDonViTinh = unitOfWork.DonViTinhRepository.GetById(detail.MaDonViTinh).TenDonViTinh;
                    detail.TenThuoc = thuoc.TenDayDu;
                }
            }

            if (phieuXuat.BillType == EBillType.Barcode)
            {
                ViewBag.MaBacSy = new SelectList(_getListBacSy(), "MaBacSy", "TenBacSy");
                ViewBag.MaKhachHang = new SelectList(_getListKhachHang(), "MaKhachHang", "TenKhachHang");
                ViewBag.SoPhieu = _generateAvaliableSoPhieu();
                ViewBag.LoaiPhieu = "2";
                ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
                ViewBag.CurrentUserName = WebSecurity.CurrentUserName;
                ViewBag.MaLoaiXuatNhap = new SelectList(db.LoaiXuatNhaps.Where(c => !c.IsHidden.HasValue || !c.IsHidden.Value).ToList(), "MaLoaiXuatNhap", "TenLoaiXuatNhap");
                ViewBag.MaNhaCungCap = new SelectList(_getListNhaCungCap(), "MaNhaCungCap", "TenNhaCungCap");
                ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
                ViewBag.MaNguoiTao = WebSecurity.GetCurrentUserId;
                var drugs = GetShortDrugInfos(null,null).ToDictionary(item => item.Barcode, item => item);
                ViewBag.Drugs = JsonConvert.SerializeObject(drugs);
                ViewBag.MaDonViTinh = _getListDonViTinh();

                return View(new PhieuXuatEditModel());
            }
            ViewBag.MaBacSy = new SelectList(_getListBacSy(), "MaBacSy", "TenBacSy");
            ViewBag.SoPhieu = _generateAvaliableSoPhieu();
            ViewBag.LoaiPhieu = phieuXuat.MaLoaiXuatNhap.ToString();
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.CurrentUserName = WebSecurity.CurrentUserName;
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            ViewBag.MaLoaiXuatNhap = new SelectList(db.LoaiXuatNhaps.Where(c => !c.IsHidden.HasValue || !c.IsHidden.Value).ToList(), "MaLoaiXuatNhap", "TenLoaiXuatNhap");
            ViewBag.MaKhachHang = new SelectList(_getListKhachHang(), "MaKhachHang", "TenKhachHang");
            ViewBag.MaNhaCungCap = new SelectList(_getListNhaCungCap(), "MaNhaCungCap", "TenNhaCungCap");

            ViewBag.MaDonViTinh = _getListDonViTinh();
            return View(phieuXuat);

        }
        //[SimpleAuthorize("Admin")]
        public ActionResult CreateWithBcScanner(string ngayxuat)
        {
            ViewBag.MaBacSy = new SelectList(_getListBacSy(), "MaBacSy", "TenBacSy");
            ViewBag.MaKhachHang = new SelectList(_getListKhachHang(), "MaKhachHang", "TenKhachHang");
            ViewBag.SoPhieu = _generateAvaliableSoPhieu();
            ViewBag.LoaiPhieu = "2";
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.CurrentUserName = WebSecurity.CurrentUserName;
            ViewBag.MaLoaiXuatNhap = new SelectList(db.LoaiXuatNhaps.Where(c => !c.IsHidden.HasValue || !c.IsHidden.Value).ToList(), "MaLoaiXuatNhap", "TenLoaiXuatNhap");
            ViewBag.MaNhaCungCap = new SelectList(_getListNhaCungCap(), "MaNhaCungCap", "TenNhaCungCap");
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            ViewBag.MaNguoiTao = WebSecurity.GetCurrentUserId;
            var drugs = GetShortDrugInfos(null,null).ToDictionary(item => item.Barcode, item => item);
            ViewBag.Drugs = JsonConvert.SerializeObject(drugs);
            ViewBag.MaDonViTinh = _getListDonViTinh();
            ViewBag.NgayXuat = ngayxuat == null ? DateTime.Now.ToString("dd/MM/yyyy") : ngayxuat;
            return View(new PhieuXuatEditModel());
        }

        private void UpdatePrice(PhieuXuat phieuXuat)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            // chi cap nhat gia khi la hoa don xuat cuoi cung
            if (!unitOfWork.PhieuXuatRepository.GetMany(
                    e => e.NgayXuat > phieuXuat.NgayXuat && e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.RecordStatusID == (byte)RecordStatus.Activated).Any())
            {
                phieuXuat.PhieuXuatChiTiets.ForEach(e =>
                {
                    var giaBanLe = e.DonViTinh.MaDonViTinh == e.Thuoc.DonViXuatLe.MaDonViTinh
                        ? e.GiaXuat
                        : e.GiaXuat / e.Thuoc.HeSo;
                    if (e.Thuoc.GiaBanLe != giaBanLe)
                        e.Thuoc.GiaBanLe = giaBanLe;
                });

            }
        }

        [SimpleAuthorize("Admin")]
        public ActionResult In(long id, int loaiPhieu = 0, int LoaiPhieuIn = 0)
        {
            var nhathuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhathuoc.MaNhaThuoc;
            var result =
                unitOfWork.PhieuXuatRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuXuat == id && e.RecordStatusID == (byte)RecordStatus.Activated)
                    .FirstOrDefault();

            if (result == null)
            {
                ViewBag.Message = "Phiếu xuất không tồn tại #" + id;
                return View("Error");
            }

            DateTime fromDate = result.NgayXuat.Value.AddDays(1);
            //Tinh no cu
            var listDebit = new List<PhieuXuat>();
            var listPhieuChi = new List<PhieuThuChi>();
            if (result.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.XuatBan || result.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe)
            {
                listDebit = unitOfWork.PhieuXuatRepository.Get(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.MaPhieuXuat != id && c.KhachHang.MaKhachHang == result.KhachHang.MaKhachHang && c.TongTien != c.DaTra && c.RecordStatusID == (byte)RecordStatus.Activated && c.NgayXuat < fromDate).ToList();
                listPhieuChi = unitOfWork.PhieuThuChiRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.LoaiPhieu == Constants.LoaiPhieuThuChi.PhieuThu && c.KhachHang.MaKhachHang == result.KhachHang.MaKhachHang && c.NgayTao < fromDate).ToList();
            }
            else
            {
                listDebit = unitOfWork.PhieuXuatRepository.Get(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.MaPhieuXuat != id && c.NhaCungCap.MaNhaCungCap == result.NhaCungCap.MaNhaCungCap && c.TongTien != c.DaTra && c.RecordStatusID == (byte)RecordStatus.Activated && c.NgayXuat < fromDate).ToList();
                listPhieuChi = unitOfWork.PhieuThuChiRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.LoaiPhieu == Constants.LoaiPhieuThuChi.PhieuThu && c.NhaCungCap.MaNhaCungCap == result.NhaCungCap.MaNhaCungCap && c.NgayTao < fromDate).ToList();
            }

            decimal nocu = 0;
            foreach (var item in listDebit)
            {
                nocu += (item.TongTien - item.DaTra);
            }

            decimal tienthuchi = 0;
            foreach (var item in listPhieuChi)
            {
                tienthuchi += item.Amount;
            }

            nocu = nocu - tienthuchi;

            DataTable dt = new DataTable();
            dt.Columns.Add("STT");
            dt.Columns.Add("TenHang");
            dt.Columns.Add("DVT");
            dt.Columns.Add("SoLuong");
            dt.Columns.Add("DonGia");
            dt.Columns.Add("ChietKhau");
            dt.Columns.Add("ThanhTien");
            int i = 1;
            decimal tongtienhang = 0;
            decimal tienhang = 0;
            foreach (var item in result.PhieuXuatChiTiets.Where(x=> x.RecordStatusID != 2))
            {
                DataRow dr = dt.NewRow();
                tienhang = (item.GiaXuat * item.SoLuong * (100 - item.ChietKhau) / 100);
                dr["STT"] = i;
                dr["TenHang"] = item.Thuoc.TenThuoc;
                dr["DVT"] = item.DonViTinh.TenDonViTinh;
                dr["SoLuong"] = item.SoLuong.ToString("#,##0");
                dr["DonGia"] = item.GiaXuat.ToString("#,##0");
                dr["ChietKhau"] = item.ChietKhau.ToString("#,##0");
                dr["ThanhTien"] = tienhang.ToString("#,##0");
                dt.Rows.Add(dr);

                tongtienhang += tienhang;
                i++;
            }

            //set param
            List<ReportParameter> listParam = new List<ReportParameter>(){
                new ReportParameter("pNhaThuoc",nhathuoc.TenNhaThuoc),
                new ReportParameter("pDiaChiNhaThuoc",nhathuoc.DiaChi),
                new ReportParameter("pSDTNhaThuoc",nhathuoc.DienThoai),
                new ReportParameter("pNgayXuat",result.NgayXuat.Value.ToString("dd/MM/yyyy")),
                new ReportParameter("pPhieuXuat",result.SoPhieuXuat.ToString()),
                new ReportParameter("pNhanVien",result.CreatedBy.TenDayDu),
                new ReportParameter("pDienGiai",result.DienGiai),
                new ReportParameter("pTongTienHang",tongtienhang.ToString("#,##0")),
                new ReportParameter("pVAT",result.VAT.ToString("#,##0")),
                new ReportParameter("pNoCu",nocu.ToString("#,##0")),
                new ReportParameter("pTongTien",(nocu + result.TongTien).ToString("#,##0")),
                new ReportParameter("pDaTra",result.DaTra.ToString("#,##0")),
                new ReportParameter("pConNo",(result.TongTien + nocu - result.DaTra).ToString("#,##0"))
            };

            ReportViewer viewer = new ReportViewer();
            viewer.ProcessingMode = ProcessingMode.Local;
            if (result.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.XuatBan)
            {
                switch (LoaiPhieuIn)
                {
                    case Constants.LoaiPhieuXuatIn.InKhachQuen:
                        {
                            if (nhathuoc.DuocSy != 'X'.ToString())
                            {
                                viewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RptPhieuXuat.rdlc");
                            }
                            else
                            {
                                viewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RptPhieuXuatKho.rdlc");
                                listParam.Add(new ReportParameter("pTongTienBangChu", tongtienhang != null ? So_chu((double)tongtienhang) : string.Empty));
                            }
                            break;
                        }                        
                    case Constants.LoaiPhieuXuatIn.InKhachLeA5:
                        {
                            if (nhathuoc.DuocSy != 'X'.ToString())
                            {
                                if(maNhaThuoc == "0204")
                                {
                                    //nhà thuốc có kết hợp phòng khám in cho bác sỹ
                                    viewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RptPhieuXuatLeBacsyA5.rdlc");
                                }
                                else
                                {
                                    //nhà thuốc không kết hợp với phòng khám
                                    viewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RptPhieuXuatLeA5.rdlc");
                                }
                                    
                            }
                            else
                            {
                                viewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RptPhieuXuatKhoA5.rdlc");
                                listParam.Add(new ReportParameter("pTongTienBangChu", tongtienhang != null ? So_chu((double)tongtienhang) : string.Empty));
                            }
                            
                            break;
                        }
                    case Constants.LoaiPhieuXuatIn.InKhachLe80mm:
                        {
                            viewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RptPhieuXuatLe80mm.rdlc");
                            break;
                        }
                    case Constants.LoaiPhieuXuatIn.InKhachLe58mm:
                        {
                            viewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RptPhieuXuatLe58mm.rdlc");
                            break;
                        }

                }      
               
                listParam.Add(new ReportParameter("pBacSy", result.BacSy != null ? result.BacSy.TenBacSy : string.Empty));
                listParam.Add(new ReportParameter("pKhachHang", result.KhachHang.TenKhachHang));
                listParam.Add(new ReportParameter("pDiaChiKhachHang", result.KhachHang.DiaChi != null ? result.KhachHang.DiaChi : string.Empty));
            }
            else
            {
                viewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RptPhieuTraHangNhaCC.rdlc");
                listParam.Add(new ReportParameter("pKhachHang", result.NhaCungCap.TenNhaCungCap));
                listParam.Add(new ReportParameter("pDiaChiKhachHang", result.NhaCungCap.DiaChi));
            }

            viewer.LocalReport.DataSources.Add(new ReportDataSource("DSPhieuXuat", dt));
            viewer.LocalReport.SetParameters(listParam);
            byte[] bytes = viewer.LocalReport.Render("PDF");
            Stream stream = new MemoryStream(bytes);

            return File(stream, "application/pdf");
        }

        [SimpleAuthorize("Admin")]
        public ActionResult InPhieuXuatLe(long id)
        {
            var nhathuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhathuoc.MaNhaThuoc;
            var result =
                unitOfWork.PhieuXuatRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuXuat == id && e.RecordStatusID == (byte)RecordStatus.Activated)
                    .FirstOrDefault();

            if (result == null)
            {
                ViewBag.Message = "Phiếu xuất không tồn tại #" + id;
                return View("Error");
            }

            DateTime fromDate = result.NgayXuat.Value.AddDays(1);
            //Tinh no cu
            var listDebit = new List<PhieuXuat>();
            var listPhieuChi = new List<PhieuThuChi>();
            if (result.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.XuatBan || result.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe)
            {
                listDebit = unitOfWork.PhieuXuatRepository.Get(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.MaPhieuXuat != id && c.KhachHang.MaKhachHang == result.KhachHang.MaKhachHang && c.TongTien != c.DaTra && c.RecordStatusID == (byte)RecordStatus.Activated && c.NgayXuat < fromDate).ToList();
                listPhieuChi = unitOfWork.PhieuThuChiRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.LoaiPhieu == Constants.LoaiPhieuThuChi.PhieuThu && c.KhachHang.MaKhachHang == result.KhachHang.MaKhachHang && c.NgayTao < fromDate).ToList();
            }
            else
            {
                listDebit = unitOfWork.PhieuXuatRepository.Get(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.MaPhieuXuat != id && c.NhaCungCap.MaNhaCungCap == result.NhaCungCap.MaNhaCungCap && c.TongTien != c.DaTra && c.RecordStatusID == (byte)RecordStatus.Activated && c.NgayXuat < fromDate).ToList();
                listPhieuChi = unitOfWork.PhieuThuChiRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.LoaiPhieu == Constants.LoaiPhieuThuChi.PhieuThu && c.NhaCungCap.MaNhaCungCap == result.NhaCungCap.MaNhaCungCap && c.NgayTao < fromDate).ToList();
            }

            decimal nocu = 0;
            foreach (var item in listDebit)
            {
                nocu += (item.TongTien - item.DaTra);
            }

            decimal tienthuchi = 0;
            foreach (var item in listPhieuChi)
            {
                tienthuchi += item.Amount;
            }

            nocu = nocu - tienthuchi;

            DataTable dt = new DataTable();
            dt.Columns.Add("STT");
            dt.Columns.Add("TenHang");
            dt.Columns.Add("DVT");
            dt.Columns.Add("SoLuong");
            dt.Columns.Add("DonGia");
            dt.Columns.Add("ChietKhau");
            dt.Columns.Add("ThanhTien");
            int i = 1;
            decimal tongtienhang = 0;
            decimal tienhang = 0;
            foreach (var item in result.PhieuXuatChiTiets)
            {
                DataRow dr = dt.NewRow();
                tienhang = (item.GiaXuat * item.SoLuong * (100 - item.ChietKhau) / 100);
                dr["STT"] = i;
                dr["TenHang"] = item.Thuoc.TenThuoc;
                dr["DVT"] = item.DonViTinh.TenDonViTinh;
                dr["SoLuong"] = item.SoLuong.ToString("#,##0");
                dr["DonGia"] = item.GiaXuat.ToString("#,##0");
                dr["ChietKhau"] = item.ChietKhau.ToString("#,##0");
                dr["ThanhTien"] = tienhang.ToString("#,##0");
                dt.Rows.Add(dr);

                tongtienhang += tienhang;
                i++;
            }

            //set param
            List<ReportParameter> listParam = new List<ReportParameter>(){
                new ReportParameter("pNhaThuoc",nhathuoc.TenNhaThuoc),
                new ReportParameter("pDiaChiNhaThuoc",nhathuoc.DiaChi),
                new ReportParameter("pSDTNhaThuoc",nhathuoc.DienThoai),
                new ReportParameter("pNgayXuat",result.NgayXuat.Value.ToString("dd/MM/yyyy")),
                new ReportParameter("pPhieuXuat",result.SoPhieuXuat.ToString()),
                new ReportParameter("pNhanVien",result.CreatedBy.TenDayDu),
                new ReportParameter("pDienGiai",result.DienGiai),
                new ReportParameter("pTongTienHang",tongtienhang.ToString("#,##0")),
                new ReportParameter("pVAT",result.VAT.ToString("#,##0")),
                new ReportParameter("pNoCu",nocu.ToString("#,##0")),
                new ReportParameter("pTongTien",(nocu + result.TongTien).ToString("#,##0")),
                new ReportParameter("pDaTra",result.DaTra.ToString("#,##0")),
                new ReportParameter("pConNo",(result.TongTien + nocu - result.DaTra).ToString("#,##0"))
            };

            ReportViewer viewer = new ReportViewer();
            viewer.ProcessingMode = ProcessingMode.Local;

            viewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RptPhieuXuatLe.rdlc");
            listParam.Add(new ReportParameter("pBacSy", result.BacSy != null ? result.BacSy.TenBacSy : string.Empty));
            listParam.Add(new ReportParameter("pKhachHang", result.KhachHang.TenKhachHang));
            listParam.Add(new ReportParameter("pDiaChiKhachHang", result.KhachHang.DiaChi != null ? result.KhachHang.DiaChi : string.Empty));

            viewer.LocalReport.DataSources.Add(new ReportDataSource("DSPhieuXuat", dt));
            viewer.LocalReport.SetParameters(listParam);
            byte[] bytes = viewer.LocalReport.Render("PDF");
            Stream stream = new MemoryStream(bytes);

            return File(stream, "application/pdf");
        }

        [SimpleAuthorize("Admin")]
        public ActionResult InPhieuXuatLeA5(long id)
        {
            var nhathuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhathuoc.MaNhaThuoc;
            var result =
                unitOfWork.PhieuXuatRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuXuat == id && e.RecordStatusID == (byte)RecordStatus.Activated)
                    .FirstOrDefault();

            if (result == null)
            {
                ViewBag.Message = "Phiếu xuất không tồn tại #" + id;
                return View("Error");
            }

            DateTime fromDate = result.NgayXuat.Value.AddDays(1);
            //Tinh no cu
            var listDebit = new List<PhieuXuat>();
            var listPhieuChi = new List<PhieuThuChi>();
            if (result.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.XuatBan || result.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe)
            {
                listDebit = unitOfWork.PhieuXuatRepository.Get(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.MaPhieuXuat != id && c.KhachHang.MaKhachHang == result.KhachHang.MaKhachHang && c.TongTien != c.DaTra && c.RecordStatusID == (byte)RecordStatus.Activated && c.NgayXuat < fromDate).ToList();
                listPhieuChi = unitOfWork.PhieuThuChiRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.LoaiPhieu == Constants.LoaiPhieuThuChi.PhieuThu && c.KhachHang.MaKhachHang == result.KhachHang.MaKhachHang && c.NgayTao < fromDate).ToList();
            }
            else
            {
                listDebit = unitOfWork.PhieuXuatRepository.Get(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.MaPhieuXuat != id && c.NhaCungCap.MaNhaCungCap == result.NhaCungCap.MaNhaCungCap && c.TongTien != c.DaTra && c.RecordStatusID == (byte)RecordStatus.Activated && c.NgayXuat < fromDate).ToList();
                listPhieuChi = unitOfWork.PhieuThuChiRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.LoaiPhieu == Constants.LoaiPhieuThuChi.PhieuThu && c.NhaCungCap.MaNhaCungCap == result.NhaCungCap.MaNhaCungCap && c.NgayTao < fromDate).ToList();
            }

            decimal nocu = 0;
            foreach (var item in listDebit)
            {
                nocu += (item.TongTien - item.DaTra);
            }

            decimal tienthuchi = 0;
            foreach (var item in listPhieuChi)
            {
                tienthuchi += item.Amount;
            }

            nocu = nocu - tienthuchi;

            DataTable dt = new DataTable();
            dt.Columns.Add("STT");
            dt.Columns.Add("TenHang");
            dt.Columns.Add("DVT");
            dt.Columns.Add("SoLuong");
            dt.Columns.Add("DonGia");
            dt.Columns.Add("ChietKhau");
            dt.Columns.Add("ThanhTien");
            int i = 1;
            decimal tongtienhang = 0;
            decimal tienhang = 0;
            foreach (var item in result.PhieuXuatChiTiets)
            {
                DataRow dr = dt.NewRow();
                tienhang = (item.GiaXuat * item.SoLuong * (100 - item.ChietKhau) / 100);
                dr["STT"] = i;
                dr["TenHang"] = item.Thuoc.TenThuoc;
                dr["DVT"] = item.DonViTinh.TenDonViTinh;
                dr["SoLuong"] = item.SoLuong.ToString("#,##0");
                dr["DonGia"] = item.GiaXuat.ToString("#,##0");
                dr["ChietKhau"] = item.ChietKhau.ToString("#,##0");
                dr["ThanhTien"] = tienhang.ToString("#,##0");
                dt.Rows.Add(dr);

                tongtienhang += tienhang;
                i++;
            }

            //set param
            List<ReportParameter> listParam = new List<ReportParameter>(){
                new ReportParameter("pNhaThuoc",nhathuoc.TenNhaThuoc),
                new ReportParameter("pDiaChiNhaThuoc",nhathuoc.DiaChi),
                new ReportParameter("pSDTNhaThuoc",nhathuoc.DienThoai),
                new ReportParameter("pNgayXuat",result.NgayXuat.Value.ToString("dd/MM/yyyy")),
                new ReportParameter("pPhieuXuat",result.SoPhieuXuat.ToString()),
                new ReportParameter("pNhanVien",result.CreatedBy.TenDayDu),
                new ReportParameter("pDienGiai",result.DienGiai),
                new ReportParameter("pTongTienHang",tongtienhang.ToString("#,##0")),
                new ReportParameter("pVAT",result.VAT.ToString("#,##0")),
                new ReportParameter("pNoCu",nocu.ToString("#,##0")),
                new ReportParameter("pTongTien",(nocu + result.TongTien).ToString("#,##0")),
                new ReportParameter("pDaTra",result.DaTra.ToString("#,##0")),
                new ReportParameter("pConNo",(result.TongTien + nocu - result.DaTra).ToString("#,##0"))
            };

            ReportViewer viewer = new ReportViewer();
            viewer.ProcessingMode = ProcessingMode.Local;

            viewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RptPhieuXuatLeA5.rdlc");
            listParam.Add(new ReportParameter("pBacSy", result.BacSy != null ? result.BacSy.TenBacSy : string.Empty));
            listParam.Add(new ReportParameter("pKhachHang", result.KhachHang.TenKhachHang));
            listParam.Add(new ReportParameter("pDiaChiKhachHang", result.KhachHang.DiaChi != null ? result.KhachHang.DiaChi : string.Empty));

            viewer.LocalReport.DataSources.Add(new ReportDataSource("DSPhieuXuat", dt));
            viewer.LocalReport.SetParameters(listParam);
            byte[] bytes = viewer.LocalReport.Render("PDF");
            Stream stream = new MemoryStream(bytes);

            return File(stream, "application/pdf");
        }

        private IEnumerable<BacSy> _getListBacSy()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            return
                unitOfWork.BacSyRespository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc)
                    .OrderBy(e => e.TenBacSy).ToList();
        }

        private IEnumerable<ShortDrugInfo> GetShortDrugInfos(string sDrugCode, string sBarcode)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            IQueryable<Thuoc> listQ = null;
            if (!string.IsNullOrEmpty(sDrugCode))
            {
                listQ = unitOfWork.ThuocRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.RecordStatusID == (byte)RecordStatus.Activated && e.MaThuoc == sDrugCode);
            }
            else if (!string.IsNullOrEmpty(sBarcode))
            {
                listQ = unitOfWork.ThuocRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.RecordStatusID == (byte)RecordStatus.Activated && e.BarCode != null && e.BarCode != string.Empty && e.BarCode == sBarcode);
            }
            else
            {
                listQ = unitOfWork.ThuocRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.BarCode != null && e.BarCode != string.Empty);
            }
            return listQ.OrderBy(e => e.MaThuoc).Select(item => new ShortDrugInfo()
                {
                    ID = item.ThuocId,
                    Code = item.MaThuoc,
                    Name = item.TenThuoc,
                    Barcode = item.BarCode,
                    Price = item.GiaBanLe,
                    BatchPrice = item.GiaBanLe * item.HeSo,
                    Unit = item.DonViXuatLe != null ? item.DonViXuatLe.MaDonViTinh : 0,
                    BatchUnit = item.DonViThuNguyen != null ? item.DonViThuNguyen.MaDonViTinh : 0,
                    SelectedUnit = item.DonViXuatLe != null ? item.DonViXuatLe.MaDonViTinh : 0,
                    UnitName = item.DonViXuatLe != null ? item.DonViXuatLe.TenDonViTinh : string.Empty,
                    BatchUnitName = item.DonViThuNguyen != null ? item.DonViThuNguyen.TenDonViTinh : string.Empty,
                    Quantity = 1,
                    OldItemCode = String.Empty,
                    CurrentPrice = item.GiaBanLe
                }).ToList();
        }
        // POST: Thuocs/GetByCode/5
        [HttpPost]
        public JsonResult GetThuocsByCode(string sDrugCode)
        {
            var drug = GetShortDrugInfos(sDrugCode,null).FirstOrDefault();
            return new JsonResult() { Data = drug, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        [HttpPost]
        public JsonResult GetThuocsByBarCode(string sDrugBarCode)
        {
            var drug = GetShortDrugInfos(null, sDrugBarCode).FirstOrDefault();
            return new JsonResult() { Data = drug, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        private IEnumerable<NhaCungCap> _getListNhaCungCap()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var list =
                unitOfWork.NhaCungCapRespository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && !Constants.Default.ConstantEntities.NhaCungCapKiemKe.Contains(e.TenNhaCungCap))
                    .OrderBy(e => e.TenNhaCungCap).ToList();
            var hangNhapLe = list.FirstOrDefault(e => e.TenNhaCungCap == "Hàng nhập lẻ");
            if (hangNhapLe != null)
            {
                hangNhapLe.Order = -1;
            }
            return list.OrderBy(e => e.Order).ToList();
        }
        private IEnumerable<KhachHang> _getListKhachHang()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var list =
                unitOfWork.KhachHangRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && !Constants.Default.ConstantEntities.KhachHangKiemKe.Contains(e.TenKhachHang))
                    .OrderBy(e => e.TenKhachHang).ToList();
            var khachHangLe = list.FirstOrDefault(e => e.TenKhachHang == "Khách hàng lẻ");
            if (khachHangLe != null)
            {
                khachHangLe.Order = -1;
            }
            return list.OrderBy(e => e.Order);

        }
        private List<DonViTinh> _getListDonViTinh()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var maNhaThuocCha = this.GetNhaThuoc().MaNhaThuocCha;
            return
                unitOfWork.DonViTinhRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha)
                    .OrderBy(e => e.TenDonViTinh).ToList();
        }
        private long _generateAvaliableSoPhieu()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var maxSoPhieu = unitOfWork.PhieuXuatRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).Any() ? unitOfWork.PhieuXuatRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).Max(e => e.SoPhieuXuat) : 0;
            return maxSoPhieu + 1;
        }


        // GET: PhieuXuats/Edit/5
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var phieuXuat = await unitOfWork.PhieuXuatRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuXuat == id).FirstOrDefaultAsync();
            if (phieuXuat == null)
            {
                return HttpNotFound();
            }
            BackgroundJobHelper.EnqueueUpdateNewestInventories(null);
            var model = new PhieuXuatEditModel(phieuXuat);

            ViewBag.SoPhieu = phieuXuat.SoPhieuXuat;
            ViewBag.LoaiPhieu = phieuXuat.LoaiXuatNhap.MaLoaiXuatNhap;
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            ViewBag.MaLoaiXuatNhap = new SelectList(db.LoaiXuatNhaps.Where(c => !c.IsHidden.HasValue || !c.IsHidden.Value).ToList(), "MaLoaiXuatNhap", "TenLoaiXuatNhap");
            ViewBag.NhaCungCaps = new SelectList(_getListNhaCungCap(), "MaNhaCungCap", "TenNhaCungCap", phieuXuat.NhaCungCap != null ? phieuXuat.NhaCungCap.MaNhaCungCap : 0);
            ViewBag.KhachHangs = new SelectList(_getListKhachHang(), "MaKhachHang", "TenKhachHang", phieuXuat.KhachHang != null ? phieuXuat.KhachHang.MaKhachHang : 0);
            ViewBag.MaBacSy = new SelectList(_getListBacSy(), "MaBacSy", "TenBacSy", phieuXuat.BacSy != null ? phieuXuat.BacSy.MaBacSy : 0);

            ViewBag.MaDonViTinh = _getListDonViTinh();
            return View(model);
        }

        // POST: PhieuXuats/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public async Task<ActionResult> Edit(PhieuXuatEditModel phieuXuat)
        {
            ModelState.Remove("NgayXuat");

            foreach (var state in ModelState.Keys.ToList().Where(c => c.Contains("ChietKhau")))
            {
                ModelState.Remove(state);
            }

            if (ModelState.IsValid)
            {
                // validate 
                var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
                var phieuXuatMoi =
                    await
                        unitOfWork.PhieuXuatRepository.GetMany(
                            e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuXuat == phieuXuat.MaPhieuXuat)
                            .FirstOrDefaultAsync();
                phieuXuatMoi.Modified = DateTime.Now;
                phieuXuatMoi.ModifiedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId);
                phieuXuatMoi.DaTra = phieuXuat.DaTra;
                phieuXuatMoi.DienGiai = phieuXuat.DienGiai;
                phieuXuatMoi.NgayXuat = phieuXuat.NgayXuat;
                phieuXuatMoi.VAT = phieuXuat.VAT;
                phieuXuatMoi.TongTien = 0;
                if (phieuXuat.MaBacSy > 0)
                    phieuXuatMoi.BacSy = unitOfWork.BacSyRespository.GetById(phieuXuat.MaBacSy);
                if (phieuXuat.MaKhachHang > 0)
                {
                    phieuXuatMoi.KhachHang = unitOfWork.KhachHangRepository.GetById(phieuXuat.MaKhachHang);
                }
                if (phieuXuat.MaNhaCungCap > 0)
                {
                    phieuXuatMoi.NhaCungCap = unitOfWork.NhaCungCapRespository.GetById(phieuXuat.MaNhaCungCap);
                }
                unitOfWork.PhieuXuatRepository.Update(phieuXuatMoi);
                var noteItems = new List<PhieuXuatChiTiet>();
                // details
                if (phieuXuat.PhieuXuatChiTiets != null)
                {
                    if (phieuXuatMoi.PhieuXuatChiTiets == null)
                        phieuXuatMoi.PhieuXuatChiTiets = new List<PhieuXuatChiTiet>();

                    var nhathuoc = unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc);

                    foreach (var detail in phieuXuat.PhieuXuatChiTiets)
                    {
                        if (detail.SoLuong > 0 && detail.GiaXuat >= 0)
                        {
                            //  chi them moi item  khi so luong va gia nhap phu hop

                            var item = phieuXuatMoi.PhieuXuatChiTiets.FirstOrDefault(e => e.MaPhieuXuatCt == detail.MaPhieuXuatCt && detail.MaPhieuXuatCt != 0);
                            Decimal chietKhau = 0;
                            Decimal.TryParse(detail.ChietKhau, out chietKhau);
                            if (item == null)
                            {
                                item = new PhieuXuatChiTiet()
                                {
                                    DonViTinh = unitOfWork.DonViTinhRepository.GetById(detail.MaDonViTinh),
                                    GiaXuat = detail.GiaXuat,
                                    PhieuXuat = phieuXuatMoi,
                                    SoLuong = detail.SoLuong,
                                    Thuoc = unitOfWork.ThuocRepository.GetById(detail.ThuocId),
                                    NhaThuoc = nhathuoc,
                                    NhaThuoc_MaNhaThuoc = maNhaThuoc,
                                    DonViTinh_MaDonViTinh = detail.MaDonViTinh,
                                    Thuoc_ThuocId = detail.ThuocId
                                };
                                item.ChietKhau = chietKhau;

                                phieuXuatMoi.TongTien += item.SoLuong * item.GiaXuat;
                                unitOfWork.PhieuXuatChiTietRepository.Insert(item);
                                noteItems.Add(item);
                            }
                            else
                            {
                                item.NhaThuoc_MaNhaThuoc = maNhaThuoc;
                                item.DonViTinh_MaDonViTinh = detail.MaDonViTinh;
                                item.Thuoc_ThuocId = detail.ThuocId;
                                item.Original = item.Clone();

                                item.ChietKhau = chietKhau;
                                item.DonViTinh = unitOfWork.DonViTinhRepository.GetById(detail.MaDonViTinh);
                                item.GiaXuat = detail.GiaXuat;
                                item.SoLuong = detail.SoLuong;
                                phieuXuatMoi.TongTien += item.SoLuong * item.GiaXuat;
                                ///unitOfWork.PhieuXuatChiTietRepository.Update(item);
                                noteItems.Add(item);
                            }

                            if (item.ChietKhau > 0)
                            {
                                phieuXuatMoi.TongTien -= item.SoLuong * item.GiaXuat * item.ChietKhau / 100;
                            }
                        }

                    }
                    // xoa items
                    phieuXuatMoi.PhieuXuatChiTiets.ForEach(item =>
                    {
                        if (phieuXuat.PhieuXuatChiTiets.All(e => e.MaPhieuXuatCt != item.MaPhieuXuatCt))
                        {
                            if (item.RecordStatusID == (byte)RecordStatus.Activated)
                            {
                                phieuXuatMoi.TongTien -= item.SoLuong * item.GiaXuat;
                                item.RecordStatusID = (byte)RecordStatus.Deleted;                             
                                noteItems.Add(item);
                            }
                        }
                    });
                }
                if (phieuXuat.VAT > 0)
                {
                    phieuXuatMoi.TongTien += phieuXuatMoi.TongTien * phieuXuatMoi.VAT / 100;
                }
                if (phieuXuatMoi.MaLoaiXuatNhap == 4)
                {
                    // khach hang tra lai, auto fill datra
                    phieuXuatMoi.DaTra = phieuXuatMoi.TongTien;
                }
                phieuXuatMoi.IsDebt = (double)Math.Abs(phieuXuatMoi.TongTien - phieuXuatMoi.DaTra) > MedConstants.EspAmount;
                if (phieuXuatMoi.PhieuXuatChiTiets != null && phieuXuatMoi.PhieuXuatChiTiets.Any())
                {
                    try
                    {
                        NoteHelper.ApplyAdditionalInfos(maNhaThuoc, noteItems.ToArray());
                        var updateItems = noteItems.Where(i => i.NeedUpdate).ToList();
                        if (updateItems.Any())
                        {
                            updateItems.ForEach(i =>
                            {
                                var entry = unitOfWork.Context.Entry(i);
                                if (entry != null)
                                {
                                    entry.State = EntityState.Detached;
                                }
                            });
                            //updateItems.ForEach(i => unitOfWork.PhieuXuatChiTietRepository.Update(i));
                            NoteHelper.UpdateNoteItems(updateItems.ToArray());
                        }
                        unitOfWork.Save();
                        var drugIds = phieuXuatMoi.PhieuXuatChiTiets.Select(i => i.Thuoc.ThuocId).Distinct().ToArray();                        
                        BackgroundJobHelper.EnqueueMakeAffectedChangesRelatedDeliveryNotes(phieuXuat.MaPhieuXuat.Value);
                    }
                    catch (Exception ex)
                    {

                    }
                    return RedirectToAction("Index", "PhieuNhaps", new { loaiPhieu = phieuXuatMoi.LoaiXuatNhap.MaLoaiXuatNhap, searchSoPhieu = phieuXuatMoi.SoPhieuXuat });
                }
                else
                {
                    ModelState.AddModelError("MaNhaCungCap", "Không có mã thuốc nào được nhập");
                }

            }
            if (phieuXuat.PhieuXuatChiTiets != null && phieuXuat.PhieuXuatChiTiets.Any())
                foreach (var detail in phieuXuat.PhieuXuatChiTiets)
                {
                    var thuoc = unitOfWork.ThuocRepository.GetById(detail.ThuocId);
                    detail.HeSo = thuoc.HeSo;
                    detail.MaDonViTinhLe = thuoc.DonViXuatLe.MaDonViTinh;
                    if (thuoc.DonViThuNguyen != null) detail.MaDonViTinhThuNguyen = thuoc.DonViThuNguyen.MaDonViTinh;
                    detail.TenDonViTinh = unitOfWork.DonViTinhRepository.GetById(detail.MaDonViTinh).TenDonViTinh;
                    detail.TenThuoc = thuoc.TenDayDu;
                }
            ViewBag.SoPhieu = phieuXuat.SoPhieuXuat;
            ViewBag.LoaiPhieu = phieuXuat.MaLoaiXuatNhap;
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            ViewBag.MaLoaiXuatNhap = new SelectList(db.LoaiXuatNhaps.Where(c => !c.IsHidden.HasValue || !c.IsHidden.Value).ToList(), "MaLoaiXuatNhap", "TenLoaiXuatNhap");
            ViewBag.NhaCungCaps = new SelectList(_getListNhaCungCap(), "MaNhaCungCap", "TenNhaCungCap", phieuXuat.MaNhaCungCap);
            ViewBag.KhachHangs = new SelectList(_getListKhachHang(), "MaKhachHang", "TenKhachHang", phieuXuat.MaKhachHang);
            ViewBag.MaBacSy = new SelectList(_getListBacSy(), "MaBacSy", "TenBacSy", phieuXuat.MaBacSy);

            ViewBag.MaDonViTinh = _getListDonViTinh();

            return View(phieuXuat);


        }

        // GET: PhieuXuats/Delete/5
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            PhieuXuat phieuXuat =
                await
                    unitOfWork.PhieuXuatRepository.GetMany(
                        e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuXuat == id).FirstAsync();
            if (phieuXuat == null)
            {
                return HttpNotFound();
            }
            // convert model to modelView
            var model = new PhieuXuatEditModel(phieuXuat);
            ViewBag.SoPhieu = model.SoPhieuXuat;
            ViewBag.LoaiPhieu = model.MaLoaiXuatNhap;
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;

            ViewBag.MaDonViTinh = _getListDonViTinh();
            return View(model);
        }

        // POST: PhieuXuats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var service = IoC.Container.Resolve<IDeliveryNoteService>();
            var loaiPhieu = 2;
            var noteTypeId = service.DeleteDeliveryNote(MedSessionManager.CurrentDrugStoreCode, id, MedSessionManager.CurrentUserId);
            if (noteTypeId <= 0)
            {
                var errorMessage = string.Format("Gặp lỗi trong khi xóa phiếu nhập có mã: {0}", id);
                ViewBag.Message = errorMessage;
                return View("Error");
            }
            //return RedirectToAction("Index", "PhieuNhaps", new { noteTypeId });
            return RedirectToAction("Index", "PhieuNhaps", new { KhoiPhuc = 0, loaiPhieu });
        }
        public async Task<ActionResult> Delete4Ever(int id)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var loaiPhieu = 0;
            PhieuXuat phieuXuat =
                await
                    unitOfWork.PhieuXuatRepository.GetMany(
                        e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuXuat == id && e.RecordStatusID == (byte)RecordStatus.Deleted).FirstAsync();

            if (phieuXuat != null)
            {
                loaiPhieu = phieuXuat.LoaiXuatNhap.MaLoaiXuatNhap;
                if (phieuXuat.PhieuXuatChiTiets != null)
                {
                    var deletingIds = phieuXuat.PhieuXuatChiTiets.Select(e => e.MaPhieuXuatCt).ToArray();
                    foreach (var item in deletingIds)
                    {
                        unitOfWork.PhieuXuatChiTietRepository.Delete(item);
                    }
                }

                unitOfWork.PhieuXuatRepository.Delete(phieuXuat);
                unitOfWork.Save();

            }
            return RedirectToAction("Index", "PhieuNhaps", new { KhoiPhuc = 1, loaiPhieu });
        }
        [SimpleAuthorize("Admin")]
        // [Audit]
        public async Task<ActionResult> Restore(int id)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var loaiPhieu = 0;
            PhieuXuat phieuXuat =
                await
                    unitOfWork.PhieuXuatRepository.GetMany(
                        e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuXuat == id).FirstAsync();

            if (phieuXuat != null)
            {
                loaiPhieu = phieuXuat.LoaiXuatNhap.MaLoaiXuatNhap;
                phieuXuat.RecordStatusID = (byte)RecordStatus.Activated;
                unitOfWork.PhieuXuatRepository.Update(phieuXuat);
                unitOfWork.Save();

            }
            return RedirectToAction("Index", "PhieuNhaps", new { loaiPhieu });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public ActionResult Upload(HttpPostedFileBase uploadFile)
        {
            return ProcessUpload(uploadFile, 2);
        }

        [SimpleAuthorize("Admin")]
        // [Audit]
        public ActionResult Upload2(HttpPostedFileBase uploadFile)
        {
            return ProcessUpload(uploadFile, 4);
        }

        private ActionResult ProcessUpload(HttpPostedFileBase uploadFile, int fileType)
        {
            var strValidations = new StringBuilder(string.Empty);
            try
            {
                if (uploadFile.ContentLength > 0)
                {
                    string filePath = Path.Combine(HttpContext.Server.MapPath("../Uploads"),
                        Path.GetFileName(uploadFile.FileName));

                    uploadFile.SaveAs(filePath);
                    var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;

                    int totaladded = 0;
                    int totalError = 0;
                    string message = "<b>Thông tin phiếu xuất ở dòng số {0} bị lỗi:</b><br/> {1}";
                    UploadObjectInfo info = new UploadObjectInfo();

                    var phieuXuats = new List<PhieuXuat>();
                    var soPhieu = db.PhieuNhaps.Count() > 0 ? Convert.ToInt32(unitOfWork.PhieuXuatRepository.GetMany(s => s.NhaThuoc.MaNhaThuoc == maNhaThuoc && s.LoaiXuatNhap.MaLoaiXuatNhap == fileType).Max(x => x.SoPhieuXuat)) + 1 : 1;
                    var flag = false;
                    foreach (var worksheet in Workbook.Worksheets(filePath))
                    {
                        for (int i = 1; i < worksheet.Rows.Count(); i++)
                        {
                            var row = worksheet.Rows[i];
                            bool isSubItem = false;
                            var msg = ValidateDataImport(row, maNhaThuoc, fileType, ref isSubItem);
                            if (!string.IsNullOrEmpty(msg))
                            {
                                if (msg == Constants.Params.msgOk)
                                {
                                    if (row.Cells[0] != null && !string.IsNullOrEmpty(row.Cells[0].Text.Trim()))
                                    {
                                        flag = true;
                                        var phieuXuat = new PhieuXuat
                                        {
                                            Created = DateTime.Now,
                                            VAT = (row.Cells[2] != null && !string.IsNullOrEmpty(row.Cells[2].Text.Trim())) ? Convert.ToInt32(row.Cells[2].Text) : 0,
                                            DaTra = (row.Cells[3] != null && !string.IsNullOrEmpty(row.Cells[3].Text.Trim())) ? Convert.ToDecimal(row.Cells[3].Text) : 0,
                                            DienGiai = row.Cells[4] != null ? row.Cells[4].Text.Trim() : string.Empty,
                                            RecordStatusID = (byte)RecordStatus.Activated,
                                            NhaThuoc = unitOfWork.NhaThuocRepository.GetById(maNhaThuoc),
                                            SoPhieuXuat = soPhieu,
                                            CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId),
                                            LoaiXuatNhap = unitOfWork.LoaiXuatNhapRepository.GetById(fileType),
                                        };

                                        var dt = new DateTime();
                                        sThuoc.Utils.Helpers.ConvertToDateTime(row.Cells[0].Text, ref dt);
                                        phieuXuat.NgayXuat = dt;

                                        //check for khach hang
                                        if (fileType == 2)
                                        {
                                            var tenKhachHang = row.Cells[1].Text.Trim();
                                            var khachHang = unitOfWork.KhachHangRepository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.TenKhachHang.ToUpper() == tenKhachHang.ToUpper()).FirstOrDefault();
                                            phieuXuat.KhachHang = khachHang;
                                        }
                                        else if (fileType == 4)
                                        {
                                            //check for nha cung cap
                                            var tenNhaCungCap = row.Cells[1].Text.Trim();
                                            var nhaCungCap = unitOfWork.NhaCungCapRespository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.TenNhaCungCap.ToUpper() == tenNhaCungCap.ToUpper()).FirstOrDefault();
                                            phieuXuat.NhaCungCap = nhaCungCap;
                                        }

                                        phieuXuat.MaLoaiXuatNhap = fileType;
                                        //Add the row to phieu nhap
                                        phieuXuat.PhieuXuatChiTiets = new List<PhieuXuatChiTiet>
                                            {
                                                GenphieuXuatChiTiet(row)
                                            };
                                        //Add the item to list
                                        phieuXuats.Add(phieuXuat);

                                        // Tinh tong tien
                                        phieuXuat.TongTien = phieuXuat.PhieuXuatChiTiets.Sum(x => x.SoLuong * x.GiaXuat * (1 - x.ChietKhau / 100) * (1 + phieuXuat.VAT / 100));
                                        //Tang so phieu
                                        soPhieu++;
                                        totaladded++;
                                    }
                                    else
                                    {
                                        if (flag && phieuXuats.Count > 0)
                                        {
                                            var phieuXuat = phieuXuats.ElementAt(phieuXuats.Count - 1);
                                            phieuXuat.PhieuXuatChiTiets.Add(GenphieuXuatChiTiet(row));
                                            //cal culate the sum
                                            phieuXuat.TongTien = phieuXuat.PhieuXuatChiTiets.Sum(x => x.SoLuong * x.GiaXuat * (1 - x.ChietKhau / 100));
                                            phieuXuat.TongTien += phieuXuat.TongTien * phieuXuat.VAT / 100;
                                        }
                                    }
                                }
                                else
                                {
                                    if (!isSubItem)
                                        flag = false;
                                    info.ErrorMsg.Add(string.Format(message, i, msg));
                                    totalError++;
                                }
                            }
                        }

                        foreach (var phieuXuat in phieuXuats)
                        {
                            unitOfWork.PhieuXuatRepository.Insert(phieuXuat);
                        }

                        unitOfWork.Save();

                        info.Title = fileType == 2 ? "Thông tin upload phiếu xuất cho khách hàng" : "Thông tin upload phiếu xuất trả nhà cung cấp";
                        info.TotalUpdated = 0;
                        info.TotalAdded = totaladded;
                        info.TotalError = totalError;
                        Session["UploadMessageInfo"] = info;

                        return RedirectToAction("index", "Upload");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                ViewBag.FullMessage = ex.Message;
                return View("Error");
            }
            return RedirectToAction("Index", "PhieuNhaps", new { loaiPhieu = fileType });
        }

        private string ValidateDataImport(Excel.Row row, string maNhaThuoc, int fileType, ref bool isSubItem)
        {
            bool flag = false;
            string msg = string.Empty;
            for (int i = 0; i < row.Cells.Count(); i++)
            {
                if (row.Cells[i] != null && !string.IsNullOrEmpty(row.Cells[i].Text))
                {
                    flag = true;
                    break;
                }
            }

            if (flag)
            {
                isSubItem = false;
                if ((row.Cells[0] == null || string.IsNullOrEmpty(row.Cells[0].Text.Trim())) &&
                    (row.Cells[1] == null || string.IsNullOrEmpty(row.Cells[1].Text.Trim())) &&
                    (row.Cells[2] == null || string.IsNullOrEmpty(row.Cells[2].Text.Trim())) &&
                    (row.Cells[3] == null || string.IsNullOrEmpty(row.Cells[3].Text.Trim())))
                {
                    isSubItem = true;
                }

                decimal dTmp = 0;

                if (!isSubItem)
                {
                    if (row.Cells[0] != null && !string.IsNullOrEmpty(row.Cells[0].Text.Trim()))
                    {
                        var value = row.Cells[0].Text.Trim().ToUpperInvariant();
                        DateTime dt = new DateTime();
                        if (!sThuoc.Utils.Helpers.ConvertToDateTime(value, ref dt))
                        {
                            msg += "    - Ngày nhập phải là định dạng ngày tháng (dd/mm/yyyy hoặc dd-mm-yyyy)<br/>";
                        }
                    }

                    if (fileType == 2)
                    {
                        if (row.Cells[1] == null || string.IsNullOrEmpty(row.Cells[1].Text.Trim()))
                        {
                            msg += "    - Tên khách hàng không được bỏ trống <br/>";
                        }
                        else
                        {
                            var tenKhachHang = row.Cells[1].Text.Trim();
                            var khachHang = unitOfWork.KhachHangRepository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.TenKhachHang.ToUpper() == tenKhachHang.ToUpper()).FirstOrDefault();
                            if (khachHang == null)
                                msg += "    - Tên khách hàng không tồn tại vui lòng kiểm tra lại <br/>";
                        }
                    }
                    else
                    {
                        if (row.Cells[1] == null || string.IsNullOrEmpty(row.Cells[1].Text.Trim()))
                        {
                            msg += "    - Tên nhà cung cấp không được bỏ trống <br/>";
                        }
                        else
                        {
                            var tenNhaCungCap = row.Cells[1].Text.Trim();
                            var nhaCungCap = unitOfWork.NhaCungCapRespository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.TenNhaCungCap.ToUpper() == tenNhaCungCap.ToUpper()).FirstOrDefault();
                            if (nhaCungCap == null)
                                msg += "    - Tên nhà cung cấp không tồn tại vui lòng kiểm tra lại <br/>";
                        }
                    }

                    if (row.Cells[2] != null && !decimal.TryParse(row.Cells[2].Text, out dTmp))
                    {
                        msg += "    - VAT nhập phải là số <br/>";
                    }

                    if (row.Cells[3] != null && !decimal.TryParse(row.Cells[3].Text, out dTmp))
                    {
                        msg += "    - Tiền trả phải là số <br/>";
                    }
                }

                if (row.Cells[5] == null || string.IsNullOrEmpty(row.Cells[5].Text.Trim()))
                {
                    msg += "    - Mã thuốc không được bỏ trống <br/>";
                }
                else
                {
                    var maThuoc = row.Cells[5].Text.Trim();
                    var thuoc = unitOfWork.ThuocRepository.GetMany(
                            x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.MaThuoc == maThuoc).FirstOrDefault();
                    if (thuoc == null)
                    {
                        msg += "    - Mã thuốc không tồn tại vui lòng kiểm tra lại <br/>";
                    }
                }

                if (row.Cells[7] == null || string.IsNullOrEmpty(row.Cells[7].Text.Trim()))
                {
                    msg += "    - Đơn vị không được bỏ trống <br/>";
                }
                else
                {
                    if (row.Cells[5] != null && !string.IsNullOrEmpty(row.Cells[5].Text.Trim()))
                    {
                        var maThuoc = row.Cells[5].Text.Trim();
                        var thuoc = unitOfWork.ThuocRepository.GetMany(
                                x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.MaThuoc == maThuoc).FirstOrDefault();
                        if (thuoc != null)
                        {
                            //var tenDonViTinh = sThuoc.Utils.Helpers.ConvertToUTF8(row.Cells[6].Text.Trim());
                            var tenDonViTinh = row.Cells[7].Text.Trim();
                            var donViTinh = unitOfWork.DonViTinhRepository.GetMany(x => x.MaNhaThuoc == maNhaThuoc && x.TenDonViTinh.Equals(tenDonViTinh, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                            if (donViTinh != null)
                            {
                                if (donViTinh.MaDonViTinh != thuoc.DonViXuatLe.MaDonViTinh && (thuoc.DonViThuNguyen == null || (thuoc.DonViThuNguyen != null && donViTinh.MaDonViTinh != thuoc.DonViThuNguyen.MaDonViTinh)))
                                {
                                    msg += "    - Đơn vị tính này không tồn tại với mã thuốc này. Vui lòng kiểm tra lại <br/>";
                                }
                            }
                            else
                            {
                                msg += "    - Đơn vị tính này không tồn tại. Vui lòng kiểm tra lại <br/>";
                            }
                        }
                    }
                }

                if (row.Cells[8] == null && string.IsNullOrEmpty(row.Cells[8].Text.Trim()))
                {
                    msg += "    - Số lượng không được bỏ trống <br/>";
                }
                else if (row.Cells[8] != null && !decimal.TryParse(row.Cells[8].Text, out dTmp))
                {
                    msg += "    - Số lượng phải là số <br/>";
                }

                if (row.Cells[9] == null && string.IsNullOrEmpty(row.Cells[9].Text.Trim()))
                {
                    msg += "    - Đơn giá không được bỏ trống <br/>";
                }
                else if (row.Cells[9] != null && !decimal.TryParse(row.Cells[9].Text, out dTmp))
                {
                    msg += "    - Đơn giá phải là số <br/>";
                }

                if (row.Cells[10] != null && !decimal.TryParse(row.Cells[10].Text, out dTmp))
                {
                    msg += "    - Chiết khấu phải là số <br/>";
                }

                return string.IsNullOrEmpty(msg) ? Constants.Params.msgOk : msg;
            }
            else
            {
                return string.Empty;
            }
        }
        private PhieuXuatChiTiet GenphieuXuatChiTiet(Excel.Row row)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var phieuXuatChiTiet = new PhieuXuatChiTiet
            {
                SoLuong = Convert.ToDecimal(row.Cells[8].Value),
                GiaXuat = Convert.ToInt32(row.Cells[9].Value),
                ChietKhau = row.Cells[10] != null ? Convert.ToDecimal(row.Cells[10].Value) : 0,
                NhaThuoc = unitOfWork.NhaThuocRepository.GetMany(s => s.MaNhaThuoc == maNhaThuoc).FirstOrDefault()
            };
            //search thuoc
            var maThuoc = row.Cells[5].Text.Trim();
            var thuoc =
                unitOfWork.ThuocRepository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.MaThuoc.ToUpper() == maThuoc.ToUpper()).FirstOrDefault();

            phieuXuatChiTiet.Thuoc = thuoc;

            //search don vi tinh
            var tenDonViTinh = sThuoc.Utils.Helpers.RemoveEncoding(row.Cells[7].Text.Trim());
            var donvitinhs = unitOfWork.DonViTinhRepository.GetMany(x => x.MaNhaThuoc == maNhaThuoc).ToList();
            var donViTinh = new DonViTinh();
            foreach (var dv in donvitinhs)
            {
                if (sThuoc.Utils.Helpers.RemoveEncoding(dv.TenDonViTinh).Equals(tenDonViTinh, StringComparison.InvariantCultureIgnoreCase))
                {
                    donViTinh = dv;
                    break;
                }
            }

            phieuXuatChiTiet.DonViTinh = donViTinh;

            return phieuXuatChiTiet;
        }
        private void UpdateHanDung(Thuoc thuoc)
        {
            thuoc.HanDung = null;
            unitOfWork.Save();
        }
        //Functions convert number to Alphabe
        private static string Chu(string gNumber)
        {
            string result = "";
            switch (gNumber)
            {
                case "0":
                    result = "không";
                    break;
                case "1":
                    result = "một";
                    break;
                case "2":
                    result = "hai";
                    break;
                case "3":
                    result = "ba";
                    break;
                case "4":
                    result = "bốn";
                    break;
                case "5":
                    result = "năm";
                    break;
                case "6":
                    result = "sáu";
                    break;
                case "7":
                    result = "bảy";
                    break;
                case "8":
                    result = "tám";
                    break;
                case "9":
                    result = "chín";
                    break;
            }
            return result;
        }

        private static string Donvi(string so)
        {
            string Kdonvi = "";

            if (so.Equals("1"))
                Kdonvi = "";
            if (so.Equals("2"))
                Kdonvi = "nghìn";
            if (so.Equals("3"))
                Kdonvi = "triệu";
            if (so.Equals("4"))
                Kdonvi = "tỷ";
            if (so.Equals("5"))
                Kdonvi = "nghìn tỷ";
            if (so.Equals("6"))
                Kdonvi = "triệu tỷ";
            if (so.Equals("7"))
                Kdonvi = "tỷ tỷ";

            return Kdonvi;
        }
        private static string Tach(string tach3)
        {
            string Ktach = "";
            if (tach3.Equals("000"))
                return "";
            if (tach3.Length == 3)
            {
                string tr = tach3.Trim().Substring(0, 1).ToString().Trim();
                string ch = tach3.Trim().Substring(1, 1).ToString().Trim();
                string dv = tach3.Trim().Substring(2, 1).ToString().Trim();
                if (tr.Equals("0") && ch.Equals("0"))
                    Ktach = " không trăm lẻ " + Chu(dv.ToString().Trim()) + " ";
                if (!tr.Equals("0") && ch.Equals("0") && dv.Equals("0"))
                    Ktach = Chu(tr.ToString().Trim()).Trim() + " trăm ";
                if (!tr.Equals("0") && ch.Equals("0") && !dv.Equals("0"))
                    Ktach = Chu(tr.ToString().Trim()).Trim() + " trăm lẻ " + Chu(dv.Trim()).Trim() + " ";
                if (tr.Equals("0") && Convert.ToInt32(ch) > 1 && Convert.ToInt32(dv) > 0 && !dv.Equals("5"))
                    Ktach = " không trăm " + Chu(ch.Trim()).Trim() + " mươi " + Chu(dv.Trim()).Trim() + " ";
                if (tr.Equals("0") && Convert.ToInt32(ch) > 1 && dv.Equals("0"))
                    Ktach = " không trăm " + Chu(ch.Trim()).Trim() + " mươi ";
                if (tr.Equals("0") && Convert.ToInt32(ch) > 1 && dv.Equals("5"))
                    Ktach = " không trăm " + Chu(ch.Trim()).Trim() + " mươi lăm ";
                if (tr.Equals("0") && ch.Equals("1") && Convert.ToInt32(dv) > 0 && !dv.Equals("5"))
                    Ktach = " không trăm mười " + Chu(dv.Trim()).Trim() + " ";
                if (tr.Equals("0") && ch.Equals("1") && dv.Equals("0"))
                    Ktach = " không trăm mười ";
                if (tr.Equals("0") && ch.Equals("1") && dv.Equals("5"))
                    Ktach = " không trăm mười lăm ";
                if (Convert.ToInt32(tr) > 0 && Convert.ToInt32(ch) > 1 && Convert.ToInt32(dv) > 0 && !dv.Equals("5"))
                    Ktach = Chu(tr.Trim()).Trim() + " trăm " + Chu(ch.Trim()).Trim() + " mươi " + Chu(dv.Trim()).Trim() + " ";
                if (Convert.ToInt32(tr) > 0 && Convert.ToInt32(ch) > 1 && dv.Equals("0"))
                    Ktach = Chu(tr.Trim()).Trim() + " trăm " + Chu(ch.Trim()).Trim() + " mươi ";
                if (Convert.ToInt32(tr) > 0 && Convert.ToInt32(ch) > 1 && dv.Equals("5"))
                    Ktach = Chu(tr.Trim()).Trim() + " trăm " + Chu(ch.Trim()).Trim() + " mươi lăm ";
                if (Convert.ToInt32(tr) > 0 && ch.Equals("1") && Convert.ToInt32(dv) > 0 && !dv.Equals("5"))
                    Ktach = Chu(tr.Trim()).Trim() + " trăm mười " + Chu(dv.Trim()).Trim() + " ";

                if (Convert.ToInt32(tr) > 0 && ch.Equals("1") && dv.Equals("0"))
                    Ktach = Chu(tr.Trim()).Trim() + " trăm mười ";
                if (Convert.ToInt32(tr) > 0 && ch.Equals("1") && dv.Equals("5"))
                    Ktach = Chu(tr.Trim()).Trim() + " trăm mười lăm ";

            }

            return Ktach;
        }
        public static string So_chu(double gNum)
        {
            if (gNum == 0)
                return "Không đồng";

            string lso_chu = "";
            string tach_mod = "";
            string tach_conlai = "";
            double Num = Math.Round(gNum, 0);
            string gN = Convert.ToString(Num);
            int m = Convert.ToInt32(gN.Length / 3);
            int mod = gN.Length - m * 3;
            string dau = "[+]";

            // Dau [+ , - ]
            if (gNum < 0)
                dau = "[-]";
            dau = "";

            // Tach hang lon nhat
            if (mod.Equals(1))
                tach_mod = "00" + Convert.ToString(Num.ToString().Trim().Substring(0, 1)).Trim();
            if (mod.Equals(2))
                tach_mod = "0" + Convert.ToString(Num.ToString().Trim().Substring(0, 2)).Trim();
            if (mod.Equals(0))
                tach_mod = "000";
            // Tach hang con lai sau mod :
            if (Num.ToString().Length > 2)
                tach_conlai = Convert.ToString(Num.ToString().Trim().Substring(mod, Num.ToString().Length - mod)).Trim();

            ///don vi hang mod
            int im = m + 1;
            if (mod > 0)
                lso_chu = Tach(tach_mod).ToString().Trim() + " " + Donvi(im.ToString().Trim());
            /// Tach 3 trong tach_conlai

            int i = m;
            int _m = m;
            int j = 1;
            string tach3 = "";
            string tach3_ = "";

            while (i > 0)
            {
                tach3 = tach_conlai.Trim().Substring(0, 3).Trim();
                tach3_ = tach3;
                lso_chu = lso_chu.Trim() + " " + Tach(tach3.Trim()).Trim();
                m = _m + 1 - j;
                if (!tach3_.Equals("000"))
                    lso_chu = lso_chu.Trim() + " " + Donvi(m.ToString().Trim()).Trim();
                tach_conlai = tach_conlai.Trim().Substring(3, tach_conlai.Trim().Length - 3);

                i = i - 1;
                j = j + 1;
            }
            if (lso_chu.Trim().Substring(0, 1).Equals("k"))
                lso_chu = lso_chu.Trim().Substring(10, lso_chu.Trim().Length - 10).Trim();
            if (lso_chu.Trim().Substring(0, 1).Equals("l"))
                lso_chu = lso_chu.Trim().Substring(2, lso_chu.Trim().Length - 2).Trim();
            if (lso_chu.Trim().Length > 0)
                lso_chu = dau.Trim() + " " + lso_chu.Trim().Substring(0, 1).Trim().ToUpper() + lso_chu.Trim().Substring(1, lso_chu.Trim().Length - 1).Trim() + " đồng chẵn.";

            return lso_chu.ToString().Trim();

        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Lock
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Lock(int id)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            PhieuXuat phieuXuat =
                await
                    unitOfWork.PhieuXuatRepository.GetMany(
                        e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuXuat == id).FirstAsync();

            if (phieuXuat != null)
            {
                phieuXuat.Locked = true;
                unitOfWork.PhieuXuatRepository.Update(phieuXuat);
                unitOfWork.Save();
            }
           
            return RedirectToAction("Edit", "PhieuXuats", new { id });
        }

        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> UnLock(int id)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            PhieuXuat phieuXuat =
                await
                    unitOfWork.PhieuXuatRepository.GetMany(
                        e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuXuat == id).FirstAsync();

            if (phieuXuat != null)
            {
                phieuXuat.Locked = false;
                unitOfWork.PhieuXuatRepository.Update(phieuXuat);
                unitOfWork.Save();
            }

            return RedirectToAction("Edit", "PhieuXuats", new { id });
        }
        #endregion
    }

    class PhieuXuatComparer : IEqualityComparer<PhieuXuatChiTiet>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(PhieuXuatChiTiet x, PhieuXuatChiTiet y)
        {

            //Check whether the compared objects reference the same data.
            if (ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.MaPhieuXuatCt == y.MaPhieuXuatCt;
        }

        public int GetHashCode(PhieuXuatChiTiet phieuXuat)
        {
            //Check whether the object is null
            if (ReferenceEquals(phieuXuat, null)) return 0;

            //Get hash code for the Code field.
            int hashPhieuXuatCode = phieuXuat.MaPhieuXuatCt.GetHashCode();

            //Calculate the hash code for the product.
            return hashPhieuXuatCode;
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.       
    }
}
