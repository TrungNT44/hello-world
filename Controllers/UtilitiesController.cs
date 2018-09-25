using App.Common.DI;
using App.Common.Extensions;
using App.Common.Http;
using App.Common.MVC;
using App.Common.MVC.Attributes;
using App.Common.Validation;
using Med.Common;
using Med.Service.Utilities;
using Med.ServiceModel.Request;
using Med.ServiceModel.Response;
using Med.ServiceModel.Utilities;
using Med.Web.Data.Session;
using MedMan.App_Start;
using sThuoc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Med.Web.Areas.Production.Controllers
{
    public class UtilitiesController : BaseController
    {
        [HttpPost]
        public JsonResult GetCanhBaoHangHetHan()
        {
            var requestParams = this.ToRequestParams<GetCanhBaoHangHetHanRequestModel>();
            IResponseData<CanhBaoHetHanResponse> response = new ResponseData<CanhBaoHetHanResponse>();
            try
            {
                var service = IoC.Container.Resolve<IUtilitiesService>();
                int iNhomThuoc = int.Parse("0" + requestParams.sNhomThuocId);
                var data = service.CanhBaoHangHetHan(WebSessionManager.Instance.CurrentDrugStoreCode, Constants.Settings.SoNgayHetHan, Constants.Settings.SoNgayKhongCoGiaoDich , requestParams.sType, iNhomThuoc, requestParams.sMaThuoc);
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
        public ActionResult NegativeRevenueWarning()
        {
            return View("~/Areas/Production/Views/Utilities/NegativeRevenueWarning.cshtml");
        }

        [HttpPost]
        [AuthorizedRequest]
        public JsonResult GetNegativeRevenueWarningData()
        {
            var requestParams = this.ToRequestParams<ReportRequestModel>();
            var filter = requestParams.ToFilterObject();

            IResponseData<NegativeRevenueResponse> response = new ResponseData<NegativeRevenueResponse>();
            try
            {
                var service = IoC.Container.Resolve<IUtilitiesService>();
                var data = service.GetNegativeRevenueWarningData(WebSessionManager.Instance.CurrentDrugStoreCode, filter);
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
        public ActionResult NearExpiredDrugWarning()
        {
            return View("~/Areas/Production/Views/Utilities/NearExpiredDrugWarning.cshtml");
        }

        [HttpPost]
        [AuthorizedRequest]
        public JsonResult GetNearExpiredDrugWarningData(int expiredOption)
        {
            var requestParams = this.ToRequestParams<ReportRequestModel>();
            var filter = requestParams.ToFilterObject();

            IResponseData<NearExpiredDrugResponse> response = new ResponseData<NearExpiredDrugResponse>();
            try
            {
                var dsSession = (DrugStoreSession)WebSessionManager.Instance.CommonSessionData;
                var service = IoC.Container.Resolve<IUtilitiesService>();
                var data = service.GetNearExpiredDrugWarningData(WebSessionManager.Instance.CurrentDrugStoreCode, dsSession.Settings, filter, expiredOption);
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
        [AuthorizedRequest]
        public JsonResult GetLinkDownloadFile(string codeFile)
        {
            switch (codeFile)
            {
                case "A111":
                    return Json("/TestLinkFile/file.exe");
                default:
                    return Json("");
            }
        }
    }
}