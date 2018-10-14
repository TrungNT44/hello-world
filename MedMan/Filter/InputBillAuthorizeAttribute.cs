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
    public class InputBillAuthorizeAttribute: SimpleAuthorize
    {
        public InputBillAuthorizeAttribute(int resourceId, Operations operation):base(resourceId, operation)
        {
        }

        public InputBillAuthorizeAttribute(string roles):base(roles)
        {
        }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool retVal = base.AuthorizeCore(httpContext);
            if (!retVal)
            {
                var maNhaThuoc = nhaThuoc.MaNhaThuoc;
                if (!string.IsNullOrEmpty(maNhaThuoc))
                {
                    var id = (httpContext.Request.RequestContext.RouteData.Values["id"] as string)
                        ?? (httpContext.Request["id"] as string);
                    int maPhieuNhap = 0;
                    int.TryParse(id, out maPhieuNhap);
                    retVal = FunctionsService.AuthorizeInputBill(WebSecurity.GetCurrentUserId, maNhaThuoc, maPhieuNhap);
                }
            }

            return retVal;
        }
    }
}
