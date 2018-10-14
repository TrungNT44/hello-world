using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web.Mvc;
using App.Common.MVC;
using sThuoc.Filter;
using sThuoc.Models;

namespace Med.Web.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {

            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        [Authorize(Roles = "admin,user")]
        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        [Authorize]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            var user = WebSecurity.GetCurrentUser();
            ViewBag.Email = user.Email;
            //Show how to get roles from claims principal
            var prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var roles = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
            ViewBag.Roles = roles;
            return View();
        }

        [SimpleAuthorize(1, Operations.Read) ]
        public ActionResult ReadFoo()
        {
            ViewBag.Message = "Read Foo";

            return View();
        }

        [SimpleAuthorize(1, Operations.Create)]
        public ActionResult WriteFoo()
        {
            ViewBag.Message = "Write Foo";

            return View();
        }

    }
}
