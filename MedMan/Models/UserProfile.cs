using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using sThuoc.DAL;

namespace sThuoc.Models
{
    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
         [Display(Name = "Tên Đăng Nhập"),Required]
        public string UserName { get; set; }
         [Display(Name = "Tên Đầy Đủ"),Required]
        public string TenDayDu { get; set; }
        [EmailAddress]
        public string Email { get; set; }
         [Display(Name = "Số Điện Thoại")]
        public string SoDienThoai { get; set; }
         [Display(Name = "Số CMT")]
        public string SoCMT { get; set; }
         [Display(Name = "Mã nhà thuốc")]
         public string MaNhaThuoc { get; set; }
         [Display(Name = "Đang Hoạt Động")]
        public bool HoatDong { get; set; }
        public bool? Enable_NT { get; set; }
        public virtual ICollection<NhanVienNhaThuoc> NhanVienNhaThuocs { get; set; } 
        public virtual ICollection<PhieuNhap> PhieuNhaps { get; set; }
        public virtual ICollection<PhieuXuat> PhieuXuats { get; set; }
        public virtual ICollection<PhieuKiemKe> PhieuKiemKes { get; set; }
        public virtual ICollection<PhieuThuChi> PhieuThuChis { get; set; }
        public virtual ICollection<UserPermission> UserPermissions { get; set; }
    }

    
}
