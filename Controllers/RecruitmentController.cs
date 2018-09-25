using App.Common.DI;
using App.Common.Http;
using App.Common.MVC;
using App.Common.MVC.Attributes;
using Med.Entity;
using Med.Service.Recruitment;
using Med.Web.Data.Session;
using Med.Web.Filter;
using MedMan.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Cors;
using System.Web.Mvc;

namespace Med.Web.Areas.Production.Controllers
{
    public class RecruitmentController : BaseController
    {
        [HttpGet]
        [Authorize]
        // [Audit]
        public ActionResult CreateRecruit()
        {
            ViewBag.DrugStoreCode = WebSessionManager.Instance.CurrentDrugStoreCode;
            ViewBag.IsAdmin = User.IsInRole(Constants.Security.Roles.SuperUser.Value)?"true":"false";
            return View("~/Areas/Production/Views/Recruitment/CreateRecruit.cshtml");
        }
        [HttpGet]
        [Authorize]
        // [Audit]
        public ActionResult UpdateRecruit(int id)
        {
            ViewBag.IsAdmin = User.IsInRole(Constants.Security.Roles.SuperUser.Value) ? "true" : "false";
            ViewBag.IdTuyenDung = id;
            return View("~/Areas/Production/Views/Recruitment/UpdateRecruit.cshtml");
        }
        [HttpGet]
        [Authorize]
        // [Audit]
        public ActionResult ViewRecruit(int id)
        {
            ViewBag.IsAdmin = User.IsInRole(Constants.Security.Roles.SuperUser.Value) ? "true" : "false";
            ViewBag.IdTuyenDung = id;
            return View("~/Areas/Production/Views/Recruitment/ViewRecruit.cshtml");
        }
        [HttpGet]
        [Authorize]
        // [Audit]
        public ActionResult DeleteRecruit(int id)
        {
            ViewBag.IdTuyenDung = id;
            ViewBag.IsAdmin = User.IsInRole(Constants.Security.Roles.SuperUser.Value) ? "true" : "false";
            return View("~/Areas/Production/Views/Recruitment/DeleteRecruit.cshtml");
        }
        [HttpGet]
        [Authorize]
        // [Audit]
        public ActionResult ListRecruits()
        {
            if(Request.Browser.IsMobileDevice)
                return View("~/Areas/Production/Views/Recruitment/ListRecruits.Mobile.cshtml");
            else
                return View("~/Areas/Production/Views/Recruitment/ListRecruits.cshtml");
        }
        [HttpGet]
        [Authorize]
        // [Audit]
        public ActionResult ListRecruitActive()
        {
            return View("~/Areas/Production/Views/Recruitment/ListRecruitActive.cshtml");
        }
        
        [HttpPost]
        public JsonResult GetRecruitInfo(int id)
        {
            var service = IoC.Container.Resolve<IRecruitService>();
            var data = service.GetRecruitInfo(id);
            return Json(data);
        }
        [HttpPost]
        public JsonResult GetListProvinces()
        {
            var service = IoC.Container.Resolve<IRecruitService>();
            var data = service.GetListProvinces();
            return Json(data);
        }
        [HttpPost]
        public JsonResult GetListDrugStores()
        {
            var service = IoC.Container.Resolve<IRecruitService>();
            var data = service.GetListDrugStores();
            return Json(data);
        }
        [HttpPost]
        // [Audit]
        public JsonResult CreateRecruit(TuyenDungs inputData)
        {
            var service = IoC.Container.Resolve<IRecruitService>();
            var data = service.CreateRecruit(inputData);
            return Json(data?"OK":"Not Ok");
        }
        [HttpPost]
        // [Audit]
        public JsonResult UpdateRecruit(TuyenDungs inputData)
        {
            var service = IoC.Container.Resolve<IRecruitService>();
            var data = service.UpdateRecruit(inputData);
            return Json(data ? "OK" : "Not Ok");
        }
        [HttpPost]
        // [Audit]
        public JsonResult RemoveRecruit(int idRecruit)
        {
            var service = IoC.Container.Resolve<IRecruitService>();
            var data = service.DeleteRecruit(idRecruit);
            return Json(data ? "OK" : "Not Ok");
        }
        [HttpPost]
        // [Audit]
        public JsonResult ActiveRecruit(int idRecruit)
        {
            var service = IoC.Container.Resolve<IRecruitService>();
            var sDrugStoreCode = WebSessionManager.Instance.CurrentDrugStoreCode;
            var data = service.ActiveRecruit(idRecruit, sDrugStoreCode);
            return Json(data ? "OK" : "Not Ok");
        }
        [HttpPost]
        public JsonResult GetListRecruitsOfGrugStore(string TieuDe)
        {
            IResponseData<Object> response = new ResponseData<Object>();
            var sDrugStoreCode = WebSessionManager.Instance.CurrentDrugStoreCode;
            var service = IoC.Container.Resolve<IRecruitService>();
            var data = service.GetListRecruitsOfDrugStore(sDrugStoreCode, TieuDe);
            response.SetData(data);
            return Json(response);
        }
        [HttpPost]
        public JsonResult GetListRecruitActive(string TieuDe, int? IdTinhThanh)
        {
            var service = IoC.Container.Resolve<IRecruitService>();
            var data = service.GetListRecruitActive(TieuDe,IdTinhThanh,null,null);
            return Json(data);
        }
    }
}