using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using MedMan.App_Start;
using WebGrease.Css.Extensions;

namespace Med.Web.Extensions
{
    public static class RoleProviderExtensions
    {
        public static IEnumerable<string> GetTextOfRolesForUser(this RoleProvider roleProvider, string UserName)
        {
            var roles = roleProvider.GetRolesForUser(UserName);
            return roles.Select(e => Constants.Security.Roles.RoleTexts[e]).AsEnumerable();
        }
    }
}