using MedMan.App_Start;
using Med.Web.Extensions;
using sThuoc.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using sThuoc.Models.ViewModels;

namespace sThuoc.Filter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true,
    AllowMultiple = true)]
    public class SimpleAuthorize : AuthorizeAttribute
    {
        public SimpleAuthorize(int resourceId, Operations operation)
        {
            _resourceId = resourceId;
            _operation = operation;
        }

        private int _resourceId;
        private Operations _operation;
        private string[] checkRoles;
        protected string controller;
        protected string action;
        protected NhaThuocSessionModel nhaThuoc;
        public SimpleAuthorize(string roles)
        {
            this.checkRoles = roles.Split(',');
        }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            //Get the current claims principal
            var prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            //Make sure they are authenticated
            if (!prinicpal.Identity.IsAuthenticated)
                return false;
            //allows if SuperUser.
            if (prinicpal.IsInRole(Constants.Security.Roles.SuperUser.Value))
            {
                return true;
            }
            //var roles = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();
            //Check if they are authorized
            return FunctionsService.Authorize(controller, action, nhaThuoc,checkRoles);
        }
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            controller = filterContext.RouteData.Values["controller"].ToString();
            action = filterContext.RouteData.Values["action"].ToString();
            if (((Controller)filterContext.Controller).GetNhaThuoc()!=null)
                nhaThuoc = ((Controller)filterContext.Controller).GetNhaThuoc();
            base.OnAuthorization(filterContext);
        }


    }
}
