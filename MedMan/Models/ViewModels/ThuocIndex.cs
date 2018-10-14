using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace sThuoc.Models.ViewModels
{
    public class ThuocIndex
    {
        
    }

    public class ThuocIndexViewModel
    {
        public string MaThuoc { get; set; }
        public string TenDayDu { get; set; }
        public string TenNhomThuoc { get; set; }
        public int GiaNhap { get; set; }
        public int GiaBanLe { get; set; }
        public string TenDonViTinh { get; set; }
        public int? GioiHan { get; set; }
    }
}
