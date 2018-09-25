using App.Common.Extensions;
using App.Common.Http;
using App.Common.MVC;
using System;
using System.Linq;
using App.Common.Validation;
using System.Net;
using System.Web.Mvc;
using App.Common.DI;
using App.Common.MVC.Attributes;
using Newtonsoft.Json;
using Med.Service.Admin;
using App.Constants.Enums;
using Med.ServiceModel.Admin;
using Med.Web.Data.Session;
using Med.Common;

namespace Med.Web.Areas.Production.Controllers
{
    public class AdminController : BaseController
    {
        [HttpGet]
        [AuthorizedRequest]
        public ActionResult SetupRoles()
        {           
            var service = IoC.Container.Resolve<IAdminService>();
            var activeRoles = service.GetAllRoles(true).Select(i =>
                new { RoleId = i.RoleId, RoleName = i.RoleName }).ToArray();
            ViewBag.Roles = JsonConvert.SerializeObject(activeRoles).Replace("\"", "'");
            ViewBag.Permissions = JsonConvert.SerializeObject(Enum.GetValues(typeof(PermissionType)).Cast<PermissionType>()
                .Select(i => new { PermissionId = (int)i, PermissionName = i.ToString().SplitCamelCase() }));

            return View("~/Areas/Production/Views/Admin/SetupRoleAction.cshtml");
        }        

        [HttpPost]
        [AuthorizedRequest]
        // [Audit]
        public JsonResult PushRemainResourcesToDB()
        {
            IResponseData<bool> response = new ResponseData<bool>();
            try
            {               
                var service = IoC.Container.Resolve<IAdminService>();
                var data = service.PushRemainResourcesToDB();
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
        public JsonResult LoadRoleActions(int roleId)
        {
            IResponseData<RolePermissionResponse> response = new ResponseData<RolePermissionResponse>();
            try
            {               
                var service = IoC.Container.Resolve<IAdminService>();
                var data = service.LoadRoleActions(roleId, MedSessionManager.CurrentDrugStoreId);
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
        public JsonResult AddRoleAction(int roleId, int resourceId, int permissionId)
        {            
            IResponseData<bool> response = new ResponseData<bool>();
            try
            {               
                var service = IoC.Container.Resolve<IAdminService>();
                var data = service.AddRoleAction(roleId, resourceId, permissionId, MedSessionManager.CurrentDrugStoreId);
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
        public JsonResult UpdatePermission(int roleId, int resourceId, int permissionId)
        {
            IResponseData<bool> response = new ResponseData<bool>();
            try
            {              
                var service = IoC.Container.Resolve<IAdminService>();
                var data = service.UpdatePermission(roleId, resourceId, permissionId, MedSessionManager.CurrentDrugStoreId);
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
        public JsonResult RemoveRoleAction(int roleId, int resourceId)
        {
            IResponseData<bool> response = new ResponseData<bool>();
            try
            {               
                var service = IoC.Container.Resolve<IAdminService>();
                var data = service.RemoveRoleAction(roleId, resourceId, MedSessionManager.CurrentDrugStoreId);
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