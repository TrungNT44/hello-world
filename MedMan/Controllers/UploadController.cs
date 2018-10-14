using App.Common.MVC;
using sThuoc.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Med.Web.Controllers
{
    public class UploadController : BaseController
    {
        // GET: Upload
        public ActionResult Index()
        {
            UploadObjectInfo info;
            if (Session["UploadDrugInfo"] != null)
            {
                info = (UploadObjectInfo)Session["UploadDrugInfo"];
                Session.Remove("UploadDrugInfo");
            }
            else
            {
                info = new UploadObjectInfo();
                info.Title = "Không đọc được dữ liệu từ Session Info";
            }

            return View(info);
        }
    }
}