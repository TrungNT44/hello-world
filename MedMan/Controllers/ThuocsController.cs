using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using Med.Service.Drug;
using Med.Web.Data.Session;
using MedMan;
using MedMan.Models.Reports;
using MvcJqGrid;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using sThuoc.DAL;
using Med.Web.Extensions;
using sThuoc.Filter;
using sThuoc.Models;
using sThuoc.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebGrease.Css.Extensions;
using Excel;
using MedMan.App_Start;
using PagedList;
using Microsoft.Reporting.WebForms;
using Zen.Barcode;
using System.Drawing.Imaging;
using App.Common.MVC;
using BarcodeLib;
using App.Common.DI;
using Med.Common.Enums;
using App.Common.Data;
using Med.DbContext;
using App.Common.Extensions;
using Med.Common;
using System.Linq.Expressions;
using Med.Service.Common;
using Med.ServiceModel.Drug;
using Med.Web.Helpers;
using App.Constants.Enums;
using Med.Service.Caching;

namespace Med.Web.Controllers
{
    public class ThuocsController : BaseController
    {
        private SecurityContext db = new SecurityContext();
        private sThuoc.Repositories.UnitOfWork unitOfWork = new sThuoc.Repositories.UnitOfWork();
        //private readonly string this.GetNhaThuoc().MaNhaThuoc = UserService.GetMaNhaThuoc();        
        private List<DangBaoChe> _getListDangBaoChe()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var maNhaThuocCha = this.GetNhaThuoc().MaNhaThuocCha;
            return unitOfWork.DangBaoCheRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha)
                .OrderBy(e => e.TenDangBaoChe).ToList();
        }

        private List<DonViTinh> _getListDonViTinh()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var maNhaThuocCha = this.GetNhaThuoc().MaNhaThuocCha;
            return
                unitOfWork.DonViTinhRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha)
                    .OrderBy(e => e.TenDonViTinh).ToList();
        }

        private List<NhomThuoc> _getListNhomThuoc()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var maNhaThuocCha = this.GetNhaThuoc().MaNhaThuocCha;
            return
                unitOfWork.NhomThuocRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha)
                    .OrderBy(e => e.TenNhomThuoc).ToList();
        }

        private List<Nuoc> _getListNuoc()
        {
            return
                unitOfWork.NuocRepository
                //.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc)
                .GetAll()
                    .OrderBy(e => e.TenNuoc).ToList();
        }

        // GET: Thuocs
        [SimpleAuthorize("Admin")]
        public ActionResult Index(string mathuoc, string tenthuoc, int? nhomthuoc, int? donvitinh, int? page)
        {
            var nhathuoc = this.GetNhaThuoc();
            var manhathuoc = nhathuoc.MaNhaThuoc;
            var manhathuoccha = nhathuoc.MaNhaThuocCha;
            ViewBag.MaNhaThuoc = manhathuoc;
            ViewBag.MaNhaThuocCha = String.IsNullOrEmpty(manhathuoccha) ? manhathuoc : manhathuoccha;
            Expression<Func<NhomThuoc, bool>> expNhomThuoc = null;
            Expression<Func<Thuoc, bool>> expThuoc = null;
            if (String.IsNullOrEmpty(manhathuoccha))
            {
                if (nhomthuoc.HasValue && nhomthuoc == 1)
                {
                    List<String> s = unitOfWork.NhaThuocRepository.GetById(manhathuoc).NhaThuocCons.Select(x => x.MaNhaThuoc).ToList();
                    expNhomThuoc = x => s.Contains(x.MaNhaThuoc) || x.MaNhaThuoc == manhathuoc;
                    expThuoc = x => x.NhaThuoc.MaNhaThuoc == manhathuoc;
                }
                else
                {
                    List<String> s = unitOfWork.NhaThuocRepository.GetById(manhathuoc).NhaThuocCons.Select(x => x.MaNhaThuoc).ToList();
                    expNhomThuoc = x => s.Contains(x.MaNhaThuoc) || x.MaNhaThuoc == manhathuoc;
                    expThuoc = x => x.NhaThuoc.MaNhaThuoc == manhathuoc && x.RecordStatusID == (byte)RecordStatus.Activated;
                }

                
            }
            else
            {
                if (nhomthuoc.HasValue && nhomthuoc == 1)
                {
                    expNhomThuoc = x => x.MaNhaThuoc == manhathuoccha;
                    expThuoc = x => x.NhaThuoc.MaNhaThuoc == manhathuoccha;
                }
                else
                {
                    expNhomThuoc = x => x.MaNhaThuoc == manhathuoccha;
                    expThuoc = x => x.NhaThuoc.MaNhaThuoc == manhathuoccha && x.RecordStatusID == (byte)RecordStatus.Activated;
                }
                
            }

            var dsnhomthuoc = unitOfWork.NhomThuocRepository.Get(expNhomThuoc).OrderBy(e=> e.TenNhomThuoc).ToList();
            dsnhomthuoc.Insert(0, new NhomThuoc() { MaNhomThuoc = 0, TenNhomThuoc = "Hàng Tư Vấn" });
            dsnhomthuoc.Insert(1, new NhomThuoc() { MaNhomThuoc = 1, TenNhomThuoc = "Hàng Đã Xoá" });
            ViewBag.NhomThuoc = dsnhomthuoc;

            ViewBag.DonViTinh = unitOfWork.ThuocRepository.Get(expThuoc).GroupBy(c => c.DonViXuatLe).Select(c => c.Key).ToList();
            var list = unitOfWork.ThuocRepository.GetMany(expThuoc);
            if (!string.IsNullOrEmpty(mathuoc))
            {
                list = list.Where(c => c.MaThuoc.Contains(mathuoc));
                ViewBag.FilterMaThuoc = mathuoc;
            }

            if (!string.IsNullOrEmpty(tenthuoc))
            {
                list = list.Where(c => c.TenThuoc.Contains(tenthuoc));
                ViewBag.FilterTenThuoc = tenthuoc;
            }

            if (nhomthuoc.HasValue)
            {
                if (nhomthuoc == 0)
                {
                    list = list.Where(c => c.HangTuVan);

                }
                else if (nhomthuoc == 1)
                {
                    list = list.Where(c => c.RecordStatusID != (byte)RecordStatus.Activated);
                }
                else
                {
                    list = list.Where(c => c.NhomThuoc.MaNhomThuoc == nhomthuoc);

                }
                ViewBag.FilterNhomThuoc = nhomthuoc;

            }

            if (donvitinh.HasValue)
            {
                list = list.Where(c => c.DonViXuatLe.MaDonViTinh == donvitinh);
                ViewBag.FilterDonViTinh = donvitinh;
            }

            const int pageSize = 10;
            int pageNumber = (page ?? 1);

            return View(list.OrderBy(c => c.TenThuoc).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult InMaVach(int? MaPhieu, int? LoaiPhieu)
        {
            var nhaThuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhaThuoc.MaNhaThuoc;
            string tenNhathuoc = nhaThuoc.TenNhaThuoc;
            var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            var list = db.NhomThuocs.Where(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc || x.NhaThuoc.MaNhaThuoc == maNhaThuocCha).OrderBy(c => c.TenNhomThuoc).ToList();
            ViewBag.MaNhomThuoc = new SelectList(list, "MaNhomThuoc", "TenNhomThuoc");
            ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
            ViewBag.MaNhaThuoc = maNhaThuoc;
            //ViewBag.InThem = new SelectList(new List<string> { Constants.InMaVachOption.Gia, Constants.InMaVachOption.MaVach, Constants.InMaVachOption.Ten });
            ViewBag.InThem = new SelectList(new List<string> { Constants.InMaVachOption.Gia, Constants.InMaVachOption.Ten, Constants.InMaVachOption.TenGia }, Constants.InMaVachOption.TenGia);

            Code128BarcodeDraw bdw = BarcodeDrawFactory.Code128WithChecksum;
            var img = bdw.Draw("123456", 50, 1);
            MemoryStream ms = new MemoryStream();
            img.Save(ms, ImageFormat.Png);
            ViewBag.ImgTest = Convert.ToBase64String(ms.ToArray());

            
            if(MaPhieu != null)
            {
                //Đọc ra thuốc trong phiếu nhập hàng
                if (LoaiPhieu == 1)
                {
                    var dataFilterService = IoC.Container.Resolve<IDataFilterService>();
                    var receitpNoteItems = dataFilterService.GetValidReceiptNoteItems(maNhaThuoc);
                    var items = receitpNoteItems.Where(i => i.NoteId == MaPhieu).Select(i => new
                    {
                        i.UnitName,
                        i.UnitId,
                        i.Price,
                        i.RetailPrice,
                        i.DrugCode,
                        i.Barcode,
                        i.DrugName,
                        i.DrugId,
                        i.Quantity
                    }).ToList().DistinctBy(i => i.DrugId).ToList();
                    var drugService = IoC.Container.Resolve<IDrugManagementService>();
                    var drugIds = items.Select(i => i.DrugId).ToArray();
                    var drugs = drugService.GetCacheDrugs(maNhaThuoc, drugIds).ToDictionary(i => i.DrugId, i => i);
                    var inItems = new List<InMaVachItemModel>();
                    items.ForEach(i =>
                    {
                        var inItem = new InMaVachItemModel()
                        {
                            DonViXuatLe = i.UnitName,
                            Gia = (Decimal)i.Price,
                            MaThuoc = i.DrugCode,
                            MaVach = i.Barcode,
                            TenThuoc = i.DrugName,
                            ThuocId = i.DrugId,
                            SoLuongTem = (int)i.Quantity
                        };
                        if (drugs.ContainsKey(i.DrugId))
                        {
                            var drug = drugs[i.DrugId];
                            var retailUnit = drug.Units.FirstOrDefault(u => u.UnitId == drug.RetailUnitId);
                            if (retailUnit != null)
                            {
                                inItem.DonViXuatLe = retailUnit.UnitName;
                            }
                            inItem.Gia = (decimal)drug.RetailOutPrice;

                        }

                        inItems.Add(inItem);
                    });


                    var inModel = new InMaVachModel();
                    inModel.Items = inItems;
                    inModel.NgayTao = DateTime.Now;
                    return View(inModel);
                }
                //Đọc ra thuốc trong phiếu nhập xuat
                if (LoaiPhieu == 2)
                {
                    var dataFilterService = IoC.Container.Resolve<IDataFilterService>();
                    var deliveryNoteItems = dataFilterService.GetValidDeliveryItems(maNhaThuoc);
                    var items = deliveryNoteItems.Where(i => i.PhieuXuat_MaPhieuXuat == MaPhieu).Select(i => new
                    {
                        i.DonViTinh_MaDonViTinh,
                        i.GiaXuat,
                        i.RetailPrice,
                        i.RetailQuantity,
                        i.SoLuong,
                        i.Thuoc_ThuocId
                    }).ToList().DistinctBy(i => i.Thuoc_ThuocId).ToList();
                    var drugService = IoC.Container.Resolve<IDrugManagementService>();
                    var drugIds = items.Select(i => i.Thuoc_ThuocId.Value).ToArray();
                    var drugs = drugService.GetCacheDrugs(maNhaThuoc, drugIds).ToDictionary(i => i.DrugId, i => i);
                    var inItems = new List<InMaVachItemModel>();
                    items.ForEach(i =>
                    {
                        var inItem = new InMaVachItemModel()
                        {
                            ThuocId = i.Thuoc_ThuocId.Value,
                            SoLuongTem = (int)i.SoLuong
                        };
                        if (drugs.ContainsKey(i.Thuoc_ThuocId.Value))
                        {
                            var drug = drugs[i.Thuoc_ThuocId.Value];
                            var retailUnit = drug.Units.FirstOrDefault(u => u.UnitId == drug.RetailUnitId);
                            if (retailUnit != null)
                            {
                                inItem.DonViXuatLe = retailUnit.UnitName;
                            }
                            inItem.Gia = (decimal)drug.RetailOutPrice;
                            inItem.MaThuoc = drug.DrugCode;
                            inItem.TenThuoc = drug.DrugBarcode;
                            inItem.MaVach = drug.DrugBarcode;

                        }

                        inItems.Add(inItem);
                    });


                    var inModel = new InMaVachModel();
                    inModel.Items = inItems;
                    inModel.NgayTao = DateTime.Now;
                    return View(inModel);
                }
                
                        
            }

            return View();
        }

        //public ActionResult InMaVach(int? MaPhieuXuat)
        //{
        //    var nhaThuoc = this.GetNhaThuoc();
        //    var maNhaThuoc = nhaThuoc.MaNhaThuoc;
        //    var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
        //    var list = db.NhomThuocs.Where(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc || x.NhaThuoc.MaNhaThuoc == maNhaThuocCha).OrderBy(c => c.TenNhomThuoc).ToList();
        //    ViewBag.MaNhomThuoc = new SelectList(list, "MaNhomThuoc", "TenNhomThuoc");
        //    ViewBag.CurrentUserId = WebSecurity.GetCurrentUserId;
        //    ViewBag.MaNhaThuoc = maNhaThuoc;
        //    //ViewBag.InThem = new SelectList(new List<string> { Constants.InMaVachOption.Gia, Constants.InMaVachOption.MaVach, Constants.InMaVachOption.Ten });
        //    ViewBag.InThem = new SelectList(new List<string> { Constants.InMaVachOption.Gia, Constants.InMaVachOption.Ten, Constants.InMaVachOption.TenGia }, Constants.InMaVachOption.TenGia);

        //    Code128BarcodeDraw bdw = BarcodeDrawFactory.Code128WithChecksum;
        //    var img = bdw.Draw("123456", 50, 1);
        //    MemoryStream ms = new MemoryStream();
        //    img.Save(ms, ImageFormat.Png);
        //    ViewBag.ImgTest = Convert.ToBase64String(ms.ToArray());

        //    //Đọc ra thuốc trong phiếu nhập hàng
        //    if (MaPhieuXuat != null)
        //    {
        //        using (var uow = new App.Common.Data.UnitOfWork(new MedDbContext()))
        //        {
        //            var dataFilterService = IoC.Container.Resolve<IDataFilterService>();
        //            var deliveryNoteItems = dataFilterService.GetValidDeliveryItems(maNhaThuoc, null, uow);
        //            var items = deliveryNoteItems.Where(i => i.PhieuXuat_MaPhieuXuat == MaPhieuXuat).Select(i => new
        //            {
        //                i.DonViTinh_MaDonViTinh,
        //                i.GiaXuat,                        
        //                i.RetailPrice,
        //                i.RetailQuantity,
        //                i.SoLuong,
        //                i.Thuoc_ThuocId                      
        //            }).ToList();
        //            var drugService = IoC.Container.Resolve<IDrugManagementService>();
        //            var drugIds = items.Select(i => i.Thuoc_ThuocId.Value).ToArray();
        //            var drugs = drugService.GetCacheDrugs(maNhaThuoc, drugIds).ToDictionary(i => i.DrugId, i => i);
        //            var inItems = new List<InMaVachItemModel>();
        //            items.ForEach(i =>
        //            {
        //                var inItem = new InMaVachItemModel()
        //                {
        //                    //DonViXuatLe = i.UnitName,
        //                    //Gia = (Decimal)i.Price,
        //                    //MaThuoc = i.DrugCode,
        //                    //MaVach = i.Barcode,
        //                    //TenThuoc = i.DrugName,
        //                    ThuocId = i.Thuoc_ThuocId.Value,
        //                    SoLuongTem = (Decimal)i.SoLuong
        //                };
        //                if (drugs.ContainsKey(i.Thuoc_ThuocId.Value))
        //                {
        //                    var drug = drugs[i.Thuoc_ThuocId.Value];
        //                    var retailUnit = drug.Units.FirstOrDefault(u => u.UnitId == drug.RetailUnitId);
        //                    if (retailUnit != null)
        //                    {
        //                        inItem.DonViXuatLe = retailUnit.UnitName;
        //                    }
        //                    inItem.Gia = (decimal)drug.RetailOutPrice;
        //                    inItem.MaThuoc = drug.DrugCode;
        //                    inItem.TenThuoc = drug.DrugBarcode;
        //                    inItem.MaVach = drug.DrugBarcode.Value;

        //                    //var factors = 1.0;
        //                    //if (i.UnitId != drug.RetailUnitId)
        //                    //{
        //                    //    factors = drug.Factors;
        //                    //}
        //                    //inItem.Gia = (decimal)(i.Price / factors);

        //                }

        //                inItems.Add(inItem);
        //            });


        //            var inModel = new InMaVachModel();
        //            inModel.Items = inItems;
        //            inModel.NgayTao = DateTime.Now;
        //            return View(inModel);
        //        }
        //    }

        //    return View();
        //}
        private string CreateBarCode(string sValue)
        {
            string path_folder = Server.MapPath("~") + "\\BarCode\\" + sValue.Trim().ToLower();
            if (!Directory.Exists(path_folder) || !System.IO.File.Exists(path_folder + "\\" + sValue.Trim().ToLower() + ".jpg"))
            {
                if (!Directory.Exists(path_folder))
                    Directory.CreateDirectory(path_folder);
                Barcode barcode = new Barcode()
                {
                    IncludeLabel = true,
                    Alignment = AlignmentPositions.CENTER,
                    Width = 150,
                    Height = 61,
                    RotateFlipType = RotateFlipType.RotateNoneFlipNone,
                    BackColor = Color.White,
                    ForeColor = Color.Black,
                };

                System.Drawing.Image img = barcode.Encode(TYPE.CODE128, sValue);
                img.Save(path_folder + "\\" + sValue.Trim().ToLower() + ".jpg");
            }
            return path_folder + "\\" + sValue.Trim().ToLower() + ".jpg";
        }
        [HttpPost]
        public ActionResult InMaVach(InMaVachModel model)
        {
            switch (Request["action"].ToLower())
            {
                case "xuất excel":
                    var thuocs = new DataTable("Thuốc");
                    thuocs.Columns.Add("STT", typeof(string));
                    thuocs.Columns.Add("Mã Hàng", typeof(string));
                    thuocs.Columns.Add("Tên Hàng", typeof(string));
                    thuocs.Columns.Add("Đơn Vị", typeof(string));
                    thuocs.Columns.Add("Số Lượng Tem", typeof(int));
                    thuocs.Columns.Add("Giá Bán", typeof(int));

                    //Add to rows
                    int i = 1;
                    foreach (var item in model.Items)
                    {
                        DataRow dr = thuocs.NewRow();
                        dr["STT"] = i.ToString();
                        dr["Mã Hàng"] = item.MaThuoc;
                        dr["Tên Hàng"] = item.TenThuoc;
                        dr["Đơn Vị"] = item.DonViXuatLe;
                        dr["Số Lượng Tem"] = item.SoLuongTem;
                        dr["Giá Bán"] = item.Gia;
                        thuocs.Rows.Add(dr);
                        i++;
                    }

                    using (var pck = new ExcelPackage())
                    {
                        //Create the worksheet
                        ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Thuốc");

                        //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                        ws.Cells["A1"].LoadFromDataTable(thuocs, true);

                        //Format the header for column 1-3
                        using (ExcelRange rng = ws.Cells["A1:F1"])
                        {
                            rng.Style.Font.Bold = true;
                            rng.Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                            rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                            rng.Style.Font.Color.SetColor(Color.White);
                        }

                        var fileDownloadName = "Danh sach thuoc-" + DateTime.Now + ".xlsx";

                        var fileStream = new MemoryStream();
                        pck.SaveAs(fileStream);
                        fileStream.Position = 0;

                        var fsr = new FileStreamResult(fileStream,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                        fsr.FileDownloadName = fileDownloadName;

                        return fsr;
                    }
                default:
                    var dt = new DataTable();
                    dt.Columns.Add("Barcode");
                    dt.Columns.Add("Barcode1");
                    //dt.Columns.Add("Barcode2");
                    //dt.Columns.Add("Barcode3");
                    //dt.Columns.Add("Barcode4");
                    dt.Columns.Add("EncodedBarcode");
                    dt.Columns.Add("EncodedBarcode1");
                    //dt.Columns.Add("EncodedBarcode2");
                    //dt.Columns.Add("EncodedBarcode3");
                    //dt.Columns.Add("EncodedBarcode4");
                    dt.Columns.Add("ExtraInfo");
                    dt.Columns.Add("ExtraInfo1");
                    //dt.Columns.Add("ExtraInfo2");
                    //dt.Columns.Add("ExtraInfo3");
                    //dt.Columns.Add("ExtraInfo4");
                    Code128BarcodeDraw bdw = BarcodeDrawFactory.Code128WithChecksum;

                    var nhaThuoc = this.GetNhaThuoc();
                    var maNhaThuoc = nhaThuoc.MaNhaThuoc;
                    string tenNhathuoc = nhaThuoc.TenNhaThuoc;
                    var listBarcode = new List<BarcodePrintInfo>();
                    BarcodePrintInfo bcPrintInfo = null;
                    for (int tmp = 0; tmp < model.Items.Count; tmp++)
                    {
                        var count = model.Items[tmp].SoLuongTem;
                        var bcItem = model.Items[tmp];
                        bcPrintInfo = new BarcodePrintInfo()
                        {
                            Barcode = bcItem.MaVach,
                            EncodedBarcode = @"file:/" + CreateBarCode(bcItem.MaVach),//Code128Content.GetCode128BString(bcItem.MaVach),
                            ExtraInfo = string.Empty,
                            Name = bcItem.TenThuoc
                        };
                        //Cập nhật tên nhà thuốc vào trường barcode
                        bcPrintInfo.Barcode = tenNhathuoc;
                        if (tenNhathuoc.Length > 22)
                            bcPrintInfo.Barcode = bcPrintInfo.Barcode.Substring(0, 22);
                        
                        if (model.ExtraPrintInfo == "In thêm tên")
                        {
                            if (bcItem.TenThuoc.Length > 22)
                                bcPrintInfo.ExtraInfo = bcItem.TenThuoc.Substring(0, 22);
                            else
                                bcPrintInfo.ExtraInfo = bcItem.TenThuoc;

                        }
                        else if (model.ExtraPrintInfo == "In thêm giá")
                        {
                            bcPrintInfo.ExtraInfo = string.Format("{0}/{1}", bcItem.Gia.ToString("0"), bcItem.DonViXuatLe);
                        }
                        else if (model.ExtraPrintInfo == "In thêm tên/giá")
                        {

                            if (bcItem.TenThuoc.Length > 22 - bcPrintInfo.ExtraInfo.Length)
                                bcItem.TenThuoc = bcItem.TenThuoc.Substring(0, 22 - bcPrintInfo.ExtraInfo.Length);                            
                            bcPrintInfo.ExtraInfo = bcItem.TenThuoc + "\n" + string.Format("{0}/{1}", bcItem.Gia.ToString("#,##0"), bcItem.DonViXuatLe);

                        }

                        if (!string.IsNullOrEmpty(bcPrintInfo.Barcode))
                        {
                            if (count > 0)
                            {
                                while (count > 0)
                                {
                                    listBarcode.Add(bcPrintInfo);
                                    count--;
                                }
                            }
                            else
                            {
                                listBarcode.Add(bcPrintInfo);
                            }
                        }
                    }

                    var total = listBarcode.Count;
                    for (int tmp = 0; tmp < listBarcode.Count; tmp++)
                    {
                        DataRow dr = dt.NewRow();

                        if (tmp + 2 <= total)
                        {
                            bcPrintInfo = listBarcode[tmp];
                            dr["Barcode"] = bcPrintInfo.Barcode;
                            dr["EncodedBarcode"] = bcPrintInfo.EncodedBarcode;
                            dr["ExtraInfo"] = bcPrintInfo.ExtraInfo;
                            tmp++;
                            bcPrintInfo = listBarcode[tmp];
                            dr["Barcode1"] = bcPrintInfo.Barcode;
                            dr["EncodedBarcode1"] = bcPrintInfo.EncodedBarcode;
                            dr["ExtraInfo1"] = bcPrintInfo.ExtraInfo;                       
                        }
                        else
                        {
                            if (tmp % 2 == 0)
                            {
                                bcPrintInfo = listBarcode[tmp];
                                dr["Barcode"] = bcPrintInfo.Barcode;
                                dr["EncodedBarcode"] = bcPrintInfo.EncodedBarcode;
                                dr["ExtraInfo"] = bcPrintInfo.ExtraInfo;
                            }

                            tmp++;
                            if (tmp % 5 == 1 && tmp < total)
                            {
                                bcPrintInfo = listBarcode[tmp];
                                dr["Barcode1"] = bcPrintInfo.Barcode;
                                dr["EncodedBarcode1"] = bcPrintInfo.EncodedBarcode;
                                dr["ExtraInfo1"] = bcPrintInfo.ExtraInfo;
                            }

                     
                        }

                        dt.Rows.Add(dr);
                    }

                    ReportViewer viewer = new ReportViewer();
                    viewer.ProcessingMode = ProcessingMode.Local;
                    viewer.LocalReport.EnableExternalImages = true;
                    viewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RptInMaVach_XPrint350B.rdlc");
                    viewer.LocalReport.DataSources.Add(new ReportDataSource("DSPhieuXuat", dt));
                    viewer.LocalReport.Refresh();
                    byte[] bytes = viewer.LocalReport.Render("PDF");
                    Stream stream = new MemoryStream(bytes);

                    return File(stream, "application/pdf");
            }

            //return View();
        }
        public ActionResult InMaVach_Backup(InMaVachModel model)
        {
            switch (Request["action"].ToLower())
            {
                case "xuất excel":
                    var thuocs = new DataTable("Thuốc");
                    thuocs.Columns.Add("STT", typeof(string));
                    thuocs.Columns.Add("Mã Hàng", typeof(string));
                    thuocs.Columns.Add("Tên Hàng", typeof(string));
                    thuocs.Columns.Add("Đơn Vị", typeof(string));
                    thuocs.Columns.Add("Số Lượng Tem", typeof(int));
                    thuocs.Columns.Add("Giá Bán", typeof(int));

                    //Add to rows
                    int i = 1;
                    foreach (var item in model.Items)
                    {
                        DataRow dr = thuocs.NewRow();
                        dr["STT"] = i.ToString();
                        dr["Mã Hàng"] = item.MaThuoc;
                        dr["Tên Hàng"] = item.TenThuoc;
                        dr["Đơn Vị"] = item.DonViXuatLe;
                        dr["Số Lượng Tem"] = item.SoLuongTem;
                        dr["Giá Bán"] = item.Gia;
                        thuocs.Rows.Add(dr);
                        i++;
                    }

                    using (var pck = new ExcelPackage())
                    {
                        //Create the worksheet
                        ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Thuốc");

                        //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                        ws.Cells["A1"].LoadFromDataTable(thuocs, true);

                        //Format the header for column 1-3
                        using (ExcelRange rng = ws.Cells["A1:F1"])
                        {
                            rng.Style.Font.Bold = true;
                            rng.Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                            rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                            rng.Style.Font.Color.SetColor(Color.White);
                        }

                        var fileDownloadName = "Danh sach thuoc-" + DateTime.Now + ".xlsx";

                        var fileStream = new MemoryStream();
                        pck.SaveAs(fileStream);
                        fileStream.Position = 0;

                        var fsr = new FileStreamResult(fileStream,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                        fsr.FileDownloadName = fileDownloadName;

                        return fsr;
                    }
                default:
                    var dt = new DataTable();
                    dt.Columns.Add("Barcode");
                    dt.Columns.Add("Barcode1");
                    dt.Columns.Add("Barcode2");
                    dt.Columns.Add("Barcode3");
                    dt.Columns.Add("Barcode4");
                    dt.Columns.Add("EncodedBarcode");
                    dt.Columns.Add("EncodedBarcode1");
                    dt.Columns.Add("EncodedBarcode2");
                    dt.Columns.Add("EncodedBarcode3");
                    dt.Columns.Add("EncodedBarcode4");
                    dt.Columns.Add("ExtraInfo");
                    dt.Columns.Add("ExtraInfo1");
                    dt.Columns.Add("ExtraInfo2");
                    dt.Columns.Add("ExtraInfo3");
                    dt.Columns.Add("ExtraInfo4");
                    Code128BarcodeDraw bdw = BarcodeDrawFactory.Code128WithChecksum;
                    
                    var listBarcode = new List<BarcodePrintInfo>();
                    BarcodePrintInfo bcPrintInfo = null;
                    for (int tmp = 0; tmp < model.Items.Count; tmp++)
                    {
                        var count = model.Items[tmp].SoLuongTem;
                        var bcItem = model.Items[tmp];
                        bcPrintInfo = new BarcodePrintInfo()
                        {
                            Barcode = bcItem.MaVach,
                            EncodedBarcode = @"file:/" + CreateBarCode(bcItem.MaVach),//Code128Content.GetCode128BString(bcItem.MaVach),
                            ExtraInfo = string.Empty,
                            Name = bcItem.TenThuoc
                        };
                        if (model.ExtraPrintInfo == "In thêm tên")
                        {
                            if (bcItem.TenThuoc.Length > 22)
                                bcPrintInfo.ExtraInfo = bcItem.TenThuoc.Substring(0, 22);
                            else
                                bcPrintInfo.ExtraInfo = bcItem.TenThuoc;

                        }
                        else if (model.ExtraPrintInfo == "In thêm giá")
                        {
                            bcPrintInfo.ExtraInfo = string.Format("{0}/{1}", bcItem.Gia.ToString("0"), bcItem.DonViXuatLe);
                        }
                        else if (model.ExtraPrintInfo == "In thêm tên/giá")
                        {
                            bcPrintInfo.ExtraInfo = "/" + string.Format("{0}/{1}", bcItem.Gia.ToString("0"), bcItem.DonViXuatLe);
                            if (bcItem.TenThuoc.Length > 22 - bcPrintInfo.ExtraInfo.Length)
                                bcPrintInfo.ExtraInfo = bcItem.TenThuoc.Substring(0, 22 - bcPrintInfo.ExtraInfo.Length) + bcPrintInfo.ExtraInfo;
                            else
                                bcPrintInfo.ExtraInfo = bcItem.TenThuoc + bcPrintInfo.ExtraInfo;

                        }

                        if (!string.IsNullOrEmpty(bcPrintInfo.Barcode))
                        {
                            if (count > 0)
                            {
                                while (count > 0)
                                {
                                    listBarcode.Add(bcPrintInfo);
                                    count--;
                                }
                            }
                            else
                            {
                                listBarcode.Add(bcPrintInfo);
                            }
                        }
                    }

                    var total = listBarcode.Count;
                    for (int tmp = 0; tmp < listBarcode.Count; tmp++)
                    {
                        DataRow dr = dt.NewRow();

                        if (tmp + 5 <= total)
                        {
                            bcPrintInfo = listBarcode[tmp];
                            dr["Barcode"] = bcPrintInfo.Barcode;
                            dr["EncodedBarcode"] = bcPrintInfo.EncodedBarcode;
                            dr["ExtraInfo"] = bcPrintInfo.ExtraInfo;
                            tmp++;
                            bcPrintInfo = listBarcode[tmp];
                            dr["Barcode1"] = bcPrintInfo.Barcode;
                            dr["EncodedBarcode1"] = bcPrintInfo.EncodedBarcode;
                            dr["ExtraInfo1"] = bcPrintInfo.ExtraInfo;
                            tmp++;
                            bcPrintInfo = listBarcode[tmp];
                            dr["Barcode2"] = bcPrintInfo.Barcode;
                            dr["EncodedBarcode2"] = bcPrintInfo.EncodedBarcode;
                            dr["ExtraInfo2"] = bcPrintInfo.ExtraInfo;
                            tmp++;
                            bcPrintInfo = listBarcode[tmp];
                            dr["Barcode3"] = bcPrintInfo.Barcode;
                            dr["EncodedBarcode3"] = bcPrintInfo.EncodedBarcode;
                            dr["ExtraInfo3"] = bcPrintInfo.ExtraInfo;
                            tmp++;
                            bcPrintInfo = listBarcode[tmp];
                            dr["Barcode4"] = bcPrintInfo.Barcode;
                            dr["EncodedBarcode4"] = bcPrintInfo.EncodedBarcode;
                            dr["ExtraInfo4"] = bcPrintInfo.ExtraInfo;
                        }
                        else
                        {
                            if (tmp % 5 == 0)
                            {
                                bcPrintInfo = listBarcode[tmp];
                                dr["Barcode"] = bcPrintInfo.Barcode;
                                dr["EncodedBarcode"] = bcPrintInfo.EncodedBarcode;
                                dr["ExtraInfo"] = bcPrintInfo.ExtraInfo;
                            }

                            tmp++;
                            if (tmp % 5 == 1 && tmp < total)
                            {
                                bcPrintInfo = listBarcode[tmp];
                                dr["Barcode1"] = bcPrintInfo.Barcode;
                                dr["EncodedBarcode1"] = bcPrintInfo.EncodedBarcode;
                                dr["ExtraInfo1"] = bcPrintInfo.ExtraInfo;
                            }

                            tmp++;
                            if (tmp % 5 == 2 && tmp < total)
                            {
                                bcPrintInfo = listBarcode[tmp];
                                dr["Barcode2"] = bcPrintInfo.Barcode;
                                dr["EncodedBarcode2"] = bcPrintInfo.EncodedBarcode;
                                dr["ExtraInfo2"] = bcPrintInfo.ExtraInfo;
                            }

                            tmp++;
                            if (tmp % 5 == 3 && tmp < total)
                            {
                                bcPrintInfo = listBarcode[tmp];
                                dr["Barcode3"] = bcPrintInfo.Barcode;
                                dr["EncodedBarcode3"] = bcPrintInfo.EncodedBarcode;
                                dr["ExtraInfo3"] = bcPrintInfo.ExtraInfo;
                            }

                            tmp++;
                            if (tmp % 5 == 4 && tmp < total)
                            {
                                bcPrintInfo = listBarcode[tmp];
                                dr["Barcode4"] = bcPrintInfo.Barcode;
                                dr["EncodedBarcode4"] = bcPrintInfo.EncodedBarcode;
                                dr["ExtraInfo4"] = bcPrintInfo.ExtraInfo;
                            }
                        }

                        dt.Rows.Add(dr);
                    }

                    ReportViewer viewer = new ReportViewer();
                    viewer.ProcessingMode = ProcessingMode.Local;
                    viewer.LocalReport.EnableExternalImages = true;
                    viewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RptInMaVach_TomyA5_bak.rdlc");
                    viewer.LocalReport.DataSources.Add(new ReportDataSource("DSPhieuXuat", dt));
                    viewer.LocalReport.Refresh();
                    byte[] bytes = viewer.LocalReport.Render("PDF");
                    Stream stream = new MemoryStream(bytes);

                    return File(stream, "application/pdf");
            }

            //return View();
        }
        [SimpleAuthorize("Admin")]
        public ActionResult KiemKe()
        {
            ViewBag.MaNhomThuoc = new SelectList(db.NhomThuocs, "MaNhomThuoc", "TenNhomThuoc");
            return View();
        }

        // GET: Thuocs/Details/5
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var NhaThuoc = this.GetNhaThuoc();
            var maNhaThuoc = NhaThuoc.MaNhaThuoc;
            var maNhaThuocCha = NhaThuoc.MaNhaThuocCha;
            Thuoc thuoc =
                unitOfWork.ThuocRepository.GetMany(e => e.ThuocId == id && (e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha))
                    .FirstOrDefault();
            if (thuoc == null)
            {
                return HttpNotFound();
            }
            return View(thuoc);
        }

        // GET: Thuocs/Create
        [SimpleAuthorize("Admin")]
        public ActionResult Create(int? id)
        {
            var thuocModel = new ThuocEditModel();
            var NhaThuoc = this.GetNhaThuoc();
            string maNhaThuoc = NhaThuoc.MaNhaThuoc;
            string maNhaThuocCha = NhaThuoc.MaNhaThuocCha;
            if (id.HasValue)
            {
                var thuocRef = unitOfWork.ThuocRepository.GetById(id.Value);
                string maNhaThuocUsing = string.IsNullOrEmpty(maNhaThuocCha) ? maNhaThuoc : maNhaThuocCha;
                if (thuocRef != null)
                {
                    bool needSave = false;
                    var nhomThuoc = unitOfWork.NhomThuocRepository.GetMany(e => e.TenNhomThuoc.ToLower() == "tất cả" && e.MaNhaThuoc == maNhaThuocUsing).FirstOrDefault();
                    if (nhomThuoc == null)
                    {
                        nhomThuoc = new NhomThuoc()
                        {
                            MaNhaThuoc = maNhaThuocUsing,
                            TenNhomThuoc = "Tất cả"
                        };
                        needSave = true;
                        unitOfWork.NhomThuocRepository.Insert(nhomThuoc);
                    }
                    var donvinguyen = thuocRef.DonViThuNguyen != null ? unitOfWork.DonViTinhRepository.GetMany(e => e.MaNhaThuoc == Constants.MaNhaThuocMapping && e.TenDonViTinh.ToLower() == thuocRef.DonViThuNguyen.TenDonViTinh.ToLower()).FirstOrDefault() : null;
                    var donvile = thuocRef.DonViXuatLe != null ? unitOfWork.DonViTinhRepository.GetMany(e => e.MaNhaThuoc == Constants.MaNhaThuocMapping && e.TenDonViTinh.ToLower() == thuocRef.DonViXuatLe.TenDonViTinh.ToLower()).FirstOrDefault() : null;
                    //Mapping theo tên đơn vị tính không khớp thì không tạo thuốc mới
                    if (thuocRef.DonViXuatLe != null && donvile == null)
                    {
                        donvile = new DonViTinh()
                        {
                            TenDonViTinh = thuocRef.DonViXuatLe.TenDonViTinh,
                            MaNhaThuoc = Constants.MaNhaThuocMapping
                        };
                        needSave = true;
                        unitOfWork.DonViTinhRepository.Insert(donvile);
                    }
                    if (thuocRef.DonViThuNguyen != null && donvinguyen == null)
                    {
                        donvinguyen = new DonViTinh()
                        {
                            TenDonViTinh = thuocRef.DonViThuNguyen.TenDonViTinh,
                            MaNhaThuoc = Constants.MaNhaThuocMapping
                        };
                        needSave = true;
                        unitOfWork.DonViTinhRepository.Insert(donvinguyen);
                    }
                    if (needSave)
                        unitOfWork.Save();
                    thuocModel.MaNhomThuoc = nhomThuoc.MaNhomThuoc;
                    thuocModel.MaDonViXuat = donvile.MaDonViTinh;
                    thuocModel.GiaNhap = thuocRef.GiaNhap;
                    thuocModel.TenThuoc = thuocRef.TenThuoc;
                    thuocModel.GiaBanLe = thuocRef.GiaBanLe;
                    thuocModel.GiaBanBuon = thuocRef.GiaBanBuon;
                    thuocModel.ThongTin = thuocRef.ThongTin;
                    thuocModel.SoDuDauKy = thuocRef.SoDuDauKy;
                    thuocModel.HangTuVan = thuocRef.HangTuVan;
                    thuocModel.GiaDauKy = thuocRef.GiaDauKy;
                    thuocModel.HanDung = thuocRef.HanDung.HasValue ? thuocRef.HanDung.Value.ToString("dd/MM/yyyy") : string.Empty;
                    if (donvinguyen != null)
                        thuocModel.MaDonViThuNguyen = donvinguyen.MaDonViTinh;
                    thuocModel.GioiHan = thuocRef.GioiHan;
                    thuocModel.HeSo = thuocRef.HeSo;
                }
            }
            ViewBag.MaDangBaoChe = new SelectList(_getListDangBaoChe(), "MaDangBaoChe", "TenDangBaoChe");
            var lstDVT = _getListDonViTinh();
            ViewBag.MaDonViThuNguyen = new SelectList(lstDVT, "MaDonViTinh", "TenDonViTinh", thuocModel.MaDonViThuNguyen); ;
            ViewBag.MaDonViXuat = new SelectList(lstDVT, "MaDonViTinh", "TenDonViTinh", thuocModel.MaDonViXuat); ;
            ViewBag.MaNhomThuoc = new SelectList(_getListNhomThuoc(), "MaNhomThuoc", "TenNhomThuoc", thuocModel.MaNhomThuoc);
            ViewBag.MaNuoc = new SelectList(_getListNuoc(), "MaNuoc", "TenNuoc");
            //Tự sinh mã thuốc
            var configMaThuoc = unitOfWork.SettingRepository.Get(c => (c.MaNhaThuoc == maNhaThuoc || c.MaNhaThuoc == maNhaThuocCha) && c.Key.Equals(Constants.Settings.TuDongTaoMaThuoc, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();            
            if (configMaThuoc != null &&
                configMaThuoc.Value.Equals(Constants.Settings.TuDongTaoMaThuoc_Value, StringComparison.OrdinalIgnoreCase))
                ViewBag.MaThuoc = CreateUniqueDrugCode(maNhaThuoc);
            else
                ViewBag.MaThuoc = "";
            //Tự sinh mã vạch thuốc
            var configMaVachThuoc = unitOfWork.SettingRepository.Get(c => (c.MaNhaThuoc == maNhaThuoc || c.MaNhaThuoc == maNhaThuocCha) && c.Key.Equals(Constants.Settings.TuDongTaoMaVachThuoc, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (configMaVachThuoc != null &&
                configMaVachThuoc.Value.Equals(Constants.Settings.TuDongTaoMaVachThuoc_Value, StringComparison.OrdinalIgnoreCase))
                ViewBag.MaVachThuoc = CreateUniqueBarcode();
            else
                ViewBag.MaVachThuoc = "";
            return View(thuocModel);
        }

        private string CreateUniqueDrugCode(string maNhaThuoc)
        {
            string drugCode = "TH1";
            var maNhaThuocCha = unitOfWork.NhaThuocRepository.GetById(maNhaThuoc).MaNhaThuocCha;
            var drug = unitOfWork.ThuocRepository.Get(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc || c.NhaThuoc.MaNhaThuoc == maNhaThuocCha).
                OrderByDescending(d => d.ThuocId).FirstOrDefault();
            var count = unitOfWork.ThuocRepository.Get(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc || c.NhaThuoc.MaNhaThuoc == maNhaThuocCha).Count();
            if (drug != null)
            {
                int drugNum = 0;
                int.TryParse(Regex.Match(drug.MaThuoc, @"\d+$").Value, out drugNum);
                drugNum += 1;
                if (drugNum < count - 2)
                {
                    drugNum = count - 2;
                }
                drugCode = "TH" + drugNum.ToString("D");
                while (IsDrugExisted(drugCode, maNhaThuoc))
                {
                    drugNum += 1;
                    drugCode = "TH" + drugNum.ToString("D");
                }
            }

            return drugCode;
        }

        private string CreateUniqueBarcode()
        {
            string barcode = sThuoc.Utils.Helpers.GenerateUniqueBarcodeV2();
            while (db.Thuocs.Any(e => e.BarCode == barcode))
            {
                barcode = sThuoc.Utils.Helpers.GenerateUniqueBarcodeV2();
            }

            return barcode;
        }
        private bool IsDrugExisted(string maThuoc, string maNhaThuoc)
        {
            string maNhaThuocCha = unitOfWork.NhaThuocRepository.GetById(maNhaThuoc).MaNhaThuocCha;
            bool isExisted = false;
            isExisted = unitOfWork.ThuocRepository.Get(c => (c.NhaThuoc.MaNhaThuoc == maNhaThuoc || c.NhaThuoc.MaNhaThuoc == maNhaThuocCha)
                && c.MaThuoc == maThuoc).Any();

            return isExisted;
        }

        // POST: Thuocs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        //[InputBillAuthorizeAttribute("Admin")]
        // [Audit]
        public ActionResult Create([Bind(Include = "MaThuoc,TenThuoc,ThongTin,HeSo,QuyCach,GiaNhap,GiaBanBuon,GiaBanLe,SoDuDauKy,GiaDauKy,MaNhaThuoc,MaNhomThuoc,MaNuoc,MaDangBaoChe,MaDonViXuat,MaDonViThuNguyen,BarCode,HoatDong,HanDung,HangTuVan,GioiHan,ThuocIdRef")] ThuocEditModel thuoc)
        {
            var nhathuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhathuoc.MaNhaThuoc;
            var maNhaThuocCha = nhathuoc.MaNhaThuocCha;
            bool conditionCheck = true;
            var ma = (String.IsNullOrEmpty(maNhaThuocCha) ? maNhaThuoc : maNhaThuocCha);
            //if (ModelState.IsValid)
            {
                try
                {
                   
                    //validate
                    if (thuoc.MaNhomThuoc <= 0)
                    {
                        ModelState.AddModelError("MaNhomThuoc", "Chưa chọn nhóm thuốc");
                        
                    }
                        
                    //if (thuoc.MaDonViThuNguyen <= 0)
                    //    ModelState.AddModelError("MaDonViThuNguyen", "Chưa chọn đơn vị thứ nguyên");
                    if (thuoc.MaDonViXuat <= 0)
                    {
                        ModelState.AddModelError("MaDonViXuat", "Chưa chọn đơn vị bán lẻ");
                        conditionCheck = false;
                    }
                        
                    if (string.IsNullOrEmpty(thuoc.MaThuoc))
                    {
                        ModelState.AddModelError("MaThuoc", "Mã thuốc không được bỏ trống");
                        conditionCheck = false;
                    }
                    else if (
                        unitOfWork.ThuocRepository.GetMany(
                            e => (e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha) && e.MaThuoc == thuoc.MaThuoc).Any())
                    {
                        ModelState.AddModelError("MaThuoc", "Mã thuốc đã tồn tại");
                        conditionCheck = false;
                    }

                    var tenthuoc = unitOfWork.ThuocRepository.Get(c => (c.NhaThuoc.MaNhaThuoc == maNhaThuoc || c.NhaThuoc.MaNhaThuoc == maNhaThuocCha) && c.TenThuoc.Equals(thuoc.TenThuoc.Trim(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (tenthuoc != null)
                    {
                        ModelState.AddModelError("TenThuoc", "Tên thuốc đã tồn tại");
                        conditionCheck = false;
                    }

                    if (!string.IsNullOrEmpty(thuoc.BarCode))
                    {
                        var barcode = unitOfWork.ThuocRepository.Get(c => (c.NhaThuoc.MaNhaThuoc == maNhaThuoc || c.NhaThuoc.MaNhaThuoc == maNhaThuocCha) && c.BarCode.Equals(thuoc.BarCode.Trim(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (barcode != null)
                        {
                            ModelState.AddModelError("BarCode", "BarCode đã tồn tại");
                            conditionCheck = false;
                        }
                    }

                    if (conditionCheck)
                    {
                        var newThuoc = new Thuoc()
                        {
                            Created = DateTime.Now,
                            CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId),
                            DonViThuNguyen = thuoc.MaDonViThuNguyen != null ? unitOfWork.DonViTinhRepository.GetById(thuoc.MaDonViThuNguyen) : null,
                            DonViXuatLe = unitOfWork.DonViTinhRepository.GetById(thuoc.MaDonViXuat),
                            GiaBanBuon = thuoc.GiaBanBuon,
                            GiaBanLe = thuoc.GiaBanLe,
                            GiaNhap = thuoc.GiaNhap,
                            GiaDauKy = thuoc.GiaDauKy,
                            SoDuDauKy = thuoc.SoDuDauKy,
                            MaThuoc = thuoc.MaThuoc.ToUpper(),
                            NhomThuoc = unitOfWork.NhomThuocRepository.GetById(thuoc.MaNhomThuoc),
                            NhaThuoc = unitOfWork.NhaThuocRepository.GetById(ma),
                            NhaThuoc_MaNhaThuocCreate = maNhaThuoc,
                            HeSo = thuoc.HeSo,
                            TenThuoc = thuoc.TenThuoc,
                            ThongTin = thuoc.ThongTin,
                            GioiHan = thuoc.GioiHan,
                            BarCode = thuoc.BarCode,
                            RecordStatusID = (byte)RecordStatus.Activated,
                            HangTuVan = thuoc.HangTuVan,
                        };
                        
                        if (thuoc.MaDangBaoChe > 0)
                            newThuoc.DangBaoChe = unitOfWork.DangBaoCheRepository.GetById(thuoc.MaDangBaoChe);
                        if (thuoc.MaNuoc > 0)
                            newThuoc.Nuoc = unitOfWork.NuocRepository.GetById(thuoc.MaNuoc);

                        if (!string.IsNullOrEmpty(thuoc.HanDung))
                        {
                            newThuoc.HanDung = DateTime.Parse(thuoc.HanDung);
                        }
                        newThuoc.PreFactors = newThuoc.HeSo;
                        newThuoc.PreInitPrice = newThuoc.GiaDauKy;
                        newThuoc.PreInitQuantity = newThuoc.SoDuDauKy;
                        newThuoc.PreRetailUnitID = newThuoc.DonViXuatLe != null ? newThuoc.DonViXuatLe.MaDonViTinh : (int?)null;
                        newThuoc.PreUnitID = newThuoc.DonViThuNguyen != null ? newThuoc.DonViThuNguyen.MaDonViTinh : (int?)null;
                        newThuoc.PreExpiredDate = newThuoc.HanDung;

                        unitOfWork.ThuocRepository.Insert(newThuoc);
                        unitOfWork.Save();
                        //mapping danh mục
                        //Khi tạo mới
                        if (thuoc.ThuocIdRef.HasValue)
                        {
                            Thuoc thuoc_mapping = unitOfWork.ThuocRepository.GetById(thuoc.ThuocIdRef.Value);
                            if(thuoc_mapping != null)
                            {
                                //Thuốc được mapping phải thỏa mãn trùng 1 trong 2 
                                if(thuoc_mapping.DonViXuatLe.TenDonViTinh.ToLower() == newThuoc.DonViXuatLe.TenDonViTinh.ToLower()
                                    || (thuoc_mapping.DonViThuNguyen != null && newThuoc.DonViThuNguyen != null && thuoc_mapping.DonViThuNguyen.TenDonViTinh.ToLower() == newThuoc.DonViThuNguyen.TenDonViTinh.ToLower()))
                                {
                                    DrugMapping drugMapping = new DrugMapping();
                                    drugMapping.MasterDrugID = thuoc.ThuocIdRef.Value;
                                    drugMapping.SlaveDrugID = newThuoc.ThuocId;
                                    drugMapping.DrugStoreID = ma;
                                    drugMapping.InPrice = 0;
                                    drugMapping.InLastUpdateDate = DateTime.Now.AddDays(-1);
                                    drugMapping.OutPrice = 0;
                                    drugMapping.OutLastUpdateDate = DateTime.Now.AddDays(-1);
                                    //Lấy đơn vị tính (xuất lẻ) của nhà thuốc
                                    drugMapping.UnitID = thuoc_mapping.DonViXuatLe.MaDonViTinh;
                                    unitOfWork.ThuocMappingRepository.Insert(drugMapping);
                                    unitOfWork.Save();
                                }
                            }
                        }
                        BackgroundJobHelper.EnqueueMakeAffectedChangesByUpdatedDrugs(newThuoc.ThuocId);
                     
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("TenThuoc", e.Message);

                }
            }

            ViewBag.MaDangBaoChe = new SelectList(_getListDangBaoChe(), "MaDangBaoChe", "TenDangBaoChe",
                thuoc.MaDangBaoChe);
            var donViTinh = _getListDonViTinh();
            ViewBag.MaDonViThuNguyen = new SelectList(donViTinh, "MaDonViTinh", "TenDonViTinh", thuoc.MaDonViThuNguyen);
            ViewBag.MaDonViXuat = new SelectList(donViTinh, "MaDonViTinh", "TenDonViTinh", thuoc.MaDonViXuat);

            ViewBag.MaNhomThuoc = new SelectList(_getListNhomThuoc(), "MaNhomThuoc", "TenNhomThuoc", thuoc.MaNhomThuoc);
            //Tự sinh mã thuốc
            var configMaThuoc = unitOfWork.SettingRepository.Get(c => (c.MaNhaThuoc == maNhaThuoc || c.MaNhaThuoc == maNhaThuocCha) && c.Key.Equals(Constants.Settings.TuDongTaoMaThuoc, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (configMaThuoc != null &&
                configMaThuoc.Value.Equals(Constants.Settings.TuDongTaoMaThuoc_Value, StringComparison.OrdinalIgnoreCase))
                ViewBag.MaThuoc = CreateUniqueDrugCode(maNhaThuoc);
            else
                ViewBag.MaThuoc = "";
            //Tự sinh mã vạch thuốc
            var configMaVachThuoc = unitOfWork.SettingRepository.Get(c => (c.MaNhaThuoc == maNhaThuoc || c.MaNhaThuoc == maNhaThuocCha) && c.Key.Equals(Constants.Settings.TuDongTaoMaVachThuoc, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (configMaVachThuoc != null &&
                configMaVachThuoc.Value.Equals(Constants.Settings.TuDongTaoMaVachThuoc_Value, StringComparison.OrdinalIgnoreCase))
                ViewBag.MaVachThuoc = CreateUniqueBarcode();
            else
                ViewBag.MaVachThuoc = "";

            ViewBag.MaNuoc = new SelectList(_getListNuoc(), "MaNuoc", "TenNuoc");
            return View(thuoc);
        }

        // GET: Thuocs/Edit/5
        [SimpleAuthorize("Admin")]
        //[InputBillAuthorizeAttribute("Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhathuoc = this.GetNhaThuoc().MaNhaThuoc;
            Thuoc thuoc =
                    unitOfWork.ThuocRepository.GetById(id);
            if (thuoc == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaDangBaoChe = new SelectList(_getListDangBaoChe(), "MaDangBaoChe", "TenDangBaoChe", thuoc.DangBaoChe != null ? thuoc.DangBaoChe.MaDangBaoChe : 0);
            var donViTinh = _getListDonViTinh();
            ViewBag.MaDonViThuNguyen = new SelectList(donViTinh, "MaDonViTinh", "TenDonViTinh", thuoc.DonViThuNguyen != null ? thuoc.DonViThuNguyen.MaDonViTinh : -1);
            ViewBag.MaDonViXuat = new SelectList(donViTinh, "MaDonViTinh", "TenDonViTinh", thuoc.DonViXuatLe != null ? thuoc.DonViXuatLe.MaDonViTinh : 0);
            ViewBag.MaNhomThuoc = new SelectList(_getListNhomThuoc(), "MaNhomThuoc", "TenNhomThuoc", thuoc.NhomThuoc.MaNhomThuoc);
            ViewBag.MaNuoc = new SelectList(_getListNuoc(), "MaNuoc", "TenNuoc", thuoc.Nuoc != null ? thuoc.Nuoc.MaNuoc : 0);
            var maNhaThuocCha = this.GetNhaThuoc().MaNhaThuocCha;
            var ma = (string.IsNullOrEmpty(maNhaThuocCha) ? maNhathuoc : maNhaThuocCha);
            var drugMappingExit = unitOfWork.ThuocMappingRepository.GetMany(x => x.SlaveDrugID == id && x.DrugStoreID == ma).FirstOrDefault();
            var model = new ThuocEditModel()
            {
                ThuocId = thuoc.ThuocId,
                ThongTin = thuoc.ThongTin,
                MaNhaThuoc = thuoc.NhaThuoc.MaNhaThuoc,
                MaDangBaoChe = thuoc.DangBaoChe != null ? thuoc.DangBaoChe.MaDangBaoChe : 0,
                MaDonViThuNguyen = thuoc.DonViThuNguyen != null ? thuoc.DonViThuNguyen.MaDonViTinh : 0,
                MaDonViXuat = thuoc.DonViXuatLe != null ? thuoc.DonViXuatLe.MaDonViTinh : 0,
                MaNhomThuoc = thuoc.NhomThuoc.MaNhomThuoc,
                GiaBanBuon = thuoc.GiaBanBuon,
                GiaBanLe = thuoc.GiaBanLe,
                GiaDauKy = thuoc.GiaDauKy,
                GiaNhap = thuoc.GiaNhap,
                GioiHan = thuoc.GioiHan,
                HeSo = thuoc.HeSo,
                TenThuoc = thuoc.TenThuoc,
                MaNuoc = thuoc.Nuoc != null ? thuoc.Nuoc.MaNuoc : 0,
                SoDuDauKy = thuoc.SoDuDauKy,
                MaThuoc = thuoc.MaThuoc,
                BarCode = thuoc.BarCode,
                HanDung = thuoc.HanDung != null ? thuoc.HanDung.Value.ToString("dd/MM/yyyy") : string.Empty,
                HoatDong = thuoc.RecordStatusID == (byte)RecordStatus.Activated,
                HangTuVan = thuoc.HangTuVan,
                ThuocIdRef = drugMappingExit != null ? drugMappingExit.MasterDrugID : -1

            };
            return View("Create", model);
        }

        // POST: Thuocs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        //[InputBillAuthorizeAttribute("Admin")]
        // [Audit]
        public ActionResult Edit(
            [Bind(
                Include =
                    "ThuocId,MaThuoc,TenThuoc,ThongTin,HeSo,QuyCach,GiaNhap,GiaBanBuon,GiaBanLe,SoDuDauKy,GiaDauKy,MaNhaThuoc,MaNhomThuoc,MaNuoc,MaDangBaoChe,MaDonViXuat,MaDonViThuNguyen,BarCode,HoatDong,HangTuVan,HanDung,GioiHan,ThuocIdRef"
                )] ThuocEditModel thuoc)
        {
            var nhaThuoc = this.GetNhaThuoc();
            string maNhaThuoc = nhaThuoc.MaNhaThuoc;
            string maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            var ma = (String.IsNullOrEmpty(maNhaThuocCha) ? maNhaThuoc : maNhaThuocCha);
            var modifyingThuoc = unitOfWork.ThuocRepository.GetById(thuoc.ThuocId);
            if (ModelState.IsValid)
            {
                try
                {
                    //validate 
                    if (thuoc.MaNhomThuoc <= 0)
                        ModelState.AddModelError("MaNhomThuoc", "Chưa chọn nhóm thuốc");
                    //if (thuoc.MaDonViThuNguyen <= 0)
                    //    ModelState.AddModelError("MaDonViThuNguyen", "Chưa chọn đơn vị thứ nguyên");
                    if (thuoc.MaDonViXuat <= 0)
                        ModelState.AddModelError("MaDonViXuat", "Chưa chọn đơn vị xuất lẻ");
                    // validate maThuoc is unique
                    var th =
                        unitOfWork.ThuocRepository.GetMany(
                            e =>
                                (e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha) && e.ThuocId != thuoc.ThuocId &&
                                e.MaThuoc == thuoc.MaThuoc).FirstOrDefault();
                    if (th != null)
                        ModelState.AddModelError("MaThuoc", "Mã thuốc đã tồn tại");

                    var tenthuoc = unitOfWork.ThuocRepository.Get(c => (c.NhaThuoc.MaNhaThuoc == maNhaThuoc || c.NhaThuoc.MaNhaThuoc == maNhaThuocCha) && c.ThuocId != thuoc.ThuocId && c.TenThuoc.Equals(thuoc.TenThuoc.Trim(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (tenthuoc != null)
                    {
                        ModelState.AddModelError("TenThuoc", "Tên thuốc đã tồn tại");
                    }

                    var barcode = unitOfWork.ThuocRepository.Get(c => (c.NhaThuoc.MaNhaThuoc == maNhaThuoc || c.NhaThuoc.MaNhaThuoc == maNhaThuocCha) && c.ThuocId != thuoc.ThuocId && c.BarCode.Equals(thuoc.BarCode.Trim(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (barcode != null)
                    {
                        //ModelState.AddModelError("BarCode", "BarCode đã tồn tại");
                        if (!string.IsNullOrEmpty(barcode.BarCode))
                        {
                            ModelState.AddModelError("BarCode", "BarCode đã tồn tại");
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        modifyingThuoc = unitOfWork.ThuocRepository.GetById(thuoc.ThuocId);
                        modifyingThuoc.Modified = DateTime.Now;
                        modifyingThuoc.ModifiedBy =
                            unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId);
                        modifyingThuoc.GiaNhap = thuoc.GiaNhap;
                        modifyingThuoc.GiaBanBuon = thuoc.GiaBanBuon;
                        modifyingThuoc.GiaBanLe = thuoc.GiaBanLe;
                        modifyingThuoc.GiaDauKy = thuoc.GiaDauKy;
                        modifyingThuoc.SoDuDauKy = thuoc.SoDuDauKy;
                        modifyingThuoc.HeSo = thuoc.HeSo;
                        modifyingThuoc.GioiHan = thuoc.GioiHan;
                        modifyingThuoc.MaThuoc = thuoc.MaThuoc.ToUpper();
                        modifyingThuoc.BarCode = thuoc.BarCode;
                        modifyingThuoc.RecordStatusID = thuoc.HoatDong ? (byte)RecordStatus.Activated : (byte)RecordStatus.Deleted;
                        modifyingThuoc.HangTuVan = thuoc.HangTuVan;
                        modifyingThuoc.NhomThuoc = unitOfWork.NhomThuocRepository.GetById(thuoc.MaNhomThuoc);
                        //modifyingThuoc.ThuocRef = thuoc.ThuocIdRef != null && thuoc.ThuocIdRef.Value != -1 ? unitOfWork.ThuocRepository.GetById(thuoc.ThuocIdRef) : null;
                        //Update phieu xuat nhap chi tiet khi don vi xuat le thay doi
                        if (modifyingThuoc.DonViXuatLe.MaDonViTinh != thuoc.MaDonViXuat)
                        {
                            var dvx = unitOfWork.DonViTinhRepository.GetById(thuoc.MaDonViXuat);
                            var listPhieuXuatCt = unitOfWork.PhieuXuatChiTietRepository.Get(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.Thuoc.ThuocId == modifyingThuoc.ThuocId && c.DonViTinh.MaDonViTinh == modifyingThuoc.DonViXuatLe.MaDonViTinh).ToList();
                            if (listPhieuXuatCt != null)
                            {
                                foreach (var item in listPhieuXuatCt)
                                {
                                    item.DonViTinh = dvx;
                                }
                            }

                            var listPhieuNhapCt = unitOfWork.PhieuNhapChiTietRepository.Get(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.Thuoc.ThuocId == modifyingThuoc.ThuocId && c.DonViTinh.MaDonViTinh == modifyingThuoc.DonViXuatLe.MaDonViTinh).ToList();
                            if (listPhieuNhapCt != null)
                            {
                                foreach (var item in listPhieuNhapCt)
                                {
                                    item.DonViTinh = dvx;
                                }
                            }

                            modifyingThuoc.DonViXuatLe = dvx;
                        }

                        if (thuoc.MaNuoc > 0)
                            modifyingThuoc.Nuoc = unitOfWork.NuocRepository.GetById(thuoc.MaNuoc);
                        if (!thuoc.MaDonViThuNguyen.HasValue)
                        {
                            // Trong truong hop don vi thu nguyen chuyen sang null thi se update bang don vi xuat le.
                            if (modifyingThuoc.DonViThuNguyen != null)
                            {
                                var dvx = unitOfWork.DonViTinhRepository.GetById(thuoc.MaDonViXuat);
                                var listPhieuXuatCt = unitOfWork.PhieuXuatChiTietRepository.Get(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.Thuoc.ThuocId == modifyingThuoc.ThuocId && c.DonViTinh.MaDonViTinh == modifyingThuoc.DonViThuNguyen.MaDonViTinh).ToList();
                                if (listPhieuXuatCt != null)
                                {
                                    foreach (var item in listPhieuXuatCt)
                                    {
                                        item.DonViTinh = dvx;
                                    }
                                }

                                var listPhieuNhapCt = unitOfWork.PhieuNhapChiTietRepository.Get(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.Thuoc.ThuocId == modifyingThuoc.ThuocId && c.DonViTinh.MaDonViTinh == modifyingThuoc.DonViThuNguyen.MaDonViTinh).ToList();
                                if (listPhieuNhapCt != null)
                                {
                                    foreach (var item in listPhieuNhapCt)
                                    {
                                        item.DonViTinh = dvx;
                                    }
                                }
                            }

                            unitOfWork.Context.Entry(modifyingThuoc).Reference(f => f.DonViThuNguyen).CurrentValue = null;
                            modifyingThuoc.HeSo = 0;
                        }
                        //Update phieu xuat nhap chi tiet khi don vi xuat nguyen thay doi
                        else
                        {
                            var dvxn = unitOfWork.DonViTinhRepository.GetById(thuoc.MaDonViThuNguyen);

                            if (modifyingThuoc.DonViThuNguyen != null && modifyingThuoc.DonViThuNguyen.MaDonViTinh != thuoc.MaDonViThuNguyen)
                            {
                                var listPhieuXuatCt = unitOfWork.PhieuXuatChiTietRepository.Get(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.Thuoc.ThuocId == modifyingThuoc.ThuocId && c.DonViTinh.MaDonViTinh == modifyingThuoc.DonViThuNguyen.MaDonViTinh).ToList();
                                if (listPhieuXuatCt != null)
                                {
                                    foreach (var item in listPhieuXuatCt)
                                    {
                                        item.DonViTinh = dvxn;
                                    }
                                }

                                var listPhieuNhapCt = unitOfWork.PhieuNhapChiTietRepository.Get(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.Thuoc.ThuocId == modifyingThuoc.ThuocId && c.DonViTinh.MaDonViTinh == modifyingThuoc.DonViThuNguyen.MaDonViTinh).ToList();
                                if (listPhieuNhapCt != null)
                                {
                                    foreach (var item in listPhieuNhapCt)
                                    {
                                        item.DonViTinh = dvxn;
                                    }
                                }
                            }

                            modifyingThuoc.DonViThuNguyen = dvxn;
                        }

                        if (!string.IsNullOrEmpty(thuoc.HanDung))
                        {
                            modifyingThuoc.HanDung = DateTime.Parse(thuoc.HanDung);
                        }
                        else
                        {
                            modifyingThuoc.HanDung = null;
                        }

                        if (thuoc.MaDangBaoChe > 0)
                            modifyingThuoc.DangBaoChe = unitOfWork.DangBaoCheRepository.GetById(thuoc.MaDangBaoChe);
                        modifyingThuoc.TenThuoc = thuoc.TenThuoc;
                        modifyingThuoc.ThongTin = thuoc.ThongTin;
                        unitOfWork.ThuocRepository.Update(modifyingThuoc);
                        unitOfWork.Save();

                        BackgroundJobHelper.EnqueueMakeAffectedChangesByUpdatedDrugs(modifyingThuoc.ThuocId);
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("TenThuoc", e.Message);

                }
            }
            ViewBag.MaDangBaoChe = new SelectList(_getListDangBaoChe(), "MaDangBaoChe", "TenDangBaoChe",
                modifyingThuoc.DangBaoChe != null ? modifyingThuoc.DangBaoChe.MaDangBaoChe : 0);
            var donViTinh = _getListDonViTinh();
            ViewBag.MaDonViThuNguyen = new SelectList(donViTinh, "MaDonViTinh", "TenDonViTinh",
                modifyingThuoc.DonViThuNguyen != null ? modifyingThuoc.DonViThuNguyen.MaDonViTinh : 0);
            ViewBag.MaDonViXuat = new SelectList(donViTinh, "MaDonViTinh", "TenDonViTinh",
                modifyingThuoc.DonViXuatLe != null ? modifyingThuoc.DonViXuatLe.MaDonViTinh : 0);
            ViewBag.MaNhomThuoc = new SelectList(_getListNhomThuoc(), "MaNhomThuoc", "TenNhomThuoc",
                modifyingThuoc.NhomThuoc != null ? modifyingThuoc.NhomThuoc.MaNhomThuoc : 0);
            ViewBag.MaNuoc = new SelectList(_getListNuoc(), "MaNuoc", "TenNuoc",
                modifyingThuoc.Nuoc != null ? modifyingThuoc.Nuoc.MaNuoc : 0);
            return View("Create", thuoc);
        }

        // GET: Thuocs/Delete/5
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            Thuoc thuoc = await
                unitOfWork.ThuocRepository.GetMany(e => e.ThuocId == id && (e.NhaThuocCreate.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuoc))
                    .FirstOrDefaultAsync();
            if (thuoc == null)
            {
                return HttpNotFound();
            }

            return View(thuoc);
        }
        private bool checkPermission(String controller, String action)
        {
            var nhaThuoc = this.GetNhaThuoc();
            if (nhaThuoc == null || string.IsNullOrEmpty(nhaThuoc.Role))
                return false;
            //WebSecurity.GetCurrentUserId;
            //if (WebSecurity.GetCurrentUserId.IsInRole(Constants.Security.Roles.SuperUser.Value))
            //{
            //    return true;
            //}
            if (nhaThuoc.Role == Constants.Security.Roles.Admin.Value)
                return true;
            //if (checkRoles != null && checkRoles.Contains(nhaThuoc.Role))
            //    return true;


            var uow = new sThuoc.Repositories.UnitOfWork();
            var maNhaThuoc = nhaThuoc.MaNhaThuoc;

            var permission = uow.UserPermissionsRespository.Get(
                e => e.Controller == controller.ToLower() && e.Action == action.ToLower() && e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.User.UserId == WebSecurity.GetCurrentUserId);
            if (permission.Any())
                return true;
            return false;
        }        

        [HttpPost]
        //[InputBillAuthorizeAttribute("Admin")]
        public ActionResult DeleteFromdb(int id)
        {
            //Kiểm tra user có quyền sử dụng:            
            if (!checkPermission("thuocs", "delete"))
                return RedirectToAction("Index");
            if (id <= 0)
            {
                return RedirectToAction("index");
            }            
            var drugService = IoC.Container.Resolve<IDrugManagementService>();           
            var thuoc = unitOfWork.ThuocRepository.GetById(id);
            if (drugService.MarkAsDeleteForeverDrugs(MedSessionManager.CurrentDrugStoreCode, id))
            {
                MainApp.Instance.ReloadCacheDrugs(MedSessionManager.CurrentDrugStoreCode);
                BackgroundJobHelper.EnqueueDeleteForeverDrugs(id);
                return RedirectToAction("index");
            }
            else
            {
                ViewBag.Message = "Bạn không thể xoá thuốc \"" + thuoc.TenThuoc + "\"";
            }
            return RedirectToAction("Index");
        }
        // POST: Thuocs/Delete/5
        [HttpPost]
        //[InputBillAuthorizeAttribute("Admin")]
        public ActionResult DeleteConfirm(int id)
        {
            //Kiểm tra user có quyền sử dụng:            
            if (!checkPermission("thuocs", "delete"))
                return RedirectToAction("Index");
            if (id <= 0)
            {
                return RedirectToAction("index");
            }
            var thuoc = unitOfWork.ThuocRepository.GetById(id);

            if (thuoc != null)
            {
                cfDelete(id);
                unitOfWork.Save();
                MainApp.Instance.ReloadCacheDrugs(MedSessionManager.CurrentDrugStoreCode);
                return RedirectToAction("index");
            }
            else
            {
                ViewBag.Message = "Không tìm thấy nhà thuốc theo mã \"" + id + "\"";
            }
            return RedirectToAction("Index");
        }
        private void cfDelete(int id)
        {
            Thuoc doiTuongThuoc = unitOfWork.ThuocRepository.GetById(id);
            doiTuongThuoc.RecordStatusID = (byte)RecordStatus.Deleted;
            unitOfWork.ThuocRepository.Update(doiTuongThuoc);
        }
        private void cfRollback(int id)
        {
            Thuoc doiTuongThuoc = unitOfWork.ThuocRepository.GetById(id);
            doiTuongThuoc.RecordStatusID = (byte)RecordStatus.Activated;
            unitOfWork.ThuocRepository.Update(doiTuongThuoc);
        }
        //[HttpPost, ActionName("Delete")]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[SimpleAuthorize("Admin")]
        public ActionResult RollbackConfirm(int id)
        {
            //Kiểm tra user có quyền không
            if (!checkPermission("thuocs", "delete"))
                return RedirectToAction("Index");
            if (id <= 0)
            {
                return RedirectToAction("index");
            }
            var thuoc = unitOfWork.ThuocRepository.GetById(id);

            if (thuoc != null)
            {
                cfRollback(id);
                unitOfWork.Save();
                MainApp.Instance.ReloadCacheDrugs(MedSessionManager.CurrentDrugStoreCode);
                return RedirectToAction("index");
            }
            else
            {
                ViewBag.Message = "Không tìm thấy thuốc theo mã \"" + id + "\"";
            }
            return RedirectToAction("Index");
        }
        /// <summary>
        /// Get the data for thuoc autocomplete
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public JsonResult GetThuocs(string term)
        {
            var nhaThuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhaThuoc.MaNhaThuoc;
            var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            var cacheDrugs = MedCacheManager.Instance.GetCacheDrugs(nhaThuoc.MaNhaThuoc, term);            
            var drugIds = cacheDrugs.Select(i => i.DrugId).Distinct().ToArray();
            var drugs = unitOfWork.ThuocRepository.GetMany(
               e => drugIds.Contains(e.ThuocId)).ToList()
               .OrderBy(e => e.TenThuoc)
               .ToList();
            

            object result = null;
            if (drugs != null && drugs.Any())
            {
                var invService = IoC.Container.Resolve<IInventoryService>();
                var lastInvetories = invService.GetLastInventoryQuantities(nhaThuoc.MaNhaThuoc, true, drugIds);
                result = drugs.Select(e => new
                {
                    label = e.TenDayDu,
                    desc = string.Format("{0} - {1}", e.MaThuoc, e.TenDayDu),
                    value = e.ThuocId,
                    unit1 = e.DonViXuatLe != null ? e.DonViXuatLe.MaDonViTinh.ToString() : "",
                    DonViXuatLe = e.DonViXuatLe != null ? e.DonViXuatLe.TenDonViTinh : "",
                    DonviThuNguyen = e.DonViThuNguyen != null ? e.DonViThuNguyen.TenDonViTinh : string.Empty,
                    unit2 = e.DonViThuNguyen != null ? e.DonViThuNguyen.MaDonViTinh.ToString() : string.Empty,
                    price1 = e.GiaBanLe,
                    price2 = e.GiaNhap,
                    price3 = e.GiaBanBuon,
                    maThuoc = e.MaThuoc,
                    tenThuoc = e.TenThuoc,
                    idThuoc = e.ThuocId,
                    heSo = e.HeSo,
                    soton = (int)lastInvetories[e.ThuocId],
                    isCatalogCommon = false
                });
            }
            return new JsonResult() { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        //Get số lượng tồn kho của 1 loại thuốc
        public JsonResult GetSLTonKhoCuaThuocs(string id_thuoc)
        {

            int iIdThuoc = Convert.ToInt32(id_thuoc);
            Thuoc th = new Thuoc()
            {
                ThuocId = iIdThuoc
            };
            decimal sltonkho = GetSoLuongHienTai(th);
            return new JsonResult() { Data = sltonkho, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public JsonResult GetSLTonKhoCuaThuocs_Warming(string id_thuoc, string donvi)
        {

            int iIdThuoc = Convert.ToInt32(id_thuoc);
            Thuoc th = new Thuoc()
            {
                ThuocId = iIdThuoc
            };
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            decimal sltonkho = GetSoLuongHienTai(th);
            var thuoc = unitOfWork.ThuocRepository.GetMany(x => x.ThuocId == iIdThuoc).FirstOrDefault();
            if (thuoc != null && thuoc.DonViThuNguyen != null)
            {
                if (thuoc.DonViThuNguyen.MaDonViTinh == int.Parse(donvi.Trim()))
                {
                    sltonkho = sltonkho / (thuoc.HeSo < 1 ? 1 : thuoc.HeSo);
                }
            }
            return new JsonResult() { Data = sltonkho, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public JsonResult GetThuocs_GiaPhieu(string term)
        {
            var nhaThuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhaThuoc.MaNhaThuoc;
            var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            var drugs = unitOfWork.ThuocRepository.GetMany(
               e =>
                   e.NhaThuoc.MaNhaThuoc == (String.IsNullOrEmpty(maNhaThuocCha) ? maNhaThuoc : maNhaThuocCha) && 
                   e.RecordStatusID == (byte)RecordStatus.Activated &&
                   (e.TenThuoc.ToLower().Contains(term.ToLower()) || e.ThongTin.ToLower().Contains(term.ToLower())))
               .OrderBy(e => e.TenThuoc)
               .ToList();
            var drugIds = drugs.Select(i => i.ThuocId).Distinct().ToArray();
            var invService = IoC.Container.Resolve<IInventoryService>();
            var drugService = IoC.Container.Resolve<IDrugManagementService>();
            var lastInvetories = invService.GetLastInventoryQuantities(nhaThuoc.MaNhaThuoc, true, drugIds);
            //var lastReceiptDrugPrices = drugService.GetLastDrugPriceOnReceiptNotes(maNhaThuoc, drugIds);


            //var result = unitOfWork.ThuocRepository.GetMany(
            //    e =>
            //        (e.NhaThuoc.MaNhaThuoc == maNhaThuocCha || e.NhaThuoc.MaNhaThuoc == maNhaThuoc) &&
            //        e.HoatDong &&
            //        (e.TenThuoc.ToLower().Contains(term.ToLower()) || e.ThongTin.ToLower().Contains(term.ToLower())))
            //    .OrderBy(e=> e.TenThuoc)
            //    .ToList()
            //    .Select(e => new
            //    {
            //        label = e.TenDayDu,
            //        desc = string.Format("{0} - {1}", e.MaThuoc, e.TenDayDu),
            //        value = e.ThuocId,
            //        unit1 = e.DonViXuatLe.MaDonViTinh,
            //        DonViXuatLe = e.DonViXuatLe.TenDonViTinh,
            //        unit2 = e.DonViThuNguyen != null ? e.DonViThuNguyen.MaDonViTinh.ToString() : string.Empty,
            //        price1 = e.GiaBanLe,
            //        price2 = GetGiaNhapCuoiCung(new Thuoc()
            //        {
            //            ThuocId = e.ThuocId,
            //            DonViThuNguyen = e.DonViThuNguyen,
            //            NhaThuoc = new NhaThuoc()
            //            {
            //                MaNhaThuoc = maNhaThuoc
            //            },
            //            HeSo = e.HeSo
            //        }),
            //        price3 = e.GiaBanBuon,
            //        maThuoc = e.MaThuoc,
            //        tenThuoc = e.TenThuoc,
            //        soton = (int)lastInvetories[e.ThuocId],
            //        heSo = e.HeSo
            //    });


            var result = drugs
               .Select(e => new
                {
                    label = e.TenDayDu,
                    desc = string.Format("{0} - {1}", e.MaThuoc, e.TenDayDu),
                    value = e.ThuocId,
                    unit1 = e.DonViXuatLe.MaDonViTinh,
                    DonViXuatLe = e.DonViXuatLe.TenDonViTinh,
                    unit2 = e.DonViThuNguyen != null ? e.DonViThuNguyen.MaDonViTinh.ToString() : string.Empty,
                    price1 = e.GiaBanLe,
                    price2 = GetGiaNhapCuoiCung(new Thuoc()
                            {
                                ThuocId = e.ThuocId,
                                DonViThuNguyen = e.DonViThuNguyen,
                                NhaThuoc = new NhaThuoc()
                                {
                                    MaNhaThuoc = maNhaThuoc
                                },
                                HeSo = e.HeSo
                            }),
                   price3 = e.GiaBanBuon,
                    maThuoc = e.MaThuoc,
                    tenThuoc = e.TenThuoc,
                    soton = (int)lastInvetories[e.ThuocId],
                    heSo = e.HeSo
                });

            return new JsonResult() { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GetThuocKiemKe(string term)
        {
            var flag = true;
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var result = unitOfWork.ThuocRepository.GetMany(
                e =>
                    e.NhaThuoc.MaNhaThuoc == maNhaThuoc &&
                    e.RecordStatusID == (byte)RecordStatus.Activated &&
                    (e.TenThuoc.ToLower().Contains(term.ToLower()) || e.ThongTin.ToLower().Contains(term.ToLower()))).ToList();

            foreach (var item in result)
            {
                foreach (var chitietkiemke in item.PhieuKiemKeChiTiets)
                {
                    if (!chitietkiemke.PhieuKiemKe.DaCanKho)
                    {
                        flag = false;
                        break;
                    }
                }

                if (flag)
                {

                }
            }




            return new JsonResult() { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GetThuocsByMaOrTen(string term)
        {
            var nhaThuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhaThuoc.MaNhaThuoc;
            var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            var cacheDrugs = MedCacheManager.Instance.GetCacheDrugs(nhaThuoc.MaNhaThuoc, term);
            var drugIds = cacheDrugs.Select(i => i.DrugId).Distinct().ToArray();
            object result = null;
            if (drugIds.Any())
            {
                var drugs = unitOfWork.ThuocRepository.GetMany(
                    e => drugIds.Contains(e.ThuocId)).ToList()
                   .OrderBy(e => e.TenThuoc)
                   .ToList();

                var invService = IoC.Container.Resolve<IInventoryService>();
                var lastInvetories = invService.GetLastInventoryQuantities(nhaThuoc.MaNhaThuoc, true, drugIds);
                result = drugs
                         .Select(e => new
                         {
                             label = e.TenDayDu,
                             desc = string.Format("{0} - {1}", e.MaThuoc, e.TenDayDu),
                             value = e.ThuocId,
                             unit1 = e.DonViXuatLe.MaDonViTinh,
                             DonViXuatLe = e.DonViXuatLe.TenDonViTinh,
                             unit2 = e.DonViThuNguyen != null ? e.DonViThuNguyen.MaDonViTinh.ToString() : string.Empty,
                             price1 = e.GiaBanLe,
                             price2 = e.GiaNhap,
                             price3 = e.GiaBanBuon,
                             maThuoc = e.MaThuoc,
                             tenThuoc = e.TenThuoc,
                             soton = (int)lastInvetories[e.ThuocId],
                             heSo = e.HeSo
                         });
            }
            


            return new JsonResult() { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public JsonResult GetThuocsByMaOrTen_CatalogsCommons(string term, string drugStoreCode)
        {
            var maNhaThuoc = string.IsNullOrEmpty(drugStoreCode)?Constants.MaNhaThuocMapping: drugStoreCode;
            var cacheDrugs = MedCacheManager.Instance.GetCacheDrugs(maNhaThuoc, term);
            var drugIds = cacheDrugs.Select(i => i.DrugId).Distinct().ToArray();

            var result = unitOfWork.ThuocRepository.GetMany(
                e => drugIds.Contains(e.ThuocId)).ToList().Select(e => new {
                    TenThuoc = e.TenThuoc,
                    TenDayDu = e.TenDayDu,
                    QuyCach = e.QuyCach,
                    drugId = e.ThuocId
                }).ToList();

            return new JsonResult() { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public JsonResult GetDetailDrugInCatalogCommons(int id)
        {
            var nhaThuoc_current = this.GetNhaThuoc();
            var maNhaThuoc_current = nhaThuoc_current.MaNhaThuoc;
            var maNhaThuocMappingCommon = Constants.MaNhaThuocMapping;
            var result = unitOfWork.ThuocRepository.GetMany(
                e =>
                    e.NhaThuoc.MaNhaThuoc == maNhaThuocMappingCommon && e.RecordStatusID == (byte)RecordStatus.Activated &&
                    (e.ThuocId == id)).ToList().Select(e => new
                    {
                        TenThuoc = e.TenThuoc,
                        drugId = e.ThuocId,
                        MaDonViThuNguyen = e.DonViThuNguyen != null ? unitOfWork.DonViTinhRepository.GetMany(x => x.MaNhaThuoc == maNhaThuoc_current && x.TenDonViTinh == e.DonViThuNguyen.TenDonViTinh).Select(s=>s.MaDonViTinh).FirstOrDefault() : -1,
                        MaDonViXuat = e.DonViXuatLe != null ? unitOfWork.DonViTinhRepository.GetMany(x => x.MaNhaThuoc == maNhaThuoc_current && x.TenDonViTinh == e.DonViXuatLe.TenDonViTinh).Select(s => s.MaDonViTinh).FirstOrDefault() : -1,
                        MaThuoc = e.MaThuoc,
                        HeSo = e.HeSo,
                        MaNhomThuoc = e.NhomThuoc != null ? e.NhomThuoc.MaNhomThuoc.ToString() : string.Empty,
                        BarCode = e.BarCode,
                        ThongTin = e.ThongTin,
                        HangTuVan = e.HangTuVan,
                        GiaNhap = e.GiaNhap,
                        HanDung = e.HanDung != null ? e.HanDung.Value.ToString("dd/MM/yyyy") : string.Empty,
                        GiaBanLe = e.GiaBanLe,
                        GiaBanBuon = e.GiaBanBuon,
                        GioiHan = e.GioiHan,
                        SoDuDauKy = e.SoDuDauKy,
                        GiaDauKy = e.GiaDauKy,
                        MaDangBaoChe = e.DangBaoChe != null ? e.DangBaoChe.MaDangBaoChe.ToString() : string.Empty,
                        MaNuoc = e.Nuoc != null ? e.Nuoc.MaNuoc.ToString() : string.Empty,
                        HoatDong = e.RecordStatusID == (byte)RecordStatus.Activated
                    }).FirstOrDefault();
            return new JsonResult() { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public JsonResult AddThuocsFromCatalog(string idCatalog)
        {
            //var maNhaThuoc_catalog = "0012";
            Thuoc thuocInCatalog = unitOfWork.ThuocRepository.GetById(int.Parse(idCatalog.Trim()));
            if (thuocInCatalog == null)
                return new JsonResult() { Data = "", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            string ma_thuoc = string.Empty;
            var nhathuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhathuoc.MaNhaThuoc;
            var maNhaThuocCha = nhathuoc.MaNhaThuocCha;
            var maNhaThuoc_ins = (String.IsNullOrEmpty(maNhaThuocCha) ? maNhaThuoc : maNhaThuocCha);
            ma_thuoc = CreateUniqueDrugCode(maNhaThuoc);
            if (string.IsNullOrEmpty(ma_thuoc) || IsDrugExisted(ma_thuoc, maNhaThuoc_ins))
                return new JsonResult() { Data = "khong_tao_duoc_ma_thuoc", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            var donvinguyen = thuocInCatalog.DonViThuNguyen != null ? unitOfWork.DonViTinhRepository.GetMany(e => e.MaNhaThuoc == maNhaThuoc_ins && e.TenDonViTinh.ToLower() == thuocInCatalog.DonViThuNguyen.TenDonViTinh.ToLower()).FirstOrDefault() : null;
            var donvile = thuocInCatalog.DonViXuatLe != null ? unitOfWork.DonViTinhRepository.GetMany(e => e.MaNhaThuoc == maNhaThuoc_ins && e.TenDonViTinh.ToLower() == thuocInCatalog.DonViXuatLe.TenDonViTinh.ToLower()).FirstOrDefault() : null;
            //Mapping theo tên đơn vị tính không khớp thì không tạo thuốc mới
            if (thuocInCatalog.DonViXuatLe != null && donvile == null)
            {
                donvile = new DonViTinh()
                {
                    TenDonViTinh = thuocInCatalog.DonViXuatLe.TenDonViTinh,
                    MaNhaThuoc = maNhaThuoc_ins
                };
                unitOfWork.DonViTinhRepository.Insert(donvile);
            }
            if (thuocInCatalog.DonViThuNguyen != null && donvinguyen == null)
            {
                donvinguyen = new DonViTinh()
                {
                    TenDonViTinh = thuocInCatalog.DonViThuNguyen.TenDonViTinh,
                    MaNhaThuoc = maNhaThuoc_ins
                };
                unitOfWork.DonViTinhRepository.Insert(donvinguyen);
            }
            var nhomThuoc = unitOfWork.NhomThuocRepository.GetMany(e => e.TenNhomThuoc.ToLower() == "tất cả" && e.MaNhaThuoc == maNhaThuoc_ins).FirstOrDefault();
            if (nhomThuoc == null)
            {
                nhomThuoc = new NhomThuoc()
                {
                    MaNhaThuoc = maNhaThuoc_ins,
                    TenNhomThuoc = "Tất cả"
                };
                unitOfWork.NhomThuocRepository.Insert(nhomThuoc);
            }
            string mavach = "";
            var configMaVachThuoc = unitOfWork.SettingRepository.Get(c => (c.MaNhaThuoc == maNhaThuoc || c.MaNhaThuoc == maNhaThuocCha) && c.Key.Equals(Constants.Settings.TuDongTaoMaVachThuoc, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (configMaVachThuoc != null &&
                configMaVachThuoc.Value.Equals(Constants.Settings.TuDongTaoMaVachThuoc_Value, StringComparison.OrdinalIgnoreCase))
                mavach = CreateUniqueBarcode();
            var newThuoc = new Thuoc()
            {
                Created = DateTime.Now,
                CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId),
                DonViThuNguyen = donvinguyen,
                DonViXuatLe = donvile,
                GiaBanBuon = thuocInCatalog.GiaBanBuon,
                GiaBanLe = thuocInCatalog.GiaBanLe,
                GiaNhap = thuocInCatalog.GiaNhap,
                GiaDauKy = thuocInCatalog.GiaDauKy,
                SoDuDauKy = thuocInCatalog.SoDuDauKy,
                MaThuoc = ma_thuoc.ToUpper(),
                NhomThuoc = nhomThuoc,
                NhaThuoc = unitOfWork.NhaThuocRepository.GetById(maNhaThuoc_ins),
                NhaThuoc_MaNhaThuocCreate = maNhaThuoc,
                HeSo = thuocInCatalog.HeSo,
                TenThuoc = thuocInCatalog.TenThuoc,
                ThongTin = thuocInCatalog.ThongTin,
                GioiHan = thuocInCatalog.GioiHan,
                BarCode = mavach,
                RecordStatusID = (byte)RecordStatus.Activated,
                HangTuVan = thuocInCatalog.HangTuVan,
                DangBaoChe = thuocInCatalog.DangBaoChe,
                Nuoc = thuocInCatalog.Nuoc,
                HanDung = thuocInCatalog.HanDung,
            };
            unitOfWork.ThuocRepository.Insert(newThuoc);
            unitOfWork.Save();
            //Mapping thuốc (tạo mới)
            //var unit_mapping = unitOfWork.DonViTinhRepository.GetMany(t => t.MaNhaThuoc == maNhaThuoc_ins && t.TenDonViTinh.Contains(newThuoc.DonViXuatLe.TenDonViTinh)).FirstOrDefault();
            DrugMapping drugMapping = new DrugMapping();
            drugMapping.MasterDrugID = thuocInCatalog.ThuocId;
            drugMapping.SlaveDrugID = newThuoc.ThuocId;
            drugMapping.InPrice = 0;
            drugMapping.InLastUpdateDate = DateTime.Now.AddDays(-1);
            drugMapping.DrugStoreID = maNhaThuoc_ins;
            drugMapping.OutLastUpdateDate = DateTime.Now.AddDays(-1);
            drugMapping.OutPrice = 0;
            //Mã đơn vị tính theo thuốc mapping
            drugMapping.UnitID = thuocInCatalog.DonViXuatLe.MaDonViTinh;
            unitOfWork.ThuocMappingRepository.Insert(drugMapping);
            unitOfWork.Save();

            BackgroundJobHelper.EnqueueMakeAffectedChangesByUpdatedDrugs(newThuoc.ThuocId);
            var result = new
            {
                label = newThuoc.TenDayDu,
                desc = string.Format("{0} - {1}", newThuoc.MaThuoc, newThuoc.TenDayDu),
                value = newThuoc.ThuocId,
                unit1 = newThuoc.DonViXuatLe.MaDonViTinh,
                DonViXuatLe = newThuoc.DonViXuatLe.TenDonViTinh,
                DonviThuNguyen = newThuoc.DonViThuNguyen != null ? newThuoc.DonViThuNguyen.TenDonViTinh : string.Empty,
                unit2 = newThuoc.DonViThuNguyen != null ? newThuoc.DonViThuNguyen.MaDonViTinh.ToString() : string.Empty,
                price1 = newThuoc.GiaBanLe,//GiaBanLe
                price2 = newThuoc.GiaNhap,//GiaNhap
                price3 = newThuoc.GiaBanBuon,//GiaBanBuon
                maThuoc = newThuoc.MaThuoc,
                tenThuoc = newThuoc.TenThuoc,
                idThuoc = newThuoc.ThuocId,
                heSo = newThuoc.HeSo
            };
            return new JsonResult() { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        /// <summary>
        /// Get all Thuoc by Ma
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public JsonResult GetThuocsByMa(string term)
        {
            var nhaThuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhaThuoc.MaNhaThuoc;
            var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            var drugs = unitOfWork.ThuocRepository.GetMany(
                e =>
                    e.NhaThuoc.MaNhaThuoc == (String.IsNullOrEmpty(maNhaThuocCha) ? maNhaThuoc : maNhaThuocCha) && 
                    e.RecordStatusID == (byte)RecordStatus.Activated && 
                    (e.MaThuoc.ToLower().Contains(term.ToLower())))
                .OrderBy(e => e.TenThuoc)
                .ToList();
            var drugIds = drugs.Select(i => i.ThuocId).Distinct().ToArray();
            var invService = IoC.Container.Resolve<IInventoryService>();
            var lastInvetories = invService.GetLastInventoryQuantities(nhaThuoc.MaNhaThuoc, true, drugIds);
            var result = drugs
                .Select(e => new
                {
                    id = e.ThuocId,
                    label = e.MaThuoc,
                    desc = string.Format("{0} - {1}", e.MaThuoc, e.TenDayDu),
                    value = e.ThuocId,
                    unit1 = e.DonViXuatLe != null?e.DonViXuatLe.MaDonViTinh:-1,
                    DonViXuatLe = e.DonViXuatLe != null?e.DonViXuatLe.TenDonViTinh:"",
                    unit2 = e.DonViThuNguyen != null ? e.DonViThuNguyen.MaDonViTinh.ToString() : string.Empty,
                    price1 = e.GiaBanLe,
                    price2 = e.GiaNhap,
                    price3 = e.GiaBanBuon,
                    maThuoc = e.MaThuoc,
                    tenThuoc = e.TenThuoc,
                    soton = (int)lastInvetories[e.ThuocId],
                    heSo = e.HeSo
                });          

            return new JsonResult() { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GetThuocsByMa_GiaPhieu(string term)
        {
            var nhaThuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhaThuoc.MaNhaThuoc;
            var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            var drugs = unitOfWork.ThuocRepository.GetMany(
                e =>
                    e.NhaThuoc.MaNhaThuoc == (String.IsNullOrEmpty(maNhaThuocCha) ? maNhaThuoc : maNhaThuocCha) &&
                    e.RecordStatusID == (byte)RecordStatus.Activated &&
                    (e.MaThuoc.ToLower().Contains(term.ToLower())))
                .OrderBy(e => e.TenThuoc)
                .ToList();
            var drugIds = drugs.Select(i => i.ThuocId).Distinct().ToArray();
            var invService = IoC.Container.Resolve<IInventoryService>();
            //var drugService = IoC.Container.Resolve<IDrugManagementService>();
            var lastInvetories = invService.GetLastInventoryQuantities(nhaThuoc.MaNhaThuoc, true, drugIds); //Lấy tồn kho hiện tại
            //var lastReceiptDrugPrices = drugService.GetLastDrugPriceOnReceiptNotes(maNhaThuoc, drugIds);  //Lấy giá nhập gần nhất

            var result = drugs
                .Select(e => new
                {
                    id = e.ThuocId,
                    label = e.MaThuoc,
                    desc = string.Format("{0} - {1}", e.MaThuoc, e.TenDayDu),
                    value = e.ThuocId,
                    unit1 = e.DonViXuatLe != null ? e.DonViXuatLe.MaDonViTinh : -1,
                    DonViXuatLe = e.DonViXuatLe != null ? e.DonViXuatLe.TenDonViTinh : "",
                    unit2 = e.DonViThuNguyen != null ? e.DonViThuNguyen.MaDonViTinh.ToString() : string.Empty,
                    price1 = e.GiaBanLe,
                    price2 = GetGiaNhapCuoiCung(new Thuoc()
                            {
                                ThuocId = e.ThuocId,
                                DonViThuNguyen = e.DonViThuNguyen,
                                NhaThuoc = new NhaThuoc()
                                {
                                    MaNhaThuoc = maNhaThuoc
                                },
                                HeSo = e.HeSo
                            }),
                    price3 = e.GiaBanBuon,
                    maThuoc = e.MaThuoc,
                    tenThuoc = e.TenThuoc,
                    soton = (int)lastInvetories[e.ThuocId],
                    heSo = e.HeSo
                });

            return new JsonResult() { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        private decimal GetGiaNhapCuoiCung(Thuoc thuoc)
        {
            var lastItem = unitOfWork.PhieuNhapChiTietRepository.GetMany(
                e => e.PhieuNhap.RecordStatusID == (byte)RecordStatus.Activated && e.Thuoc.ThuocId == thuoc.ThuocId && e.NhaThuoc.MaNhaThuoc == thuoc.NhaThuoc.MaNhaThuoc)
                .OrderByDescending(e => e.PhieuNhap.Created)
                .FirstOrDefault();
            if (lastItem != null)
            {
                decimal hs = 1;
                if (thuoc.HeSo > MedConstants.EspQuantity)
                {
                    hs = thuoc.HeSo;
                }
                return thuoc.DonViThuNguyen != null && lastItem.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh ? lastItem.GiaNhap / hs : lastItem.GiaNhap;
            }
            return thuoc.GiaNhap;
        }



        public JsonResult GetIdThuocsByBarcode(string barcode)
        {
            var nhaThuoc = this.GetNhaThuoc();
            string maNhaThuoc = nhaThuoc.MaNhaThuoc;
            string maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            barcode = barcode.Replace("/", "");
            var id = 0;
            Thuoc thuoc =
                    unitOfWork.ThuocRepository.GetMany(e => (e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha) && e.BarCode == barcode).First();
            //.FirstAsync();

            if (thuoc != null)
            {
                id = thuoc.ThuocId;
                //RedirectToAction("DialogDetail", new { id = thuoc.Id });
            }

            return new JsonResult() { Data = id, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GetMaThuocsByBarcode(string barcode)
        {
            var nhaThuoc = this.GetNhaThuoc();
            string maNhaThuoc = nhaThuoc.MaNhaThuoc;
            string maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            barcode = barcode.Replace("/", "");
            var maThuoc = "";
            Thuoc thuoc =
                    unitOfWork.ThuocRepository.GetMany(e => (e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha) && e.BarCode == barcode && e.RecordStatusID == (byte)RecordStatus.Activated).First();
            //.FirstAsync();

            if (thuoc != null)
            {
                maThuoc = thuoc.MaThuoc;
                //RedirectToAction("DialogDetail", new { id = thuoc.Id });
            }

            return new JsonResult() { Data = maThuoc, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public JsonResult GetThuocsByBarcode(string barcode)
        {
            var nhaThuoc = this.GetNhaThuoc();
            string maNhaThuoc = nhaThuoc.MaNhaThuoc;
            string maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            barcode = barcode.Replace("/", "");
            Thuoc thuoc = unitOfWork.ThuocRepository.GetMany(
                e => (e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha) && e.RecordStatusID == (byte)RecordStatus.Activated && (e.BarCode == barcode))
                .FirstOrDefault();
            //
            if (thuoc != null)
            {
                var result = (new
                {
                    id = thuoc.ThuocId,
                    //maThuoc = e.MaThuoc,
                    //desc = string.Format("{0} - {1}", e.MaThuoc, e.TenDayDu),
                    value = thuoc.ThuocId,
                    unit1 = thuoc.DonViXuatLe.MaDonViTinh,
                    DonViXuatLe = thuoc.DonViXuatLe.TenDonViTinh,
                    unit2 = thuoc.DonViThuNguyen != null ? thuoc.DonViThuNguyen.MaDonViTinh.ToString() : string.Empty,
                    price1 = thuoc.GiaBanLe,
                    price2 = thuoc.GiaNhap,
                    price3 = thuoc.GiaBanBuon,
                    maThuoc = thuoc.MaThuoc,
                    tenThuoc = thuoc.TenThuoc,
                    heSo = thuoc.HeSo
                });
                return new JsonResult() { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            }

            return new JsonResult() { Data = string.Empty, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        /// <summary>
        /// Get all possible Thuoc by Id and So Luongs
        /// </summary>
        /// <param name="thuocId"></param>
        /// <param name="soLuong"></param>
        /// <returns></returns>
        //public JsonResult GetValidThuocs(int thuocId, int soLuong)
        //{
        //    var thuocsUtil = new ThuocsUtil(db, this.GetNhaThuoc().MaNhaThuoc);
        //    return new JsonResult() { Data = thuocsUtil.GetThuocsLeft(thuocId, soLuong), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        //}
        public JsonResult GetThuocsWithQuantity(int? nhomThuoc, string maThuoc, string ngayTao)
        {
            var results = new List<ThuocWithQuantity>();
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            DateTime toDate = Convert.ToDateTime(ngayTao);
            toDate = toDate.AbsoluteEnd();
           
            var inventoryRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Med.Entity.PhieuKiemKe>>();
            var inventoryItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Med.Entity.PhieuKiemKeChiTiet>>();
            var dataFilterService = IoC.Container.Resolve<IDataFilterService>();
            var drugGroupRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Med.Entity.NhomThuoc>>();
            var drugUnitRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Med.Entity.DonViTinh>>();
            var drugService = IoC.Container.Resolve<IDrugManagementService>();
            var service = IoC.Container.Resolve<IInventoryService>();

            var drugs = dataFilterService.GetValidDrugs(maNhaThuoc);
            if (nhomThuoc >= 0)
            {
                drugs = drugs.Where(i => i.NhomThuoc_MaNhomThuoc == nhomThuoc && i.RecordStatusID == (byte)RecordStatus.Activated);
            }
            if (!string.IsNullOrEmpty(maThuoc))
            {
                drugs = drugs.Where(i => i.MaThuoc.ToLower() == maThuoc.ToLower() && i.RecordStatusID == (byte)RecordStatus.Activated);
            }
            var drugInfos = (from dr in drugs
                             join dg in drugGroupRepo.GetAll() on dr.NhomThuoc_MaNhomThuoc equals dg.MaNhomThuoc
                             join u in drugUnitRepo.GetAll() on dr.DonViXuatLe_MaDonViTinh equals u.MaDonViTinh
                             select new ThuocWithQuantity()
                             {
                                 ThuocId = dr.ThuocId,
                                 TenNhomThuoc = dg.TenNhomThuoc,
                                 MaThuoc = dr.MaThuoc,
                                 TenThuoc = dr.TenThuoc,
                                 DonViXuatLe = u.TenDonViTinh,
                                 Gia = dr.GiaBanLe,
                                 MaVach = dr.BarCode ?? string.Empty
                             }).ToList();

            var drugIds = drugInfos.Select(i => i.ThuocId).Distinct().ToArray();
            var inventoryItems = (from it in inventoryItemRepo.GetAll()
                                  join i in inventoryRepo.GetAll()
                                  on it.PhieuKiemKe_MaPhieuKiemKe equals i.MaPhieuKiemKe
                                  where i.NhaThuoc_MaNhaThuoc == maNhaThuoc && drugIds.Contains(it.Thuoc_ThuocId.Value) && !i.DaCanKho
                                  select new { DrugId = it.Thuoc_ThuocId.Value, InventoryId = i.MaPhieuKiemKe }).ToList();

            var drugQuantities = service.GetLastInventoryQuantities(WebSessionManager.Instance.CurrentDrugStoreCode,
                true, drugIds);
            drugInfos.ForEach(i =>
            {
                i.MaPhieuDaTonTai = string.Empty;
                if (drugQuantities.ContainsKey(i.ThuocId))
                {
                    i.SoLuong = (decimal)drugQuantities[i.ThuocId];
                }
                var it = inventoryItems.FirstOrDefault(iv => iv.DrugId == i.ThuocId);
                if (it != null)
                {
                    i.MaPhieuDaTonTai = it.InventoryId.ToString();
                }
            });

            return new JsonResult() { Data = drugInfos, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GetThuocsByBarcodeOrTenHang(string term)
        {
            var nhaThuoc = this.GetNhaThuoc();
            string maNhaThuoc = nhaThuoc.MaNhaThuoc;
            string maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            term = term.Replace("/", "");
            var result = unitOfWork.ThuocRepository.GetMany(
                e =>
                    (e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha) && e.RecordStatusID == (byte)RecordStatus.Activated &&
                    ((e.BarCode != null && e.BarCode.ToLower().Contains(term.ToLower())) || e.TenThuoc.ToLower().Contains(term.ToLower()) ||
                     e.ThongTin.ToLower().Contains(term.ToLower())))
                     .OrderBy(e=> e.TenThuoc)
                     .ToList()                     
                     .Select(e => new
                     {
                         label = e.TenDayDu,
                         desc = string.Format("{0} - {1}", e.MaThuoc, e.TenDayDu),
                         value = e.ThuocId,
                         unit1 = e.DonViXuatLe.MaDonViTinh,
                         DonViXuatLe = e.DonViXuatLe.TenDonViTinh,
                         unit2 = e.DonViThuNguyen != null ? e.DonViThuNguyen.MaDonViTinh.ToString() : string.Empty,
                         price1 = e.GiaBanLe,
                         price2 = e.GiaNhap,
                         price3 = e.GiaBanBuon,
                         maThuoc = e.MaThuoc,
                         tenThuoc = e.TenThuoc,
                         heSo = e.HeSo
                     });

            return new JsonResult() { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private string ThuocDaTonTai(int thuocId, string manhathuoc)
        {

            var item = unitOfWork.PhieuKiemKeChiTietRepository.Get(c => !c.PhieuKiemKe.DaCanKho && c.PhieuKiemKe.NhaThuoc.MaNhaThuoc == manhathuoc && c.Thuoc.ThuocId == thuocId).FirstOrDefault();
            if (item != null)
                return item.PhieuKiemKe.MaPhieuKiemKe.ToString();
            return string.Empty;
        }

        /// <summary>
        /// Get all Thuoc by Nhom Thuoc Id
        /// </summary>
        /// <param name="nhomId"></param>
        /// <returns></returns>
        public JsonResult GetThuocsByNhom(int nhomId)
        {
            var nhaThuoc = this.GetNhaThuoc();
            string maNhaThuoc = nhaThuoc.MaNhaThuoc;
            //string maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            var result = new List<ThuocWithQuantity>();
            if (nhomId >= 0)
            {
                var qry =
                    db.Thuocs.Where(
                        x =>
                            (x.NhaThuoc.MaNhaThuoc == maNhaThuoc) &&
                            (nhomId == 0 || x.NhomThuoc.MaNhomThuoc == nhomId)).OrderBy(x=> x.TenThuoc)
                        .Select(x => new ThuocWithQuantity()
                        {
                            ThuocId = x.ThuocId,
                            MaThuoc = x.MaThuoc,
                            TenThuoc = x.TenThuoc,
                            SoLuong = x.PhieuNhapChiTiets.Where(z => z.PhieuNhap.RecordStatusID == (byte)RecordStatus.Activated).Sum(y => y.SoLuong)
                        });
                result = qry.ToList();
            }

            return new JsonResult() { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GenUniqueBarcode()
        {
            string barcode = sThuoc.Utils.Helpers.GenerateUniqueBarcodeV2();
            while (db.Thuocs.Any(e => e.BarCode == barcode))
            {
                barcode = sThuoc.Utils.Helpers.GenerateUniqueBarcodeV2();
            }

            return new JsonResult() { Data = barcode, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        /// <summary>
        /// Get the Han Dung By Thuoc
        /// </summary>
        /// <param name="thuocId"></param>
        /// <returns></returns>
        //public JsonResult GetHanDung(int thuocId)
        //{
        //    var qry = db.PhieuNhapChiTiets.Where(x => x.MaNhaThuoc == this.GetNhaThuoc().MaNhaThuoc && x.ThuocId == thuocId && !x.PhieuNhap.Xoa)
        //            .GroupBy(y => y.HanDung).OrderBy(x => x.Key).Select(z => z.Key);
        //    return new JsonResult() { Data = qry.ToList(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        //}

        [AcceptVerbs(HttpVerbs.Post)]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public ActionResult Upload(HttpPostedFileBase uploadFile)
        {
            var strValidations = new StringBuilder(string.Empty);
            try
            {
                if (uploadFile.ContentLength > 0)
                {
                    var nhaThuoc = this.GetNhaThuoc();
                    string maNhaThuoc = nhaThuoc.MaNhaThuoc;
                    string maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
                    string filePath = Path.Combine(HttpContext.Server.MapPath("../Uploads"),
                        Path.GetFileName(uploadFile.FileName));
                    uploadFile.SaveAs(filePath);
                    int totalupdated = 0;
                    int totaladded = 0;
                    int totalError = 0;
                    string message = "<b>Thông tin thuốc ở dòng số {0} bị lỗi:</b><br/> {1}";
                    UploadObjectInfo info = new UploadObjectInfo();
                    var drugs = new List<Thuoc>();
                    foreach (var worksheet in Workbook.Worksheets(filePath))
                    {
                        for (int i = 1; i < worksheet.Rows.Count(); i++)
                        {
                            var msg = ValidateDataImport(worksheet.Rows[i]);
                            if (!string.IsNullOrEmpty(msg))
                            {
                                if (msg == Constants.Params.msgOk)
                                {
                                    var maThuoc = sThuoc.Utils.Helpers.RemoveEncoding(worksheet.Rows[i].Cells[0].Text.Trim());
                                    var thuoc =
                                        unitOfWork.ThuocRepository.GetMany(
                                            e => (e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha) && e.MaThuoc == maThuoc)
                                            .FirstOrDefault();                                    
                                    //Check if thuoc already exist
                                    if (thuoc != null)
                                    {
                                        BindThuoc(ref thuoc, worksheet.Rows[i]);
                                        unitOfWork.ThuocRepository.Update(thuoc);
                                        totalupdated++;
                                        drugs.Add(thuoc);
                                    }
                                    else
                                    {
                                        thuoc = new Thuoc()
                                        {
                                            ThuocId = 0,
                                            MaThuoc = maThuoc,
                                            NhaThuoc = unitOfWork.NhaThuocRepository.GetById(maNhaThuoc),
                                            NhaThuoc_MaNhaThuocCreate = maNhaThuoc,
                                            Created = DateTime.Now,
                                            CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId)
                                        };

                                        BindThuoc(ref thuoc, worksheet.Rows[i]);
                                        unitOfWork.ThuocRepository.Insert(thuoc);
                                        thuoc.PreFactors = thuoc.HeSo;
                                        thuoc.PreInitPrice = thuoc.GiaDauKy;
                                        thuoc.PreInitQuantity = thuoc.SoDuDauKy;
                                        thuoc.PreRetailUnitID = thuoc.DonViXuatLe != null ? thuoc.DonViXuatLe.MaDonViTinh : (int?)null;
                                        thuoc.PreUnitID = thuoc.DonViThuNguyen != null ? thuoc.DonViThuNguyen.MaDonViTinh : (int?)null;
                                        totaladded++;
                                        drugs.Add(thuoc);
                                    }
                                }
                                else
                                {
                                    info.ErrorMsg.Add(string.Format(message, i, msg));
                                    totalError++;
                                }
                            }

                        }

                        unitOfWork.Save();
                        info.Title = "Thông tin upload thuốc";
                        info.TotalUpdated = totaladded;
                        info.TotalAdded = totaladded;
                        info.TotalError = totalError;
                        Session["UploadDrugInfo"] = info;

                        var drugIds = drugs.Select(i => i.ThuocId).Distinct().ToArray();
                        BackgroundJobHelper.EnqueueMakeAffectedChangesByUpdatedDrugs(drugIds);                                      
                        
                        return RedirectToAction("Index", "Upload");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                ViewBag.FullMessage = ex.Message;
                return View("Error");
            }
            return RedirectToAction("Index");
        }

        private string ValidateDataImport(Excel.Row row)
        {
            var nhaThuoc = this.GetNhaThuoc();
            string maNhaThuoc = nhaThuoc.MaNhaThuoc;
            string maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
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
                decimal dTmp = 0;
                int iTmp = 0;
                string maThuoc = "";
                if (row.Cells[0] == null || string.IsNullOrEmpty(row.Cells[0].Text.Trim()))
                {
                    msg += "    - Mã thuốc không được bỏ trống <br/>";
                }
                else
                {
                    maThuoc = row.Cells[0].Text.Trim();
                }

                if (row.Cells[1] == null || string.IsNullOrEmpty(row.Cells[1].Text.Trim()))
                {
                    msg += "    - Tên nhóm thuốc không được bỏ trống <br/>";
                }

                if (row.Cells[2] == null || string.IsNullOrEmpty(row.Cells[2].Text.Trim()))
                {
                    msg += "    - Tên thuốc không được bỏ trống <br/>";
                }

                if (row.Cells[4] != null && !decimal.TryParse(row.Cells[4].Text, out dTmp))
                {
                    msg += "    - Giá nhập phải là số <br/>";
                }

                if (row.Cells[5] != null && !decimal.TryParse(row.Cells[5].Text, out dTmp))
                {
                    msg += "    - Giá bán lẻ phải là số <br/>";
                }

                if (row.Cells[7] == null || string.IsNullOrEmpty(row.Cells[7].Text.Trim()))
                {
                    msg += "    - Tên đơn vị bán lẻ không được bỏ trống <br/>";
                }

                if (row.Cells[9] != null && !int.TryParse(row.Cells[9].Text, out iTmp))
                {
                    msg += "    - Giới hạn phải là số <br/>";
                }

                if (row.Cells[10] != null && !int.TryParse(row.Cells[10].Text, out iTmp))
                {
                    msg += "    - Hệ số phải là số <br/>";
                }

                if (row.Cells[12] != null && !decimal.TryParse(row.Cells[12].Text, out dTmp))
                {
                    msg += "    - Giá đầu kỳ phải là số <br/>";
                }

                if (row.Cells[15] != null && !string.IsNullOrEmpty(row.Cells[15].Text))
                {
                    var barcode = row.Cells[15].Text.Trim().Replace(",", "");
                    var thuoc = unitOfWork.ThuocRepository.Get(c => (c.NhaThuoc.MaNhaThuoc == maNhaThuoc || c.NhaThuoc.MaNhaThuoc == maNhaThuocCha) && c.BarCode.Equals(barcode, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                    if (thuoc != null && thuoc.MaThuoc.ToLower() != maThuoc.ToLower())
                        msg += "    - Barcode này đã tồn tại. Vui lòng nhập barcode khác <br/>";
                }

                if (row.Cells[16] != null && !string.IsNullOrEmpty(row.Cells[16].Text.Trim()))
                {
                    var value = row.Cells[16].Text.Trim();
                    DateTime dt = new DateTime();
                    if (!sThuoc.Utils.Helpers.ConvertToDateTime(value, ref dt))
                    {
                        msg += "    - Hạn dùng phải là định dạng ngày tháng (dd/mm/yyyy hoặc dd-mm-yyyy)<br/>";
                    }
                }

                return string.IsNullOrEmpty(msg) ? Constants.Params.msgOk : msg;
            }
            else
            {
                return string.Empty;
            }
        }

        private bool CheckAuthorize(string controller, string action, NhaThuocSessionModel nhaThuoc)
        {
            //if (nhaThuoc == null || string.IsNullOrEmpty(nhaThuoc.Role))
            //    return false;
            //if (HttpContext.Current.User.IsInRole(Constants.Security.Roles.SuperUser.Value))
            //{
            //    return true;
            //}
            //if (nhaThuoc.Role == Constants.Security.Roles.Admin.Value)
            //    return true;


            //// kiem tra co quyen cho action bạn muốn?
            //var uow = new UnitOfWork();
            //var permission = uow.UserPermissionsRespository.Get(
            //    e => e.Controller == controller.ToLower() && e.Action == action.ToLower() && e.NhaThuoc.MaNhaThuoc == nhaThuoc.MaNhaThuoc && e.User.UserId == WebSecurity.GetCurrentUserId);
            //if (permission.Any())
            //    return true;


            return false;
        }
        private Thuoc BindThuoc(ref Thuoc thuoc, Excel.Row row)
        {
            var nhaThuoc = this.GetNhaThuoc();
            string maNhaThuoc = nhaThuoc.MaNhaThuoc;
            string maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            //Nhom Thuoc
            var tenNhomThuoc = sThuoc.Utils.Helpers.RemoveEncoding(row.Cells[1].Text.Trim());
            var nhomThuoc =
                unitOfWork.NhomThuocRepository.GetMany(
                    e => (e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha) && e.TenNhomThuoc == tenNhomThuoc).FirstOrDefault();
            if (nhomThuoc == null && !String.IsNullOrEmpty(tenNhomThuoc))
            {
                nhomThuoc = new NhomThuoc
                {
                    TenNhomThuoc = tenNhomThuoc,
                    MaNhaThuoc = maNhaThuoc,
                    KyHieuNhomThuoc = "",
                    Created = DateTime.Now,
                    CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId)
                };

                unitOfWork.NhomThuocRepository.Insert(nhomThuoc);
            }
            if (nhomThuoc != null) thuoc.NhomThuoc = nhomThuoc;
            //DonViXuat
            var tenDonViLe = sThuoc.Utils.Helpers.RemoveEncoding(row.Cells[7].Text.Trim());
            var donViLe =
                unitOfWork.DonViTinhRepository.GetMany(
                    e => (e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha) && e.TenDonViTinh == tenDonViLe).FirstOrDefault();


            if (donViLe == null && !String.IsNullOrEmpty(tenDonViLe))
            {
                donViLe = new DonViTinh()
                {
                    TenDonViTinh = tenDonViLe,
                    MaNhaThuoc = maNhaThuoc
                };

                unitOfWork.DonViTinhRepository.Insert(donViLe);
            }

            if (donViLe != null) thuoc.DonViXuatLe = donViLe;

            if (row.Cells[8] != null && !string.IsNullOrEmpty(row.Cells[8].Text))
            {
                var tenDonViTn = sThuoc.Utils.Helpers.RemoveEncoding(row.Cells[8].Text.Trim());
                var donViTn = unitOfWork.DonViTinhRepository.GetMany(e => (e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha) && e.TenDonViTinh == tenDonViTn).FirstOrDefault();
                if (donViTn == null && !String.IsNullOrEmpty(tenDonViTn))
                {
                    donViTn = new DonViTinh()
                    {
                        TenDonViTinh = tenDonViTn,
                        MaNhaThuoc = maNhaThuoc,
                        MaDonViTinh = 0

                    };
                    unitOfWork.DonViTinhRepository.Insert(donViTn);
                }

                if (donViTn != null) thuoc.DonViThuNguyen = donViTn;
            }

            //Dang Bao Che
            if (row.Cells[13] != null)
            {
                var tenDangBaoChe = sThuoc.Utils.Helpers.RemoveEncoding(row.Cells[13].Text.Trim());
                var dangBaoChe =
                    unitOfWork.DangBaoCheRepository.GetMany(
                        e => (e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha) && e.TenDangBaoChe == tenDangBaoChe).FirstOrDefault();
                if (dangBaoChe == null && !String.IsNullOrEmpty(tenDangBaoChe))
                {
                    dangBaoChe = new DangBaoChe()
                    {
                        TenDangBaoChe = tenDangBaoChe,
                        MaNhaThuoc = maNhaThuoc,
                        MaDangBaoChe = 0
                    };
                    unitOfWork.DangBaoCheRepository.Insert(dangBaoChe);
                }
                if (dangBaoChe != null) thuoc.DangBaoChe = dangBaoChe;
            }

            //Nuoc
            if (row.Cells[14] != null)
            {
                var tenNuoc = sThuoc.Utils.Helpers.RemoveEncoding(row.Cells[14].Text.Trim());
                if (!String.IsNullOrWhiteSpace(tenNuoc))
                {
                    var nuoc =
                        unitOfWork.NuocRepository.GetMany(e =>
                            //e.NhaThuoc.MaNhaThuoc == maNhaThuoc &&
                            e.TenNuoc == tenNuoc)
                            .FirstOrDefault();
                    if (nuoc == null && !string.IsNullOrEmpty(tenNuoc))
                    {
                        nuoc = new Nuoc()
                        {
                            TenNuoc = tenNuoc
                            //,MaNhaThuoc = maNhaThuoc
                        };
                        unitOfWork.NuocRepository.Insert(nuoc);
                    }

                    thuoc.Nuoc = nuoc;
                }
                else
                {
                    thuoc.Nuoc = null;
                }
            }

            unitOfWork.Save();

            var tenThuoc = row.Cells[2].Text.Trim();
            var thongTin = row.Cells[3] != null ? row.Cells[3].Text.Trim() : string.Empty;
            var giaNhap = row.Cells[4] != null ? row.Cells[4].Value.Trim() : string.Empty;
            var giaBanLe = row.Cells[5] != null ? row.Cells[5].Value.Trim() : string.Empty;
            var giaBuon = row.Cells[6] != null ? row.Cells[6].Value.Trim() : string.Empty;
            var gioiHan = row.Cells[9] != null ? row.Cells[9].Value.Trim() : "0";
            var heSo = row.Cells[10] != null ? row.Cells[10].Value.Trim() : string.Empty;
            var duDauKy = row.Cells[11] != null ? row.Cells[11].Value.Trim() : string.Empty;
            var giaDauKy = row.Cells[12] != null ? row.Cells[12].Value.Trim() : string.Empty;
            if (!String.IsNullOrEmpty(tenThuoc)) thuoc.TenThuoc = sThuoc.Utils.Helpers.RemoveEncoding(tenThuoc);
            if (!String.IsNullOrEmpty(thongTin)) thuoc.ThongTin = sThuoc.Utils.Helpers.RemoveEncoding(thongTin);
            if (!String.IsNullOrEmpty(giaNhap)) thuoc.GiaNhap = Convert.ToDecimal(giaNhap);
            if (!String.IsNullOrEmpty(giaBanLe)) thuoc.GiaBanLe = Convert.ToDecimal(giaBanLe);
            if (!String.IsNullOrEmpty(giaBuon)) thuoc.GiaBanBuon = Convert.ToDecimal(giaBuon);
            if (!String.IsNullOrEmpty(gioiHan)) thuoc.GioiHan = Convert.ToInt32(gioiHan);
            if (!String.IsNullOrEmpty(heSo)) thuoc.HeSo = Convert.ToInt32(heSo);
            if (!String.IsNullOrEmpty(duDauKy)) thuoc.SoDuDauKy = Convert.ToDecimal(duDauKy);
            if (!String.IsNullOrEmpty(giaDauKy)) thuoc.GiaDauKy = Convert.ToDecimal(giaDauKy);
            if (row.Cells[15] != null) thuoc.BarCode = sThuoc.Utils.Helpers.RemoveEncoding(row.Cells[15].Text.Trim().Replace(",", ""));
            if (row.Cells[16] != null)
            {
                var dt = new DateTime();
                if (sThuoc.Utils.Helpers.ConvertToDateTime(row.Cells[16].Text, ref dt))
                {
                    thuoc.HanDung = dt;
                }
            }
           
            thuoc.RecordStatusID = (byte)RecordStatus.Activated;

            return thuoc;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Thuoc JqGrid

        [SimpleAuthorize("Admin")]
        public ActionResult GridIndex(GridSettings gridSettings)
        {

            var list = _getListNhomThuoc();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var item in list)
            {
                dic.Add(item.MaNhomThuoc.ToString(), item.TenNhomThuoc);
            }

            var list2 = _getListDonViTinh();
            Dictionary<string, string> dic2 = new Dictionary<string, string>();
            foreach (var item in list2)
            {
                dic2.Add(item.MaDonViTinh.ToString(), item.TenDonViTinh);
            }

            ViewData["NhomThuocs"] = dic;
            ViewData["DonViTinhs"] = dic2;//_getListDonViTinh().Select(e => e.TenDonViTinh).Distinct().ToArray();
            return View();
        }

        /// <summary>
        /// Get the data for grid
        /// </summary>
        /// <param name="gridSettings"></param>
        /// <returns></returns>
        public ActionResult GridDataBasic(GridSettings gridSettings)
        {
            var thuocs = unitOfWork.ThuocRepository.GetThuocs(this.GetNhaThuoc().MaNhaThuoc, gridSettings);
            var totalThuocs = unitOfWork.ThuocRepository.CountThuocs(this.GetNhaThuoc().MaNhaThuoc, gridSettings);

            var jsonData = new
            {
                total = totalThuocs / gridSettings.PageSize + 1,
                page = gridSettings.PageIndex,
                records = totalThuocs,
                rows = (
                    from c in thuocs
                    select new
                    {
                        id = c.ThuocId,
                        cell = new[]
                        {
                            c.ThuocId.ToString(),
                            c.MaThuoc,
                            c.TenDayDu,
                            c.NhomThuoc.TenNhomThuoc,
                            c.DonViXuatLe.TenDonViTinh,
                            c.GiaBanLe.ToString(),
                            c.GiaNhap.ToString(),
                            c.GioiHan.ToString()
                        }
                    }).ToArray()
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        #endregion

        [SimpleAuthorize("Admin")]
        public ActionResult ExportToExcel()
        {
            var thuocs = new DataTable("Thuốc");
            thuocs.Columns.Add("Mã Thuốc", typeof(string));
            thuocs.Columns.Add("Nhóm Thuốc", typeof(string));
            thuocs.Columns.Add("Tên Thuốc", typeof(string));
            thuocs.Columns.Add("Thông Tin", typeof(string));

            thuocs.Columns.Add("Giá Nhập", typeof(int));
            thuocs.Columns.Add("Giá Bán Lẻ", typeof(int));
            thuocs.Columns.Add("Giá Buôn", typeof(int));
            thuocs.Columns.Add("Đơn Vị Lẻ", typeof(string));
            thuocs.Columns.Add("Đơn Vị Thứ Nguyên", typeof(string));
            thuocs.Columns.Add("Giới Hạn", typeof(int));
            thuocs.Columns.Add("Hệ Số", typeof(int));
            thuocs.Columns.Add("Số dư đầu kỳ", typeof(int));
            thuocs.Columns.Add("Giá đầu kỳ", typeof(int));
            thuocs.Columns.Add("Dạng Bào Chế", typeof(string));
            thuocs.Columns.Add("Nước", typeof(string));
            thuocs.Columns.Add("Barcode", typeof(string));
            thuocs.Columns.Add("Hạn Dùng", typeof(string));

            var nhaThuoc = this.GetNhaThuoc();
            string maNhaThuoc = nhaThuoc.MaNhaThuoc;
            string maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            var thuocList =
                unitOfWork.ThuocRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha).Where(x => x.RecordStatusID == (byte)RecordStatus.Activated)
                    .OrderBy(e => e.TenThuoc)
                    .AsEnumerable()
                    .Select(i => new
                    {
                        i.MaThuoc,
                        TenNhomThuoc = i.NhomThuoc != null ? i.NhomThuoc.TenNhomThuoc : "",
                        i.TenThuoc,
                        i.ThongTin,
                        i.GiaNhap,
                        i.GiaBanLe,
                        i.GiaBanBuon,
                        DonViLe = i.DonViXuatLe != null ? i.DonViXuatLe.TenDonViTinh : "",
                        DonViThuNguyen = i.DonViThuNguyen != null ? i.DonViThuNguyen.TenDonViTinh : "",
                        i.GioiHan,
                        i.HeSo,
                        i.SoDuDauKy,
                        i.GiaDauKy,
                        TenDangBaoChe = i.DangBaoChe != null ? i.DangBaoChe.TenDangBaoChe : "",
                        TenNuoc = i.Nuoc != null ? i.Nuoc.TenNuoc : "",
                        BarCode = i.BarCode,
                        HanDung = i.HanDung.HasValue ? i.HanDung.Value.ToString("dd/MM/yyyy") : string.Empty
                    });
            //Add to rows
            foreach (var item in thuocList)
            {
                DataRow dr = thuocs.NewRow();
                dr["Mã Thuốc"] = item.MaThuoc;
                dr["Nhóm Thuốc"] = item.TenNhomThuoc;
                dr["Tên Thuốc"] = item.TenThuoc;
                dr["Thông Tin"] = item.ThongTin ?? "";
                dr["Giá Nhập"] = item.GiaNhap;
                dr["Giá Bán Lẻ"] = item.GiaBanLe;
                dr["Giá Buôn"] = item.GiaBanBuon;
                dr["Đơn Vị Lẻ"] = item.DonViLe;
                dr["Đơn Vị Thứ Nguyên"] = item.DonViThuNguyen;
                dr["Giới Hạn"] = item.GioiHan ?? 0;
                dr["Hệ Số"] = item.HeSo;
                dr["Số dư đầu kỳ"] = item.SoDuDauKy;
                dr["Giá đầu kỳ"] = item.GiaDauKy;
                dr["Dạng Bào Chế"] = item.TenDangBaoChe ?? "";
                dr["Nước"] = item.TenNuoc ?? "";
                dr["Barcode"] = item.BarCode;
                dr["Hạn Dùng"] = item.HanDung;
                thuocs.Rows.Add(dr);
            }

            using (var pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Thuốc");

                //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                ws.Cells["A1"].LoadFromDataTable(thuocs, true);

                //Format the header for column 1-3
                using (ExcelRange rng = ws.Cells["A1:Q1"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                    rng.Style.Font.Color.SetColor(Color.White);
                }

                //Example how to Format Column 1 as numeric 
                using (ExcelRange col = ws.Cells[2, 5, 2 + thuocs.Rows.Count, 7])
                {
                    col.Style.Numberformat.Format = "0.##0";
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                using (ExcelRange col = ws.Cells[2, 10, 2 + thuocs.Rows.Count, 13])
                {
                    col.Style.Numberformat.Format = "0.##0";
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }

                var fileDownloadName = "Thuoc-" + DateTime.Now + ".xlsx";

                var fileStream = new MemoryStream();
                pck.SaveAs(fileStream);
                fileStream.Position = 0;

                var fsr = new FileStreamResult(fileStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                fsr.FileDownloadName = fileDownloadName;

                return fsr;
            }
        }
        public async Task<ActionResult> DialogDetailV2(int id)
        {
            var thuoc =  await
                    unitOfWork.ThuocRepository.GetMany(e => e.ThuocId == id)
                        .FirstOrDefaultAsync();
                if (thuoc != null)
                {
                    var tblMin = GetMinPriceOfDrugs(new List<int>() { thuoc.ThuocId });
                    var tableJoin = from tblThuoc in unitOfWork.ThuocRepository.Get(e=>e.ThuocId == thuoc.ThuocId)
                                    join tableMap in tblMin
                                        on tblThuoc.ThuocId equals tableMap.DrugID into tbljoin
                                    from tableMap in tbljoin.DefaultIfEmpty()
                                    select new
                                    {
                                        Id = tblThuoc.ThuocId,
                                        TenDayDu = tblThuoc.TenDayDu,
                                        DonViXuatLe = tblThuoc.DonViXuatLe.TenDonViTinh,
                                        GiaNhapLe = string.Format("{0:#,###}", tableMap != null ? (tblThuoc.HeSo > 0 ? tblThuoc.HeSo * tableMap.InPrice : tableMap.InPrice) : tblThuoc.GiaNhap * (tblThuoc.HeSo > 0 ? tblThuoc.HeSo : 1)),
                                        GiaNhapNgayCapNhat = tableMap != null && tableMap.InLastUpdateDate != null && tableMap.InLastUpdateDate.HasValue ? tableMap.InLastUpdateDate.Value.ToString("dd/MM/yyyy") : tblThuoc.Created.Value.ToString("dd/MM/yyyy"),
                                        GiaBanLe = string.Format("{0:#,###}", tableMap != null ? (tblThuoc.HeSo > 0 ? tblThuoc.HeSo * tableMap.OutPrice : tableMap.OutPrice) : tblThuoc.GiaBanLe * (tblThuoc.HeSo > 0 ? tblThuoc.HeSo : 1)),
                                        GiaBanNgayCapNhat = tableMap != null && tableMap.OutLastUpdateDate != null && tableMap.OutLastUpdateDate.HasValue ? tableMap.OutLastUpdateDate.Value.ToString("dd/MM/yyyy") : tblThuoc.Created.Value.ToString("dd/MM/yyyy"),
                                        QuyCach = tblThuoc.QuyCach,
                                        DonViNguyen = (tblThuoc.DonViThuNguyen != null && tblThuoc.DonViThuNguyen.MaDonViTinh != tblThuoc.DonViXuatLe.MaDonViTinh ? tblThuoc.DonViThuNguyen.TenDonViTinh : tblThuoc.DonViXuatLe.TenDonViTinh)
                                    };
                var results_temp = tableJoin.FirstOrDefault();
                if(results_temp != null)
                {
                    ViewBag.GiaNhapLe = results_temp.GiaNhapLe;
                    ViewBag.GiaNhapNgayCapNhat = results_temp.GiaNhapNgayCapNhat;
                    ViewBag.GiaBanLe = results_temp.GiaBanLe;
                }
            }
            return View(thuoc);
        }
        public async Task<ActionResult> DialogDetail(int? id, string sMaNhaThuoc)
        {
            var nhaThuoc = this.GetNhaThuoc();
            string maNhaThuoc = nhaThuoc.MaNhaThuoc;
            string maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            if (!string.IsNullOrEmpty(sMaNhaThuoc))
            {
                if(sMaNhaThuoc == Constants.MaNhaThuocMapping)
                    return View(getThuocCatalogDetail(id.Value));
                else
                {
                    maNhaThuoc = sMaNhaThuoc;
                    maNhaThuocCha = sMaNhaThuoc;
                }
            }
            var thuoc = new Thuoc();
            if (id.HasValue)
            {
                thuoc = await
                    unitOfWork.ThuocRepository.GetMany(e => (e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha) && e.ThuocId == id)
                        .FirstOrDefaultAsync();
                if (thuoc != null)
                {
                    var thuocnhap = unitOfWork.PhieuNhapChiTietRepository.Get(c => c.PhieuNhap.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.Thuoc.ThuocId == thuoc.ThuocId).OrderByDescending(c => c.MaPhieuNhapCt).FirstOrDefault();
                    var thuocxuat = unitOfWork.PhieuXuatChiTietRepository.Get(c => c.PhieuXuat.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.Thuoc.ThuocId == thuoc.ThuocId).OrderByDescending(c => c.MaPhieuXuatCt).FirstOrDefault();
                    //ViewBag.GiaNhap = GetGiaNhapHienTai(thuoc);
                    //ViewBag.GiaXuat = GetGiaXuatHienTai(thuoc);
                    //ViewBag.SoLuong = GetSoLuongHienTai(thuoc);
                    ViewBag.GiaNhapDonGanNhat = thuocnhap != null ? thuocnhap.GiaNhap.ToString("#,##0") : string.Empty;
                    ViewBag.GiaNhapXuatGanNhat = thuocxuat != null ? thuocxuat.GiaXuat.ToString("#,##0") : string.Empty;
                    ViewBag.DonViLe = thuoc.DonViXuatLe.TenDonViTinh;
                    ViewBag.QuyCach = string.Empty;
                    if (thuoc.DonViThuNguyen != null)
                    {
                        ViewBag.QuyCach = thuoc.DonViThuNguyen.TenDonViTinh + " = " + thuoc.HeSo.ToString() + " " + thuoc.DonViXuatLe.TenDonViTinh;
                    }
                    ViewBag.GiaNhap = thuoc.GiaNhap.ToString("#,##0");
                    ViewBag.GiaXuat = thuoc.GiaBanLe.ToString("#,##0");
                    ViewBag.GiaSi = thuoc.GiaBanBuon.ToString("#,##0");
                    ViewBag.SoLuong = GetSoLuongHienTai(thuoc);
                    ViewBag.GioiHan = thuoc.GioiHan.ToString();
                    ViewBag.HanDung = thuoc.HanDung.ToString();
                    //ViewBag.LastDayUpdate = thuoc.LastDateUpdate.HasValue ? thuoc.LastDateUpdate.Value.ToString("dd/MM/yyyy") : string.Empty;
                }

            }
            return View(thuoc);
        }
        private Thuoc getThuocCatalogDetail(int id)
        {
            var thuoc = new Thuoc();
            var maNhaThuoc = Constants.MaNhaThuocMapping;
            var tblDrugSearched = unitOfWork.ThuocRepository.Get(e => e.ThuocId == id);
            var tblMin = GetMinPriceOfDrugs(new List<int>() { id });
            var tableJoin = from catalogs in tblDrugSearched
                            join tableMap in tblMin
                                on catalogs.ThuocId equals tableMap.DrugID into tbljoin
                            from tableMap in tbljoin.DefaultIfEmpty()
                            select new
                            {
                                Id = catalogs.ThuocId,
                                TenThuoc = catalogs.TenThuoc,
                                ThongTin = catalogs.ThongTin,
                                BarCode = catalogs.BarCode,
                                HangTuVan = catalogs.HangTuVan,
                                DonViThuNguyen = catalogs.DonViThuNguyen,
                                HeSo = catalogs.HeSo,
                                DonViXuatLe = catalogs.DonViXuatLe,
                                GiaNhap = string.Format("{0:#,###}", tableMap != null ? (catalogs.HeSo > 0? catalogs.HeSo *tableMap.InPrice: tableMap.InPrice) : catalogs.GiaNhap),
                                GiaXuat = string.Format("{0:#,###}", tableMap != null ? (catalogs.HeSo > 0 ? catalogs.HeSo * tableMap.OutPrice: tableMap.OutPrice) : catalogs.GiaBanLe),
                                QuyCach = catalogs.QuyCach
                            };
            var CurrentThuoc = tableJoin.FirstOrDefault();
            if(CurrentThuoc != null)
            {
                thuoc.TenThuoc = CurrentThuoc.TenThuoc;
                thuoc.ThongTin = CurrentThuoc.ThongTin;
                thuoc.BarCode = CurrentThuoc.BarCode;
                thuoc.HangTuVan = CurrentThuoc.HangTuVan;
                thuoc.HeSo = CurrentThuoc.HeSo;
                thuoc.DonViXuatLe = CurrentThuoc.DonViXuatLe;
                thuoc.DonViThuNguyen = CurrentThuoc.DonViThuNguyen;
                ViewBag.QuyCach = thuoc.QuyCach;
                ViewBag.GiaNhap = CurrentThuoc.GiaNhap;
                ViewBag.GiaXuat = CurrentThuoc.GiaXuat;
                ViewBag.GiaSi = 0;
                ViewBag.SoLuong = 0;
                ViewBag.GioiHan = "";
                ViewBag.HanDung = "";
                ViewBag.GiaNhapDonGanNhat = string.Empty;
                ViewBag.GiaNhapXuatGanNhat = string.Empty;
            }
            return thuoc;
        }
        private void GetThongTinNhapXuatGanNhat(int thuocId)
        {

        }
        private decimal GetGiaNhapHienTai(Thuoc th)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var phieuNhapCuoiCung =
                unitOfWork.PhieuNhapChiTietRepository.GetMany(
                    e => e.PhieuNhap.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.Thuoc.ThuocId == th.ThuocId)
                    .OrderByDescending(e => e.PhieuNhap.NgayNhap)
                    .FirstOrDefault();
            if (phieuNhapCuoiCung != null)
            {
                // neu nhap = dvtn thi tinh gia sang dvxl
                return th.DonViThuNguyen != null && phieuNhapCuoiCung.DonViTinh.MaDonViTinh == th.DonViThuNguyen.MaDonViTinh ?
                    phieuNhapCuoiCung.GiaNhap / th.HeSo : phieuNhapCuoiCung.GiaNhap;
            }
            var thuoc = unitOfWork.ThuocRepository.GetById(th.ThuocId);
            if (thuoc != null)
                return thuoc.GiaNhap;
            return 0;
        }

        private decimal GetGiaXuatHienTai(Thuoc th)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var phieuXuatCuoiCung =
                unitOfWork.PhieuXuatChiTietRepository.GetMany(
                    e => e.PhieuXuat.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.Thuoc.ThuocId == th.ThuocId)
                    .OrderByDescending(e => e.PhieuXuat.NgayXuat)
                    .FirstOrDefault();
            if (phieuXuatCuoiCung != null)
            {
                return th.DonViThuNguyen != null && phieuXuatCuoiCung.DonViTinh.MaDonViTinh == th.DonViThuNguyen.MaDonViTinh ?
                   phieuXuatCuoiCung.GiaXuat / th.HeSo : phieuXuatCuoiCung.GiaXuat;
            }
            var thuoc = unitOfWork.ThuocRepository.GetById(th.ThuocId);
            if (thuoc != null)
                return thuoc.GiaNhap;
            return 0;
        }

        private decimal GetSoLuongHienTai(Thuoc th)
        {
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

        public JsonResult GetThuocBienDongTrongNgay(int id, string date)
        {
            var listTmp = new List<PhieuKiemKeChiTiet>();
            var manhathuoc = this.GetNhaThuoc().MaNhaThuoc;
            List<PhieuKiemKeChiTiet> list = unitOfWork.PhieuKiemKeChiTietRepository.Get(c => c.PhieuKiemKe.MaPhieuKiemKe == id).ToList();
            var tmpDt = date.Trim().Split(' ');
            var arr = tmpDt[0].Split('/');
            var dt = new DateTime(int.Parse(arr[2]), int.Parse(arr[1]), int.Parse(arr[0])).Date;
            var listFromXuat = unitOfWork.PhieuXuatChiTietRepository.Get(c => c.PhieuXuat.NhaThuoc.MaNhaThuoc == manhathuoc && DbFunctions.TruncateTime(c.PhieuXuat.NgayXuat.Value) == dt).Select(f => f.Thuoc).ToList();
            var listFromNhap = unitOfWork.PhieuNhapChiTietRepository.Get(c => c.PhieuNhap.NhaThuoc.MaNhaThuoc == manhathuoc && DbFunctions.TruncateTime(c.PhieuNhap.NgayNhap.Value) == dt).Select(f => f.Thuoc).ToList();
            listFromNhap.AddRange(listFromXuat);
            listFromNhap = listFromNhap.Distinct().ToList();
            foreach (var item in listFromNhap)
            {
                var tmp = list.Where(c => c.Thuoc.ThuocId == item.ThuocId).FirstOrDefault();
                if (tmp != null)
                {
                    listTmp.Add(tmp);
                }
            }

            var results = listTmp.Select(x => new
            {
                MaThuoc = x.Thuoc.MaThuoc
            });

            return new JsonResult() { Data = results, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        //[Authorize]
        [SimpleAuthorize("Admin")]
        public ActionResult SearchDrugCatalog()
        {
            return View();
        }

        public JsonResult SearchDrugCatalogByName(string keyWord, int pageIndex, int pageSize)
        {
            var tblDrugMasterSearched = unitOfWork.ThuocRepository.Get(e =>
                        e.NhaThuoc.MaNhaThuoc == Constants.MaNhaThuocMapping && e.RecordStatusID == (byte)RecordStatus.Activated && (string.IsNullOrEmpty(keyWord) || e.TenThuoc.ToLower().Contains(keyWord.ToLower())));
            
            var totalSize = tblDrugMasterSearched.Count();
            tblDrugMasterSearched = tblDrugMasterSearched.Skip(pageIndex * pageSize).Take(pageSize);
            var lstDrugSearched = tblDrugMasterSearched.Select(e => e.ThuocId).ToList();
            var tblMin = GetMinPriceOfDrugs(lstDrugSearched);
            var tableJoin = from tblThuoc in tblDrugMasterSearched
                            join tableMap in tblMin
                                on tblThuoc.ThuocId equals tableMap.DrugID into tbljoin
                            from tableMap in tbljoin.DefaultIfEmpty()
                            select new
                            {
                                Id = tblThuoc.ThuocId,
                                TenDayDu = tblThuoc.TenDayDu,
                                DonViXuatLe = tblThuoc.DonViXuatLe.TenDonViTinh,
                                GiaNhapLe = string.Format("{0:#,###}", tableMap != null? (tblThuoc.HeSo > 0? tblThuoc.HeSo*tableMap.InPrice: tableMap.InPrice) : tblThuoc.GiaNhap * (tblThuoc.HeSo > 0 ? tblThuoc.HeSo : 1)),
                                GiaNhapNgayCapNhat = tableMap != null && tableMap.InLastUpdateDate != null && tableMap.InLastUpdateDate.HasValue? tableMap.InLastUpdateDate.Value.ToString("dd/MM/yyyy"): (tblThuoc.Created.HasValue? tblThuoc.Created.Value.ToString("dd/MM/yyyy"):""),
                                GiaBanLe = string.Format("{0:#,###}", tableMap != null ? (tblThuoc.HeSo > 0 ? tblThuoc.HeSo * tableMap.OutPrice: tableMap.OutPrice) : tblThuoc.GiaBanLe * (tblThuoc.HeSo > 0 ? tblThuoc.HeSo : 1)),
                                GiaBanNgayCapNhat = tableMap != null && tableMap.OutLastUpdateDate != null && tableMap.OutLastUpdateDate.HasValue ? tableMap.OutLastUpdateDate.Value.ToString("dd/MM/yyyy") : (tblThuoc.Created.HasValue ? tblThuoc.Created.Value.ToString("dd/MM/yyyy") : ""),
                                QuyCach = tblThuoc.QuyCach,
                                DonViNguyen = (tblThuoc.DonViThuNguyen != null && tblThuoc.DonViThuNguyen.MaDonViTinh != tblThuoc.DonViXuatLe.MaDonViTinh? tblThuoc.DonViThuNguyen.TenDonViTinh: tblThuoc.DonViXuatLe.TenDonViTinh)
                            };
            var results_temp = tableJoin.AsEnumerable().OrderBy(a => a.TenDayDu).Select((e,index)=> new
            {
                e.Id,
                Order= index + (pageIndex * pageSize) + 1,
                e.TenDayDu,
                e.GiaNhapLe,
                GiaNhapNgayCapNhat = string.IsNullOrEmpty(e.GiaNhapLe) && string.IsNullOrEmpty(e.GiaBanLe)?"": e.GiaNhapNgayCapNhat,
                e.GiaBanLe,
                GiaBanNgayCapNhat = string.IsNullOrEmpty(e.GiaNhapLe) && string.IsNullOrEmpty(e.GiaBanLe) ? "" : e.GiaBanNgayCapNhat,
                e.QuyCach,
                e.DonViNguyen
            }).ToList();
            var results = new
            {
                results = results_temp,
                totalSize = totalSize
            };
            return new JsonResult() { Data = results, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public List<DrugPriceModel> GetMinPriceOfDrugs(List<int> lstID)
        {
            var drugService = IoC.Container.Resolve<IDrugManagementService>();
            return drugService.GetLastestDrugMinPrices(lstID.ToArray());
        }

        [Authorize]
        public ActionResult MappingCatalog()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            if (maNhaThuoc != "0012")
                return RedirectToAction("CheckAuthorization","TienIch");
            return View();
        }
        /*
         * 0 - getAll; 1 - chi lay nhung thuoc chưa được mapping
         */
        public JsonResult GetDrugByStoreDrugCode(string storeDrugCode, string drugName, int getAll, int pageIndex, int pageSize)
        {
            var nhaThuoc = unitOfWork.NhaThuocRepository.GetMany(e=>e.MaNhaThuoc == storeDrugCode).FirstOrDefault();
            if (nhaThuoc == null)
                return null;
            int count = pageIndex * pageSize + 1;
            var tblThuocOfStoreDrug = unitOfWork.ThuocRepository.GetMany(e => (string.IsNullOrEmpty(drugName.Trim()) || e.TenThuoc.ToLower().Contains(drugName.ToLower().Trim())) &&
                (e.NhaThuoc.MaNhaThuoc == nhaThuoc.MaNhaThuoc || e.NhaThuoc.MaNhaThuoc == nhaThuoc.MaNhaThuocCha) && e.RecordStatusID == (byte)RecordStatus.Activated);
            //Nếu chỉ lấy thuốc chưa được mapping
            if (getAll != 0)
            {
                var tableMapping = unitOfWork.ThuocMappingRepository.GetAll().Where(e => e.DrugStoreID == nhaThuoc.MaNhaThuoc).Select(e => e.SlaveDrugID);
                tblThuocOfStoreDrug = tblThuocOfStoreDrug.Where(e => !tableMapping.Contains(e.ThuocId));
            }
            int totalSize = tblThuocOfStoreDrug.Count();
            //Hạn chế số phần tử đem đi join
            tblThuocOfStoreDrug = tblThuocOfStoreDrug.OrderBy(e => e.TenThuoc).Skip(pageSize * pageIndex).Take(pageSize);
            var query = from _tblThuoc in tblThuocOfStoreDrug
                        select new
                        {
                            SlaveDrugID = _tblThuoc.ThuocId,
                            sTenThuoc = "",
                            sQuyCach = "",
                            sCode = "",
                            MasterDrugID = -1,
                            mTenThuoc = "",
                            mQuyCach = ""
                        };
            //Lấy tất cả
            if (getAll == 0)
            {
                var lstThuocIDAvai = query.Select(q => q.SlaveDrugID).ToList();
                //Lấy thông tin của Master
                var qtblMappingGetInfoMaster = from _tblMapping in unitOfWork.ThuocMappingRepository.GetMany(e => lstThuocIDAvai.Contains(e.SlaveDrugID))
                                               join _tblThuoc in unitOfWork.ThuocRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == Constants.MaNhaThuocMapping)
                                               on _tblMapping.MasterDrugID equals _tblThuoc.ThuocId
                                               select new
                                               {
                                                   _tblMapping.MasterDrugID,
                                                   _tblMapping.SlaveDrugID,
                                                   mTenThuoc = _tblThuoc.TenThuoc,
                                                   mQuyCach = _tblThuoc.DonViThuNguyen != null && _tblThuoc.DonViThuNguyen.MaDonViTinh != _tblThuoc.DonViXuatLe.MaDonViTinh ?
                                                    _tblThuoc.DonViThuNguyen.TenDonViTinh + " " + _tblThuoc.HeSo + " " + _tblThuoc.DonViXuatLe.TenDonViTinh : _tblThuoc.DonViXuatLe.TenDonViTinh
                                               };
                //Map thông tin của Master vào thông tin thuốc
                query = (from _tblThuoc in unitOfWork.ThuocRepository.GetMany(e => lstThuocIDAvai.Contains(e.ThuocId))
                         join _tblMapping in qtblMappingGetInfoMaster
                            on _tblThuoc.ThuocId equals _tblMapping.SlaveDrugID into rMap
                            from _tblMapping in rMap.DefaultIfEmpty()
                        select new
                            {
                                SlaveDrugID = _tblThuoc.ThuocId,
                                sTenThuoc = _tblThuoc.TenThuoc,
                                sQuyCach = _tblThuoc.DonViThuNguyen != null && _tblThuoc.DonViThuNguyen.MaDonViTinh != _tblThuoc.DonViXuatLe.MaDonViTinh ?
                                    _tblThuoc.DonViThuNguyen.TenDonViTinh + " " + _tblThuoc.HeSo + " " + _tblThuoc.DonViXuatLe.TenDonViTinh 
                                    : _tblThuoc.DonViXuatLe.TenDonViTinh,
                                sCode = _tblThuoc.MaThuoc,
                                MasterDrugID = _tblMapping == null?-1: _tblMapping.MasterDrugID,
                                mTenThuoc = _tblMapping == null ? "" : _tblMapping.mTenThuoc,
                                mQuyCach = _tblMapping == null ? "" : _tblMapping.mQuyCach
                            });
            }
            else
            {
                query = from _tblThuoc in tblThuocOfStoreDrug
                        select new
                        {
                            SlaveDrugID = _tblThuoc.ThuocId,
                            sTenThuoc = _tblThuoc.TenThuoc,
                            sQuyCach = _tblThuoc.DonViThuNguyen != null && _tblThuoc.DonViThuNguyen.MaDonViTinh != _tblThuoc.DonViXuatLe.MaDonViTinh ?
                                _tblThuoc.DonViThuNguyen.TenDonViTinh + " " + _tblThuoc.HeSo + " " + _tblThuoc.DonViXuatLe.TenDonViTinh : _tblThuoc.DonViXuatLe.TenDonViTinh,
                            sCode = _tblThuoc.MaThuoc,
                            MasterDrugID = -1,
                            mTenThuoc = "",
                            mQuyCach = ""
                        };
            }
            var results_temp = query.ToList()
                    .Select(e => new
                    {
                        Id = e.SlaveDrugID,
                        Order = count++,
                        TenDayDu = e.sTenThuoc,
                        Code = e.sCode,
                        QuyCach = e.sQuyCach,
                        ThuocRefId = e.MasterDrugID,
                        ThuocRefTen = e.mTenThuoc,
                        ThuocRefQuyCach = e.mQuyCach
                    });
            var results = new
            {
                Data = results_temp,
                TotalSize = totalSize
            };
            return new JsonResult() { Data = results, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public JsonResult MappingDrugCatalog(int id, int idRef, string drugStoreCode)
        {
            Thuoc drugSlave = unitOfWork.ThuocRepository.GetById(id);
            Thuoc drugRefMaster = unitOfWork.ThuocRepository.GetById(idRef);
            if(!checkDrugOkMapping(drugRefMaster, drugSlave))
            {
                return new JsonResult() { Data = "khong_trung_don_vi_tinh", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            DrugMapping drugMapping = unitOfWork.ThuocMappingRepository.GetMany(x => x.SlaveDrugID == id && x.DrugStoreID == drugStoreCode).FirstOrDefault();
            if (drugMapping == null)
            {
                drugMapping = new DrugMapping();
                drugMapping.MasterDrugID = drugRefMaster.ThuocId;
                drugMapping.SlaveDrugID = drugSlave.ThuocId;
                drugMapping.InPrice = 0;
                drugMapping.InLastUpdateDate = DateTime.Now.AddDays(-1);
                drugMapping.DrugStoreID = drugStoreCode;
                drugMapping.OutLastUpdateDate = DateTime.Now.AddDays(-1);
                drugMapping.OutPrice = 0;
                //Mã đơn vị tính theo thuốc mapping
                drugMapping.UnitID = drugRefMaster.DonViXuatLe.MaDonViTinh;
                unitOfWork.ThuocMappingRepository.Insert(drugMapping);
            }
            else
            {
                drugMapping.MasterDrugID = drugRefMaster.ThuocId;
                unitOfWork.ThuocMappingRepository.Update(drugMapping);
            }
            unitOfWork.Save();
            return new JsonResult() { Data = drugMapping.ID, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }
        private bool checkDrugOkMapping(Thuoc drugRefMaster, Thuoc drugSlave)
        {
            bool result = false;
            
            if (checkNameDrug(drugRefMaster.DonViXuatLe.TenDonViTinh, drugSlave.DonViXuatLe.TenDonViTinh))
            {
                    //1. Kiểm tra đơn vị  lẻ của master và đơn vị lẻ của slave
                result = true;
            }
            else
            {                
                if (drugRefMaster.DonViThuNguyen != null)
                {
                    //2. Kiểm tra đơn vị nguyên của master và đơn vị lẻ của salve
                    if (checkNameDrug(drugRefMaster.DonViThuNguyen.TenDonViTinh, drugSlave.DonViXuatLe.TenDonViTinh))
                        result = true;
                    else if (drugSlave.DonViThuNguyen != null)
                    {
                    //3. Kiểm tra đơn vị nguyên của master và đơn vị nguyên của salve
                        if (checkNameDrug(drugRefMaster.DonViThuNguyen.TenDonViTinh, drugSlave.DonViThuNguyen.TenDonViTinh))
                            result = true;
                        else result = false;
                    }
                    else result = false;

                }
                else
                    result = false;
               
            }           
            return result;
        }
        public bool checkNameDrug(string nameMaster, string nameSlave)
        {
            nameMaster = nameMaster.ToLower();
            nameSlave = nameSlave.ToLower();
            if (nameMaster == nameSlave)
                return true;
            else
            {
                switch (nameMaster)
                {
                    case "tuýp":
                        if (nameSlave == "tube") return true;
                        else return false;
                    case "lọ":
                        if (nameSlave == "chai") return true;
                        else return false;
                    case "chai":
                        if (nameSlave == "lọ") return true;
                        else return false;
                    default:
                        return false;
                }
                   
            }         
                
        }
        //Xóa mapping
        public JsonResult ClearMappingDrugCatalog(int id, string drugStoreCode)
        {
            DrugMapping drugMapping = unitOfWork.ThuocMappingRepository.GetMany(x => x.SlaveDrugID == id && x.DrugStoreID == drugStoreCode).FirstOrDefault();
            if (drugMapping != null)
            {
                unitOfWork.ThuocMappingRepository.Delete(drugMapping.ID);
                unitOfWork.Save();
                return new JsonResult() { Data = drugMapping.ID, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            else
                return new JsonResult() { Data = "", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }




    /// <summary>
    /// Get thuoc in store model
    /// </summary>
    public class CurrentThuoc
    {
        public IEnumerable<int> MaPhieuNhapCt { get; set; }
        public int SoLuong { get; set; }
        public DateTime HanDung { get; set; }
        public string Message { get; set; }

    }

    public class ThuocsUtil
    {
        private readonly SecurityContext _db;
        private readonly string _maNhaThuoc;

        public ThuocsUtil(SecurityContext db, string maNhaThuoc)
        {
            _db = db;
            _maNhaThuoc = maNhaThuoc;
        }
        //public List<CurrentThuoc> GetThuocsLeft(int thuocId, int soLuong)
        //{
        //    var qry =
        //        _db.PhieuNhapChiTiets.Where(x => x.MaNhaThuoc == this.GetNhaThuoc().MaNhaThuoc && x.ThuocId == thuocId && !x.PhieuNhap.Xoa)
        //            .GroupBy(y => y.HanDung).OrderBy(x => x.Key).Select(z => new CurrentThuoc
        //            {
        //                HanDung = z.Key,
        //                MaPhieuNhapCt = z.Select(a => a.MaPhieuNhapCt),
        //                SoLuong = z.Sum(a => a.SoLuong)
        //            }).ToList();

        //    var currentTake = 0;
        //    var result = new List<CurrentThuoc>();
        //    foreach (var currentThuoc in qry)
        //    {

        //        var currentSoLuong = 0;
        //        var thuoc = currentThuoc;
        //        var thuocSold = _db.PhieuXuatChiTiets.Where(
        //            x => x.MaNhaThuoc == this.GetNhaThuoc().MaNhaThuoc && x.ThuocId == thuocId && !x.PhieuXuat.Xoa && x.HanDung == thuoc.HanDung).Select(x => x.SoLuong).DefaultIfEmpty(0).Sum();

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
        //        result[0].Message = "Thuốc trong kho thiếu " + (soLuong - currentTake) + " đơn vị";
        //    }
        //    return result;
        //}
    }

    /// <summary>
    /// Get thouc by nhom model
    /// </summary>
    public class ThuocWithQuantity
    {
        public int ThuocId { get; set; }
        public string TenNhomThuoc { get; set; }
        public string MaThuoc { get; set; }
        public string TenThuoc { get; set; }
        public decimal SoLuong { get; set; }
        public decimal Gia { get; set; }
        public string DonViXuatLe { get; set; }
        public string MaVach { get; set; }
        public string MaPhieuDaTonTai { get; set; }
    }
}

