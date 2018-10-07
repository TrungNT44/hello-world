using App.Common.DI;
using App.Common.Extensions;
using App.Common.Http;
using App.Common.MVC;
using App.Common.MVC.Attributes;
using App.Common.Validation;
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

namespace Med.Web.Areas.Utilities.Controllers
{
    public class UtilityController : BaseController
    {
        [HttpGet]
        [AuthorizedRequest]
        //[AllowAnonymous]
        public ActionResult ScanBarcode()
        {
            return View("~/Areas/Utilities/Views/Barcode/MobileBarcodeScanner.cshtml");
        }
    }
}