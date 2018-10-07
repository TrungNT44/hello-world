using App.Common.DI;
using App.Common.MVC.Attributes;
using Med.Common.Enums.Notification;
using Med.Entity.Notifications;
using Med.Service.Notifications;
using Med.Web.Data.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MedMan.App_Start;
using App.Common.MVC;
using sThuoc.Filter;

namespace Med.Web.Areas.Production.Controllers
{
    public class NotificationController : BaseController
    {       
        [HttpGet]        
        [Authorize(Roles = "SuperUser")]
        public ActionResult Create()
        {
            ViewBag.action = "create";
            return View("~/Areas/Production/Views/Notification/Create.cshtml");
        }
        [HttpGet]
        [Authorize]
        [Authorize(Roles = "SuperUser")]
        public ActionResult List()
        {
            ViewBag.action = "list";
            ViewBag.IsAdmin = User.IsInRole(Constants.Security.Roles.SuperUser.Value) ? "true" : "false";
            ViewBag.CurrentDrugStoreCode = WebSessionManager.Instance.CurrentDrugStoreCode;
            return View("~/Areas/Production/Views/Notification/List.cshtml");
        }
        [HttpGet]
        [Authorize]
        public ActionResult History()
        {
            ViewBag.action = "history";
            ViewBag.IsAdmin = User.IsInRole(Constants.Security.Roles.SuperUser.Value) ? "true" : "false";
            ViewBag.CurrentDrugStoreCode = WebSessionManager.Instance.CurrentDrugStoreCode;
            return View("~/Areas/Production/Views/Notification/History.cshtml");
        }
        [HttpGet]
        [Authorize]
        public ActionResult View(int id)
        {
            ViewBag.noticationId = id;
            ViewBag.action = "view";
            return View("~/Areas/Production/Views/Notification/Create.cshtml");
        }
        [HttpGet]
        [Authorize]
        [Authorize(Roles = "SuperUser")]
        public ActionResult Update(int id)
        {
            ViewBag.noticationId = id;
            ViewBag.action = "update";
            return View("~/Areas/Production/Views/Notification/Create.cshtml");
        }
        [AuthorizedRequest]
        public JsonResult SaveNotification(int? ID, string DrugStoreID, int NotificationTypeID, string Title, string Link)
        {
            var service = IoC.Container.Resolve<INotificationService>();
            var data = false;
            var notifi = new Notification()
            {
                DrugStoreID = DrugStoreID.Trim(),
                Title = Title.Trim(),
                Link = Link.Trim(),
                NotificationTypeID = NotificationTypeID
            };
            if (ID.HasValue)
            {
                notifi.ID = ID.Value;
                data = service.UpdateNotification(notifi);
            }
            else
            {
                data = service.CreateNotification(notifi);
            }
            return Json(data ? "OK" : "Not Ok");
        }
        [AuthorizedRequest]
        public JsonResult GetListNotificationType()
        {
            var service = IoC.Container.Resolve<INotificationService>();
            return Json(service.GetListNotificationType());
        }
        [AuthorizedRequest]
        public JsonResult SearchNotification(string DrugStoreID, string NotificationTypeID, string Title, int pageIndex, int pageSize)
        {
            var service = IoC.Container.Resolve<INotificationService>();
            int? notificationType = null;
            if (!string.IsNullOrEmpty(NotificationTypeID))
                notificationType = int.Parse(NotificationTypeID.Trim());
            return Json(service.SearchNotification(DrugStoreID, notificationType, Title, pageIndex, pageSize));
        }
        [AuthorizedRequest]
        public JsonResult GetNotificationHistory(string DrugStoreID, string NotificationTypeID, string Title, int pageIndex, int pageSize)
        {
            var service = IoC.Container.Resolve<INotificationService>();
            int? notificationType = null;
            if (!string.IsNullOrEmpty(NotificationTypeID))
                notificationType = int.Parse(NotificationTypeID.Trim());
            return Json(service.GetNotificationHistory(DrugStoreID, notificationType, Title, pageIndex, pageSize));
        }
        [AuthorizedRequest]
        public JsonResult GetNotificationInfo(int id)
        {
            var service = IoC.Container.Resolve<INotificationService>();
            return Json(service.GetNotificationInfo(id));
        }
        [AuthorizedRequest]
        public JsonResult DeleteNotification(int id)
        {
            var service = IoC.Container.Resolve<INotificationService>();
            var data = service.DeleteNotification(id);
            return Json(data ? "OK" : "Not Ok");
        }
        [AuthorizedRequest]
        public JsonResult EvictNotification(int id)
        {
            var service = IoC.Container.Resolve<INotificationService>();
            var data = service.EvictNotification(id);
            return Json(data ? "OK" : "Not Ok");
        }
        [AuthorizedRequest]
        public JsonResult ReleaseNotification(int id)
        {
            var service = IoC.Container.Resolve<INotificationService>();
            var data = service.ReleaseNotification(id);
            return Json(data ? "OK" : "Not Ok");
        }
        [HttpPost]
        [AuthorizedRequest]
        public JsonResult GetNumberNotification()
        {
            var service = IoC.Container.Resolve<INotificationService>();
            bool isViewPopup = User.IsInRole(Constants.Security.Roles.SuperUser.Value) || User.IsInRole(Constants.Security.Roles.User.Value);
            var data = service.GetNumberNotification(WebSessionManager.Instance.CurrentDrugStoreCode, isViewPopup);
            return Json(data);
        }
        
        [HttpPost]
        [AuthorizedRequest]
        public JsonResult GetListNotificationForView()
        {
            var service = IoC.Container.Resolve<INotificationService>();
            return Json(service.GetListNotificationForView(WebSessionManager.Instance.CurrentDrugStoreCode));
        }
        [HttpPost]
        [AuthorizedRequest]
        public JsonResult Test()
        {
            var service = IoC.Container.Resolve<Med.Service.Common.INotificationBaseService>();
            service.NotifyToUsers("loadNotification", null, null);
            return Json(true);
        }
    }
}