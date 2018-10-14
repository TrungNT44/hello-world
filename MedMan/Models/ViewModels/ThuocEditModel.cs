using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace sThuoc.Models.ViewModels
{
    public class ThuocEditModel
    {
        public int ThuocId { get; set; }
        [Display(Name = "Mã thuốc"), Required(ErrorMessage = "Mã thuốc không được bỏ trống")]
        public string MaThuoc { get; set; }
        [Display(Name = "Tên thuốc"), Required(ErrorMessage = "Tên thuốc không được bỏ trống")]
        public string TenThuoc { get; set; }
        [Display(Name = "Thông tin")]
        public string ThongTin { get; set; }
        [Display(Name = "Tên đầy đủ")]
        public string TenDayDu { get { return TenThuoc + " " + ThongTin; } }
        [Display(Name = "Hệ số"), Required(ErrorMessage = "hệ số phải là số và không được bỏ trống"), RegularExpression(@"(^0|[2-9]$)|(^[1-9]+\d+$)", ErrorMessage = "Hệ số là số nguyên lớn hơn 1")]
        public int HeSo { get; set; }
        [Display(Name = "Giá nhập lẻ"), Required(ErrorMessage = "Giá nhập phải là số và không được bỏ trống")]
        public decimal GiaNhap { get; set; }
        [Display(Name = "Giá bán sỉ"), Required(ErrorMessage = "Gái bán buôn phải là số và không được bỏ trống")]
        public decimal GiaBanBuon { get; set; }
        [Display(Name = "Giá bán lẻ"), Required(ErrorMessage = "Gái bán lẻ phải là số và không được bỏ trống")]
        public decimal GiaBanLe { get; set; }
        [Display(Name = "Số dư b.đầu")] 
        public decimal SoDuDauKy { get; set; }
        [Display(Name = "Giá nhập b.đầu")]
        public decimal GiaDauKy { get; set; }
        [Display(Name = "Giới hạn tồn")]
        public int? GioiHan { get; set; }
        [Display(Name = "Mã nhà thuốc")]
        public string MaNhaThuoc { get; set; }
        [Display(Name = "Mã nhóm thuốc"),Required(ErrorMessage = "Nhóm thuốc chưa được chọn")]
        public int MaNhomThuoc { get; set; }
        [Display(Name = "Nước")]
        public int? MaNuoc { get; set; }
        [Display(Name = "Dạng bào chế")]
        public int? MaDangBaoChe { get; set; }
        public virtual DangBaoChe DangBaoChe { get; set; }
        [Display(Name = "Đơn vị xuất lẻ"), Required(ErrorMessage = "Đơn vị xuất chưa được chọn")]
        public int MaDonViXuat { get; set; }
        //[Display(Name = "Đơn vị thứ nguyên"), Required(ErrorMessage = "Đơn vị thứ nguyên chưa được chọn")]
        [Display(Name = "Đơn vị thứ nguyên")]
        public int? MaDonViThuNguyen { get; set; }
        [Display(Name = "Mã vạch")]
        public string BarCode { get; set; }
        [Display(Name = "Hoạt động")]       
        public bool HoatDong { get; set; }

        [Display(Name = "Hàng tư vấn")]
        public bool HangTuVan { get; set; }

        [Display(Name = "Hạn dùng")]
        public string HanDung { get; set; }

        [Display(Name = "Thuốc Id Ref")]
        public int? ThuocIdRef { get; set; }
    }
}