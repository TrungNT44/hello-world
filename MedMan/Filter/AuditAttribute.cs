using App.Common.DI;
using App.Common.FaultHandling;
using Med.Entity.Log;
using Med.Service.Log;
using Med.Web.Data.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Mvc;

namespace Med.Web.Filter
{
    public class AuditAttribute : System.Web.Mvc.ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                // Stores the Request in an Accessible object
                var request = filterContext.HttpContext.Request;
                // Generate an audit
                var audit = new Audit()
                {
                    // Your Audit Identifier     
                    AuditID = Guid.NewGuid(),
                    // Our Username (if available)
                    UserName = (request.IsAuthenticated) ? filterContext.HttpContext.User.Identity.Name : "Anonymous",
                    UserID = WebSessionManager.Instance.CurrentUserId,
                    DrugStoreCode = WebSessionManager.Instance.CurrentDrugStoreCode,
                    // The IP Address of the Request
                    IPAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? request.UserHostAddress,
                    // The URL that was accessed
                    AreaAccessed = request.RawUrl,
                    // Creates our Timestamp
                    CreatedDateTime = DateTime.UtcNow
                };
                var service = IoC.Container.Resolve<IAuditLogService>();
                service.Add(audit);
            }
            catch (Exception ex)
            {
                FaultHandler.Instance.Handle(ex, this);
            }                     

            // Finishes executing the Action as normal 
            base.OnActionExecuting(filterContext);
        }
    }
}