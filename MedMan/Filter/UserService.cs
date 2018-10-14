using System.Web;
using sThuoc.Repositories;

namespace sThuoc.Filter
{
    public static class UserService
    {
        //public static string GetMaNhaThuoc()
        //{
        //    var uow = new UnitOfWork();
        //    if (HttpContext.Current.Session["MaNhaThuoc"] != null)
        //    {
        //        var maNhaThuoc = HttpContext.Current.Session["MaNhaThuoc"].ToString();
        //        NhaThuocService.NhaThuoc = uow.NhaThuocRepository.GetById(maNhaThuoc);
        //        return maNhaThuoc;
        //    }                
        //    var user = WebMatrix.WebData.WebSecurity.CurrentUserId;
            
        //    if (user != -1)
        //    {
        //        var maNhaThuoc = uow.UserProfileRepository.GetById(user).MaNhaThuoc;
        //        NhaThuocService.NhaThuoc = uow.NhaThuocRepository.GetById(maNhaThuoc);
        //        HttpContext.Current.Session["MaNhaThuoc"] = maNhaThuoc;
        //    }
                
        //    if (HttpContext.Current.Session["MaNhaThuoc"] != null)
        //        return HttpContext.Current.Session["MaNhaThuoc"].ToString();
        //    return "";
        //}

        //public static bool CanAccess(string maNhaThuoc)
        //{
        //    if (maNhaThuoc != GetMaNhaThuoc())
        //        return false;
        //    return true;
        //}
    }
}