using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Med.Web.Data.Session;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using MedMan.App_Start;
using sThuoc.Filter;
using sThuoc.Models.ViewModels;
using WebGrease.Css.Extensions;
using Med.Common;
using App.Common.DI;
using Med.Service.Utilities;

namespace Med.Web.Extensions
{
    public static class ViewExtensions
    {
        public static NhaThuocSessionModel GetNhaThuoc(this WebViewPage view,bool redirect =true)
        {
            if (view.Session["nhathuoc"] != null)
            {
                var sessModel = JsonConvert.DeserializeObject<NhaThuocSessionModel>(view.Session["nhathuoc"].ToString());
                if (sessModel != null)
                {
                    WebSessionManager.Instance.CurrentDrugStoreCode = sessModel.MaNhaThuoc;
                    var dsSession = new DrugStoreSession()
                    {
                        DrugStoreCode = sessModel.MaNhaThuoc,
                        ParentDrugStoreCode = sessModel.MaNhaThuocCha,
                        DrugStoreID = sessModel.DrugStoreID
                    };
                    var service = IoC.Container.Resolve<IUtilitiesService>();
                    dsSession.Settings = service.GetDrugStoreSetting(dsSession.DrugStoreCode);
                    WebSessionManager.Instance.CommonSessionData = dsSession;
                }

                return sessModel;
            }
            else
            {
                if (redirect)
                {
                    view.Response.RedirectToRoute("ChonNhaThuocMacDinh");
                    view.Response.End();
                }
            }
            return null;
        }
        public static bool MultiNhaThuoc(this WebViewPage view)
        {
            return WebSessionManager.Instance.NumberOfDrugStores > 1;
        }

        public static bool IsNhaThuocAdmin(this WebViewPage view)
        {
            var nhaThuoc = GetNhaThuoc(view, false);
            if (nhaThuoc == null)
                return false;
            if (nhaThuoc.Role == Constants.Security.Roles.Admin.Value|| nhaThuoc.Role==Constants.Security.Roles.SuperUser.Value)
                return true;
            return false;
        }

        public static bool HasPermisson(this WebViewPage view)
        {
            var controller = view.ViewContext.RouteData.Values["controller"].ToString();
            var action = view.ViewContext.RouteData.Values["action"].ToString();
            return FunctionsService.Authorize(controller, action, view.GetNhaThuoc(false));
        }
        public static bool HasPermisson(this WebViewPage view,string controller,string action)
        {
            return FunctionsService.Authorize(controller.ToLower(), action.ToLower(), view.GetNhaThuoc(false));
        }

        public static string GetFirstErrorMessage(this ModelStateDictionary modelState)
        {
            var error = "";
            modelState.ForEach(m=> m.Value.Errors.ForEach(e =>
            {
                error= e.ErrorMessage;
            }));
            return error;
        }
    }
}