using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using App.Common.DI;
using App.Common.Http;
using App.Common.MVC.Attributes;
using Med.Service.Common;
using Med.Web.Data.Session;
using Med.Web.Extensions;
using Newtonsoft.Json;
using sThuoc.DAL;
using App.Common.Validation;
using sThuoc.Filter;
using Med.ServiceModel;
using Med.ServiceModel.Inventory;
using System.Globalization;
using Med.ServiceModel.Drug;
using Med.ServiceModel.Response;
using Med.Service.Drug;
using App.Common.Extensions;
using System.Data;
using Microsoft.Reporting.WebForms;
using System.IO;

namespace Med.Web.Areas.Production.Controllers
{
    public class InventoryController : Controller
    {
        // GET: Inventory/Index
        [SimpleAuthorize("Admin")]
        public ActionResult Index(string searchTen, string fromDate, string toDate)
        {
            //DateTime? fromDateConverted = null;
            //DateTime? toDateConverted = null;

            //// Parse tham số fromDate và toDate từ String -> Datetime
            //if (!string.IsNullOrEmpty(fromDate))
            //{
            //    fromDateConverted = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            //}

            //if (!string.IsNullOrEmpty(toDate))
            //{
            //    toDateConverted = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).AddDays(1);
            //}

            //// set vào ViewBag để truyền tham số sang View Cshtml
            //ViewBag.searchTen = searchTen;
            //ViewBag.fromDate = fromDate;
            //ViewBag.toDate = toDate;

            //// gọi hàm GetInventoryList lấy toàn bộ danh sách phiếu Kiểm Kê.
            //var service = IoC.Container.Resolve<IInventoryAdjustmentService>();
            //ViewBag.ViewModel = JsonConvert.SerializeObject(
            //    service.GetInventoryList(MedSessionManager.CurrentDrugStoreCode, searchTen, fromDateConverted, toDateConverted),
            //    Formatting.Indented);

            return View("~/Areas/Production/Views/Inventory/Index.cshtml");
        }

        [HttpPost]
        [AuthorizedRequest]
        public JsonResult GetInventoryList(string searchTen, string fromDate, string toDate)
        {
            IResponseData<List<InventoryDetailModel>> response = new ResponseData<List<InventoryDetailModel>>();
            try
            {
                DateTime? fromDateConverted = null;
                DateTime? toDateConverted = null;

                // Parse tham số fromDate và toDate từ String -> Datetime
                if (!string.IsNullOrEmpty(fromDate))
                {
                    fromDateConverted = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(toDate))
                {
                    toDateConverted = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).AddDays(1);
                }
                var service = IoC.Container.Resolve<IInventoryAdjustmentService>();
                var data = service.GetInventoryList(MedSessionManager.CurrentDrugStoreCode, searchTen, fromDateConverted, toDateConverted);
                response.SetData(data);
            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }

            return Json(response);
        }


        // GET: Inventory/DrugsNotInventoried

        // Lấy danh sách những thuốc chưa được kiểm kê
        [SimpleAuthorize("Admin")]
        public ActionResult DrugsNotInventoried()
        {
            //DateTime? fromDateConverted = null;
            //DateTime? toDateConverted = null;

            //// Parse tham số fromDate và toDate từ String -> Datetime
            //if (!string.IsNullOrEmpty(fromDate))
            //{
            //    fromDateConverted = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            //}

            //if (!string.IsNullOrEmpty(toDate))
            //{
            //    toDateConverted = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).AddDays(1);
            //}
            //// set vào ViewBag để truyền tham số sang View Cshtml
            //ViewBag.fromDate = fromDate;
            //ViewBag.toDate = toDate;
            //var service = IoC.Container.Resolve<IInventoryAdjustmentService>();
            //// gọi hàm GetDrugsHaveNotInventoried lấy danh sách thuốc chưa được Kiểm Kê.
            //ViewBag.notInventoriedDrugs = JsonConvert.SerializeObject(
            //    service.GetDrugsHaveNotInventoried(MedSessionManager.CurrentDrugStoreCode, fromDateConverted, toDateConverted),
            //    Formatting.Indented);

            return View("~/Areas/Production/Views/Inventory/DrugsNotInventoried.cshtml");
        }

        [HttpPost]
        [AuthorizedRequest]
        public JsonResult GetDrugsHaveNotInventoried(string fromDate, string toDate)
        {
            IResponseData<List<ThuocModel>> response = new ResponseData<List<ThuocModel>>();
            try
            {
                DateTime? fromDateConverted = null;
                DateTime? toDateConverted = null;

                // Parse tham số fromDate và toDate từ String -> Datetime
                if (!string.IsNullOrEmpty(fromDate))
                {
                    fromDateConverted = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(toDate))
                {
                    toDateConverted = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).AddDays(1);
                }
                var service = IoC.Container.Resolve<IInventoryAdjustmentService>();
                var data = service.GetDrugsHaveNotInventoried(MedSessionManager.CurrentDrugStoreCode, fromDateConverted, toDateConverted);
                response.SetData(data);
            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }

            return Json(response);
        }

        // GET: Inventory/Create
        // Tạo phiếu Kiểm kê mới
        //[SimpleAuthorize("Admin")]
        //public ActionResult Create(int?[] drugIds)
        //{
        //    var service = IoC.Container.Resolve<IInventoryAdjustmentService>();

        //    var nhaThuoc = this.GetNhaThuoc();
        //    var maNhaThuoc = MedSessionManager.CurrentDrugStoreCode;
        //    var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;

        //    // set vào ViewBag để truyền tham số sang View Cshtml
        //    ViewBag.NhomThuoc = JsonConvert.SerializeObject(service.GetTenNhomThuoc(maNhaThuoc, maNhaThuocCha), Formatting.Indented);
        //    ViewBag.MedicineList = JsonConvert.SerializeObject(null);
        //    if (drugIds != null)
        //    {
        //        int maNhomThuoc = -1;
        //        string ngayTao = DateTime.Now.ToString("dd/MM/yyyy");
        //        ViewBag.MedicineList = JsonConvert.SerializeObject(
        //        service.GetDrugInfo(maNhaThuoc, maNhomThuoc, drugIds, ngayTao),
        //                Formatting.Indented);
        //    }

        //    return View("~/Areas/Production/Views/Inventory/Create.cshtml");
        //}

        // GET: Inventory/Create
        // Tạo phiếu Kiểm kê mới
        [SimpleAuthorize("Admin")]
        public ActionResult Create()
        {
            var service = IoC.Container.Resolve<IInventoryAdjustmentService>();

            var nhaThuoc = this.GetNhaThuoc();
            var maNhaThuoc = MedSessionManager.CurrentDrugStoreCode;
            var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;

            // set vào ViewBag để truyền tham số sang View Cshtml
            ViewBag.NhomThuoc = JsonConvert.SerializeObject(service.GetTenNhomThuoc(maNhaThuoc, maNhaThuocCha), Formatting.Indented);
            //ViewBag.MedicineList = JsonConvert.SerializeObject(null);
            

            return View("~/Areas/Production/Views/Inventory/Create.cshtml");
        }


        [HttpPost]
        [AuthorizedRequest]
        public JsonResult GetDrugInfo(int? maNhomThuoc, int?[] drugIds, string ngayTao, string barcode = "")
        {
            IResponseData<List<ThuocModel>> response = new ResponseData<List<ThuocModel>>();
            try
            {
                var service = IoC.Container.Resolve<IInventoryAdjustmentService>();
                var data = service.GetDrugInfo(MedSessionManager.CurrentDrugStoreCode, maNhomThuoc, drugIds, ngayTao, barcode);
                response.SetData(data);
            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }

            return Json(response);
        }

        // Lưu phiếu Kiêm kê (cả trường hợp tạo mới và chỉnh sửa)
        [HttpPost]
        [AuthorizedRequest]
        // [Audit]
        public JsonResult SaveInventory(InventoryDetailModel model)
        {
            IResponseData<int> response = new ResponseData<int>();
            try
            {
                var service = IoC.Container.Resolve<IInventoryAdjustmentService>();
                var nhaThuoc = this.GetNhaThuoc();
                var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
                int currentUserId = WebSessionManager.Instance.CurrentUserId;
                // khởi tạo biến data = -1, sau khi lưu thành công set data = Mã Phiếu Kiểm Kê
                int data = -1;
                data = service.SaveInventory(MedSessionManager.CurrentDrugStoreCode, maNhaThuocCha, currentUserId, model);
                response.SetData(data);
            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }

            return Json(response);
        }

        // GET: Inventory/Edit/id
        // Update phiếu Kiểm kê đã tạo
        [SimpleAuthorize("Admin")]
        public ActionResult Edit(int? id)
        {
            var nhaThuoc = this.GetNhaThuoc();
            var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var service = IoC.Container.Resolve<IInventoryAdjustmentService>();
            var inventoryDetailInfo = service.GetInventoryDetailInfo(MedSessionManager.CurrentDrugStoreCode, id);
            if (inventoryDetailInfo == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            if (inventoryDetailInfo.IsCompareStore)
            {
                ViewBag.Message = "Không thể sửa phiếu đã cân kho!\nChỉ có thể cập nhật Giá/Lô/Hạn dùng của thuốc ở màn hình Phiếu Kiểm kê chi tiết";
                return View("~/Views/Shared/Error.cshtml");
            }
            // gọi hàm GetInventoryDetailInfo lấy thông tin phiếu muốn update

            // set vào ViewBag để truyền tham số sang View Cshtml
            ViewBag.NhomThuoc = JsonConvert.SerializeObject(service.GetTenNhomThuoc(MedSessionManager.CurrentDrugStoreCode, maNhaThuocCha), Formatting.Indented);
            ViewBag.ViewModel = JsonConvert.SerializeObject(inventoryDetailInfo);


            return View("~/Areas/Production/Views/Inventory/Edit.cshtml");
        }

        // GET: Inventory/Details/id
        // Xem thông tin chi tiết của Phiếu đã tạo
        [SimpleAuthorize("Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var service = IoC.Container.Resolve<IInventoryAdjustmentService>();
            var inventoryDetailInfo = service.GetInventoryDetailInfo(MedSessionManager.CurrentDrugStoreCode, id);
            if (inventoryDetailInfo == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            // set vào ViewBag để truyền tham số sang View Cshtml
            ViewBag.ViewModel = JsonConvert.SerializeObject(inventoryDetailInfo);
            return View("~/Areas/Production/Views/Inventory/Details.cshtml");
        }

        // GET: Inventory/Delete/id
        // Màn hình xóa Phiếu đã tạo
        [SimpleAuthorize("Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var service = IoC.Container.Resolve<IInventoryAdjustmentService>();
            var inventoryDetailInfo = service.GetInventoryDetailInfo(MedSessionManager.CurrentDrugStoreCode, id);
            if (inventoryDetailInfo == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            // set vào ViewBag để truyền tham số sang View Cshtml
            ViewBag.ViewModel = JsonConvert.SerializeObject(inventoryDetailInfo);

            return View("~/Areas/Production/Views/Inventory/Delete.cshtml");
        }

        // Webservice dùng để Xóa phiếu trong Database
        [HttpPost]
        [AuthorizedRequest]
        // [Audit]
        public JsonResult DeleteInventory(int inventoryId)
        {
            IResponseData<bool> response = new ResponseData<bool>();
            try
            {
                var service = IoC.Container.Resolve<IInventoryAdjustmentService>();
                var data = service.DeleteInventory(MedSessionManager.CurrentDrugStoreCode, MedSessionManager.CurrentUserId, inventoryId);

                response.SetData(true);
            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }

            return Json(response);
        }

        //[SimpleAuthorize("Admin")]
        public ActionResult In(int? id)
        {
            var nhathuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhathuoc.MaNhaThuoc;
            var service = IoC.Container.Resolve<IInventoryAdjustmentService>();
            var inventory = service.GetInventoryDetailInfo(MedSessionManager.CurrentDrugStoreCode, id);
            if (inventory == null)
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

            if (inventory.IsCompareStore)
            {
                reportPath = "~/Reports/RptPhieuKiemKe_DaCan.rdlc";
                dt.Columns.Add("SoLuongChenhLech");
            }


            int i = 1;
            decimal tongtienhang = 0;
            decimal tienhang = 0;
            foreach (var item in inventory.MedicineList)
            {
                DataRow dr = dt.NewRow();
                dr["STT"] = i;
                dr["MaThuoc"] = item.MaThuoc;
                dr["TenNhom"] = item.MaNhomThuoc > 0 ? item.TenNhomThuoc : string.Empty;
                dr["TenHang"] = item.TenThuoc;
                dr["DVT"] = item.TenDonViTinh;
                dr["SoLuong"] = item.TonKho.ToString("#,##0");
                dr["SoLuongThuc"] = item.ThucTe.HasValue ? item.ThucTe.Value.ToString("#,##0") : string.Empty;
                if (inventory.IsCompareStore)
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
                new ReportParameter("pNgayXuat",inventory.CreateTime.ToString("dd/MM/yyyy")),
                new ReportParameter("pPhieuXuat",inventory.Id.ToString()),
                new ReportParameter("pNhanVien",inventory.FullName)
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

        [HttpPost]
        // [Audit]
        public JsonResult UpdateDrugSerialNoAndExpDate(InventoryEditModel inventoryEditModel)
        {
            var response = new ResponseData<string>();
            try
            {
                var service = IoC.Container.Resolve<IInventoryAdjustmentService>();

                service.UpdateDrugSerialNoAndExpDate(MedSessionManager.CurrentDrugStoreCode, inventoryEditModel);

            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }

            return Json(response);
        }
    }
}