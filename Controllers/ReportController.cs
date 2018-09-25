using App.Common.Extensions;
using App.Common.Http;
using App.Common.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Med.Common;
using Med.Common.Enums;
using Med.Service.Report;
using App.Common.Validation;
using System.Net;
using System.Web.Mvc;
using App.Common.DI;
using Med.ServiceModel.Common;
using Med.ServiceModel.Report;
using Med.ServiceModel.Request;
using Med.Web.Data.Session;
using MedMan.App_Start;
using App.Common.MVC.Attributes;
using sThuoc.Filter;
using Med.Web.Extensions;
using App.Common;
using Med.Web.Filter;
using MedMan;
using Med.ServiceModel.CacheObjects;
using Newtonsoft.Json;

namespace Med.Web.Areas.Production.Controllers
{
    public class ReportController : BaseController
    {
        [HttpGet]
        [AuthorizedRequest]
        public ActionResult ReportExample()
        {
            return View("~/Areas/Production/Views/Reports/ReportExample.cshtml");
        }

        [HttpPut]
        public IResponseData<string> UpdateFuncTest(int id)
        {
            IResponseData<string> response = new ResponseData<string>();
            try
            {
                var service = IoC.Container.Resolve<IReportService>();
              
                // Call service function
                
            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }
            return response;
        }

        [HttpGet]
        [AuthorizedRequest]
        // [Audit]
        public ActionResult RevenueDetailsByDay()
        {
            return View("~/Areas/Production/Views/Reports/RevenueDetailsByDay.cshtml");
        }

        [HttpPost]
        [AuthorizedRequest]
        // [Audit]
        public JsonResult GetRevenueDrugSynthesis()
        {
            var requestParams =  this.ToRequestParams<ReportRequestModel>();
            if (requestParams.reportFromDate == DateTime.MinValue) return null;

            IResponseData<RevenueDrugSynthesisResponse> response = new ResponseData<RevenueDrugSynthesisResponse>();
            try
            {
                var reportDate = requestParams.reportFromDate;
                var filter = new FilterObject()
                {
                    FromDate = reportDate.AbsoluteStart(),
                    ToDate = reportDate.AbsoluteEnd()
                };
                if (User.IsInRole(MedMan.App_Start.Constants.Security.Roles.User.Value))
                {
                    var nhathuoc = this.GetNhaThuoc();                 
                    var hasPermis = FunctionsService.Authorize("Baocao", "Index", nhathuoc);
                    if (!hasPermis)
                    {
                        filter.StaffIds = new int[] { WebSessionManager.Instance.CurrentUserId };
                    }                    
                }
                var service = IoC.Container.Resolve<IRevenueDrugSynthesisReportService>();
                var data = service.GetRevenueDrugSynthesis(WebSessionManager.Instance.CurrentDrugStoreCode, filter);
                response.SetData(data);
            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }

            return Json(response);
        }

        [HttpGet]
        [AuthorizedRequest]
        // [Audit]
        public ActionResult SynthesisReport()
        {
            return View("~/Areas/Production/Views/Reports/SynthesisReport.cshtml");
        }

        [HttpPost]
        [AuthorizedRequest]
        public JsonResult GetSynthesisReportData()
        {
            var requestParams = this.ToRequestParams<ReportRequestModel>();
            var filter = requestParams.ToFilterObject();

            IResponseData<SynthesisReportResponse> response = new ResponseData<SynthesisReportResponse>();
            try
            {
                var service = IoC.Container.Resolve<ISynthesisReportService>();
                var data = service.GetSynthesisReportData(WebSessionManager.Instance.CurrentDrugStoreCode, filter);
                response.SetData(data);
            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }

            return Json(response);
        }

        [HttpGet]
        [AuthorizedRequest]
        // [Audit]
        public ActionResult ViewCustomizeReport()
        {
            return View("~/Areas/Production/Views/Reports/CustomizeReport.cshtml");
        }

        [HttpPost]
        [AuthorizedRequest]
        public JsonResult CustomizeReport(DateTime fromDate, DateTime toDate)
        {
            IResponseData<CustomizeReportItemResponse> response = new ResponseData<CustomizeReportItemResponse>();
            try
            {
                var filter = new FilterObject()
                {
                    FromDate = fromDate.AbsoluteStart(),
                    ToDate = toDate.AbsoluteEnd()
                };

                var currentDrugStoreCode = WebSessionManager.Instance.CurrentDrugStoreCode;
                var customizeReportService = IoC.Container.Resolve<ICustomizeReportService>();
                var data = customizeReportService.GetCustomizeReportItems(currentDrugStoreCode, filter);
                response.SetData(data);
            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }

            return Json(response);
        }

        [HttpGet]
        [AuthorizedRequest]
        // [Audit]
        public ActionResult DrugWarehouses()
        {
            return View("~/Areas/Production/Views/Reports/WarehouseReport.cshtml");
        }

        [HttpPost]
        public JsonResult GetDrugWarehouses()
        {
            var requestParams = this.ToRequestParams<ReportRequestModel>();
            var filter = requestParams.ToFilterObject();

            IResponseData<DrugWarehouseResponse> response = new ResponseData<DrugWarehouseResponse>();
            try
            {
                var service = IoC.Container.Resolve<IDrugWarehouseReportService>();
                var data = service.GetDrugWarehouses(WebSessionManager.Instance.CurrentDrugStoreCode, filter);
                response.SetData(data);
            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }

            return Json(response);
        }

        [HttpGet]
        [AuthorizedRequest]
        // [Audit]
        public ActionResult ReportByStaff()
        {
            return View("~/Areas/Production/Views/Reports/ReportByStaff.cshtml");
        }

        [HttpGet]
        [AuthorizedRequest]
        // [Audit]
        public ActionResult ReportByCustomer()
        {
            return View("~/Areas/Production/Views/Reports/ReportByCustomer.cshtml");
        }

        [HttpGet]
        [AuthorizedRequest]
        // [Audit]
        public ActionResult ReportByDoctor()
        {
            return View("~/Areas/Production/Views/Reports/ReportByDoctor.cshtml");
        }

        [HttpGet]
        [AuthorizedRequest]
        // [Audit]
        public ActionResult ReportBySupplyer()
        {
            return View("~/Areas/Production/Views/Reports/ReportBySupplyer.cshtml");
        }

        [HttpGet]
        [AuthorizedRequest]
        // [Audit]
        public ActionResult ReportByGoods(int? reportByTypeId, int? filterObjectId)
        {
            ViewBag.ViewModel = JsonConvert.SerializeObject(
                new
                {
                    FilterItemTypeId = reportByTypeId == (int)ReportByType.ByGoodsByDoctor ? (int)ItemFilterType.Doctor : (int)ItemFilterType.Staff,
                    FilteObjectId = filterObjectId
                });
            return View("~/Areas/Production/Views/Reports/ReportByGoods.cshtml");
        }

        [HttpPost]
        [AuthorizedRequest]
        public JsonResult GetReportByData()
        {
            var requestParams = this.ToRequestParams<ReportRequestModel>();      
            var filter = requestParams.ToFilterObject();

            IResponseData<ReportByResponse> response = new ResponseData<ReportByResponse>();
            try
            {
                if (filter.ReportByTypeId == ReportByType.ByStaff)
                {
                    ApplyCurrentStaffFilter(filter);
                }
                var service = IoC.Container.Resolve<IReportService>();
                var data = service.GetReportByData(WebSessionManager.Instance.CurrentDrugStoreCode, filter);
                response.SetData(data);
            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }

            return Json(response);
        }

        private void ApplyCurrentStaffFilter(FilterObject filter)
        {
            int? currentStaffId = null;
            if (User.IsInRole(MedMan.App_Start.Constants.Security.Roles.User.Value))
            {
                currentStaffId = WebSecurity.GetCurrentUserId;
            }

            if (currentStaffId.HasValue)
            {
                if (filter.StaffIds == null)
                {
                    filter.StaffIds = new int[] { currentStaffId.Value };
                }
                else
                {
                    if (!filter.StaffIds.Contains(currentStaffId.Value))
                    {
                        var staffList = filter.StaffIds.ToList();
                        staffList.Add(currentStaffId.Value);
                        filter.StaffIds = staffList.ToArray();
                    }
                }
            }
        }

        [HttpGet]
        [AuthorizedRequest]
        public ActionResult DrugTransHistories(int? drugId)
        {
            var drug = new CacheDrug();
            if (drugId > 0)
            {
                var drugInfo = MainApp.Instance.GetCacheDrugs(MedSessionManager.CurrentDrugStoreCode, drugId.Value).FirstOrDefault();
                if (drugInfo != null)
                {
                    drug = drugInfo;
                }
            }
            var nhathuoc = this.GetNhaThuoc();
            var viewModel = new
            {
                Drug = drug,
                HasViewReceiptNotePrivilage = FunctionsService.Authorize("PhieuNhaps", "Details", nhathuoc) ? 1 : 0,
                HasViewDeliveryNotePrivilage = FunctionsService.Authorize("PhieuXuats", "Details", nhathuoc) ? 1 : 0
            };
            ViewBag.ViewModel = JsonConvert.SerializeObject(viewModel);           

            return View("~/Areas/Production/Views/Reports/TransHistoryReport.cshtml");
        }

        [HttpPost]
        public JsonResult GetDrugTransHistoryData(int noteTypeId)
        {
            var requestParams = this.ToRequestParams<ReportRequestModel>();
            var filter = requestParams.ToFilterObject();

            IResponseData<DrugTransHistoryResponse> response = new ResponseData<DrugTransHistoryResponse>();
            try
            {
                var service = IoC.Container.Resolve<ITransactionReportService>();
                var data = service.GetDrugTransHistoryData(WebSessionManager.Instance.CurrentDrugStoreCode, filter, noteTypeId);

                response.SetData(data);
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