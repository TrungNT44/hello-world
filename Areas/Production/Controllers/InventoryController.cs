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
        public ActionResult Index()
        {
            return View("~/Areas/Production/Views/Inventory/Index.cshtml");
        }

        [HttpPost]
        [AuthorizedRequest]
        public string GetInventoryList(int thuocId, DateTime? fromDate, DateTime? toDate)
        {
            IResponseData<InventoryResponseModel> response = new ResponseData<InventoryResponseModel>();
            try
            {
                var service = IoC.Container.Resolve<IInventoryAdjustmentService>();
                toDate = toDate != null ? toDate.Value.AddDays(1) : toDate;
                var data = service.GetInventoryList(MedSessionManager.CurrentDrugStoreCode, thuocId, fromDate, toDate);
                response.SetData(data);
            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }

            return JsonConvert.SerializeObject(response);
        }


        // GET: Inventory/DrugsNotInventoried

        // Lấy danh sách những thuốc chưa được kiểm kê
        [SimpleAuthorize("Admin")]
        public ActionResult DrugsNotInventoried()
        {
            return View("~/Areas/Production/Views/Inventory/DrugsNotInventoried.cshtml");
        }

        [HttpPost]
        [AuthorizedRequest]
        public string GetDrugsHaveNotInventoried(DateTime? fromDate, DateTime? toDate)
        {
            IResponseData<InventoryResponseModel> response = new ResponseData<InventoryResponseModel>();
            try
            {
                var service = IoC.Container.Resolve<IInventoryAdjustmentService>();
                toDate = toDate != null ? toDate.Value.AddDays(1) : toDate;
                var data = service.GetDrugsHaveNotInventoried(MedSessionManager.CurrentDrugStoreCode, fromDate, toDate);
                response.SetData(data);
            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }

            return JsonConvert.SerializeObject(response);
        }

        // GET: Inventory/Create
        // Tạo phiếu Kiểm kê mới
        [SimpleAuthorize("Admin")]
        public ActionResult Create()
        {
            var drugMngService = IoC.Container.Resolve<IDrugManagementService>();
            // set vào ViewBag để truyền tham số sang View Cshtml
            ViewBag.NhomThuoc = JsonConvert.SerializeObject(
                drugMngService.GetListGroupDrug(MedSessionManager.CurrentDrugStoreCode), Formatting.Indented);
            return View("~/Areas/Production/Views/Inventory/Create.cshtml");
        }

        [HttpPost]
        [AuthorizedRequest]
        public string GetDrugInfo(int? maNhomThuoc, int?[] drugIds, string ngayTao, string barcode = "")
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

            return JsonConvert.SerializeObject(response);
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
                // khởi tạo biến data = -1, sau khi lưu thành công set data = Mã Phiếu Kiểm Kê
                int data = -1;
                data = service.SaveInventory(MedSessionManager.CurrentDrugStoreCode, MedSessionManager.CurrentUserId, model);
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
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var inventoryService = IoC.Container.Resolve<IInventoryAdjustmentService>();
            // gọi hàm GetInventoryDetailInfo lấy thông tin phiếu muốn update
            var inventoryDetailInfo = inventoryService.GetInventoryDetailInfo(MedSessionManager.CurrentDrugStoreCode, id);
            if (inventoryDetailInfo == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            // báo lỗi khi chuyển đến trang Edit phiếu đã cân kho
            if (inventoryDetailInfo.DaCanKho)
            {
                ViewBag.Message = "Không thể sửa phiếu đã cân kho!\nChỉ có thể cập nhật Giá/Lô/Hạn dùng của thuốc ở màn hình Phiếu Kiểm kê chi tiết";
                return View("~/Views/Shared/Error.cshtml");
            }            
            var drugMngService = IoC.Container.Resolve<IDrugManagementService>();
            // set vào ViewBag để truyền tham số sang View Cshtml
            ViewBag.NhomThuoc = JsonConvert.SerializeObject(drugMngService.GetListGroupDrug(MedSessionManager.CurrentDrugStoreCode), Formatting.Indented);
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
                response.SetData(data);
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

            if (inventory.DaCanKho)
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
                dr["SoLuong"] = item.TonKho.ToString("#,##0.##");
                dr["SoLuongThuc"] = item.ThucTe.HasValue ? item.ThucTe.Value.ToString("#,##0.##") : string.Empty;
                if (inventory.DaCanKho)
                {
                    dr["SoLuongChenhLech"] = item.ThucTe.HasValue ? (item.TonKho - item.ThucTe.Value).ToString("#,##0.##") : string.Empty;
                }

                dt.Rows.Add(dr);

                tongtienhang += tienhang;
                i++;
            }

            List<ReportParameter> listParam = new List<ReportParameter>(){
                new ReportParameter("pNhaThuoc",nhathuoc.TenNhaThuoc),
                new ReportParameter("pDiaChiNhaThuoc",nhathuoc.DiaChi),
                new ReportParameter("pSDTNhaThuoc",nhathuoc.DienThoai),
                new ReportParameter("pNgayXuat",inventory.CreateTime.Value.ToString("dd/MM/yyyy")),
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