using System.Linq;
using System.Web.Security;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using MedMan.App_Start;
using sThuoc.Models;
using sThuoc.Models.ViewModels;
using System.Web.Mvc;
using WebMatrix.WebData;
using WebSecurity = sThuoc.Filter.WebSecurity;
using Med.Web.Data.Session;
using Med.Common;
using App.Common.DI;
using Med.Service.Utilities;

namespace Med.Web.Extensions
{
    public static class ControllerExtensions
    {
        public static void SetNhaThuoc(this Controller controller, NhaThuoc nhaThuoc)
        {
            var nhathuocSession = new NhaThuocSessionModel(nhaThuoc);
            var loggedUser = WebSecurity.GetCurrentUser();
            WebSessionManager.Instance.CurrentDrugStoreCode = nhaThuoc.MaNhaThuoc;
            var dsSession = new DrugStoreSession()
            {
                DrugStoreCode = nhaThuoc.MaNhaThuoc,
                ParentDrugStoreCode = nhaThuoc.MaNhaThuocCha,
                DrugStoreID = nhaThuoc.ID
            };
            var service =  IoC.Container.Resolve<IUtilitiesService>();
            dsSession.Settings = service.GetDrugStoreSetting(dsSession.DrugStoreCode);
            WebSessionManager.Instance.CommonSessionData = dsSession;

            WebSessionManager.Instance.CurrentUserId = loggedUser.UserId;
            if (Roles.Provider.IsUserInRole(loggedUser.UserName,
                Constants.Security.Roles.SuperUser.Value))
            {
                nhathuocSession.Role = Constants.Security.Roles.SuperUser.Value;
            }
            else
            {
                var nhanVien = nhaThuoc.Nhanviens.FirstOrDefault(e => e.User.UserId == loggedUser.UserId);
                if (nhanVien == null)
                    if(nhaThuoc.NhaThuocCha != null)
                    {
                        nhanVien = nhaThuoc.NhaThuocCha.Nhanviens.FirstOrDefault(e => e.User.UserId == loggedUser.UserId);
                    }
                if (nhanVien != null)
                    nhathuocSession.Role = nhanVien.Role;
            }
            controller.Session["nhathuoc"] = JsonConvert.SerializeObject(nhathuocSession);
        }
        public static NhaThuocSessionModel GetNhaThuoc(this Controller controller)
        {
            if (controller.Session != null && controller.Session["nhathuoc"] != null)
                return JsonConvert.DeserializeObject<NhaThuocSessionModel>(controller.Session["nhathuoc"].ToString());
            else
            {
                controller.Response.RedirectToRoute("ChonNhaThuocMacDinh");
                controller.Response.End();
            }
            return null;
        }

    }
}