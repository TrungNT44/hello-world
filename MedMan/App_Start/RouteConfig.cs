using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MedMan.App_Start
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*apple}", new { apple = @"(.*/)?apple-touch-icon.*\.png(/.*)?" });
            routes.MapRoute("ChonNhaThuocMacDinh", "Account/ChonNhaThuocMacDinh", new { controller = "Account", action = "ChonNhaThuocMacDinh" });
            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new string[] { "Med.Web.Controllers" }
            );

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Production",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Production", action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "MedMan.Areas.Production.Controllers" }
            ).DataTokens.Add("Area", "Production");
        }
    }
}

