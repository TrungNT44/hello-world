using App.Common;
using App.Common.Authorize;
using App.Common.DI;
using App.Common.FaultHandling;
using App.Common.Validation;
using App.Constants.Enums;
using Med.Entity.Log;
using Med.Service.Log;
using Med.Service.Registration;
using Med.Web.Data.Session;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;

namespace MedMan.Share.Authorize
{
    public class UserLoginAuthorization : IUserLoginAuthorization
    {
        private readonly IUserService _userService;
        public UserLoginAuthorization()
        {
            _userService = IoC.Container.Resolve<IUserService>();
        }

        public bool IsAuthorized(System.Web.HttpContextBase httpContext, HttpActionEnumCombinationRule combineRule = HttpActionEnumCombinationRule.Any, PermissionType permission = PermissionType.Write, int[] actions = null)
        {
            return true;
            //if (WebSessionManager.Instance.CurrentUserId <= 0 || string.IsNullOrEmpty(WebSessionManager.Instance.CurrentDrugStoreCode))
            //{
            //    retVal = false;
            //}

            //Get the current claims principal
            //var prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            //Make sure they are authenticated
            //if (!prinicpal.Identity.IsAuthenticated)
            //    return false;
            //allows if SuperUser.
            //if (prinicpal.IsInRole(MedMan.App_Start.Constants.Security.Roles.SuperUser.Value))
            //{
            //    return true;
            //}
            //var roles = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();
            //Check if they are authorized
            //retVal = FunctionsService.Authorize(controller, action, nhaThuoc, checkRoles);

            //var request = httpContext.Request;      
            //string controller = request.RequestContext.RouteData.Values["controller"].ToString();
            //string action = request.RequestContext.RouteData.Values["action"].ToString();
            var session = WebSessionManager.Instance;
            var currentUserId = session.CurrentUserId;
            if (currentUserId > 0)
            {
                //var drugStoreCode = session.CurrentDrugStoreCode;
                //var request = httpContext.Request;
                //// Generate an audit
                //var audit = new Audit()
                //{
                //    // Your Audit Identifier     
                //    AuditID = Guid.NewGuid(),
                //    // Our Username (if available)
                //    UserName = (request.IsAuthenticated) ? httpContext.User.Identity.Name : "Anonymous",
                //    UserID = currentUserId,
                //    DrugStoreCode = drugStoreCode,
                //    // The IP Address of the Request
                //    IPAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? request.UserHostAddress,
                //    // The URL that was accessed
                //    AreaAccessed = request.RawUrl,
                //    // Creates our Timestamp
                //    CreatedDateTime = DateTime.UtcNow
                //};
                //HostingEnvironment.QueueBackgroundWorkItem(ct => AuditAction(audit, currentUserId, drugStoreCode));
            }
            var user = session.CurrentUser;
            if (null == user)
            {
                return false;
            }

            if (user.IsSystemAdmin() || actions == null || !actions.Any())
            {
                return true;
            }

            switch (combineRule)
            {
                case HttpActionEnumCombinationRule.Any: return actions.Any(a => user.HasPermission(a));
                case HttpActionEnumCombinationRule.All: return actions.All(a => user.HasPermission(a));
                case HttpActionEnumCombinationRule.NotAny: return !actions.Any(a => user.HasPermission(a));
                case HttpActionEnumCombinationRule.NotAll: return !actions.All(a => user.HasPermission(a));
            }

            return false;
        }

        private void AuditAction(Audit audit, int currentUserId, string drugStoreCode)
        {
            try
            {                
                var service = IoC.Container.Resolve<IAuditLogService>();
                service.Add(audit);
            }
            catch (Exception ex)
            {
                FaultHandler.Instance.Handle(ex, this);
            }
        }

        public bool IsAuthorized(string authenticationToken, HttpActionEnumCombinationRule combineRule = HttpActionEnumCombinationRule.Any, PermissionType permission = PermissionType.Write, int[] actions = null)
        {
            if (string.IsNullOrWhiteSpace(authenticationToken))
            {
                throw new AuthenticationException(AuthenticationType.User, "AuthenticationException.UnAuthorizedRequest");
            }

            return _userService.IsValidToken(authenticationToken);
        }

        public void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("/Account/Login");
            return;

            if (WebSessionManager.Instance.CurrentUserId <= 0)
            {
                //filterContext.Result = new RedirectResult("/Account/Login?returnUrl=" +
                //    HttpUtility.UrlEncode(filterContext.RequestContext.HttpContext.Request.Url.PathAndQuery));
                filterContext.Result = new RedirectResult("/Account/Logout");
                return;
            }

            if (filterContext.HttpContext.Request.AcceptTypes.Any(t => t.Contains("json")))
            {
                var result = new JsonResult();
                result.Data = "Lỗi 401- Không được quyền truy cập - vui lòng liên hệ với quản trị viên của WEBNHATHUOC.";
                filterContext.Result = result;
                filterContext.HttpContext.Response.StatusCode = 401;
                filterContext.HttpContext.Response.End();
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(
                    new
                    {
                        controller = "Account",
                        action = "Unauthorised",
                    }));
            }
        }
    }
}