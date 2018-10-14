using App.Common.Http;
using App.Common.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Med.Service.Drug;
using Med.Service.Report;
using App.Common.Validation;
using System.Net;
using System.Web.Mvc;
using App.Common.DI;
using Med.Web.Data.Session;
using Med.Web.Extensions;
using Med.ServiceModel.Drug;
using Med.ServiceModel.Response;
using App.Common.Extensions;
using System.Data;
using Microsoft.Reporting.WebForms;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using Med.Web.Filter;
using Med.Common;

namespace Med.Web.Areas.Production.Controllers
{
    public class DrugManagementController : BaseController
    {
        [HttpPost]
        // [Audit]
        public JsonResult UpdateDrugPrice(string drugCode, string drugStoreCode, double inPrice, double outPriceL, double outPriceB, int unitCode)
        {
            var response = new ResponseData<string>();
            try
            {
                var service = IoC.Container.Resolve<IDrugManagementService>();

                service.UpdateDrugPrice(drugCode, WebSessionManager.Instance.CurrentDrugStoreCode, inPrice,outPriceL,outPriceB, unitCode, null);

            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }

            return Json(response);
        }

        [HttpPost]
        // [Audit]
        public JsonResult UpdateOutDrugPrice(string drugCode, double price, int unitCode)
        {
            var response = new ResponseData<string>();
            try
            {
                var service = IoC.Container.Resolve<IDrugManagementService>();

                service.UpdateOutDrugPrice(drugCode, WebSessionManager.Instance.CurrentDrugStoreCode, price, unitCode);

            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }

            return Json(response);
        }

        [HttpPost]
        // [Audit]
        public JsonResult UpdateExpiredDateDrug(int noteItemId, string batchNumber, DateTime? expiredDate)
        {           
            var response = new ResponseData<string>();
            try
            {
                var service = IoC.Container.Resolve<IDrugManagementService>();

                service.UpdateExpiredDateDrug(noteItemId, batchNumber, expiredDate, MedSessionManager.CurrentDrugStoreCode, MedSessionManager.DSSession.Settings);

            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }

            return Json(response);
        }
        

        [HttpPost]
        public JsonResult GetDrugInfo(string drugCode, int drugUnit)
        {
            var response = new ResponseData<DrugInfo>();
            try
            {
                var service = IoC.Container.Resolve<IDrugManagementService>();
                var drugStoreCode = WebSessionManager.Instance.CurrentDrugStoreCode;
                var drugInfo = service.GetDrugInfo(drugCode, drugStoreCode, drugUnit);
                response.SetData(drugInfo);

            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }

            return Json(response);
        }

        [HttpGet]
        // [Audit]
        public ActionResult CreateReserve()
        {
            return View("~/Areas/Production/Views/Drug/CreateReserve.cshtml");
        }

        [HttpPost]
        public JsonResult InitCreateReserve(int type, int provider, int group_drug, string name_drug, bool get_drug_empty)
        {
            CreateReserveResponse oRes = new CreateReserveResponse()
            {
                Total = 0
            };
            IResponseData<CreateReserveResponse> response = new ResponseData<CreateReserveResponse>();
            try
            {
                var service = IoC.Container.Resolve<IDrugManagementService>();
                var data = service.GetListDrugForCreateReserve(WebSessionManager.Instance.CurrentDrugStoreCode, type, provider, group_drug, name_drug, get_drug_empty);
                oRes.PagingResultModel = new PagingResultModel<CreateReserveItem>(data, data.Count);
                response.SetData(oRes);
            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }
            return Json(response);
        }
        [HttpPost]
        public JsonResult GetListProvider()
        {
            IResponseData<List<ProviderInfo>> response = new ResponseData<List<ProviderInfo>>();
            try
            {
                var service = IoC.Container.Resolve<IDrugManagementService>();
                var data = service.GetListProvider(WebSessionManager.Instance.CurrentDrugStoreCode);
                response.SetData(data);
            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }
            return Json(response);
        }
        [HttpPost]
        public JsonResult GetListGroupDrug()
        {
            IResponseData<List<GroupDrugInfo>> response = new ResponseData<List<GroupDrugInfo>>();
            try
            {
                var service = IoC.Container.Resolve<IDrugManagementService>();
                var data = service.GetListGroupDrug(WebSessionManager.Instance.CurrentDrugStoreCode);
                response.SetData(data);
            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }
            return Json(response);
        }

        [HttpPost]
        public string PrintInventoryWarning(List<CreateReserveItem> inputItem)
        {
            if (inputItem != null)
            {
                var nhathuoc = this.GetNhaThuoc();
                DataTable dt = new DataTable();
                dt.Columns.Add("STT");
                dt.Columns.Add("TenHang");
                dt.Columns.Add("DVT");
                dt.Columns.Add("SoLuong");
                dt.Columns.Add("SoLuongThuc");
                dt.Columns.Add("DuTru");
                dt.Columns.Add("DonGia");
                int i = 1;
                foreach (var item in inputItem)
                {
                    DataRow dr = dt.NewRow();
                    dr["STT"] = i;
                    dr["TenHang"] = item.TenThuoc;
                    dr["DVT"] = item.DonViLe;
                    dr["SoLuong"] = item.SoLuongCanhBao;
                    dr["SoLuongThuc"] = item.TonKho;
                    dr["DuTru"] = item.DuTru;
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
                viewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RptPhieuDSInventoryWarning.rdlc");

                viewer.LocalReport.DataSources.Add(new ReportDataSource("DSPhieuXuat", dt));
                viewer.LocalReport.SetParameters(listParam);
                byte[] bytes = viewer.LocalReport.Render("PDF");
                Stream stream = new MemoryStream(bytes);
                string url = HttpContext.Server.MapPath("../Uploads");
                string sfileName = DateTime.Now.ToString("yyyMMddHHmmss") + "_" + Guid.NewGuid() + ".pdf";
                using (var fileStream = new FileStream(url + @"\" + sfileName, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(fileStream);
                }
                return "/Uploads/" + sfileName;
            }
            else
            {
                return "";
            }
        }

        [HttpPost]
        public string ExportExcelInventoryWarning(List<CreateReserveItem> inputItem)
        {
            if (inputItem != null)
            {
                var thuocs = new DataTable("Thuốc");
                thuocs.Columns.Add("Mã Thuốc", typeof(string));
                thuocs.Columns.Add("Tên Thuốc", typeof(string));
                thuocs.Columns.Add("Tên nhóm Thuốc", typeof(string));
                thuocs.Columns.Add("Đơn vị", typeof(string));
                thuocs.Columns.Add("Số lượng cảnh báo", typeof(string));
                thuocs.Columns.Add("Tồn kho", typeof(string));
                //thuocs.Columns.Add("Dự trù", typeof(string));
                //thuocs.Columns.Add("Đơn giá", typeof(string));
                //thuocs.Columns.Add("Thành tiền", typeof(string));

                var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
                var thuocList = inputItem;
                //Add to rows
                foreach (var item in thuocList)
                {
                    DataRow dr = thuocs.NewRow();
                    dr["Mã Thuốc"] = item.MaThuoc;
                    dr["Tên Thuốc"] = item.TenThuoc;
                    dr["Tên nhóm Thuốc"] = item.TenNhomThuoc;
                    dr["Đơn vị"] = item.DonViLe;
                    dr["Số lượng cảnh báo"] = item.SoLuongCanhBao.ToString("#,##0");
                    dr["Tồn kho"] = item.TonKho.ToString("#,##0");
                    //dr["Dự trù"] = item.DuTru;
                    //dr["Đơn giá"] = item.DonGia.ToString("#,##0");
                    //if (!string.IsNullOrEmpty(item.DuTru))
                    //{
                    //    decimal dutru = decimal.Parse("0" + item.DuTru.Trim().Replace(",", ""));
                    //    item.ThanhTien = (dutru * item.DonGia).ToString("#,##0");
                    //}
                    //dr["Thành tiền"] = item.ThanhTien;
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
                    var fileStream1 = new MemoryStream();
                    pck.SaveAs(fileStream1);
                    fileStream1.Position = 0;
                    string url = HttpContext.Server.MapPath("../Uploads");
                    string sfileName = DateTime.Now.ToString("yyyMMddHHmmss") + "_" + Guid.NewGuid() + ".xlsx";
                    using (var fileStream = new FileStream(url + @"\" + sfileName, FileMode.Create, FileAccess.Write))
                    {
                        fileStream1.CopyTo(fileStream);
                    }
                    return "/Uploads/" + sfileName;
                }
            }
            else
            {
                return "";
            }
        }

        [HttpGet]
        public ActionResult InventoryWarning()
        {
            return View("~/Areas/Production/Views/Drug/InventoryWarning.cshtml");
        }

        [HttpPost]
        public JsonResult InitInventoryWarning(int type, int provider, int group_drug, string name_drug, bool get_drug_empty)
        {
            CreateReserveResponse oRes = new CreateReserveResponse()
            {
                Total = 0
            };
            IResponseData<CreateReserveResponse> response = new ResponseData<CreateReserveResponse>();
            try
            {
                var service = IoC.Container.Resolve<IDrugManagementService>();
                var data = service.GetListDrugForCreateReserve(WebSessionManager.Instance.CurrentDrugStoreCode, type,
                    provider, group_drug, name_drug, false);
                IEnumerable<CreateReserveItem> returnData;

                returnData = get_drug_empty ? data.Where(x => x.TonKho < 0) : data.Where(x => x.TonKho <= x.SoLuongCanhBao);

                oRes.PagingResultModel = new PagingResultModel<CreateReserveItem>(returnData, returnData.Count());
                response.SetData(oRes);
            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }
            return Json(response);
        }
        [HttpPost]
        public string PrintReserve(List<CreateReserveItem> inputItem)
        {
            if (inputItem != null)
            {
                var nhathuoc = this.GetNhaThuoc();
                DataTable dt = new DataTable();
                dt.Columns.Add("STT");
                dt.Columns.Add("MaThuoc");
                dt.Columns.Add("TenHang");
                dt.Columns.Add("TenNhom");
                dt.Columns.Add("DVNguyen");
                //dt.Columns.Add("SoLuong");
                //dt.Columns.Add("SoLuongThuc");
                dt.Columns.Add("DuTru");
                //dt.Columns.Add("DVNguyen");
                //dt.Columns.Add("DonGia");
                int i = 1;
                foreach (var item in inputItem)
                {
                    DataRow dr = dt.NewRow();
                    dr["STT"] = i;
                    dr["MaThuoc"] = item.MaThuoc;
                    dr["TenHang"] = item.TenThuoc;
                    dr["TenNhom"] = item.TenNhomThuoc;
                    dr["DVNguyen"] = item.DonViNguyen;
                    //dr["SoLuong"] = item.SoLuongCanhBao;
                    //dr["SoLuongThuc"] = item.TonKho;
                    dr["DuTru"] = item.DuTru;
                    //dr["DVNguyen"] = item.DonViNguyen;
                    //dr["DonGia"] = item.DonGia.ToString("#,##0");
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
                string url = HttpContext.Server.MapPath("../Uploads");
                string sfileName = DateTime.Now.ToString("yyyMMddHHmmss") + "_" + Guid.NewGuid() + ".pdf";
                using (var fileStream = new FileStream(url + @"\" + sfileName, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(fileStream);
                }
                return "/Uploads/" + sfileName;
            }
            else
            {
                return "";
            }
        }
        [HttpPost]
        public string ExportExcelReserve(List<CreateReserveItem> inputItem)
        {
            if (inputItem != null)
            {
                var thuocs = new DataTable("Thuốc");
                thuocs.Columns.Add("Mã Thuốc", typeof(string));
                thuocs.Columns.Add("Tên Thuốc", typeof(string));
                thuocs.Columns.Add("Nhóm Thuốc", typeof(string));
                thuocs.Columns.Add("Đơn vị tồn", typeof(string));
                thuocs.Columns.Add("Số lượng cảnh báo", typeof(string));
                thuocs.Columns.Add("Tồn kho", typeof(string));
                thuocs.Columns.Add("Dự trù", typeof(string));
                thuocs.Columns.Add("Đơn vị dự trù", typeof(string));
                thuocs.Columns.Add("Đơn giá", typeof(string));
                thuocs.Columns.Add("Thành tiền", typeof(string));

                var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
                var thuocList = inputItem;
                //Add to rows
                foreach (var item in thuocList)
                {
                    DataRow dr = thuocs.NewRow();
                    dr["Mã Thuốc"] = item.MaThuoc;
                    dr["Tên Thuốc"] = item.TenThuoc;
                    dr["Nhóm Thuốc"] = item.TenNhomThuoc;
                    dr["Đơn vị tồn"] = item.DonViNguyen;
                    dr["Số lượng cảnh báo"] = item.SoLuongCanhBao.ToString("#,##0");
                    dr["Tồn kho"] = item.TonKho.ToString("#,##0");
                    dr["Dự trù"] = item.DuTru;
                    dr["Đơn vị dự trù"] = item.DonViNguyen;
                    dr["Đơn giá"] = item.DonGia.ToString("#,##0");
                    if (!string.IsNullOrEmpty(item.DuTru))
                    {
                        decimal dutru = decimal.Parse("0" + item.DuTru.Trim().Replace(",",""));
                        item.ThanhTien = (dutru * item.DonGia).ToString("#,##0");
                    }
                    dr["Thành tiền"] = item.ThanhTien;
                    thuocs.Rows.Add(dr);
                }

                using (var pck = new ExcelPackage())
                {
                    //Create the worksheet
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Thuốc");

                    //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                    ws.Cells["A1"].LoadFromDataTable(thuocs, true);

                    //Format the header for column 1-3
                    using (ExcelRange rng = ws.Cells["A1:J1"])
                    {
                        rng.Style.Font.Bold = true;
                        rng.Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                        rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                        rng.Style.Font.Color.SetColor(Color.White);
                    }
                    var fileStream1 = new MemoryStream();
                    pck.SaveAs(fileStream1);
                    fileStream1.Position = 0;
                    string url = HttpContext.Server.MapPath("../Uploads");
                    string sfileName = DateTime.Now.ToString("yyyMMddHHmmss") + "_" + Guid.NewGuid() + ".xlsx";
                    using (var fileStream = new FileStream(url + @"\" + sfileName, FileMode.Create, FileAccess.Write))
                    {
                        fileStream1.CopyTo(fileStream);
                    }
                    return "/Uploads/" + sfileName;
                }
            }
            else
            {
                return "";
            }
        }
    }
}