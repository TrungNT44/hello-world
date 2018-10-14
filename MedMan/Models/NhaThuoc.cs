using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using sThuoc.DAL;
using System.ComponentModel.DataAnnotations.Schema;

namespace sThuoc.Models
{
    public class NhaThuoc:BaseModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? ID { get; set; }

        [Key]
        [Display(Name = "Mã nhà thuốc"), Required]
        public string MaNhaThuoc { get; set; }
        [Display(Name = "Tên nhà thuốc"),Required]
        public string TenNhaThuoc { get; set; }
        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }
        [Display(Name = "Số kinh doanh")]
        public string SoKinhDoanh { get; set; }
        [Display(Name = "Điện thoại")]
        public string DienThoai { get; set; }
        [Display(Name = "Người đại diện")]
        public string NguoiDaiDien { get; set; }
        public int? TinhThanhId { set; get; }
        [Display(Name = "Hoạt động")]
        public bool HoatDong { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [Display(Name = "Di động")]
        public string Mobile { get; set; }
        [Display(Name = "Dược sỹ")]
        public string DuocSy { get; set; }
        [Display(Name ="Nhà thuốc quản lý")]
        public string MaNhaThuocCha { get; set; }
        public virtual ICollection<NhanVienNhaThuoc> Nhanviens { get; set; }
        [ForeignKey("MaNhaThuocCha")]
        public virtual ICollection<NhaThuoc> NhaThuocCons { set; get; }

        public virtual NhaThuoc NhaThuocCha { set; get; }
        public virtual ICollection<BacSy> BacSys { get; set; }
        public virtual ICollection<DangBaoChe> DangBaoChes { get; set; }
        public virtual ICollection<DonViTinh> DonViTinhs { get; set; }
        public virtual ICollection<KhachHang> KhachHangs { get; set; }
        public virtual ICollection<NhaCungCap> NhaCungCaps { get; set; }
        public virtual ICollection<NhomKhachHang> NhomKhachHangs { get; set; }
        public virtual ICollection<NhomNhaCungCap> NhomNhaCungCaps { get; set; }
        public virtual ICollection<NhomThuoc> NhomThuocs { get; set; }        
        public virtual ICollection<PhieuNhap> PhieuNhaps { get; set; }
        //public virtual ICollection<PhieuNhapChiTiet> PhieuNhapChiTiets { get; set; }
        public virtual ICollection<PhieuXuat> PhieuXuats { get; set; }
        //public virtual ICollection<PhieuXuatChiTiet> PhieuXuatChiTiets { get; set; }
        public virtual ICollection<PhieuKiemKe> PhieuKiemKes { get; set; }
        //public virtual ICollection<PhieuKiemKeChiTiet> PhieuKiemKeChiTiets { get; set; }
        public virtual ICollection<Thuoc> Thuocs { get; set; }
        public virtual ICollection<Thuoc> Thuocs_Sua { get; set; }
        public virtual ICollection<PhieuThuChi> PhieuThuChis { get; set; }
        public virtual ICollection<UserPermission> UserPermissions { get; set; }
    }
}