using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using sThuoc.DAL;

namespace sThuoc.Models
{
    public class KhachHang:BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaKhachHang { get; set; }
        [Display(Name = "Khách hàng"),Required]
        public string TenKhachHang { get; set; }
        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }
        [Display(Name = "Số ĐT")]
        public string SoDienThoai { get; set; }
        [Display(Name = "Nợ ĐK")]
        public decimal? NoDauKy { get; set; }
        [Display(Name = "Đơn vị công tác")]
        public string DonViCongTac { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; }
        [Display(Name = "Mã Nhà Thuốc")]
        public string MaNhaThuoc { get; set; }
        [Display(Name = "Nhóm khách")]
        public int MaNhomKhachHang { get; set; }
        public int? CustomerTypeId { get; set; }
        public virtual NhaThuoc NhaThuoc { get; set; }
        public virtual NhomKhachHang NhomKhachHang { get; set; }
        [NotMapped]
        public int Order { get; set; }

        public virtual ICollection<PhieuXuat> PhieuXuats { get; set; }
        public virtual ICollection<PhieuNhap> PhieuNhaps { get; set; }
        public virtual ICollection<PhieuThuChi> PhieuThuChis { get; set; }        
    }
}