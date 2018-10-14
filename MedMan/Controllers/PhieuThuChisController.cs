using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using PagedList;
using MedMan.App_Start;
using sThuoc.DAL;
using Med.Web.Extensions;
using sThuoc.Filter;
using sThuoc.Models;
using sThuoc.Repositories;
using Microsoft.Reporting.WebForms;
using System.IO;
using sThuoc.Utils;
using App.Common.MVC;
using Med.Web.Filter;
using App.Constants.Enums;

namespace Med.Web.Controllers
{
    public class PhieuThuChisController : BaseController
    {
        private SecurityContext db = new SecurityContext();
        //private readonly string this.GetNhaThuoc().MaNhaThuoc = UserService.GetMaNhaThuoc();
        private readonly UnitOfWork unitOfWork = new UnitOfWork();
        [SimpleAuthorize("Admin")]
        // GET: PhieuThuChis
        public ActionResult Index(string currentFilterLP, string loaiPhieu, string currentFilterTen, string searchTen, string currentFilterSoPhieu,
            string searchSoPhieu, string currentFilterTuNgay, string searchTuNgay, string currentFilterDenNgay, string searchDenNgay, int? page)
        {
            if (string.IsNullOrEmpty(loaiPhieu))
            {
                if (string.IsNullOrEmpty(currentFilterLP))
                {
                    loaiPhieu = "1";
                }
                else
                {
                    loaiPhieu = currentFilterLP;
                }
            }
            else
            {
                page = 1;
            }

            ViewBag.CurrentFilterLP = loaiPhieu;

            int loaiPhieuConverted = 0;
            if (!string.IsNullOrEmpty(loaiPhieu))
                loaiPhieuConverted = int.Parse(loaiPhieu);
            if (searchTen != null)
                page = 1;
            else
                searchTen = currentFilterTen;

            ViewBag.CurrentFilterTen = searchTen;

            if (searchSoPhieu != null)
                page = 1;
            else
                searchSoPhieu = currentFilterSoPhieu;
            ViewBag.CurrentFilterSoPhieu = searchSoPhieu;
            int? intSoPhieu = null;
            if (!String.IsNullOrEmpty(searchSoPhieu))
            {
                intSoPhieu = Convert.ToInt32(searchSoPhieu);
            }

            if (searchTuNgay != null)
                page = 1;
            else
            {
                searchTuNgay = currentFilterTuNgay;
                if (searchDenNgay == null)
                    searchDenNgay = currentFilterDenNgay;

            }
                
            DateTime? searchTuNgayConverted = null;
            DateTime? searchDenNgayConverted = null;
            if (!string.IsNullOrEmpty(searchTuNgay))
            {
                searchTuNgayConverted = DateTime.ParseExact(searchTuNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                //ViewBag.searchTuNgay = searchTuNgayConverted;
                ViewBag.CurrentFilterTuNgay = searchTuNgay;
            }
                
            if (!string.IsNullOrEmpty(searchDenNgay))
            {
                searchDenNgayConverted = DateTime.ParseExact(searchDenNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                //ViewBag.searchDenNgay = searchDenNgayConverted;
                ViewBag.CurrentFilterDenNgay = searchDenNgay;
            }                        

            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var phieuThuChisQry = unitOfWork.PhieuThuChiRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.LoaiPhieu == loaiPhieuConverted);
            if (!string.IsNullOrEmpty(searchTen))
            {
                phieuThuChisQry = phieuThuChisQry.Where(
                    e => e.NhaCungCap.TenNhaCungCap.Contains(searchTen) || e.KhachHang.TenKhachHang.Contains(searchTen) || e.NguoiNhan.Contains(searchTen));
                
            }
            if (!string.IsNullOrEmpty(searchSoPhieu))
            {
                phieuThuChisQry = phieuThuChisQry.Where(
                    e => e.SoPhieu == intSoPhieu);
            }
            if (!string.IsNullOrEmpty(searchTuNgay))
            {
                if(string.IsNullOrEmpty(searchDenNgay))
                {
                    phieuThuChisQry = phieuThuChisQry.Where(
                        e => DbFunctions.TruncateTime(e.NgayTao) == searchTuNgayConverted);
                }                    
                else
                {
                    phieuThuChisQry = phieuThuChisQry.Where(
                        e => DbFunctions.TruncateTime(e.NgayTao) >= searchTuNgayConverted && DbFunctions.TruncateTime(e.NgayTao) <= searchDenNgayConverted);
                }
            }
            phieuThuChisQry = phieuThuChisQry.OrderByDescending(e => e.NgayTao);
            ViewBag.TongTien = 0;
            ViewBag.TongTien =phieuThuChisQry.Any()? phieuThuChisQry.Sum(e => e.Amount):0;
            const int pageSize = 10;
            int pageNumber = (page ?? 1);
            var lstLoaiPhieu = GetThuChiKhac().ToList();
            lstLoaiPhieu.Insert(0, new SelectListItem
            {
                Text = "Phiếu Chi",
                Value = "2"
            });
            lstLoaiPhieu.Insert(0, new SelectListItem
            {
                Text = "Phiếu Thu",
                Value = "1"

            });
            ViewBag.loaiPhieu = lstLoaiPhieu;
            return View(phieuThuChisQry.ToPagedList(pageNumber, pageSize));
        }

        // GET: PhieuThuChis/Details/5
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PhieuThuChi phieuThuChi = await db.PhieuThuChis.FindAsync(id);
            if (phieuThuChi == null)
            {
                return HttpNotFound();
            }
            return View(phieuThuChi);
        }

        // GET: PhieuThuChis/Create
        [SimpleAuthorize("Admin")]
        public ActionResult Create(int? loaiPhieu, string ngayTao)
        {
            if (loaiPhieu == null)
                loaiPhieu = 1;

            ViewBag.MaKhachHang = new SelectList(_getListKhachHang(), "MaKhachHang", "TenKhachHang");

            ViewBag.MaNhaCungCap = new SelectList(_getListNhaCungCap(), "MaNhaCungCap", "TenNhaCungCap");
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.LoaiPhieu = loaiPhieu;
            ViewBag.MaLoaiPhieu = new SelectList(GetThuChiKhac(), "Value", "Text");
            ViewBag.SoPhieu = _generateAvailableSoPhieu();
            ViewBag.NgayTao = ngayTao == null ? DateTime.Now.ToString("dd/MM/yyyy HH:mm") : ngayTao;
            return View();
        }

        private int _generateAvailableSoPhieu()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var maxSoPhieu = unitOfWork.PhieuThuChiRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).Any() ? unitOfWork.PhieuThuChiRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).Max(e => e.SoPhieu) : 0;
            return maxSoPhieu + 1;
        }
        private List<NhaCungCap> _getListNhaCungCap()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var list =
                unitOfWork.NhaCungCapRespository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && !Constants.Default.ConstantEntities.NhaCungCapKiemKe.Contains(e.TenNhaCungCap))
                    .OrderBy(e => e.TenNhaCungCap).ToList();
            return list;
        }
        private List<KhachHang> _getListKhachHang()
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
            return list.OrderBy(e => e.Order).ToList();
        }
        // POST: PhieuThuChis/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public async Task<ActionResult> Create(PhieuThuChiEditModel phieuThuChi, string MaLoaiPhieu, int loaiPhieu)
        {
            ModelState.Remove("NgayTao");
            if (ModelState.IsValid)
            {
                var phieuThuChiMoi = new PhieuThuChi()
                {
                    LoaiPhieu = loaiPhieu,
                    Created = DateTime.Now,
                    CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId),
                    Amount = phieuThuChi.Amount,
                    DienGiai = phieuThuChi.DienGiai,
                    NgayTao = phieuThuChi.NgayTao,
                    NguoiNhan = phieuThuChi.NguoiNhan,
                    DiaChi = phieuThuChi.DiaChi,
                    NhaThuoc = unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc),
                    SoPhieu = _generateAvailableSoPhieu()
                };
                if (!String.IsNullOrEmpty(MaLoaiPhieu))
                    phieuThuChiMoi.LoaiPhieu = Convert.ToInt32(MaLoaiPhieu);

                if (phieuThuChi.MaKhachHang > 0)
                    phieuThuChiMoi.KhachHang = unitOfWork.KhachHangRepository.GetById(phieuThuChi.MaKhachHang);
                if (phieuThuChi.MaNhaCungCap > 0)
                    phieuThuChiMoi.NhaCungCap = unitOfWork.NhaCungCapRespository.GetById(phieuThuChi.MaNhaCungCap);

                unitOfWork.PhieuThuChiRepository.Insert(phieuThuChiMoi);
                unitOfWork.Save();
                //return RedirectToAction("In", new { id = phieuThuChiMoi.MaPhieu, loaiPhieu = phieuThuChiMoi.LoaiPhieu });
                return RedirectToAction("InDetail", new { id = phieuThuChiMoi.MaPhieu });
            }

            ViewBag.MaKhachHang = new SelectList(_getListKhachHang(), "MaKhachHang", "TenKhachHang");
            ViewBag.MaNhaCungCap = new SelectList(_getListNhaCungCap(), "MaNhaCungCap", "TenNhaCungCap");
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.LoaiPhieu = loaiPhieu;
            ViewBag.MaLoaiPhieu = new SelectList(GetThuChiKhac(), "Value", "Text");
            ViewBag.SoPhieu = _generateAvailableSoPhieu();
            return View(phieuThuChi);
        }

        public ActionResult In(int? id)
        {
            if (!id.HasValue)
            {
                ViewBag.Message = "Đường dẫn không đúng";
                return View("Error");
            }

            var nhathuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhathuoc.MaNhaThuoc;
            var item = unitOfWork.PhieuThuChiRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieu == id).FirstOrDefault();
            if (item == null)
            {
                ViewBag.Message = "Phiếu thu/chi không tồn tại";
                return View("Error");
            }

            decimal nocu = 0M;
            string khachhang = "";
            string diachikhachhang = "";
            string reportPath = "~/Reports/RptPhieuThuChiKhac.rdlc";
            List<ReportParameter> listParam = new List<ReportParameter>();
            if (item.LoaiPhieu == 1)
            {
                reportPath = "~/Reports/RptPhieuThu.rdlc";
                nocu = GetNoKhachHang(item.KhachHang.MaKhachHang, item.NgayTao);
                if (item.KhachHang != null)
                {
                    khachhang = item.KhachHang.TenKhachHang;
                    diachikhachhang = item.KhachHang.DiaChi;
                }

                listParam = new List<ReportParameter>(){
                new ReportParameter("pNhaThuoc",nhathuoc.TenNhaThuoc),
                new ReportParameter("pDiaChiNhaThuoc",nhathuoc.DiaChi),
                new ReportParameter("pSDTNhaThuoc",nhathuoc.DienThoai),
                new ReportParameter("pPhieuXuat",item.SoPhieu.ToString()),
                new ReportParameter("pNgayXuat",item.NgayTao.ToString("dd/MM/yyyy")),
                new ReportParameter("pKhachHang",khachhang),
                new ReportParameter("pDiaChiKhachHang",diachikhachhang),
                new ReportParameter("pNhanVien",item.CreatedBy.TenDayDu),
                new ReportParameter("pDienGiai",item.DienGiai),
                new ReportParameter("pSoTien",item.Amount.ToString("#,##0")),
                new ReportParameter("pSoTienBangChu",VNCurrency.ToString(item.Amount)),              
                new ReportParameter("pNoCu",nocu.ToString("#,##0")),                       
                new ReportParameter("pConNo",(nocu - item.Amount).ToString("#,##0"))};
            }
            else if (item.LoaiPhieu == 2)
            {
                nocu = GetNoNhaCungCap(item.NhaCungCap.MaNhaCungCap, item.NgayTao);
                reportPath = "~/Reports/RptPhieuChi.rdlc";
                if (item.NhaCungCap != null)
                {
                    khachhang = item.NhaCungCap.TenNhaCungCap;
                    diachikhachhang = item.NhaCungCap.DiaChi;
                }

                listParam = new List<ReportParameter>(){
                new ReportParameter("pNhaThuoc",nhathuoc.TenNhaThuoc),
                new ReportParameter("pDiaChiNhaThuoc",nhathuoc.DiaChi),
                new ReportParameter("pSDTNhaThuoc",nhathuoc.DienThoai),
                new ReportParameter("pPhieuXuat",item.SoPhieu.ToString()),
                new ReportParameter("pNgayXuat",item.NgayTao.ToString("dd/MM/yyyy")),
                new ReportParameter("pKhachHang",khachhang),
                new ReportParameter("pDiaChiKhachHang",diachikhachhang),
                new ReportParameter("pNhanVien",item.CreatedBy.TenDayDu),
                new ReportParameter("pDienGiai",item.DienGiai),
                new ReportParameter("pSoTien",item.Amount.ToString("#,##0")),
                new ReportParameter("pSoTienBangChu",VNCurrency.ToString(item.Amount)),              
                new ReportParameter("pNoCu",nocu.ToString("#,##0")),                       
                new ReportParameter("pConNo",(nocu - item.Amount).ToString("#,##0"))};
            }
            else
            {
                string type = "";
                string cusType = "";
                if (item.LoaiPhieu == Constants.LoaiPhieuThuChi.PhieuThuKhac)
                {
                    type = "Thu khác";
                    cusType = "Người nộp";
                }
                else if (item.LoaiPhieu == Constants.LoaiPhieuThuChi.PhieuChiKhac)
                {
                    type = "Chi khác";
                    cusType = "Người nhận";
                }
                else if (item.LoaiPhieu == Constants.LoaiPhieuThuChi.PhieuChiPhiKinhDoanh)
                {
                    type = "Chi phí kinh doanh";
                    cusType = "Người nhận";
                }

                listParam = new List<ReportParameter>(){
                new ReportParameter("pNhaThuoc",nhathuoc.TenNhaThuoc),
                new ReportParameter("pDiaChiNhaThuoc",nhathuoc.DiaChi),
                new ReportParameter("pSDTNhaThuoc",nhathuoc.DienThoai),
                new ReportParameter("pPhieuXuat",item.SoPhieu.ToString()),
                new ReportParameter("pNgayXuat",item.NgayTao.ToString("dd/MM/yyyy")),
                new ReportParameter("pCusType",cusType),
                new ReportParameter("pType",type),
                new ReportParameter("pKhachHang",item.NguoiNhan),
                new ReportParameter("pDiaChiKhachHang",item.DiaChi),
                new ReportParameter("pNhanVien",item.CreatedBy.TenDayDu),
                new ReportParameter("pDienGiai",item.DienGiai),
                new ReportParameter("pSoTien",item.Amount.ToString("#,##0"))};
            }




            ReportViewer viewer = new ReportViewer();
            viewer.ProcessingMode = ProcessingMode.Local;
            viewer.LocalReport.ReportPath = Server.MapPath(reportPath);
            viewer.LocalReport.DataSources.Add(new ReportDataSource());
            viewer.LocalReport.SetParameters(listParam);
            //var deviceInf = "<DeviceInfo><PageHeight>8.5in</PageHeight><PageWidth>11in</PageWidth></DeviceInfo>";
            byte[] bytes = viewer.LocalReport.Render("PDF");
            Stream stream = new MemoryStream(bytes);

            return File(stream, "application/pdf");
        }
        // GET: PhieuThuChis/Edit/5
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            PhieuThuChi phieuThuChi =
                await
                    unitOfWork.PhieuThuChiRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieu == id)
                        .FirstAsync();
            if (phieuThuChi == null)
            {
                return HttpNotFound();
            }
            var model = new PhieuThuChiEditModel()
            {
                Amount = phieuThuChi.Amount,
                DienGiai = phieuThuChi.DienGiai,
                LoaiPhieu = phieuThuChi.LoaiPhieu,
                MaKhachHang = phieuThuChi.KhachHang != null ? phieuThuChi.KhachHang.MaKhachHang : 0,
                MaNhaCungCap = phieuThuChi.NhaCungCap != null ? phieuThuChi.NhaCungCap.MaNhaCungCap : 0,
                MaNhaThuoc = phieuThuChi.NhaThuoc.MaNhaThuoc,
                MaPhieu = phieuThuChi.MaPhieu,
                NgayTao = phieuThuChi.NgayTao,
                SoPhieu = phieuThuChi.SoPhieu,
                NguoiNhan = phieuThuChi.NguoiNhan,
                DiaChi = phieuThuChi.DiaChi,
                NguoiLapPhieu = phieuThuChi.CreatedBy.TenDayDu
            };
            ViewBag.KhachHangs = new SelectList(_getListKhachHang(), "MaKhachHang", "TenKhachHang", model.MaKhachHang);
            ViewBag.NhaCungCaps = new SelectList(_getListNhaCungCap(), "MaNhaCungCap", "TenNhaCungCap", model.MaNhaCungCap);
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.LoaiPhieu = phieuThuChi.LoaiPhieu;
            ViewBag.LoaiPhieus = new SelectList(GetThuChiKhac(), "Value", "Text", model.LoaiPhieu);
            ViewBag.SoPhieu = phieuThuChi.SoPhieu;
            ViewBag.NguoiLapPhieu = phieuThuChi.CreatedBy.TenDayDu;
            if (phieuThuChi.KhachHang != null)
            {
                ViewBag.CongNo = GetNoKhachHang(phieuThuChi.KhachHang.MaKhachHang, phieuThuChi.NgayTao);
            }
            if (phieuThuChi.NhaCungCap != null)
            {
                ViewBag.CongNo = GetNoNhaCungCap(phieuThuChi.NhaCungCap.MaNhaCungCap, phieuThuChi.NgayTao);
            }
            return View(model);
        }

        // POST: PhieuThuChis/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]        
        public async Task<ActionResult> Edit(PhieuThuChiEditModel phieuThuChi, string MaLoaiPhieu, int loaiPhieu)
        {
            ModelState.Remove("NgayTao");
            if (ModelState.IsValid)
            {
                var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
                var phieuThuChiEdit = await unitOfWork.PhieuThuChiRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieu == phieuThuChi.MaPhieu).FirstOrDefaultAsync();
                if (phieuThuChiEdit != null)
                {
                    phieuThuChiEdit.LoaiPhieu = phieuThuChi.LoaiPhieu;
                    phieuThuChiEdit.NgayTao = phieuThuChi.NgayTao;
                    phieuThuChiEdit.Amount = phieuThuChi.Amount;
                    phieuThuChiEdit.DienGiai = phieuThuChi.DienGiai;
                    phieuThuChiEdit.Modified = DateTime.Now;
                    phieuThuChiEdit.NguoiNhan = phieuThuChi.NguoiNhan;
                    phieuThuChiEdit.DiaChi = phieuThuChi.DiaChi;
                    phieuThuChiEdit.ModifiedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId);
                    if (phieuThuChi.MaKhachHang > 0)
                        phieuThuChiEdit.KhachHang = unitOfWork.KhachHangRepository.GetById(phieuThuChi.MaKhachHang);
                    else
                        phieuThuChiEdit.KhachHang = null;
                    if (phieuThuChi.MaNhaCungCap > 0)
                        phieuThuChiEdit.NhaCungCap = unitOfWork.NhaCungCapRespository.GetById(phieuThuChi.MaNhaCungCap);
                    else
                        phieuThuChiEdit.NhaCungCap = null;
                    unitOfWork.PhieuThuChiRepository.Update(phieuThuChiEdit);
                    unitOfWork.Save();
                }

                return RedirectToAction("Index", new { loaiPhieu = phieuThuChi.LoaiPhieu });
            }

            ViewBag.KhachHangs = new SelectList(_getListKhachHang(), "MaKhachHang", "TenKhachHang");
            ViewBag.NhaCungCaps = new SelectList(_getListNhaCungCap(), "MaNhaCungCap", "TenNhaCungCap");
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.LoaiPhieu = loaiPhieu;
            ViewBag.LoaiPhieus = new SelectList(GetThuChiKhac(), "Value", "Text", phieuThuChi.LoaiPhieu);
            ViewBag.SoPhieu = _generateAvailableSoPhieu();
            return View(phieuThuChi);
        }

        // GET: PhieuThuChis/Delete/5
        //[SimpleAuthorize("Admin")]
        public async Task<ActionResult> InDetail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            PhieuThuChi phieuThuChi =
                await
                    unitOfWork.PhieuThuChiRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieu == id)
                        .FirstAsync();
            if (phieuThuChi == null)
            {
                return HttpNotFound();
            }
            ViewBag.LoaiPhieu = phieuThuChi.LoaiPhieu;
            ViewBag.SoPhieu = phieuThuChi.SoPhieu;
            ViewBag.ngayTao = phieuThuChi.NgayTao;
            return View(phieuThuChi);
        }

        // GET: PhieuThuChis/Delete/5
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            PhieuThuChi phieuThuChi =
                await
                    unitOfWork.PhieuThuChiRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieu == id)
                        .FirstAsync();
            if (phieuThuChi == null)
            {
                return HttpNotFound();
            }
            ViewBag.LoaiPhieu = phieuThuChi.LoaiPhieu;
            ViewBag.SoPhieu = phieuThuChi.SoPhieu;
            return View(phieuThuChi);
        }

        // POST: PhieuThuChis/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // [Audit]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var phieuThuChi =
                await
                    unitOfWork.PhieuThuChiRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieu == id)
                        .FirstOrDefaultAsync();
            if (phieuThuChi != null)
            {
                unitOfWork.PhieuThuChiRepository.Delete(phieuThuChi);
                unitOfWork.Save();
                return RedirectToAction("Index", new { loaiPhieu = phieuThuChi.LoaiPhieu });
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Get No from an entity
        /// </summary>
        /// <param name="ma"></param>
        /// <param name="isKhachHang"></param>
        /// <returns></returns>
        public JsonResult GetNo(int ma, bool isKhachHang, string ngaytao)
        {
            DateTime dt = DateTime.Now;
            DateTime.TryParse(ngaytao, out dt);
            decimal? result;
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            if (isKhachHang)
            {
                result = GetNoKhachHang(ma, dt);
            }
            else
            {
                result = GetNoNhaCungCap(ma, dt);
            }
            return new JsonResult() { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private decimal GetNoKhachHang(int maKhachHang, DateTime ngaytao)
        {
            ngaytao = ngaytao.AddDays(1);
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var result = unitOfWork.PhieuXuatRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.KhachHang.MaKhachHang == maKhachHang && e.RecordStatusID == (byte)RecordStatus.Activated && e.NgayXuat < ngaytao)
                       .Sum(x => (int?)x.TongTien - (int?)x.DaTra);
            var slThuChi = unitOfWork.PhieuThuChiRepository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.KhachHang.MaKhachHang == maKhachHang && x.NgayTao < ngaytao)
                .Sum(x => (int?)x.Amount) ?? 0;
            result = result - slThuChi;
            return result.HasValue ? result.Value : 0;
        }
        private decimal GetNoNhaCungCap(int maNhaCungCap, DateTime ngaytao)
        {
            ngaytao = ngaytao.AddDays(1);
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var result =
                  unitOfWork.PhieuNhapRepository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.NhaCungCap.MaNhaCungCap == maNhaCungCap && x.RecordStatusID == (byte)RecordStatus.Activated && x.NgayNhap < ngaytao)
                      .Sum(x => (int?)x.TongTien - (int?)x.DaTra);
            var slThuChi = unitOfWork.PhieuThuChiRepository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.NhaCungCap.MaNhaCungCap == maNhaCungCap && x.NgayTao < ngaytao).Sum(x => (int?)x.Amount) ?? 0;
            result = result - slThuChi;
            return result.HasValue ? result.Value : 0;
        }

        private IEnumerable<SelectListItem> GetThuChiKhac()
        {
            return new List<SelectListItem>
            {
                new SelectListItem(){Text="Thu Khác", Value="3"},
                new SelectListItem(){Text="Chi Khác", Value="4"},
                new SelectListItem(){Text="Chi Phí Kinh Doanh", Value="5"}
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
