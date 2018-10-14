using System.Web.Mvc;

namespace MedMan.Areas.Common
{
    public class UtilitiesAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "UniverseUtilities";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "UniverseUtilities_default",
                "UniverseUtilities/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}