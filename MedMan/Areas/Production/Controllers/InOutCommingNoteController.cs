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
using App.Constants.Enums;
using Med.Service.Common;
using Med.ServiceModel.InOutComming;
using Newtonsoft.Json;
using Med.Web.Helpers;

namespace Med.Web.Areas.Production.Controllers
{
    public class InOutCommingNoteController : BaseController
    {
        [HttpGet]
        [AuthorizedRequest]
        // [Audit]
        public ActionResult InOutcommingNoteScreen(int? noteId, int? noteTypeId, int? taskMode)
        {
            var service = IoC.Container.Resolve<IInOutCommingNoteService>();
            var model = service.GetInOutcommingNoteModel(WebSessionManager.Instance.CurrentDrugStoreCode, WebSessionManager.Instance.CurrentUserId, noteId, noteTypeId, taskMode);
            ViewBag.ViewModel = JsonConvert.SerializeObject(model);
            ViewBag.NoteTypeId = noteTypeId;
            ViewBag.NoteId = noteId;

            return View("~/Areas/Production/Views/InOutCommingNote/InOutCommingNote.cshtml");
        }

        [HttpPost]
        [AuthorizedRequest]
        public JsonResult GetReceiverDebtInfo(int receiverId, int noteTypeId, int inOutComingNoteId)
        {         
            IResponseData<ReceiverDebtInfo> response = new ResponseData<ReceiverDebtInfo>();
            try
            {                
                var service = IoC.Container.Resolve<IInOutCommingNoteService>();
                var data = service.GetReceiverDebtInfo(WebSessionManager.Instance.CurrentDrugStoreCode, receiverId, noteTypeId, inOutComingNoteId);
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
        public JsonResult SaveInOutCommingNote(InOutcommingNoteModel model)
        {
            IResponseData<int> response = new ResponseData<int>();
            try
            {
                var service = IoC.Container.Resolve<IInOutCommingNoteService>();
                var data = service.SaveInOutCommingNote(WebSessionManager.Instance.CurrentDrugStoreCode, WebSessionManager.Instance.CurrentUserId, model);
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
        public JsonResult DeleteInOutCommingNote(int noteId)
        {
            IResponseData<bool> response = new ResponseData<bool>();
            try
            {
                var service = IoC.Container.Resolve<IInOutCommingNoteService>();
                var data = service.DeleteInOutCommingNote(WebSessionManager.Instance.CurrentDrugStoreCode, noteId);
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
        public JsonResult TransitWarehouse(string targetDrugStoreCode, int deliveryNoteId)
        {
            IResponseData<int> response = new ResponseData<int>();
            try
            {
                var service = IoC.Container.Resolve<IInOutCommingNoteService>();
                var data = service.TransitWarehouse(WebSessionManager.Instance.CurrentDrugStoreCode, targetDrugStoreCode, deliveryNoteId, WebSessionManager.Instance.CurrentUserId);
                response.SetData(data);
                
                BackgroundJobHelper.EnqueueMakeAffectedChangesRelatedReceiptNotes(data);
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