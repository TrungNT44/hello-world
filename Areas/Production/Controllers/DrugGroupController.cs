using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using App.Common.DI;
using App.Common.Http;
using App.Common.MVC.Attributes;
using App.Common.Validation;
using Med.Service.Drug;
using Med.ServiceModel.Drug;
using Med.Web.Data.Session;
using Newtonsoft.Json;
using sThuoc.Filter;

namespace Med.Web.Areas.Production.Controllers
{
    public class DrugGroupController : Controller
    {
        // GET: Production/DrugGroup
        [SimpleAuthorize("Admin")]
        public ActionResult Index()
        {
            return View("~/Areas/Production/Views/DrugGroup/Index.cshtml");
        }

        [HttpPost]
        [AuthorizedRequest]
        public string GetDrugGroupList(string dgSearchName = null)
        {
            IResponseData<List<GroupDrugInfo>> response = new ResponseData<List<GroupDrugInfo>>();
            try
            {
                var service = IoC.Container.Resolve<IDrugGroupService>();
                var data = service.GetListDrugGroup(MedSessionManager.CurrentDrugStoreCode, dgSearchName);
                response.SetData(data);
            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }

            return JsonConvert.SerializeObject(response);
        }

        // GET: Production/DrugGroup
        [SimpleAuthorize("Admin")]
        public ActionResult Create()
        {
            return View("~/Areas/Production/Views/DrugGroup/Create.cshtml");
        }

        [HttpPost]
        [AuthorizedRequest]
        public JsonResult SaveDrugGroup(GroupDrugInfo model)
        {
            IResponseData<int> response = new ResponseData<int>();
            try
            {
                int data = -1;
                var drugGroupService = IoC.Container.Resolve<IDrugGroupService>();
                data = drugGroupService.SaveDrugGroup(MedSessionManager.CurrentDrugStoreCode, MedSessionManager.CurrentUserId, model);
                response.SetData(data);
            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }

            return Json(response);
        }

        // GET: DrugGroup/Edit/id
        [AuthorizedRequest]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var drugGroupService = IoC.Container.Resolve<IDrugGroupService>();
            var nhomThuoc = drugGroupService.GetDrugGroupDetail(MedSessionManager.CurrentDrugStoreCode, id);
            if (nhomThuoc == null)
            {
                return HttpNotFound();
            }
            ViewBag.ViewModel = JsonConvert.SerializeObject(nhomThuoc);
            return View("~/Areas/Production/Views/DrugGroup/Edit.cshtml");
        }

        //GET: DrugGroup/Delete/id
        [AuthorizedRequest]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var drugGroupService = IoC.Container.Resolve<IDrugGroupService>();
            var nhomThuoc = drugGroupService.GetDrugGroupDetail(MedSessionManager.CurrentDrugStoreCode, id);
            if (nhomThuoc == null)
            {
                return HttpNotFound();
            }
            ViewBag.ViewModel = JsonConvert.SerializeObject(nhomThuoc);
            return View("~/Areas/Production/Views/DrugGroup/Delete.cshtml");
        }

        // Webservice dùng để Xóa nhóm thuốc trong Database
        [HttpPost]
        [AuthorizedRequest]
        // [Audit]
        public JsonResult DeleteDrugGroup(int maNhomThuoc)
        {
            IResponseData<bool> response = new ResponseData<bool>();
            try
            {
                var service = IoC.Container.Resolve<IDrugGroupService>();
                var data = service.DeleteDrugGroup(MedSessionManager.CurrentDrugStoreCode, MedSessionManager.CurrentUserId, maNhomThuoc);
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