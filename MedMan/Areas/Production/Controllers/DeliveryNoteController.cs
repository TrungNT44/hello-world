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
using Med.ServiceModel.Delivery;
using Med.Service.Delivery;
using Med.Web.Filter;
using Newtonsoft.Json;
using Med.Web.Helpers;

namespace Med.Web.Areas.Production.Controllers
{
    public class DeliveryNoteController : BaseController
    {
        [HttpGet]
        [AuthorizedRequest]
        // [Audit]
        public ActionResult DeliveryWithBarcode()
        {
            var service = IoC.Container.Resolve<IDeliveryNoteService>();        
            var noteNumber = service.GetNewDeliveryNoteNumber(WebSessionManager.Instance.CurrentDrugStoreCode);
            var dsSession = (DrugStoreSession)WebSessionManager.Instance.CommonSessionData;
            var allowToChangeTotalAmount = dsSession.Settings.AllowToChangeTotalAmount;
            var model = new
            {
                AllowToChangeTotalAmount = allowToChangeTotalAmount,
                NoteNumber = noteNumber
            };
            ViewBag.ViewModel = JsonConvert.SerializeObject(model);
            BackgroundJobHelper.EnqueueUpdateNewestInventories(null);

            return View("~/Areas/Production/Views/Delivery/DeliveryNoteWithBarcode.cshtml");
        }

        [HttpPost]
        [AuthorizedRequest]
        public JsonResult GetDrugDeliveryItem(int drugId, string barcode)
        {         
            IResponseData<DrugDeliveryItem> response = new ResponseData<DrugDeliveryItem>();
            try
            {                
                var service = IoC.Container.Resolve<IDeliveryNoteService>();
                var data = service.GetDrugDeliveryItem(WebSessionManager.Instance.CurrentDrugStoreCode, drugId, barcode);
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
        // [Audit]
        public JsonResult SaveDeliveryNote(List<DrugDeliveryItem> deliveryItems, double paymentAmount, int noteNumber,
            DateTime? noteDate, int? customerId, int? doctorId, string description)
        {
            IResponseData<int> response = new ResponseData<int>();
            try
            {
                var service = IoC.Container.Resolve<IDeliveryNoteService>();
                var data = service.SaveDeliveryNote(WebSessionManager.Instance.CurrentDrugStoreCode, WebSessionManager.Instance.CurrentUserId, 
                    deliveryItems, paymentAmount, noteNumber, noteDate, customerId, doctorId, description);
                response.SetData(data);
            
                BackgroundJobHelper.EnqueueMakeAffectedChangesRelatedDeliveryNotes(data);
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