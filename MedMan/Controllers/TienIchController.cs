using System.Diagnostics;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using sThuoc.DAL;
using sThuoc.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Med.Web.Extensions;
using PagedList;
using sThuoc.Models;
using System.Data;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.IO;
using MedMan.App_Start;
using sThuoc.Utils;
using sThuoc.Models.ViewModels;
using Microsoft.Reporting.WebForms;
using App.Common.MVC;
using Med.Common.Enums;
using App.Common.DI;
using Med.Service.Common;
using Med.DbContext;
using App.Constants.Enums;

namespace Med.Web.Controllers
{
    public class TienIchController : BaseController
    {
        private SecurityContext db = new SecurityContext();
        private UnitOfWork unitOfWork = new UnitOfWork();
        // GET: TienIch
        public ActionResult Index(int? page)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            //ViewBag.NhaCungCaps =
            //   unitOfWork.NhaCungCapRespository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc)
            //       .OrderBy(e => e.TenNhaCungCap)
            //       .AsEnumerable();
            const int pageSize = 10;
            int pageNumber = (page ?? 1);
            var listHethang = getListHetHang();
            Session["hethang"] = listHethang;
            return View(listHethang.OrderBy(c => c.TenThuoc).ToPagedList(pageNumber, pageSize));
        }
        private List<HetHang> getListHetHang()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var list = unitOfWork.ThuocRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc 
                && c.GioiHan.HasValue && c.RecordStatusID == (byte)RecordStatus.Activated)
                .Select(c => new
                {
                    c.ThuocId,
                    c.MaThuoc,
                    c.TenThuoc,
                    c.GioiHan
                }).ToArray();
            var listHethang = new List<HetHang>();
            foreach (var item in list)
            {
                var tonkho = TonKho(item.ThuocId);

                if (tonkho <= item.GioiHan.Value)
                {
                    var hethang = new HetHang();
                    hethang.ThuocId = item.ThuocId;
                    hethang.MaThuoc = item.MaThuoc;
                    hethang.TenThuoc = item.TenThuoc;
                    hethang.SoLuongCanhBao = item.GioiHan.HasValue ? item.GioiHan.Value.ToString("#,##0") : string.Empty;
                    hethang.TonKho = tonkho.ToString("#,##0");
                    listHethang.Add(hethang);
                }
            }
            return listHethang;
        }
        private List<NhaCungCap> _getListNhaCungCap()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var list =
                unitOfWork.NhaCungCapRespository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && !Constants.Default.ConstantEntities.NhaCungCapKiemKe.Contains(e.TenNhaCungCap))
                    .OrderBy(e => e.TenNhaCungCap).ToList();
            return list;
        }
        private List<NhomThuoc> _getListNhomThuoc()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            return
                unitOfWork.NhomThuocRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc)
                    .OrderBy(e => e.TenNhomThuoc).ToList();
        }
        public ActionResult HetHangIn()
        {
            var nhathuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhathuoc.MaNhaThuoc;

            var list = unitOfWork.ThuocRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.GioiHan.HasValue);

            DataTable dt = new DataTable();
            dt.Columns.Add("STT");
            dt.Columns.Add("TenHang");
            dt.Columns.Add("DVT");
            dt.Columns.Add("SoLuong");
            dt.Columns.Add("SoLuongThuc");

            int i = 1;
            foreach (var item in list)
            {
                var tonkho = TonKho(item.ThuocId);
                if (tonkho <= item.GioiHan.Value)
                {
                    DataRow dr = dt.NewRow();
                    dr["STT"] = i;
                    dr["TenHang"] = item.TenThuoc;
                    dr["DVT"] = item.DonViXuatLe.TenDonViTinh;
                    dr["SoLuong"] = item.GioiHan.HasValue ? item.GioiHan.Value.ToString("#,##0") : string.Empty;
                    dr["SoLuongThuc"] = tonkho.ToString("#,##0");
                    dt.Rows.Add(dr);

                    i++;
                }
            }

            //set param
            List<ReportParameter> listParam = new List<ReportParameter>(){
                new ReportParameter("pNhaThuoc",nhathuoc.TenNhaThuoc),
                new ReportParameter("pDiaChiNhaThuoc",nhathuoc.DiaChi),
                new ReportParameter("pSDTNhaThuoc",nhathuoc.DienThoai),
                new ReportParameter("pNgayXuat",DateTime.Now.ToString("dd/MM/yyyy"))                                                            
            };

            ReportViewer viewer = new ReportViewer();
            viewer.ProcessingMode = ProcessingMode.Local;
            viewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RptPhieuDSHangHet.rdlc");

            viewer.LocalReport.DataSources.Add(new ReportDataSource("DSPhieuXuat", dt));
            viewer.LocalReport.SetParameters(listParam);
            byte[] bytes = viewer.LocalReport.Render("PDF");
            Stream stream = new MemoryStream(bytes);

            return File(stream, "application/pdf");
        }

        public ActionResult DuTruIn()
        {
            if (Session["dutru"] != null)
            {

                var nhathuoc = this.GetNhaThuoc();
                var maNhaThuoc = nhathuoc.MaNhaThuoc;

                var list = Session["dutru"] as List<CanhBaoDuTru>;

                DataTable dt = new DataTable();
                dt.Columns.Add("STT");
                dt.Columns.Add("TenHang");
                dt.Columns.Add("DVT");
                dt.Columns.Add("SoLuong");
                dt.Columns.Add("SoLuongThuc");
                dt.Columns.Add("DuTru");
                dt.Columns.Add("DonGia");
                int i = 1;
                foreach (var item in list)
                {
                    DataRow dr = dt.NewRow();
                    dr["STT"] = i;
                    dr["TenHang"] = item.TenThuoc;
                    dr["DVT"] = item.DonViTinh;
                    dr["SoLuong"] = item.SoLuongCanhBao;
                    dr["SoLuongThuc"] = item.TonKho;
                    dr["DuTru"] = item.DuTru.ToString("#,##0");
                    dr["DonGia"] = item.DonGia.ToString("#,##0");
                    dt.Rows.Add(dr);

                    i++;
                }

                //set param
                List<ReportParameter> listParam = new List<ReportParameter>(){
                new ReportParameter("pNhaThuoc",nhathuoc.TenNhaThuoc),
                new ReportParameter("pDiaChiNhaThuoc",nhathuoc.DiaChi),
                new ReportParameter("pSDTNhaThuoc",nhathuoc.DienThoai),
                new ReportParameter("pNgayXuat",DateTime.Now.ToString("dd/MM/yyyy"))                                                            
            };

                ReportViewer viewer = new ReportViewer();
                viewer.ProcessingMode = ProcessingMode.Local;
                viewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RptPhieuDSHangDuTru.rdlc");

                viewer.LocalReport.DataSources.Add(new ReportDataSource("DSPhieuXuat", dt));
                viewer.LocalReport.SetParameters(listParam);
                byte[] bytes = viewer.LocalReport.Render("PDF");
                Stream stream = new MemoryStream(bytes);

                return File(stream, "application/pdf");
            }

            return RedirectToAction("DuTru");
        }

        public ActionResult HetHanIn()
        {
            if (Session["hethan"] != null)
            {
                var nhathuoc = this.GetNhaThuoc();
                var manhathuoc = nhathuoc.MaNhaThuoc;

                var listCanhBaoHetHan = Session["hethan"] as List<CanhBaoHetHan>;
                DataTable dt = new DataTable();
                dt.Columns.Add("STT");
                dt.Columns.Add("TenHang");
                dt.Columns.Add("DVT");
                dt.Columns.Add("SoLuong");
                dt.Columns.Add("SoLuongThuc");
                dt.Columns.Add("Date");

                int i = 1;
                foreach (var item in listCanhBaoHetHan)
                {
                    DataRow dr = dt.NewRow();
                    dr["STT"] = i;
                    dr["TenHang"] = item.TenThuoc;
                    dr["DVT"] = item.DonVi;
                    dr["SoLuong"] = item.HangItGiaoDich;
                    dr["SoLuongThuc"] = item.SoLuong;
                    dr["Date"] = item.Han;
                    dt.Rows.Add(dr);

                    i++;
                }

                //set param
                List<ReportParameter> listParam = new List<ReportParameter>(){
                new ReportParameter("pNhaThuoc",nhathuoc.TenNhaThuoc),
                new ReportParameter("pDiaChiNhaThuoc",nhathuoc.DiaChi),
                new ReportParameter("pSDTNhaThuoc",nhathuoc.DienThoai),
                new ReportParameter("pNgayXuat",DateTime.Now.ToString("dd/MM/yyyy"))                                                            
            };

                ReportViewer viewer = new ReportViewer();
                viewer.ProcessingMode = ProcessingMode.Local;
                viewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RptPhieuDSHetHan.rdlc");

                viewer.LocalReport.DataSources.Add(new ReportDataSource("DSPhieuXuat", dt));
                viewer.LocalReport.SetParameters(listParam);
                byte[] bytes = viewer.LocalReport.Render("PDF");
                Stream stream = new MemoryStream(bytes);

                return File(stream, "application/pdf");
            }

            return RedirectToAction("CanhBaoHangHetHan");
        }

        public ActionResult MoiBatDau()
        {

            return View();
        }

        public ActionResult Dutru()
        {
            var manhathuoc = this.GetNhaThuoc().MaNhaThuoc;
            ViewBag.MaNhomThuoc = new SelectList(unitOfWork.NhomThuocRepository.Get(c => c.MaNhaThuoc == manhathuoc).ToList(), "MaNhomThuoc", "TenNhomThuoc");
            var listHetHang = new List<HetHang>();
            var list = new List<CanhBaoDuTru>();
            if (Session["hethang"] != null)
            {
                listHetHang = Session["hethang"] as List<HetHang>;
                foreach (var item in listHetHang)
                {
                    var dutru = new CanhBaoDuTru()
                    {
                        ThuocId = item.ThuocId,
                        MaThuoc = item.MaThuoc,
                        TenThuoc = item.TenThuoc,
                        SoLuongCanhBao = item.SoLuongCanhBao,
                        TonKho = item.TonKho,
                    };

                    UpdateDuTruItem(dutru);

                    list.Add(dutru);
                }

                if (list.Count > 0)
                {
                    Session["dutru"] = list;
                    Session.Remove("hethang");
                }
            }
            else if (Session["dutru"] != null)
            {
                list = Session["dutru"] as List<CanhBaoDuTru>;
            }

            return View(list);
        }

        public void AddDuTru(string maThuoc)
        {
            var result = new CanhBaoDuTru();
            var manhathuoc = this.GetNhaThuoc().MaNhaThuoc;
            if (!string.IsNullOrEmpty(maThuoc))
            {
                var item = unitOfWork.ThuocRepository.GetMany(x => x.NhaThuoc.MaNhaThuoc == manhathuoc && x.MaThuoc.ToLower() == maThuoc.ToLower().Trim()).FirstOrDefault();
                result.ThuocId = item.ThuocId;
                result.MaThuoc = item.MaThuoc;
                result.TenThuoc = item.TenThuoc;
                result.DuTru = item.DuTru;
                if (item.DonViThuNguyen != null)
                {
                    if (item.HeSo != 0)
                    {
                        result.DonGia = item.GiaNhap * item.HeSo;
                    }
                    else
                    {
                        result.DonGia = item.GiaNhap;
                    }

                }
                else
                {
                    result.DonGia = item.GiaNhap;
                }

                result.DonViTinh = item.DonViXuatLe.TenDonViTinh;
                result.TonKho = TonKho(item.ThuocId).ToString("#,##0");
                result.SoLuongCanhBao = item.GioiHan.HasValue ? item.GioiHan.Value.ToString("#,##0") : string.Empty;
                result.ThanhTien = result.DuTru * result.DonGia;

                var list = new List<CanhBaoDuTru>();
                if (Session["dutru"] != null)
                {
                    list = Session["dutru"] as List<CanhBaoDuTru>;
                }

                list.Add(result);
                Session["dutru"] = list;
            }
        }

        public void UpdateDuTru(int id, int dutru)
        {
            if (Session["dutru"] != null)
            {
                var list = Session["dutru"] as List<CanhBaoDuTru>;
                var item = list.Find(c => c.ThuocId == id);
                if (item != null)
                {
                    item.DuTru = dutru;
                    var thuoc = unitOfWork.ThuocRepository.Get(c => c.ThuocId == id).FirstOrDefault();
                    if (thuoc != null)
                    {
                        thuoc.DuTru = dutru;
                        unitOfWork.ThuocRepository.Update(thuoc);
                        unitOfWork.Save();
                    }
                }
            }
        }

        public void DeleteDutru(int id)
        {
            if (Session["dutru"] != null)
            {
                var list = Session["dutru"] as List<CanhBaoDuTru>;
                var item = list.Find(c => c.ThuocId == id);
                if (item != null)
                {
                    list.Remove(item);
                    Session["dutru"] = list;
                }
            }
        }

        public void AddHetHan(int id)
        {
            var result = new CanhBaoHetHan();
            var manhathuoc = this.GetNhaThuoc().MaNhaThuoc;
            if (id != null)
            {
                var item = unitOfWork.ThuocRepository.Get(x => x.ThuocId == id).FirstOrDefault();
                result.ThuocId = item.ThuocId;
                result.MaThuoc = item.MaThuoc;
                result.TenThuoc = item.TenThuoc;
                result.SoLuong = TonKho(item.ThuocId).ToString("#,##0");
                result.Han = string.Empty;

                var list = new List<CanhBaoHetHan>();
                if (Session["hethan"] != null)
                {
                    list = Session["hethan"] as List<CanhBaoHetHan>;
                }

                list.Add(result);
                Session["hethan"] = list;
            }
        }
        public void DeleteHetHan(int id)
        {
            if (Session["hethan"] != null)
            {
                var list = Session["hethan"] as List<CanhBaoHetHan>;
                var item = list.Find(c => c.ThuocId == id);
                if (item != null)
                {
                    list.Remove(item);
                    Session["hethan"] = list;
                }
            }
        }
        public void UpdateHetHan(int id, string han)
        {
            if (Session["hethan"] != null)
            {
                var list = Session["hethan"] as List<CanhBaoHetHan>;
                var item = list.Find(c => c.ThuocId == id);
                if (item != null)
                {
                    item.Han = han;
                    DateTime dt = new DateTime();
                    if (sThuoc.Utils.Helpers.ConvertToDateTime(han, ref dt))
                    {
                        var thuoc = unitOfWork.ThuocRepository.Get(c => c.ThuocId == id).FirstOrDefault();
                        if (thuoc != null)
                        {
                            thuoc.HanDung = dt;
                            unitOfWork.ThuocRepository.Update(thuoc);
                            unitOfWork.Save();
                        }
                    }
                }
            }
        }
        public JsonResult GetThuocsWithDuTru(string maThuoc)
        {
            var result = new CanhBaoDuTru();
            var manhathuoc = this.GetNhaThuoc().MaNhaThuoc;
            if (!string.IsNullOrEmpty(maThuoc))
            {
                var item = unitOfWork.ThuocRepository.GetMany(x => x.NhaThuoc.MaNhaThuoc == manhathuoc && x.MaThuoc.ToLower() == maThuoc.ToLower().Trim()).FirstOrDefault();
                var tonkho = TonKho(item.ThuocId).ToString("#,##0");
                result.ThuocId = item.ThuocId;
                result.MaThuoc = item.MaThuoc;
                result.TenThuoc = item.TenThuoc;
                result.DuTru = item.DuTru;
                if (item.DonViThuNguyen != null)
                {
                    if (item.HeSo != 0)
                    {
                        result.DonGia = item.GiaNhap * item.HeSo;
                        result.DonViTinh = item.DonViThuNguyen.TenDonViTinh;
                    }
                    else
                    {
                        result.DonGia = item.GiaNhap;
                        result.DonViTinh = item.DonViXuatLe.TenDonViTinh;
                    }

                }
                else
                {
                    result.DonGia = item.GiaNhap;
                    result.DonViTinh = item.DonViXuatLe.TenDonViTinh;
                }

                result.TonKho = tonkho;
                result.SoLuongCanhBao = item.GioiHan.HasValue ? item.GioiHan.Value.ToString("#,##0") : string.Empty;
            }

            return new JsonResult() { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public JsonResult GetThuocsWithHetHan(string sType,string sNhomThuocId, string sTenThuoc)
        {
            var result = new List<CanhBaoHetHan>();
            var lstThuoc = new List<Thuoc>();
            CanhBaoHetHan cbhh;
            var manhathuoc = this.GetNhaThuoc().MaNhaThuoc;
            switch (sType)
            {
                case "1":
                    {
                         lstThuoc = unitOfWork.ThuocRepository.GetMany(x => x.NhaThuoc.MaNhaThuoc == manhathuoc && x.RecordStatusID == (byte)RecordStatus.Activated).ToList();
                    }
                    break;
                case "2":
                    {
                        lstThuoc = unitOfWork.ThuocRepository.GetMany(x => x.RecordStatusID == (byte)RecordStatus.Activated && x.NhaThuoc.MaNhaThuoc == manhathuoc && x.NhomThuoc.MaNhomThuoc == int.Parse(sNhomThuocId)).ToList();
                    }
                    break;
                case "3":
                    {
                        lstThuoc = unitOfWork.ThuocRepository.GetMany(x => x.RecordStatusID == (byte)RecordStatus.Activated && x.NhaThuoc.MaNhaThuoc == manhathuoc && x.TenThuoc.Contains(sTenThuoc)).ToList();
                    }
                    break;
            }
            Setting setting = unitOfWork.SettingRepository.Get(c => c.MaNhaThuoc == manhathuoc && c.Key == Constants.Settings.SoNgayHetHan).FirstOrDefault();
            if(setting == null)
            {
                setting = new Setting() { Value = "0" };
            }
            foreach (Thuoc item in lstThuoc)
            {
                cbhh = new CanhBaoHetHan();
                cbhh.ThuocId = item.ThuocId;
                cbhh.MaThuoc = item.MaThuoc;
                cbhh.TenThuoc = item.TenThuoc;
                cbhh.SoLuong = TonKho(item.ThuocId).ToString("#,##0");
                cbhh.Han = string.Empty;
                cbhh.HangItGiaoDich = string.Empty;
                if(decimal.Parse(cbhh.SoLuong) < decimal.Parse(setting.Value))
                    result.Add(cbhh);
            }

            return new JsonResult() { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult CanhBaoHangHetHan(int? page)
        {
            var manhathuoc = this.GetNhaThuoc().MaNhaThuoc;
            var list = new List<Med.Entity.Thuoc>();
            var tenDonViTinhs = new Dictionary<int, string>();
            var dataFilterService = IoC.Container.Resolve<IDataFilterService>();
            list = dataFilterService.GetValidDrugs(manhathuoc).Where(i => i.HanDung.HasValue).ToList();
            tenDonViTinhs = dataFilterService.GetValidUnits(manhathuoc)
                .Select(i => new { i.MaDonViTinh, i.TenDonViTinh }).ToDictionary(i => i.MaDonViTinh, i => i.TenDonViTinh);

            var listCanhBaoHetHan = new List<CanhBaoHetHan>();
            var songaysaphethan = unitOfWork.SettingRepository.Get(c => c.MaNhaThuoc == manhathuoc && c.Key == Constants.Settings.SoNgayHetHan).FirstOrDefault().Value;
            var handung = DateTime.Now.AddDays(int.Parse(songaysaphethan) - 1).Date;
            foreach (var item in list)
            {
                if (item.HanDung.Value.Date <= handung)
                {
                    var thuoc = new CanhBaoHetHan()
                    {
                        ThuocId = item.ThuocId,
                        MaThuoc = item.MaThuoc,
                        TenThuoc = item.TenThuoc,
                        HangItGiaoDich = string.Empty,
                        SoLuong = TonKho(item.ThuocId).ToString("#,##0"),
                        Han = item.HanDung.Value.ToString("dd/MM/yyyy")
                    };
                    if (tenDonViTinhs.ContainsKey(item.DonViXuatLe_MaDonViTinh.Value))
                    {
                        thuoc.DonVi = tenDonViTinhs[item.DonViXuatLe_MaDonViTinh.Value];
                    }

                    listCanhBaoHetHan.Add(thuoc);
                }                
            }
            ViewBag.NhomThuoc = unitOfWork.NhomThuocRepository.Get(c => c.MaNhaThuoc == manhathuoc).ToList();
            if (listCanhBaoHetHan != null)
                Session["hethan"] = listCanhBaoHetHan;

            const int pageSize = 10;
            int pageNumber = (page ?? 1);

            return View(listCanhBaoHetHan.OrderBy(c => c.TenThuoc).ToPagedList(pageNumber, pageSize));
        }       

        public ActionResult ExportToExcel()
        {
            var thuocs = new DataTable("Thuốc");
            thuocs.Columns.Add("Mã Thuốc", typeof(string));
            thuocs.Columns.Add("Tên Thuốc", typeof(string));
            thuocs.Columns.Add("Số lượng cảnh báo", typeof(string));
            thuocs.Columns.Add("Tồn kho", typeof(string));

            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var thuocList =
                unitOfWork.ThuocRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.GioiHan.HasValue)
                    .OrderBy(e => e.TenThuoc)
                    .AsEnumerable()
                    .Select(i => new
                    {
                        i.MaThuoc,
                        i.TenThuoc,
                        GioiHan = i.GioiHan.Value,
                        TonKho = TonKho(i.ThuocId)
                    });
            //Add to rows
            foreach (var item in thuocList)
            {
                DataRow dr = thuocs.NewRow();
                dr["Mã Thuốc"] = item.MaThuoc;
                dr["Tên Thuốc"] = item.TenThuoc;
                dr["Số lượng cảnh báo"] = item.GioiHan.ToString("#,##0");
                dr["Tồn kho"] = item.TonKho.ToString("#,##0");
                thuocs.Rows.Add(dr);
            }

            using (var pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Thuốc");

                //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                ws.Cells["A1"].LoadFromDataTable(thuocs, true);

                //Format the header for column 1-3
                using (ExcelRange rng = ws.Cells["A1:D1"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                    rng.Style.Font.Color.SetColor(Color.White);
                }

                var fileDownloadName = "CanhBaoHetHang-" + DateTime.Now + ".xlsx";

                var fileStream = new MemoryStream();
                pck.SaveAs(fileStream);
                fileStream.Position = 0;

                var fsr = new FileStreamResult(fileStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                fsr.FileDownloadName = fileDownloadName;

                return fsr;
            }
        }

        public ActionResult ExportDuTruToExcel()
        {
            var thuocs = new DataTable("Thuốc");
            thuocs.Columns.Add("Mã Thuốc", typeof(string));
            thuocs.Columns.Add("Tên Thuốc", typeof(string));
            thuocs.Columns.Add("Số lượng cảnh báo", typeof(string));
            thuocs.Columns.Add("Tồn kho", typeof(string));
            thuocs.Columns.Add("Dự trù", typeof(string));
            thuocs.Columns.Add("Đơn giá", typeof(string));
            thuocs.Columns.Add("Thành tiền", typeof(string));

            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var thuocList = Session["dutru"] as List<CanhBaoDuTru>;
            //Add to rows
            foreach (var item in thuocList)
            {
                DataRow dr = thuocs.NewRow();
                dr["Mã Thuốc"] = item.MaThuoc;
                dr["Tên Thuốc"] = item.TenThuoc;
                dr["Số lượng cảnh báo"] = decimal.Parse(item.SoLuongCanhBao).ToString("#,##0");
                dr["Tồn kho"] = decimal.Parse(item.TonKho).ToString("#,##0");
                dr["Dự trù"] = item.DuTru.ToString("#,##0");
                dr["Đơn giá"] = item.DonGia.ToString("#,##0");
                dr["Thành tiền"] = item.ThanhTien.ToString("#,##0");
                thuocs.Rows.Add(dr);
            }

            using (var pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Thuốc");

                //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                ws.Cells["A1"].LoadFromDataTable(thuocs, true);

                //Format the header for column 1-3
                using (ExcelRange rng = ws.Cells["A1:G1"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                    rng.Style.Font.Color.SetColor(Color.White);
                }

                var fileDownloadName = "DuTru-" + DateTime.Now + ".xlsx";

                var fileStream = new MemoryStream();
                pck.SaveAs(fileStream);
                fileStream.Position = 0;

                var fsr = new FileStreamResult(fileStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                fsr.FileDownloadName = fileDownloadName;

                return fsr;
            }
        }

        public ActionResult ExportHetHanToExcel()
        {
            var thuocs = new DataTable("Thuốc");
            thuocs.Columns.Add("Mã Thuốc", typeof(string));
            thuocs.Columns.Add("Tên Thuốc", typeof(string));
            thuocs.Columns.Add("Hàng ít giao dịch", typeof(string));
            thuocs.Columns.Add("Số lượng", typeof(string));
            thuocs.Columns.Add("Hạn", typeof(string));

            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var thuocList = Session["hethan"] as List<CanhBaoHetHan>;
            //Add to rows
            foreach (var item in thuocList)
            {
                DataRow dr = thuocs.NewRow();
                dr["Mã Thuốc"] = item.MaThuoc;
                dr["Tên Thuốc"] = item.TenThuoc;
                dr["Hàng ít giao dịch"] = item.HangItGiaoDich;
                dr["Số lượng"] = decimal.Parse(item.SoLuong).ToString("#,##0");
                dr["Hạn"] = item.Han;
                thuocs.Rows.Add(dr);
            }

            using (var pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Thuốc");

                //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                ws.Cells["A1"].LoadFromDataTable(thuocs, true);

                //Format the header for column 1-3
                using (ExcelRange rng = ws.Cells["A1:E1"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                    rng.Style.Font.Color.SetColor(Color.White);
                }

                var fileDownloadName = "CanhBaoHetHan-" + DateTime.Now + ".xlsx";

                var fileStream = new MemoryStream();
                pck.SaveAs(fileStream);
                fileStream.Position = 0;

                var fsr = new FileStreamResult(fileStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                fsr.FileDownloadName = fileDownloadName;

                return fsr;
            }
        }

        private decimal TonKhoOld(int id)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var manhathuoc = this.GetNhaThuoc().MaNhaThuoc;
            decimal qtyNhap = 0;
            decimal qtyXuat = 0;
            var thuoc = unitOfWork.ThuocRepository.Get(c => c.ThuocId == id).FirstOrDefault();
            if (thuoc != null)
            {
                qtyNhap = thuoc.SoDuDauKy;
            }

            var phieuNhapChiTiets = unitOfWork.PhieuNhapChiTietRepository.Get(c => c.Thuoc.ThuocId == id && c.PhieuNhap.RecordStatusID == (byte)RecordStatus.Activated && c.PhieuNhap.NhaThuoc.MaNhaThuoc == manhathuoc).ToList();
            // tinh tong so luong da nhap
            phieuNhapChiTiets.ForEach(e =>
            {
                if (thuoc.DonViThuNguyen != null && e.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)
                {
                    qtyNhap += e.SoLuong * thuoc.HeSo;
                }
                else
                {
                    qtyNhap += e.SoLuong;
                }
            });

            var phieuXuatChiTiets = unitOfWork.PhieuXuatChiTietRepository.Get(c => c.Thuoc.ThuocId == id && c.PhieuXuat.NhaThuoc.MaNhaThuoc == manhathuoc && c.PhieuXuat.RecordStatusID == (byte)RecordStatus.Activated).ToList();

            phieuXuatChiTiets.ForEach(e =>
            {
                if (thuoc.DonViThuNguyen != null && e.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)
                {
                    qtyXuat += e.SoLuong * thuoc.HeSo;
                }
                else
                {
                    qtyXuat += e.SoLuong;
                }
            });
            stopWatch.Stop();
            Debug.WriteLine("Old ton kho: " + stopWatch.ElapsedMilliseconds);
            return qtyNhap - qtyXuat;
        }

        private decimal TonKho(int id, int? maNhaCungCap = null)
        {
            var manhathuoc = this.GetNhaThuoc().MaNhaThuoc;
            decimal qtyNhap = 0;
            decimal qtyXuat = 0;
            var thuoc = unitOfWork.ThuocRepository.Get(c => c.ThuocId == id).Select(c=> new
            {
                c.DonViThuNguyen,
                c.HeSo,
                c.SoDuDauKy
            }).FirstOrDefault();
            if (thuoc != null)
            {
                qtyNhap = thuoc.SoDuDauKy;
            }

            var phieuNhapChiTiets =unitOfWork.PhieuNhapChiTietRepository.Get(
                    c => c.Thuoc.ThuocId == id 
                    && c.PhieuNhap.RecordStatusID == (byte)RecordStatus.Activated
                    && c.PhieuNhap.NhaThuoc.MaNhaThuoc == manhathuoc
                    && (!maNhaCungCap.HasValue || c.PhieuNhap.KhachHang.MaKhachHang == maNhaCungCap))
                    .Select(c => new
                    {
                        HeSo = (thuoc.DonViThuNguyen != null && c.DonViTinh != null && c.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)?thuoc.HeSo:1,
                        c.SoLuong
                    }).ToList();
            // tinh tong so luong da nhap
            phieuNhapChiTiets.ForEach(e =>
            {
                qtyNhap += e.SoLuong * e.HeSo;
            });

            var phieuXuatChiTiets = unitOfWork.PhieuXuatChiTietRepository.Get(
                c => c.Thuoc.ThuocId == id 
                    && c.PhieuXuat.NhaThuoc.MaNhaThuoc == manhathuoc 
                    && c.PhieuXuat.RecordStatusID == (byte)RecordStatus.Activated).Select(c => new
                    {
                        HeSo = (thuoc.DonViThuNguyen != null && c.DonViTinh != null && c.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)?thuoc.HeSo:1,
                        c.SoLuong
                    }).ToList();

            phieuXuatChiTiets.ForEach(e =>
            {
                qtyXuat += e.SoLuong * e.HeSo;
            });

            return qtyNhap - qtyXuat;
        }       
        private void UpdateDuTruItem(CanhBaoDuTru item)
        {
            var thuoc = unitOfWork.ThuocRepository.Get(c => c.MaThuoc == item.MaThuoc).FirstOrDefault();
            if (thuoc != null)
            {
                item.DuTru = thuoc.DuTru;
                if (thuoc.DonViThuNguyen != null)
                {
                    if (thuoc.HeSo != 0)
                    {
                        item.DonGia = thuoc.GiaNhap * thuoc.HeSo;
                        item.DonViTinh = thuoc.DonViThuNguyen.TenDonViTinh;
                    }
                    else
                    {
                        item.DonGia = thuoc.GiaNhap;
                        item.DonViTinh = thuoc.DonViXuatLe.TenDonViTinh;
                    }
                }
                else
                {
                    item.DonGia = thuoc.GiaNhap;
                    item.DonViTinh = thuoc.DonViXuatLe.TenDonViTinh;
                }

                item.ThanhTien = thuoc.DuTru * thuoc.GiaNhap;
            }
        }

        public ActionResult TuvanOnline()
        {
            //if (Request.Browser.IsMobileDevice)
            //    return View("~/Areas/Production/Views/Recruitment/ListRecruits.Mobile.cshtml");
            //else
            //    return View("~/Areas/Production/Views/Recruitment/ListRecruits.cshtml");
            //ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }
        [Authorize]
        public ActionResult CheckAuthorization()
        { 
            return View();
        }
        //[Authorize]
        [sThuoc.Filter.SimpleAuthorize("Admin")]
        public ActionResult PhatTrienKinhDoanh()
        {
           
            return View();
        }
    }
}