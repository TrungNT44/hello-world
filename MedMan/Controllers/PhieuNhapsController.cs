using System.Configuration;
using System.Drawing;
using System.Web.Services.Configuration;
using System.Web.UI.WebControls;
using App.Common.DI;
using Med.Service.Drug;
using Med.Service.Receipt;
using Med.Web.Data.Session;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using OfficeOpenXml.Style;
using PagedList;
using MedMan.App_Start;
using sThuoc.DAL;
using Med.Web.Extensions;
using sThuoc.Filter;
using sThuoc.Models;
using sThuoc.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebGrease.Css.Extensions;
using sThuoc.Utils;
using sThuoc.Models.ViewModels;
using Excel;
using sThuoc.Models.Reports;
using Microsoft.Reporting.WebForms;
using App.Common.MVC;
using Med.Common;
using Med.Web.Filter;
using System.ComponentModel.DataAnnotations;
using Med.Service.Log;
using Med.Web.Helpers;
using Med.Service.Caching;
using App.Common.Helpers;
using App.Constants.Enums;

namespace Med.Web.Controllers
{
    public class PhieuNhapsController : BaseController
    {
        private SecurityContext db = new SecurityContext();
        //private readonly string this.GetNhaThuoc().MaNhaThuoc = UserService.GetMaNhaThuoc();
        public ImportError ImportError = new ImportError();
        private readonly UnitOfWork unitOfWork = new UnitOfWork();
        // GET: PhieuNhaps
        [InputBillAuthorizeAttribute("Admin")]
        public ActionResult Index(int? loaiPhieu, string currentFilterTen, string searchTen,
            long? searchSoPhieu, string searchMaThuoc, string searchTenNhanVien, string searchDienGiai, string searchSoLo, string searchHanDung, string searchNgay, int? page, int? khoiPhuc = 0, int export = 0)
        {
            if (!loaiPhieu.HasValue)
                loaiPhieu = 1;
            ViewBag.CurrentFilterLP = loaiPhieu;
            ViewBag.CurrentFilterTen = searchTen;
            ViewBag.CurrentFilterKhoiPhuc = khoiPhuc;
            ViewBag.CurrentFilterSoPhieu = searchSoPhieu;
            ViewBag.CurrentFilterMaThuoc = searchMaThuoc;
            ViewBag.CurrentFilterTenNhanVien = searchTenNhanVien;
            ViewBag.CurrentFilterDienGiai = searchDienGiai;
            ViewBag.CurrentFilterSoLo = searchSoLo;
            ViewBag.CurrentFilterHanDung = searchHanDung;
            ViewBag.CurrentFilterNgay = searchNgay;

            //Dvtu: đánh dấu xem đang search theo tiêu chí Tìm Theo nào. Mặc định là 2
            if (searchSoPhieu.HasValue)
                ViewBag.SearchFor = "1";
            else if (!string.IsNullOrEmpty(searchMaThuoc))
                ViewBag.SearchFor = "2";
            else if (!string.IsNullOrEmpty(searchTenNhanVien))
                ViewBag.SearchFor = "3";
            else if (!string.IsNullOrEmpty(searchDienGiai))
                ViewBag.SearchFor = "4";
            else if (!string.IsNullOrEmpty(searchSoLo))
                ViewBag.SearchFor = "5";
            else if (!string.IsNullOrEmpty(searchHanDung))
                ViewBag.SearchFor = "6";         
            else
                ViewBag.SearchFor = "2";

            DateTime? searchNgayConverted = null;
            DateTime? searchHanDungConverted = null;
            if (!string.IsNullOrEmpty(searchNgay))
                searchNgayConverted = DateTime.ParseExact(searchNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            ViewBag.CurrentFilterNgay = searchNgay;

            if (!string.IsNullOrEmpty(searchHanDung))
                searchHanDungConverted = DateTime.ParseExact(searchHanDung, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var recStatusId = khoiPhuc > 0 ? (byte)RecordStatus.Deleted : (byte)RecordStatus.Activated;
            
            var restore = (khoiPhuc > 0);
            // phieu nhap
            var pNhapsQuery = unitOfWork.PhieuNhapRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.RecordStatusID == recStatusId && e.LoaiXuatNhap.MaLoaiXuatNhap == loaiPhieu);
            if (searchSoPhieu.HasValue)
                pNhapsQuery = pNhapsQuery.Where(e => e.SoPhieuNhap == searchSoPhieu.Value);
            if (!string.IsNullOrEmpty(searchNgay))
            {
                pNhapsQuery = pNhapsQuery.Where(e => DbFunctions.TruncateTime(e.NgayNhap) == searchNgayConverted);
            }
            // nhap tu NCC
            if (loaiPhieu.Value == 1 && !string.IsNullOrEmpty(searchTen))
            {
                pNhapsQuery = pNhapsQuery.Where(e => e.NhaCungCap.TenNhaCungCap.Contains(searchTen));
            }
            // nhap tu KH
            if (loaiPhieu.Value == 3 && !string.IsNullOrEmpty(searchTen))
            {
                pNhapsQuery = pNhapsQuery.Where(e => e.KhachHang.TenKhachHang.Contains(searchTen));
            }
            // tim theo ma sp
            if (!string.IsNullOrEmpty(searchMaThuoc))
            {
                pNhapsQuery =
                    pNhapsQuery.Where(e => e.PhieuNhapChiTiets.Any(ee => ee.Thuoc.MaThuoc.ToLower() == searchMaThuoc));
            }
            // tim theo nhan vien
            if (!string.IsNullOrEmpty(searchTenNhanVien))
            {
                pNhapsQuery =
                    pNhapsQuery.Where(e => e.CreatedBy != null && e.CreatedBy.TenDayDu.Contains(searchTenNhanVien));
            }
            // tim theo dien giai
            if (!string.IsNullOrEmpty(searchDienGiai))
            {
                pNhapsQuery =
                    pNhapsQuery.Where(e => e.CreatedBy != null && e.DienGiai.Contains(searchDienGiai));
            }
            // tim theo so lo
            if (!string.IsNullOrEmpty(searchSoLo))
            {
                pNhapsQuery =
                    pNhapsQuery.Where(e => e.PhieuNhapChiTiets.Any(ee => ee.SoLo.ToLower() == searchSoLo));
            }
            // Tim theo han dung
            if (!string.IsNullOrEmpty(searchHanDung))
            {
                pNhapsQuery =
                    pNhapsQuery.Where(e => e.PhieuNhapChiTiets.Any(ee => ee.HanDung == searchHanDungConverted));
            }
            //if(khoiPhuc.HasValue)
            //    pNhapsQuery =
            //        pNhapsQuery.Where(e => e.Xoa==true);
            //pNhapsQuery = pNhapsQuery.OrderByDescending(e => e.NgayNhap).Include(e => e.CreatedBy);
            pNhapsQuery = pNhapsQuery.OrderByDescending(e => e.SoPhieuNhap).ThenByDescending(e => e.NgayNhap);

            recStatusId = khoiPhuc > 0 ? (byte)RecordStatus.Deleted : (byte)RecordStatus.Activated;
            // phieu xuat
            var pXuatsQuery = unitOfWork.PhieuXuatRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.RecordStatusID == recStatusId && e.LoaiXuatNhap.MaLoaiXuatNhap == loaiPhieu);
            if (searchSoPhieu.HasValue)
                pXuatsQuery = pXuatsQuery.Where(e => e.SoPhieuXuat == searchSoPhieu.Value);
            if (!string.IsNullOrEmpty(searchNgay))
            {
                pXuatsQuery = pXuatsQuery.Where(e => DbFunctions.TruncateTime(e.NgayXuat) == searchNgayConverted);
            }
            // xuat cho KH
            if (loaiPhieu.Value == 2 && !string.IsNullOrEmpty(searchTen))
            {
                pXuatsQuery = pXuatsQuery.Where(e => e.KhachHang.TenKhachHang.Contains(searchTen));
            }
            // xuat cho NCC
            if (loaiPhieu.Value == 4 && !string.IsNullOrEmpty(searchTen))
            {
                pXuatsQuery = pXuatsQuery.Where(e => e.NhaCungCap.TenNhaCungCap.Contains(searchTen));
            }
            // tim theo ma sp
            if (!string.IsNullOrEmpty(searchMaThuoc))
            {
                pXuatsQuery =
                    pXuatsQuery.Where(e => e.PhieuXuatChiTiets.Any(ee => ee.Thuoc.MaThuoc.ToLower() == searchMaThuoc));
            }
            // tim theo tên nhân viên
            if (!string.IsNullOrEmpty(searchTenNhanVien))
            {
                pXuatsQuery =
                    pXuatsQuery.Where(e => e.CreatedBy != null && e.CreatedBy.TenDayDu.Contains(searchTenNhanVien));
            }
            // tìm theo diễn giải
            if (!string.IsNullOrEmpty(searchDienGiai))
            {
                pXuatsQuery =
                    pXuatsQuery.Where(e => e.CreatedBy != null && e.DienGiai.Contains(searchDienGiai));
            }
            //if (khoiPhuc.HasValue)
            //    pXuatsQuery =
            //        pXuatsQuery.Where(e => e.Xoa == khoiPhuc.HasValue);

            //pXuatsQuery = pXuatsQuery.OrderByDescending(e => e.NgayXuat).Include(e => e.CreatedBy);
            pXuatsQuery = pXuatsQuery.OrderByDescending(e => e.SoPhieuXuat).ThenByDescending(e => e.NgayXuat);

            const int pageSize = 10;
            int pageNumber = (page ?? 1);
            var result = new PhieusModel();
            if (export != 1)
            {


                if ((loaiPhieu == 1) || (loaiPhieu == 3))
                {
                    result.PhieuNhaps = pNhapsQuery.ToPagedList(pageNumber, pageSize);
                }
                else if ((loaiPhieu == 2) || (loaiPhieu == 4))
                {
                    result.PhieuXuats = pXuatsQuery.ToPagedList(pageNumber, pageSize);
                }
                ViewBag.LoaiPhieu = db.LoaiXuatNhaps.Where(c => !c.TenLoaiXuatNhap.Equals(Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe)
                    && (!c.IsHidden.HasValue || !c.IsHidden.Value)).ToList();
                return View(result);
            }
            else
            {
                // export 
                if ((loaiPhieu == 1) || (loaiPhieu == 3))
                {
                    return ExportPhieuNhaps(pNhapsQuery, loaiPhieu.Value);
                }

                return ExportPhieuXuats(pXuatsQuery, loaiPhieu.Value);
            }
        }

        private ActionResult ExportPhieuXuats(IQueryable<PhieuXuat> pXuatsQuery, int type)
        {
            var dataTable = new DataTable("Phiếu xuất");
            dataTable.Columns.Add("Ngày xuất (0)", typeof(string));
            if (type == 2)
                dataTable.Columns.Add("Khách hàng (1)", typeof(string));
            else
                dataTable.Columns.Add("Nhà cung cấp (1)", typeof(string));

            dataTable.Columns.Add("VAT (2)", typeof(string));
            dataTable.Columns.Add("Trả (3)", typeof(string));

            dataTable.Columns.Add("Diễn giải (4)", typeof(string));
            dataTable.Columns.Add("Mã Thuốc (5)", typeof(string));
            dataTable.Columns.Add("Tên Thuốc (6)", typeof(string));
            dataTable.Columns.Add("Đơn vị (7)", typeof(string));
            dataTable.Columns.Add("Số lượng (8)", typeof(int));
            dataTable.Columns.Add("Đơn giá (9)", typeof(int));
            dataTable.Columns.Add("Chiết khấu (10)", typeof(int));


            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var items = pXuatsQuery.ToList();
            //Add to rows
            foreach (var item in items)
            {
                var isHeader = true;
                foreach (var detail in item.PhieuXuatChiTiets)
                {
                    if (isHeader)
                    {
                        DataRow dr = dataTable.NewRow();
                        if (item.NgayXuat != null) dr["Ngày xuất (0)"] = item.NgayXuat.Value.ToString("dd/MM/yyyy");
                        if (type == 2)
                            dr["Khách hàng (1)"] = item.KhachHang.TenKhachHang;
                        else
                            dr["Nhà cung cấp (1)"] = item.NhaCungCap.TenNhaCungCap;

                        dr["VAT (2)"] = item.VAT;
                        dr["Trả (3)"] = item.DaTra;
                        dr["Diễn giải (4)"] = item.DienGiai;
                        dr["Mã Thuốc (5)"] = detail.Thuoc.MaThuoc;
                        dr["Tên Thuốc (6)"] = detail.Thuoc.TenThuoc;
                        dr["Đơn vị (7)"] = detail.DonViTinh.TenDonViTinh;
                        dr["Số lượng (8)"] = detail.SoLuong;
                        dr["Đơn giá (9)"] = detail.GiaXuat;
                        dr["Chiết khấu (10)"] = detail.ChietKhau;
                        dataTable.Rows.Add(dr);
                        isHeader = false;
                    }
                    else
                    {
                        DataRow dr = dataTable.NewRow();
                        dr["Ngày xuất (0)"] = "";
                        if (type == 2)
                            dr["Khách hàng (1)"] = "";
                        else
                            dr["Nhà cung cấp (1)"] = "";

                        dr["VAT (2)"] = "";
                        dr["Trả (3)"] = "";
                        dr["Diễn giải (4)"] = "";
                        dr["Mã Thuốc (5)"] = detail.Thuoc.MaThuoc;
                        dr["Tên Thuốc (6)"] = detail.Thuoc.TenThuoc;
                        dr["Đơn vị (7)"] = detail.DonViTinh.TenDonViTinh;
                        dr["Số lượng (8)"] = detail.SoLuong;
                        dr["Đơn giá (9)"] = detail.GiaXuat;
                        dr["Chiết khấu (10)"] = detail.ChietKhau;
                        dataTable.Rows.Add(dr);
                    }

                }

            }

            using (var pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Phiếu xuất");

                //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                ws.Cells["A1"].LoadFromDataTable(dataTable, true);

                //Format the header for column 1-3
                using (ExcelRange rng = ws.Cells["A1:O1"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                    rng.Style.Font.Color.SetColor(Color.White);
                }

                //Example how to Format Column 1 as numeric 
                //using (ExcelRange col = ws.Cells[2, 5, 2 + dataTable.Rows.Count, 7])
                //{
                //    col.Style.Numberformat.Format = "0.##0";
                //    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                //}
                //using (ExcelRange col = ws.Cells[2, 10, 2 + dataTable.Rows.Count, 13])
                //{
                //    col.Style.Numberformat.Format = "0.##0";
                //    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                //}

                var fileDownloadName = "phieuxuat-" + DateTime.Now + ".xlsx";

                var fileStream = new MemoryStream();
                pck.SaveAs(fileStream);
                fileStream.Position = 0;

                var fsr = new FileStreamResult(fileStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                fsr.FileDownloadName = fileDownloadName;

                return fsr;
            }
        }

        private ActionResult ExportPhieuNhaps(IQueryable<PhieuNhap> pNhapsQuery, int type)
        {
            var dataTable = new DataTable("Phiếu nhập");
            dataTable.Columns.Add("Ngày nhập (0)", typeof(string));
            if (type == 1)
                dataTable.Columns.Add("Nhà cung cấp (1)", typeof(string));
            else
                dataTable.Columns.Add("Khách hàng (1)", typeof(string));

            dataTable.Columns.Add("VAT (2)", typeof(string));
            dataTable.Columns.Add("Trả (3)", typeof(string));

            dataTable.Columns.Add("Diễn giải (4)", typeof(string));
            dataTable.Columns.Add("Mã Thuốc (5)", typeof(string));
            dataTable.Columns.Add("Tên Thuốc (6)", typeof(string));
            dataTable.Columns.Add("Đơn vị (7)", typeof(string));
            dataTable.Columns.Add("Số lượng (8)", typeof(int));
            dataTable.Columns.Add("Đơn giá (9)", typeof(int));
            dataTable.Columns.Add("Chiết khấu (10)", typeof(int));
            dataTable.Columns.Add("Số lô (11)", typeof(string));
            dataTable.Columns.Add("Hạn dùng (12)", typeof(string));

            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var items = pNhapsQuery.ToList();
            //Add to rows
            foreach (var item in items)
            {
                var isHeader = true;
                foreach (var detail in item.PhieuNhapChiTiets)
                {
                    if (isHeader)
                    {
                        DataRow dr = dataTable.NewRow();
                        if (item.NgayNhap != null) dr["Ngày nhập (0)"] = item.NgayNhap.Value.ToString("dd/MM/yyyy");
                        if (type == 1)
                            dr["Nhà cung cấp (1)"] = item.NhaCungCap.TenNhaCungCap;
                        else
                            dr["Khách hàng (1)"] = item.KhachHang.TenKhachHang;

                        dr["VAT (2)"] = item.VAT;
                        dr["Trả (3)"] = item.DaTra;
                        dr["Diễn giải (4)"] = item.DienGiai;
                        dr["Mã Thuốc (5)"] = detail.Thuoc.MaThuoc;
                        dr["Tên Thuốc (6)"] = detail.Thuoc.TenThuoc;
                        dr["Đơn vị (7)"] = detail.DonViTinh.TenDonViTinh;
                        dr["Số lượng (8)"] = detail.SoLuong;
                        dr["Đơn giá (9)"] = detail.GiaNhap;
                        dr["Chiết khấu (10)"] = detail.ChietKhau;
                        if (detail.SoLo != null)                        
                            dr["Số lô (11)"] = detail.SoLo;                     
                        else
                        {
                            dr["Số lô (11)"] = "";
                        }
                            
                       
                        if (detail.HanDung != null)
                            dr["Hạn dùng (12)"] = detail.HanDung.Value.ToString("dd/MM/yyyy");                      
                        else                      
                            dr["Hạn dùng (12)"] = "";
                     
                        
                        dataTable.Rows.Add(dr);
                        isHeader = false;
                    }
                    else
                    {
                        DataRow dr = dataTable.NewRow();
                        dr["Ngày nhập (0)"] = "";
                        if (type == 1)
                            dr["Nhà cung cấp (1)"] = "";
                        else
                            dr["Khách hàng (1)"] = "";
                        dr["VAT (2)"] = "";
                        dr["Trả (3)"] = "";
                        dr["Diễn giải (4)"] = "";
                        dr["Mã Thuốc (5)"] = detail.Thuoc.MaThuoc;
                        dr["Tên Thuốc (6)"] = detail.Thuoc.TenThuoc;
                        dr["Đơn vị (7)"] = detail.DonViTinh.TenDonViTinh;
                        dr["Số lượng (8)"] = detail.SoLuong;
                        dr["Đơn giá (9)"] = detail.GiaNhap;
                        dr["Chiết khấu (10)"] = detail.ChietKhau;

                        if (detail.SoLo != null)
                            dr["Số lô (11)"] = detail.SoLo;
                        else
                        {
                            dr["Số lô (11)"] = "";
                        }


                        if (detail.HanDung != null)
                            dr["Hạn dùng (12)"] = detail.HanDung.Value.ToString("dd/MM/yyyy"); 
                        else
                            dr["Hạn dùng (12)"] = "";
                        dataTable.Rows.Add(dr);
                    }

                }

            }

            using (var pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Phiếu nhập");

                //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                ws.Cells["A1"].LoadFromDataTable(dataTable, true);

                //Format the header for column 1-3
                using (ExcelRange rng = ws.Cells["A1:O1"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                    rng.Style.Font.Color.SetColor(Color.White);
                }

                //Example how to Format Column 1 as numeric 
                //using (ExcelRange col = ws.Cells[2, 5, 2 + dataTable.Rows.Count, 7])
                //{
                //    col.Style.Numberformat.Format = "0.##0";
                //    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                //}
                //using (ExcelRange col = ws.Cells[2, 10, 2 + dataTable.Rows.Count, 13])
                //{
                //    col.Style.Numberformat.Format = "0.##0";
                //    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                //}

                var fileDownloadName = "phieunhap-" + DateTime.Now + ".xlsx";

                var fileStream = new MemoryStream();
                pck.SaveAs(fileStream);
                fileStream.Position = 0;

                var fsr = new FileStreamResult(fileStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                fsr.FileDownloadName = fileDownloadName;

                return fsr;
            }
        }

        // GET: PhieuNhaps/Details/5
        [InputBillAuthorizeAttribute("Admin")]
        public async Task<ActionResult> Details(int? id, int? soPhieu = null)
        {
            if (id == null && soPhieu == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            PhieuNhap phieuNhap = null;
            if (id.HasValue)
            {
                phieuNhap = await
                       unitOfWork.PhieuNhapRepository.GetMany(
                           e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuNhap == id).FirstAsync();
            }
            else
            {
                phieuNhap = await
                       unitOfWork.PhieuNhapRepository.GetMany(
                           e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.SoPhieuNhap == soPhieu).FirstAsync();
            }
            
            if (phieuNhap == null)
            {
                return HttpNotFound();
            }
            // convert model to modelView
            var model = new PhieuNhapEditModel(phieuNhap);
            ViewBag.SoPhieu = model.SoPhieuNhap;
            ViewBag.LoaiPhieu = model.MaLoaiXuatNhap;
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;

            ViewBag.MaDonViTinh = _getListDonViTinh();
            return View(model);
        }

        public async Task<ActionResult> InDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var phieuNhap =
                await
                    unitOfWork.PhieuNhapRepository.GetMany(
                        e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuNhap == id).FirstAsync();
            if (phieuNhap == null)
            {
                return HttpNotFound();
            }
            // convert model to modelView
            var model = new PhieuNhapEditModel(phieuNhap);
            ViewBag.SoPhieu = model.SoPhieuNhap;
            ViewBag.LoaiPhieu = model.MaLoaiXuatNhap;
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;

            ViewBag.MaDonViTinh = _getListDonViTinh();
            return View(model);
        }

        // GET: PhieuNhaps/Create
        [InputBillAuthorizeAttribute("Admin")]
        public ActionResult Create(string loaiPhieu, string ngaynhap)
        {
            if (string.IsNullOrEmpty(loaiPhieu))
            {
                loaiPhieu = "1";
            }

            ViewBag.SoPhieu = _generateAvaliableSoPhieu();
            ViewBag.LoaiPhieu = loaiPhieu;
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            ViewBag.MaLoaiXuatNhap = new SelectList(db.LoaiXuatNhaps.Where(c => !c.IsHidden.HasValue || !c.IsHidden.Value).ToList(), "MaLoaiXuatNhap", "TenLoaiXuatNhap");
            ViewBag.MaNhaCungCap = new SelectList(_getListNhaCungCap(), "MaNhaCungCap", "TenNhaCungCap");
            ViewBag.MaKhachHang = new SelectList(_getListKhachHang(), "MaKhachHang", "TenKhachHang");
            ViewBag.NgayNhap = ngaynhap == null ? DateTime.Now.ToString("dd/MM/yyyy") : ngaynhap;
            ViewBag.MaDonViTinh = _getListDonViTinh();
            return View(new PhieuNhapEditModel());
        }

        private long _generateAvaliableSoPhieu()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var maxSoPhieu = unitOfWork.PhieuNhapRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).Any() ? unitOfWork.PhieuNhapRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).Max(e => e.SoPhieuNhap) : 0;
            return maxSoPhieu + 1;
        }
        private List<DonViTinh> _getListDonViTinh()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var maNhaThuocCha = this.GetNhaThuoc().MaNhaThuocCha;
            return
                unitOfWork.DonViTinhRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha)
                    .OrderBy(e => e.TenDonViTinh).ToList();
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
            //return list;
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
            return list.OrderBy(e => e.Order).ToList();
        }
        // POST: PhieuNhaps/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [InputBillAuthorizeAttribute("Admin")]
        // [Audit]
        public ActionResult Create(PhieuNhapEditModel phieuNhap)
        {
            ModelState.Remove("NgayNhap");
            foreach (var state in ModelState.Keys.ToList().Where(c => c.Contains("ChietKhau")))
            {
                ModelState.Remove(state);
            }
            if (!phieuNhap.NgayNhap.HasValue || phieuNhap.NgayNhap.Value < MedConstants.MinProductionDataDate)
            {
                ViewBag.Message = "Ngày nhập hàng không hợp lệ. Ngày nhập hàng không được trống và phải lớn hơn ngày 01-01-2010.";
                return View("Error");
            }

            if (ModelState.IsValid)
            {
                // validate 
                var nhathuoc = unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc);
                string maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
                string maNhaThuocCha = this.GetNhaThuoc().MaNhaThuocCha;
                var ma = (String.IsNullOrEmpty(maNhaThuocCha) ? maNhaThuoc : maNhaThuocCha);
                var dtNow = DateTime.Now;
                var createDate = dtNow;
                var noteItems = new List<PhieuNhapChiTiet>();
                if (phieuNhap.NgayNhap.HasValue)
                {
                    var noteDate = phieuNhap.NgayNhap.Value;
                    createDate = new DateTime(noteDate.Year, noteDate.Month, noteDate.Day
                        , dtNow.Hour, dtNow.Minute, dtNow.Second);
                }
                var phieuNhapMoi = new PhieuNhap()
                {
                    Created = DateTime.Now,
                    CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId),
                    NhaThuoc = nhathuoc,
                    DaTra = phieuNhap.DaTra,
                    DienGiai = phieuNhap.DienGiai,
                    LoaiXuatNhap = unitOfWork.LoaiXuatNhapRepository.GetById(phieuNhap.MaLoaiXuatNhap),
                    NgayNhap = createDate,
                    SoPhieuNhap = _generateAvaliableSoPhieu(),
                    VAT = phieuNhap.VAT,
                    RecordStatusID = (byte)RecordStatus.Activated,
                    PreNoteDate = createDate
                };

                if (phieuNhap.MaKhachHang > 0)
                {
                    phieuNhapMoi.KhachHang = unitOfWork.KhachHangRepository.GetById(phieuNhap.MaKhachHang);
                }
                if (phieuNhap.MaNhaCungCap > 0)
                {
                    phieuNhapMoi.NhaCungCap = unitOfWork.NhaCungCapRespository.GetById(phieuNhap.MaNhaCungCap);
                }
                unitOfWork.PhieuNhapRepository.Insert(phieuNhapMoi);
              
                // details
                if (phieuNhap.PhieuNhapChiTiets != null)
                    foreach (var detail in phieuNhap.PhieuNhapChiTiets)
                    {
                        if (detail.SoLuong > 0 && detail.GiaNhap >= 0)
                        { //  chi them moi item  khi so luong va gia nhap phu hop
                            var item = new PhieuNhapChiTiet()
                            {
                                DonViTinh = unitOfWork.DonViTinhRepository.GetById(detail.MaDonViTinh),
                                GiaNhap = detail.GiaNhap,
                                PhieuNhap = phieuNhapMoi,
                                SoLuong = detail.SoLuong,
                                Thuoc = unitOfWork.ThuocRepository.GetById(detail.ThuocId),
                                NhaThuoc = nhathuoc,
                                SoLo = detail.SoLo,
                                HanDung = detail.HanDung,
                                NhaThuoc_MaNhaThuoc = nhathuoc.MaNhaThuoc,
                                DonViTinh_MaDonViTinh = detail.MaDonViTinh,
                                Thuoc_ThuocId = detail.ThuocId
                            };
                            Decimal chietKhau = 0;
                            Decimal.TryParse(detail.ChietKhau, out chietKhau);
                            item.ChietKhau = chietKhau;
                            phieuNhapMoi.TongTien += item.SoLuong * item.GiaNhap;// - (item.SoLuong* item.GiaNhap*item.ChietKhau)
                            if (item.ChietKhau > 0)
                            {
                                phieuNhapMoi.TongTien -= item.SoLuong * item.GiaNhap * item.ChietKhau / 100;
                            }
                            item.GiaBanLe = detail.GiaBan;
                            unitOfWork.PhieuNhapChiTietRepository.Insert(item);
                            noteItems.Add(item);
                        }

                    }
                if (phieuNhap.VAT > 0)
                {
                    phieuNhapMoi.TongTien += phieuNhapMoi.TongTien * phieuNhapMoi.VAT / 100;
                }
                //Cập nhật lại đã trả để luôn bằng tổng tiền nếu người dùng click vào trả hết
                if (phieuNhap.MaLoaiXuatNhap == 1)
                {
                    if (Math.Abs(phieuNhapMoi.DaTra - phieuNhapMoi.TongTien) < 5) phieuNhapMoi.DaTra = phieuNhapMoi.TongTien;
                }
                // khach hang tra lai, auto fill datra
                if (phieuNhap.MaLoaiXuatNhap == 3)
                {                    
                    phieuNhapMoi.DaTra = phieuNhapMoi.TongTien;
                }
                phieuNhapMoi.IsDebt = (double)Math.Abs(phieuNhapMoi.TongTien - phieuNhapMoi.DaTra) > MedConstants.EspAmount;
                if (phieuNhapMoi.PhieuNhapChiTiets != null && phieuNhapMoi.PhieuNhapChiTiets.Any())
                {
                    NoteHelper.ApplyAdditionalInfos(maNhaThuoc, noteItems.ToArray());
                    unitOfWork.Save();
                    
                    BackgroundJobHelper.EnqueueMakeAffectedChangesRelatedReceiptNotes(phieuNhapMoi.MaPhieuNhap);                  
                    return RedirectToAction("InDetails", new { id = phieuNhapMoi.MaPhieuNhap });

                }
                else
                {
                    ModelState.AddModelError("MaNhaCungCap", "Không có mã thuốc nào được nhập");
                }

            }
            if (phieuNhap.PhieuNhapChiTiets != null && phieuNhap.PhieuNhapChiTiets.Any())
                foreach (var detail in phieuNhap.PhieuNhapChiTiets)
                {
                    var thuoc = unitOfWork.ThuocRepository.GetById(detail.ThuocId);
                    detail.HeSo = thuoc.HeSo;
                    detail.MaDonViTinhLe = thuoc.DonViXuatLe.MaDonViTinh;
                    if (thuoc.DonViThuNguyen != null) detail.MaDonViTinhThuNguyen = thuoc.DonViThuNguyen.MaDonViTinh;
                    detail.TenDonViTinh = unitOfWork.DonViTinhRepository.GetById(detail.MaDonViTinh).TenDonViTinh;
                    detail.TenThuoc = thuoc.TenDayDu;
                }
            ViewBag.SoPhieu = _generateAvaliableSoPhieu();
            ViewBag.LoaiPhieu = phieuNhap.MaLoaiXuatNhap.ToString();
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            ViewBag.MaLoaiXuatNhap = new SelectList(db.LoaiXuatNhaps.Where(c => !c.IsHidden.HasValue || !c.IsHidden.Value).ToList(), "MaLoaiXuatNhap", "TenLoaiXuatNhap");
            ViewBag.MaNhaCungCap = new SelectList(_getListNhaCungCap(), "MaNhaCungCap", "TenNhaCungCap");
            ViewBag.MaKhachHang = new SelectList(_getListKhachHang(), "MaKhachHang", "TenKhachHang");

            ViewBag.MaDonViTinh = _getListDonViTinh();

            return View(phieuNhap);
        }
        public void updateDrugMapping_InPrice(DrugMapping objMapping, Thuoc thuocInput, decimal inPrice, decimal outPrice, int idUnitInput, bool getPriceMin, DateTime inputDate)
        {
            Thuoc thuoc_mapping = unitOfWork.ThuocRepository.GetById(objMapping.MasterDrugID);
            ThuocsController thuocControl = new ThuocsController();
            //Nếu thuốc đc nhập vào trùng đơn vị lẻ với thuốc mapping
            if (thuocControl.checkNameDrug(thuoc_mapping.DonViXuatLe.TenDonViTinh, thuocInput.DonViXuatLe.TenDonViTinh))
            {
                //Nếu đơn vị nhập vào là đơn vị nguyên -> đưa về giá theo đơn vị lẻ
                if (thuocInput.DonViThuNguyen != null && thuocInput.DonViThuNguyen.MaDonViTinh == idUnitInput)
                {
                    inPrice = inPrice / thuoc_mapping.HeSo;
                    outPrice = outPrice / thuoc_mapping.HeSo;
                }
                //Nếu nhập đơn vị lẻ thì giữ nguyên giá
            }
            //Nếu thuốc đc nhập vào trùng đơn vị nguyên với thuốc mapping
            else if (thuoc_mapping.DonViThuNguyen != null && thuocInput.DonViThuNguyen != null 
                && thuocControl.checkNameDrug(thuoc_mapping.DonViThuNguyen.TenDonViTinh, thuocInput.DonViThuNguyen.TenDonViTinh))
            {
                //Nếu đơn vị nhập vào là đơn vị lẻ -> đưa về giá theo đơn vị nguyên -> đưa về giá theo đơn vị lẻ của thuốc Mapping
                if (thuocInput.DonViXuatLe.MaDonViTinh == idUnitInput)
                {
                    inPrice = (inPrice * thuocInput.HeSo)/ thuoc_mapping.HeSo;
                    outPrice = (outPrice * thuocInput.HeSo) / thuoc_mapping.HeSo;
                }
                //Nếu trùng đơn vị nguyên thì -> đưa về giá theo đơn vị lẻ của thuốc mapping
                else
                {
                    inPrice = inPrice / thuoc_mapping.HeSo;
                    outPrice = outPrice / thuoc_mapping.HeSo;
                }
            }
            //Nếu đơn vị nguyên của thuốc mapping == đơn vị lẻ của thuốc nhập vào
            else if (thuoc_mapping.DonViThuNguyen != null && thuocControl.checkNameDrug(thuoc_mapping.DonViThuNguyen.TenDonViTinh, thuocInput.DonViXuatLe.TenDonViTinh))
            {
                inPrice = inPrice / thuoc_mapping.HeSo;
                outPrice = outPrice / thuoc_mapping.HeSo;
            }
            else
                return;
            if (getPriceMin)
            {
                //Nếu giá trong mapping mà < giá nhập hiện tại thì lấy giá trong mapping
                if (objMapping.InPrice >0 && objMapping.InPrice <= inPrice)
                {                    
                    inPrice = objMapping.InPrice;
                }
                //Nếu giá trong mapping mà < giá nhập hiện tại thì lấy giá trong mapping
                if (objMapping.OutPrice >0 && objMapping.OutPrice <= outPrice)
                {                    
                    outPrice = objMapping.OutPrice;
                }
            }
            objMapping.InPrice = inPrice;
            objMapping.OutPrice = outPrice;
            objMapping.InLastUpdateDate = inputDate;
            objMapping.OutLastUpdateDate = inputDate;
            unitOfWork.ThuocMappingRepository.Update(objMapping);
        }
        private void UpdatePrice(PhieuNhap phieuNhap)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            // chi cap nhat gia khi la hoa don xuat cuoi cung
            if (!unitOfWork.PhieuXuatRepository.GetMany(
                    e => e.NgayXuat > phieuNhap.NgayNhap && e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.RecordStatusID == (byte)RecordStatus.Activated).Any())
            {
                phieuNhap.PhieuNhapChiTiets.ForEach(e =>
                {
                    var giaNhap = e.DonViTinh.MaDonViTinh == e.Thuoc.DonViXuatLe.MaDonViTinh
                        ? e.GiaNhap
                        : (e.Thuoc.HeSo > 0 ? e.GiaNhap / e.Thuoc.HeSo : e.GiaNhap);
                    if (e.Thuoc.GiaNhap != giaNhap)
                        e.Thuoc.GiaNhap = giaNhap;
                });

            }
        }

        [InputBillAuthorizeAttribute("Admin")]
        public ActionResult In(long id, int loaiPhieu = 0)
        {
            try
            {
                var nhathuoc = this.GetNhaThuoc();
                var maNhaThuoc = nhathuoc.MaNhaThuoc;
                var result =
                    unitOfWork.PhieuNhapRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuNhap == id && e.RecordStatusID == (byte)RecordStatus.Activated)
                        .FirstOrDefault();

                if (result == null)
                {
                    ViewBag.Message = "Phiếu xuất không tồn tại #" + id;
                    return View("Error");
                }

                DateTime fromDate = result.NgayNhap.Value.AddDays(1);
                string khachang = "";
                string diachi = "";
                string reportPath = "~/Reports/RptPhieuNhap.rdlc";
                //Tinh no cu
                var listDebit = new List<PhieuNhap>();
                var listPhieuChi = new List<PhieuThuChi>();
                if (result.LoaiXuatNhap.TenLoaiXuatNhap.Equals(Constants.LoaiPhieuXuatNhap.NhapKho) || result.LoaiXuatNhap.TenLoaiXuatNhap.Equals(Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe))
                {
                    listDebit = unitOfWork.PhieuNhapRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.MaPhieuNhap != id && c.NhaCungCap.MaNhaCungCap == result.NhaCungCap.MaNhaCungCap && c.TongTien != c.DaTra && c.RecordStatusID == (byte)RecordStatus.Activated && c.NgayNhap < fromDate).ToList();
                    listPhieuChi = unitOfWork.PhieuThuChiRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.LoaiPhieu == Constants.LoaiPhieuThuChi.PhieuChi && c.NhaCungCap.MaNhaCungCap == result.NhaCungCap.MaNhaCungCap && c.NgayTao < fromDate).ToList();
                    khachang = result.NhaCungCap.TenNhaCungCap;
                    diachi = result.NhaCungCap.DiaChi;
                }
                else
                {
                    listDebit = unitOfWork.PhieuNhapRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.MaPhieuNhap != id && c.KhachHang.MaKhachHang == result.KhachHang.MaKhachHang && c.TongTien != c.DaTra && c.RecordStatusID == (byte)RecordStatus.Activated && c.NgayNhap < fromDate).ToList();
                    listPhieuChi = unitOfWork.PhieuThuChiRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.LoaiPhieu == Constants.LoaiPhieuThuChi.PhieuChi && c.KhachHang.MaKhachHang == result.KhachHang.MaKhachHang && c.NgayTao < fromDate).ToList();
                    khachang = result.KhachHang.TenKhachHang;
                    diachi = result.KhachHang.DiaChi;
                    reportPath = "~/Reports/RptPhieuKhachTraHang.rdlc";
                }

                decimal nocu = 0;
                foreach (var item in listDebit)
                {
                    nocu += (item.TongTien - item.DaTra);
                }

                //tinh tien phieu chi            
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
                foreach (var item in result.PhieuNhapChiTiets.Where(x=> x.RecordStatusID != 2))
                {
                    DataRow dr = dt.NewRow();
                    tienhang = (item.GiaNhap * item.SoLuong * (100 - item.ChietKhau) / 100);
                    dr["STT"] = i;
                    dr["TenHang"] = item.Thuoc.TenThuoc;
                    dr["DVT"] = item.DonViTinh.TenDonViTinh;
                    dr["SoLuong"] = item.SoLuong.ToString("#,##0");
                    dr["DonGia"] = item.GiaNhap.ToString("#,##0");
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
                new ReportParameter("pNgayXuat",result.NgayNhap.Value.ToString("dd/MM/yyyy")),
                new ReportParameter("pPhieuXuat",result.SoPhieuNhap.ToString()),
                new ReportParameter("pKhachHang",khachang),
                new ReportParameter("pDiaChiKhachHang",diachi),
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
                viewer.LocalReport.ReportPath = Server.MapPath(reportPath);
                viewer.LocalReport.DataSources.Add(new ReportDataSource("DSPhieuXuat", dt));
                viewer.LocalReport.SetParameters(listParam);
                byte[] bytes = viewer.LocalReport.Render("PDF");
                Stream stream = new MemoryStream(bytes);

                return File(stream, "application/pdf");
            }
            catch (Exception ex)
            {
                ViewBag.FullMessage = ex.Message;
                return View("Error");
            }
        }

        // GET: PhieuNhaps/Edit/5
        [InputBillAuthorizeAttribute("Admin")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            PhieuNhap phieuNhap = await unitOfWork.PhieuNhapRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuNhap == id).FirstOrDefaultAsync();
            if (phieuNhap == null)
            {
                return HttpNotFound();
            }
            var model = new PhieuNhapEditModel(phieuNhap);

            ViewBag.SoPhieu = phieuNhap.SoPhieuNhap;
            ViewBag.LoaiPhieu = phieuNhap.LoaiXuatNhap.MaLoaiXuatNhap;
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            ViewBag.MaLoaiXuatNhap = new SelectList(db.LoaiXuatNhaps.Where(c => !c.IsHidden.HasValue || !c.IsHidden.Value).ToList(), "MaLoaiXuatNhap", "TenLoaiXuatNhap");
            ViewBag.NhaCungCaps = new SelectList(_getListNhaCungCap(), "MaNhaCungCap", "TenNhaCungCap", phieuNhap.NhaCungCap != null ? phieuNhap.NhaCungCap.MaNhaCungCap : 0);
            ViewBag.KhachHangs = new SelectList(_getListKhachHang(), "MaKhachHang", "TenKhachHang", phieuNhap.KhachHang != null ? phieuNhap.KhachHang.MaKhachHang : 0);

            ViewBag.MaDonViTinh = _getListDonViTinh();
            return View(model);
        }

        // POST: PhieuNhaps/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [InputBillAuthorizeAttribute("Admin")]
        // [Audit]
        public async Task<ActionResult> Edit(PhieuNhapEditModel phieuNhap)
        {
            ModelState.Remove("NgayNhap");

            foreach (var state in ModelState.Keys.ToList().Where(c => c.Contains("ChietKhau")))
            {
                ModelState.Remove(state);
            }

            if (ModelState.IsValid)
            {
                var noteItems = new List<PhieuNhapChiTiet>();
                var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
                var phieuNhapMoi =
                    await
                        unitOfWork.PhieuNhapRepository.GetMany(
                            e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuNhap == phieuNhap.MaPhieuNhap)
                            .FirstOrDefaultAsync();
                phieuNhapMoi.Modified = DateTime.Now;
                phieuNhapMoi.ModifiedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId);
                phieuNhapMoi.DaTra = phieuNhap.DaTra;
                phieuNhapMoi.DienGiai = phieuNhap.DienGiai;
                phieuNhapMoi.NgayNhap = phieuNhap.NgayNhap;
                phieuNhapMoi.VAT = phieuNhap.VAT;
                phieuNhapMoi.TongTien = 0;
                if (phieuNhap.MaKhachHang > 0)
                {
                    phieuNhapMoi.KhachHang = unitOfWork.KhachHangRepository.GetById(phieuNhap.MaKhachHang);
                }
                if (phieuNhap.MaNhaCungCap > 0)
                {
                    phieuNhapMoi.NhaCungCap = unitOfWork.NhaCungCapRespository.GetById(phieuNhap.MaNhaCungCap);
                }
                unitOfWork.PhieuNhapRepository.Update(phieuNhapMoi);
                var nhathuoc = unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc);
              
                // details
                if (phieuNhap.PhieuNhapChiTiets != null)
                {
                    if (phieuNhapMoi.PhieuNhapChiTiets == null)
                        phieuNhapMoi.PhieuNhapChiTiets = new List<PhieuNhapChiTiet>();                   

                    foreach (var detail in phieuNhap.PhieuNhapChiTiets)
                    {
                        if (detail.SoLuong > 0 && detail.GiaNhap >= 0)
                        {
                            //  chi them moi item  khi so luong va gia nhap phu hop

                            var item = phieuNhapMoi.PhieuNhapChiTiets.FirstOrDefault(e => e.MaPhieuNhapCt == detail.MaPhieuNhapCt && detail.MaPhieuNhapCt != 0);
                            Decimal chietKhau = 0;
                            Decimal.TryParse(detail.ChietKhau, out chietKhau);
                            if (item == null)
                            {
                                item = new PhieuNhapChiTiet()
                                {
                                    DonViTinh = unitOfWork.DonViTinhRepository.GetById(detail.MaDonViTinh),
                                    GiaNhap = detail.GiaNhap,
                                    PhieuNhap = phieuNhapMoi,
                                    SoLuong = detail.SoLuong,
                                    Thuoc = unitOfWork.ThuocRepository.GetById(detail.ThuocId),
                                    NhaThuoc = nhathuoc,
                                    HanDung = detail.HanDung,
                                    SoLo = detail.SoLo,
                                    NhaThuoc_MaNhaThuoc = nhathuoc.MaNhaThuoc,
                                    DonViTinh_MaDonViTinh = detail.MaDonViTinh,
                                    Thuoc_ThuocId = detail.ThuocId
                                };
                                item.ChietKhau = chietKhau;

                                phieuNhapMoi.TongTien += item.SoLuong * item.GiaNhap;
                                unitOfWork.PhieuNhapChiTietRepository.Insert(item);
                                noteItems.Add(item);
                            }
                            else
                            {
                                item.NhaThuoc_MaNhaThuoc = nhathuoc.MaNhaThuoc;
                                item.DonViTinh_MaDonViTinh = detail.MaDonViTinh;
                                item.Thuoc_ThuocId = detail.ThuocId;
                                item.Original = item.Clone();

                                item.ChietKhau = chietKhau;
                                item.DonViTinh = unitOfWork.DonViTinhRepository.GetById(detail.MaDonViTinh);
                                item.GiaNhap = detail.GiaNhap;
                                item.SoLuong = detail.SoLuong;
                                item.HanDung = detail.HanDung;
                                item.SoLo = detail.SoLo;
                                phieuNhapMoi.TongTien += item.SoLuong * item.GiaNhap;
                                //unitOfWork.PhieuNhapChiTietRepository.Update(item);
                                noteItems.Add(item);
                            }
                            item.GiaBanLe = detail.GiaBan;
                            if (item.ChietKhau > 0)
                            {
                                phieuNhapMoi.TongTien -= item.SoLuong * item.GiaNhap * item.ChietKhau / 100;
                            }
                        }

                    }
                    // xoa items
                    phieuNhapMoi.PhieuNhapChiTiets.ForEach(item =>
                    {
                        if (phieuNhap.PhieuNhapChiTiets.All(e => e.MaPhieuNhapCt != item.MaPhieuNhapCt))
                        {
                            if (item.RecordStatusID == (byte)RecordStatus.Activated)
                            {
                                phieuNhapMoi.TongTien -= item.SoLuong * item.GiaNhap;
                                item.RecordStatusID = (byte)RecordStatus.Deleted;
                                noteItems.Add(item);
                            }
                        }
                    });
                }
                if (phieuNhap.VAT > 0)
                {
                    phieuNhapMoi.TongTien += phieuNhapMoi.TongTien * phieuNhapMoi.VAT / 100;
                }
                //Cập nhật lại đã trả để luôn bằng tổng tiền nếu người dùng click vào trả hết
                if (phieuNhap.MaLoaiXuatNhap == 1)
                {
                    if (Math.Abs(phieuNhapMoi.DaTra - phieuNhapMoi.TongTien) < 5) phieuNhapMoi.DaTra = phieuNhapMoi.TongTien;
                }
                // khach hang tra lai, auto fill datra
                if (phieuNhap.MaLoaiXuatNhap == 3)
                {
                    phieuNhapMoi.DaTra = phieuNhapMoi.TongTien;
                }
                phieuNhapMoi.IsDebt = (double)Math.Abs(phieuNhapMoi.TongTien - phieuNhapMoi.DaTra) > MedConstants.EspAmount;
                if (phieuNhapMoi.PhieuNhapChiTiets != null && phieuNhapMoi.PhieuNhapChiTiets.Any())
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
                        //updateItems.ForEach(i => unitOfWork.PhieuNhapChiTietRepository.Update(i));
                        NoteHelper.UpdateNoteItems(updateItems.ToArray());
                    }
                    unitOfWork.Save();
                    var drugIds = phieuNhapMoi.PhieuNhapChiTiets.Select(i => i.Thuoc.ThuocId).Distinct().ToArray();
                    BackgroundJobHelper.EnqueueMakeAffectedChangesRelatedReceiptNotes(phieuNhap.MaPhieuNhap.Value);
                    return RedirectToAction("Index", new { loaiPhieu = phieuNhapMoi.LoaiXuatNhap.MaLoaiXuatNhap, searchSoPhieu = phieuNhapMoi.SoPhieuNhap });
                }
                else
                {
                    ModelState.AddModelError("MaNhaCungCap", "Không có mã thuốc nào được nhập");
                }

            }
            if (phieuNhap.PhieuNhapChiTiets != null && phieuNhap.PhieuNhapChiTiets.Any())
                foreach (var detail in phieuNhap.PhieuNhapChiTiets)
                {
                    var thuoc = unitOfWork.ThuocRepository.GetById(detail.ThuocId);
                    detail.HeSo = thuoc.HeSo;
                    detail.MaDonViTinhLe = thuoc.DonViXuatLe.MaDonViTinh;
                    if (thuoc.DonViThuNguyen != null) detail.MaDonViTinhThuNguyen = thuoc.DonViThuNguyen.MaDonViTinh;
                    detail.TenDonViTinh = unitOfWork.DonViTinhRepository.GetById(detail.MaDonViTinh).TenDonViTinh;
                    detail.TenThuoc = thuoc.TenDayDu;
                }
            //Làm trong xuống cho tổng tiền trên phiếu nhập.
            phieuNhap.TongTien = Math.Round(phieuNhap.TongTien, 0);
            //
            ViewBag.SoPhieu = phieuNhap.SoPhieuNhap;
            ViewBag.LoaiPhieu = phieuNhap.MaLoaiXuatNhap;
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            ViewBag.MaLoaiXuatNhap = new SelectList(db.LoaiXuatNhaps.Where(c => !c.IsHidden.HasValue || !c.IsHidden.Value).ToList(), "MaLoaiXuatNhap", "TenLoaiXuatNhap");
            ViewBag.NhaCungCaps = new SelectList(_getListNhaCungCap(), "MaNhaCungCap", "TenNhaCungCap", phieuNhap.MaNhaCungCap);
            ViewBag.KhachHangs = new SelectList(_getListKhachHang(), "MaKhachHang", "TenKhachHang", phieuNhap.MaKhachHang);

            ViewBag.MaDonViTinh = _getListDonViTinh();

            return View(phieuNhap);
        }

        // GET: PhieuNhaps/Delete/5
        [InputBillAuthorizeAttribute("Admin")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var phieuNhap =
                await
                    unitOfWork.PhieuNhapRepository.GetMany(
                        e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuNhap == id).FirstAsync();
            if (phieuNhap == null)
            {
                return HttpNotFound();
            }
            // convert model to modelView
            var model = new PhieuNhapEditModel(phieuNhap);
            ViewBag.SoPhieu = model.SoPhieuNhap;
            ViewBag.LoaiPhieu = model.MaLoaiXuatNhap;
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;

            ViewBag.MaDonViTinh = _getListDonViTinh();
            return View(model);
        }

        // POST: PhieuNhaps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [InputBillAuthorizeAttribute("Admin")]
        // [Audit]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var service = IoC.Container.Resolve<IReceiptNoteService>();
            var noteTypeId = service.DeleteReceiptNote(MedSessionManager.CurrentDrugStoreCode, id, MedSessionManager.CurrentUserId);
            if (noteTypeId <= 0)
            {
                var errorMessage = string.Format("Gặp lỗi trong khi xóa phiếu nhập có mã: {0}", id);
                ViewBag.Message = errorMessage;
                return View("Error");
            }

            return RedirectToAction("Index", "PhieuNhaps", new { noteTypeId });
        }
        public async Task<ActionResult> Delete4Ever(int id)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var loaiPhieu = 0;
            var phieuNhap =
                await
                    unitOfWork.PhieuNhapRepository.GetMany(
                        e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuNhap == id && e.RecordStatusID == (byte)RecordStatus.Deleted).FirstAsync();

            if (phieuNhap != null)
            {
                loaiPhieu = phieuNhap.LoaiXuatNhap.MaLoaiXuatNhap;
                unitOfWork.PhieuNhapRepository.Delete(phieuNhap);
                unitOfWork.Save();

            }
            return RedirectToAction("Index", "PhieuNhaps", new { KhoiPhuc = 1, loaiPhieu });
        }
        [InputBillAuthorizeAttribute("Admin")]
        // [Audit]
        public async Task<ActionResult> Restore(int id)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var loaiPhieu = 0;
            var phieuNhap =
                await
                    unitOfWork.PhieuNhapRepository.GetMany(
                        e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuNhap == id).FirstAsync();

            if (phieuNhap != null)
            {
                loaiPhieu = phieuNhap.LoaiXuatNhap.MaLoaiXuatNhap;
                phieuNhap.RecordStatusID = (byte)RecordStatus.Activated;
                unitOfWork.PhieuNhapRepository.Update(phieuNhap);
                unitOfWork.Save();

            }
            return RedirectToAction("Index", "PhieuNhaps", new { KhoiPhuc = 1, loaiPhieu });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [InputBillAuthorizeAttribute("Admin")]
        // [Audit]
        public ActionResult Upload(HttpPostedFileBase uploadFile)
        {
            return ProcessUpload(uploadFile, 1);
            //return RedirectToAction("Index",new{loaiPhieu=1});
        }

        [InputBillAuthorizeAttribute("Admin")]
        // [Audit]
        public ActionResult Upload2(HttpPostedFileBase uploadFile)
        {
            return ProcessUpload(uploadFile, 3);
            //return RedirectToAction("Index", new { loaiPhieu = 3 });
        }

        private ActionResult ProcessUpload(HttpPostedFileBase uploadFile, int fileType)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var strValidations = new StringBuilder(string.Empty);
            //try
            {
                if (uploadFile.ContentLength > 0)
                {
                    string filePath = Path.Combine(HttpContext.Server.MapPath("../Uploads"),
                        Path.GetFileName(uploadFile.FileName));

                    uploadFile.SaveAs(filePath);

                    int totalupdated = 0;
                    int totaladded = 0;
                    int totalError = 0;
                    string message = "<b>Thông tin phiếu nhập ở dòng số {0} bị lỗi:</b><br/> {1}";
                    UploadObjectInfo info = new UploadObjectInfo();

                    //A 32-bit provider which enables the use of
                    var phieuNhaps = new List<PhieuNhap>();
                    var soPhieu = 1; 
                    if (db.PhieuNhaps.Where(s=> s.NhaThuoc.MaNhaThuoc == maNhaThuoc && s.LoaiXuatNhap.MaLoaiXuatNhap == fileType).Count() > 0)
                        soPhieu = Convert.ToInt32(unitOfWork.PhieuNhapRepository.GetMany(s => s.NhaThuoc.MaNhaThuoc == maNhaThuoc && s.LoaiXuatNhap.MaLoaiXuatNhap == fileType).Max(x => x.SoPhieuNhap)) + 1;
                    var flag = false;
                    foreach (var worksheet in Workbook.Worksheets(filePath))
                    {
                        for (int i = 1; i < worksheet.Rows.Count(); i++)
                        {
                            bool isSubItem = false;
                            var msg = ValidateDataImport(worksheet.Rows[i], maNhaThuoc, fileType, ref isSubItem);
                            if (!string.IsNullOrEmpty(msg))
                            {
                                if (msg == Constants.Params.msgOk)
                                {
                                    var row = worksheet.Rows[i];
                                    if (row.Cells[0] != null && !string.IsNullOrEmpty(row.Cells[0].Text.Trim()))
                                    {
                                        flag = true;
                                        var phieuNhap = new PhieuNhap
                                        {
                                            Created = DateTime.Now,
                                            VAT = (row.Cells[2] != null && !string.IsNullOrEmpty(row.Cells[2].Text.Trim())) ? Convert.ToInt32(row.Cells[2].Text) : 0,
                                            DaTra = (row.Cells[3] != null && !string.IsNullOrEmpty(row.Cells[3].Text.Trim())) ? Convert.ToDecimal(row.Cells[3].Text) : 0,
                                            DienGiai = row.Cells[4] != null ? row.Cells[4].Text.Trim() : string.Empty,
                                            CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId),
                                            RecordStatusID = (byte)RecordStatus.Activated,
                                            NhaThuoc = unitOfWork.NhaThuocRepository.GetById(maNhaThuoc),
                                            LoaiXuatNhap = unitOfWork.LoaiXuatNhapRepository.GetById(fileType),
                                            SoPhieuNhap = soPhieu,
                                        };

                                        var dt = new DateTime();
                                        sThuoc.Utils.Helpers.ConvertToDateTime(row.Cells[0].Text, ref dt);
                                        phieuNhap.NgayNhap = dt;

                                        if (fileType == 1)
                                        {

                                            //check for nha cung cap
                                            var tenNhaCungCap = row.Cells[1].Text.Trim();
                                            var nhaCungCap = unitOfWork.NhaCungCapRespository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.TenNhaCungCap.ToUpper() == tenNhaCungCap.ToUpper()).FirstOrDefault();
                                            phieuNhap.NhaCungCap = nhaCungCap;
                                        }
                                        else if (fileType == 3)
                                        {
                                            //check for khach hang
                                            var tenKhachHang = row.Cells[1].Text.Trim();
                                            var khachHang = unitOfWork.KhachHangRepository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.TenKhachHang.ToUpper() == tenKhachHang.ToUpper()).FirstOrDefault();
                                            phieuNhap.KhachHang = khachHang;
                                        }

                                        //Add the row to phieu nhap
                                        phieuNhap.PhieuNhapChiTiets = new List<PhieuNhapChiTiet>
                                            {
                                                GenPhieuNhapChiTiet(row)
                                            };
                                        //Add the item to list
                                        phieuNhaps.Add(phieuNhap);

                                        // Tinh tong tien
                                        phieuNhap.TongTien = phieuNhap.PhieuNhapChiTiets.Sum(x => x.SoLuong * x.GiaNhap * (1 - x.ChietKhau / 100) * (1 + phieuNhap.VAT / 100));
                                        //Tang so phieu
                                        soPhieu++;
                                        totaladded++;
                                    }
                                    else
                                    {
                                        if (flag && phieuNhaps.Count > 0)
                                        {
                                            var phieuNhap = phieuNhaps.ElementAt(phieuNhaps.Count - 1);
                                            phieuNhap.PhieuNhapChiTiets.Add(GenPhieuNhapChiTiet(row));
                                            //cal culate the sum
                                            phieuNhap.TongTien = phieuNhap.PhieuNhapChiTiets.Sum(x => x.SoLuong * x.GiaNhap * (1 - x.ChietKhau / 100));
                                            phieuNhap.TongTien = phieuNhap.TongTien * (1 + phieuNhap.VAT / 100);
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

                        foreach (var phieuNhap in phieuNhaps)
                        {
                            unitOfWork.PhieuNhapRepository.Insert(phieuNhap);
                        }

                        unitOfWork.Save();

                        info.Title = fileType == 1 ? "Thông tin upload phiếu nhập từ nhà cung cấp" : "Thông tin upload phiếu nhập khách trả hàng";
                        info.TotalUpdated = totalupdated;
                        info.TotalAdded = totaladded;
                        info.TotalError = totalError;
                        Session["UploadMessageInfo"] = info;

                        return RedirectToAction("index", "Upload");
                    }
                }
            }
            //catch (Exception ex)
            //{
            //    ViewBag.Message = ex.Message;
            //    ViewBag.FullMessage = ex.Message;
            //    return View("Error");
            //}

            return RedirectToAction("Index", new { loaiPhieu = fileType });
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
                        var value = row.Cells[0].Text.Trim();
                        DateTime dt = new DateTime();
                        if (!sThuoc.Utils.Helpers.ConvertToDateTime(value, ref dt))
                        {
                            msg += "    - Ngày nhập phải là định dạng ngày tháng (dd/mm/yyyy hoặc dd-mm-yyyy)<br/>";
                        }
                    }

                    if (fileType == 1)
                    {
                        if (row.Cells[1] == null || string.IsNullOrEmpty(row.Cells[1].Text.Trim()))
                        {
                            msg += "    - Tên nhà cung cấp không được bỏ trống <br/>";
                        }
                        else
                        {
                            var tenNhaCungCap = row.Cells[1].Text.Trim().ToUpperInvariant();
                            var nhaCungCap = unitOfWork.NhaCungCapRespository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.TenNhaCungCap.ToUpper() == tenNhaCungCap.ToUpper()).FirstOrDefault();
                            if (nhaCungCap == null)
                                msg += "    - Tên nhà cung cấp không tồn tại vui lòng kiểm tra lại <br/>";
                        }
                    }
                    else
                    {
                        if (row.Cells[1] == null || string.IsNullOrEmpty(row.Cells[1].Text.Trim()))
                        {
                            msg += "    - Tên khách hàng không được bỏ trống <br/>";
                        }
                        else
                        {
                            var tenKhachHang = row.Cells[1].Text.Trim().ToUpperInvariant();
                            var khachHang = unitOfWork.KhachHangRepository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.TenKhachHang.ToUpper() == tenKhachHang.ToUpper()).FirstOrDefault();
                            if (khachHang == null)
                                msg += "    - Tên khách hàng không tồn tại vui lòng kiểm tra lại <br/>";
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
                            x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.MaThuoc.Equals(maThuoc, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
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
                                x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.MaThuoc.Equals(maThuoc, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (thuoc != null)
                        {
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

        private PhieuNhapChiTiet GenPhieuNhapChiTiet(Excel.Row row)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var phieuNhapChiTiet = new PhieuNhapChiTiet
            {
                SoLuong = Convert.ToDecimal(row.Cells[8].Value),
                GiaNhap = Convert.ToInt32(row.Cells[9].Value),
                ChietKhau = Convert.ToDecimal(row.Cells[10].Value),
               
                
            };
            //Hạn dùng và số lô
            if (row.Cells[11] != null)
            {
                if (row.Cells[11].Text.Trim() != "")
                    phieuNhapChiTiet.SoLo = row.Cells[11].Text.Trim();
            }
            if (row.Cells[12] != null)
            {
                if (row.Cells[12].Text.Trim() != "")
                {
                    var dt = new DateTime();
                    sThuoc.Utils.Helpers.ConvertToDateTime(row.Cells[12].Text.Trim(), ref dt);
                    phieuNhapChiTiet.HanDung = dt;
                }
            }
            
            
           
            phieuNhapChiTiet.NhaThuoc = unitOfWork.NhaThuocRepository.GetMany(s=>s.MaNhaThuoc == maNhaThuoc).FirstOrDefault();
            //search thuoc
            var maThuoc = row.Cells[5].Text.Trim().ToUpperInvariant();
            var thuoc = unitOfWork.ThuocRepository.GetMany(
                    x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.MaThuoc.Equals(maThuoc, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            phieuNhapChiTiet.Thuoc = thuoc;
            //
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

            phieuNhapChiTiet.DonViTinh = donViTinh;

            return phieuNhapChiTiet;
        }

        public JsonResult GetMaSoPhieu(string term, int type)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            object result = null;
            if (!string.IsNullOrEmpty(term))
            {
                if (type == 1 || type == 3)
                {
                    result = unitOfWork.PhieuNhapRepository.GetMany(
                   e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && (e.SoPhieuNhap.ToString().ToLower().Contains(term.ToLower())))
                   .ToList()
                   .Select(e => new
                   {
                       label = e.SoPhieuNhap,
                   });
                }
                else
                {
                    result = unitOfWork.PhieuXuatRepository.GetMany(
                  e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && (e.SoPhieuXuat.ToString().ToLower().Contains(term.ToLower())))
                  .ToList()
                  .Select(e => new
                  {
                      label = e.SoPhieuXuat,
                  });

                }
            }
            
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

        #region Lock
        [InputBillAuthorizeAttribute("Admin")]
        public async Task<ActionResult> Lock(int id)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var loaiPhieu = 0;
            long soPhieu = 0;
            var phieuNhap =
                await
                    unitOfWork.PhieuNhapRepository.GetMany(
                        e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuNhap == id).FirstAsync();

            if (phieuNhap != null)
            {
                //loaiPhieu = phieuNhap.LoaiXuatNhap.MaLoaiXuatNhap;
                //soPhieu = phieuNhap.SoPhieuNhap;
                phieuNhap.Locked = true;
                unitOfWork.PhieuNhapRepository.Update(phieuNhap);
                unitOfWork.Save();

            }
            return RedirectToAction("Edit", "PhieuNhaps", new { id });//, soPhieu = soPhieu 
        }
        [InputBillAuthorizeAttribute("Admin")]
        public async Task<ActionResult> UnLock(int id)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var phieuNhap =
                await
                    unitOfWork.PhieuNhapRepository.GetMany(
                        e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaPhieuNhap == id).FirstAsync();

            if (phieuNhap != null)
            {
                phieuNhap.Locked = false;
                unitOfWork.PhieuNhapRepository.Update(phieuNhap);
                unitOfWork.Save();

            }
            return RedirectToAction("Edit", "PhieuNhaps", new { id });//, soPhieu = soPhieu 
        }
        #endregion
    }

    public class ThuocsList
    {
        public ThuocsList(int thuocId, string tenThuoc)
        {
            ThuocId = thuocId;
            TenThuoc = tenThuoc;
        }
        public int ThuocId { get; set; }
        public string TenThuoc { get; set; }
    }

    public class DonViTinhList
    {
        public DonViTinhList(int maDonViTinh, string tenDonViTinh)
        {
            MaDonViTinh = maDonViTinh;
            TenDonViTinh = tenDonViTinh;
        }
        public int MaDonViTinh { get; set; }
        public string TenDonViTinh { get; set; }
    }

    class PhieuNhapComparer : IEqualityComparer<PhieuNhapChiTiet>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(PhieuNhapChiTiet x, PhieuNhapChiTiet y)
        {

            //Check whether the compared objects reference the same data.
            if (ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.MaPhieuNhapCt == y.MaPhieuNhapCt;
        }

        public int GetHashCode(PhieuNhapChiTiet phieuNhap)
        {
            //Check whether the object is null
            if (ReferenceEquals(phieuNhap, null)) return 0;

            //Get hash code for the Code field.
            int hashPhieuNhapCode = phieuNhap.MaPhieuNhapCt.GetHashCode();

            //Calculate the hash code for the product.
            return hashPhieuNhapCode;
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.

    }

    public class ImportError
    {
        public ImportError()
        {
            ErrKhachHang = new List<int>();
            ErrNhaCungCap = new List<int>();
            ErrThoiGianNhap = new List<int>();
            ErrThuoc = new List<int>();
        }
        public List<int> ErrKhachHang { get; set; }
        public List<int> ErrNhaCungCap { get; set; }
        public List<int> ErrThuoc { get; set; }
        public List<int> ErrThoiGianNhap { get; set; }
    }
}
