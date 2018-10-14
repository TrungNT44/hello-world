using App.Common.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace Med.Web.Controllers
{
    public class ViewSwithcherController : BaseController
    {
        // GET: ViewSwithcherh
        public ActionResult Index()
        {
            return View();
        }
        //Switch to mobile view
        public RedirectResult SwitchView (bool mobile, string returnUrl)
        {
            if (Request.Browser.IsMobileDevice == mobile)
            {
                HttpContext.ClearOverriddenBrowser();
            }
            else
            {
                HttpContext.SetOverriddenBrowser(BrowserOverride.Mobile);
            }            
            return Redirect(returnUrl);
        }
    }
}