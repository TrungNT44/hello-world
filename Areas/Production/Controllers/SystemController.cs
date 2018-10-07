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
using Med.Service.System;
using Med.ServiceModel.Common;
using Med.ServiceModel.Report;
using Med.ServiceModel.Request;
using Med.ServiceModel.System;
using Med.Web.Data.Session;
using MedMan;
using MedMan.App_Start;
using App.Common.MVC.Attributes;

namespace Med.Web.Areas.Production.Controllers
{
    public class SystemController : BaseController
    {
        [HttpPost]
        [AuthorizedRequest]
        public JsonResult GetSystemMessages()
        {
            IResponseData<SystemMessageResponse> response = new ResponseData<SystemMessageResponse>();
            try
            {
                var service = IoC.Container.Resolve<ISystemService>();
                var data = service.GetSystemMessages(WebSessionManager.Instance.CurrentDrugStoreCode);
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
        public JsonResult GetSystemMessagesCount()
        {
            var messagesCount = 0; // MainApp.Instance.GetMessagesCount(WebSessionManager.Instance.CurrentDrugStoreCode);

            return Json(messagesCount);
        }
    }
}