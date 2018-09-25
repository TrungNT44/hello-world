using App.Common.Extensions;
using App.Common.Http;
using App.Common.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Med.Common;
using Med.Common.Enums;
using Med.Service.Report;
using App.Common.Validation;
using System.Net;
using System.Web.Mvc;
using App.Common.DI;
using Med.ServiceModel.Common;
using Med.ServiceModel.Report;
using Med.ServiceModel.Request;
using Med.Web.Data.Session;
using MedMan;
using MedMan.App_Start;
using System.Dynamic;
using sThuoc.Filter;

namespace Med.Web.Areas.Production.Controllers
{
    public class SearchController : BaseController
    {
        [HttpPost]
        public JsonResult SearchDrugs()
        {
            var requestParams =  this.ToRequestParams<SearchRequestModel>();
            IResponseData<Object> response = new ResponseData<Object>();
            if (requestParams.searchType != (int)SearchType.Drug) 
            {
                response.SetData(null);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }
            else
            {
                var data = MainApp.Instance.GetCacheDrugs(WebSessionManager.Instance.CurrentDrugStoreCode,
                requestParams.searchText);

                response.SetData(data);
            }

            return Json(response);
        }

        [HttpPost]
        public JsonResult GetFilterItems()
        {
            var requestParams = this.ToRequestParams<FilterRequestModel>();
            IResponseData<Object> response = new ResponseData<Object>();
            if (requestParams.filterItemType < (int)ItemFilterType.DrugGoup)
            {
                response.SetData(null);
                response.SetStatus(HttpStatusCode.PreconditionFailed);
            }
            else
            {
                int? currentUserId = null;
                if (User.IsInRole(Constants.Security.Roles.User.Value))
                {
                    currentUserId = WebSecurity.GetCurrentUserId;
                }
                var service = IoC.Container.Resolve<IReportService>();
                var data = service.GetFilterItems(WebSessionManager.Instance.CurrentDrugStoreCode,
                    (ItemFilterType)requestParams.filterItemType, currentUserId, requestParams.optionAllItems);

                response.SetData(data);
            }

            return Json(response);
        }
    }
}