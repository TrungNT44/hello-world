
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web.Security;
using MedMan.App_Start;
using WebGrease.Css.Extensions;

namespace Med.Web.Extensions
{
    public static class UserExtensions
    {
        /// <summary>
        /// Check for multiple roles.
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public static bool IsInRoles(this IPrincipal principal, string roles)
        {
            var arrRole = roles.Split(',');
            var isInRoles = false;
            arrRole.ForEach(e =>
            {
                var role = e.Trim();
                if (principal.IsInRole(role))
                    isInRoles = true;
            });
            return isInRoles;
        }
    }
}