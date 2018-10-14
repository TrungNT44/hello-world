using App.Common.Http;
using App.Common.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Med.Service.Report;
using App.Common.Validation;
using System.Net;
using System.Web.Mvc;
using App.Common.DI;

namespace Med.Web.Areas.Production.Controllers
{
    public class ProductionController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPut]
        public IResponseData<string> UpdateFuncTest(int id)
        {
            IResponseData<string> response = new ResponseData<string>();
            try
            {
                var service = IoC.Container.Resolve<IReportService>();
                // Call service function
                
            }
            catch (ValidationException ex)
            {
                response.SetErrors(ex.Errors);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }
            return response;
        }
    }
}