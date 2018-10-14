using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using App.Common.DI;
using App.Common.Http;
using App.Common.MVC.Attributes;
using Med.Service.Drug;
using Med.ServiceModel.Drug;
using Med.Web.Extensions;
using Newtonsoft.Json;
using App.Common.Validation;
using Med.Web.Data.Session;

namespace Med.Web.Areas.Production.Controllers
{
    public class DrugGroupController : Controller
    {
        // GET: /DrugGroup/Index
        [AuthorizedRequest]
        public ActionResult Index(string searchTen)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var service = IoC.Container.Resolve<IDrugManagementService>();
            var drugGroupModel = service.GetListGroupDrug(maNhaThuoc);
            if (!String.IsNullOrEmpty(searchTen))
            {
                drugGroupModel = drugGroupModel.Where(x => x.TenNhomThuoc.ToLower().Contains(searchTen.ToLower())).ToList();
            }
            ViewBag.ViewModel = JsonConvert.SerializeObject(drugGroupModel, Formatting.Indented);

            return View("~/Areas/Production/Views/DrugGroup/Index.cshtml");
        }

        // GET: /DrugGroup/Create
        [AuthorizedRequest]
        public ActionResult Create()
        {
            //var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            //var service = IoC.Container.Resolve<IDrugManagementService>();
            //var drugGroupModel = service.GetListGroupDrug(maNhaThuoc);
            //if (!String.IsNullOrEmpty(searchTen))
            //{
            //    drugGroupModel = drugGroupModel.Where(x => x.TenNhomThuoc.ToLower().Contains(searchTen.ToLower())).ToList();
            //}
            //ViewBag.ViewModel = JsonConvert.SerializeObject(drugGroupModel, Formatting.Indented);

            return View("~/Areas/Production/Views/DrugGroup/Create.cshtml");
        }

        [HttpPost]
        [AuthorizedRequest]
        // [Audit]
        public JsonResult SaveDrugGroup(GroupDrugInfo model)
        {
            IResponseData<int> response = new ResponseData<int>();
            try
            {
                var nhaThuoc = this.GetNhaThuoc();
                var maNhaThuoc = nhaThuoc.MaNhaThuoc;
                var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
                int currentUserId = WebSessionManager.Instance.CurrentUserId;
                int data = -1;
                var drugManagementService = IoC.Container.Resolve<IDrugManagementService>();
                var drugGroupModel = drugManagementService.GetListGroupDrug(maNhaThuoc);

                // kiểm tra tên nhóm thuốc mới tạo đã tồn tại chưa
                foreach (var drugGroup in drugGroupModel)
                {
                    if (drugGroup.TenNhomThuoc.Equals(model.TenNhomThuoc))
                    {
                        response.SetData(data);
                        return Json(response);
                    }
                }
                var drugGroupService = IoC.Container.Resolve<IDrugGroupService>();
                data = drugGroupService.SaveDrugGroup(maNhaThuoc, currentUserId, model);
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
            var nhaThuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhaThuoc.MaNhaThuoc;
            var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            var drugGroupService = IoC.Container.Resolve<IDrugGroupService>();
            var nhomThuoc = drugGroupService.GetGroupDrugInfo(maNhaThuoc, id);
            if (nhomThuoc == null)
            {
                return HttpNotFound();
            }
            ViewBag.ViewModel = JsonConvert.SerializeObject(nhomThuoc);
            return View("~/Areas/Production/Views/DrugGroup/Edit.cshtml");
        }

        // GET: DrugGroup/Delete/id
        [AuthorizedRequest]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var nhaThuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhaThuoc.MaNhaThuoc;
            var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            var drugGroupService = IoC.Container.Resolve<IDrugGroupService>();
            var nhomThuoc = drugGroupService.GetGroupDrugInfo(maNhaThuoc, id);
            if (nhomThuoc == null)
            {
                return HttpNotFound();
            }
            ViewBag.ViewModel = JsonConvert.SerializeObject(nhomThuoc);
            return View("~/Areas/Production/Views/DrugGroup/Delete.cshtml");
        }
    }
}