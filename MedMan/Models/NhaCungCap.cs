using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using sThuoc.DAL;

namespace sThuoc.Models
{
    public class NhaCungCap:BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaNhaCungCap { get; set; }
        [Display(Name = "Nhà cung cấp"),Required]
        public string TenNhaCungCap { get; set; }
        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }
        [Display(Name = "Số ĐT")]
        public string SoDienThoai { get; set; }
        [Display(Name = "Số fax")]
        public string SoFax { get; set; }
        [Display(Name = "Mã số thuế")]
        public string MaSoThue { get; set; }
        [Display(Name = "Người đại diện")]
        public string NguoiDaiDien { get; set; }
        [Display(Name = "Người liên hệ")]
        public string NguoiLienHe { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Display(Name = "Nợ ĐK")]
        public decimal? NoDauKy { get; set; }
        [Display(Name = "Mã nhà thuốc")]
        public string MaNhaThuoc { get; set; }
        [Display(Name = "Tên nhóm")]
        public int MaNhomNhaCungCap { get; set; }
        public int? SupplierTypeId { get; set; }
        public virtual NhaThuoc NhaThuoc { get; set; }
        public virtual NhomNhaCungCap NhomNhaCungCap { get; set; }

        [NotMapped]
        public int Order { get; set; }
        public virtual ICollection<PhieuNhap> PhieuNhaps { get; set; }
        public virtual ICollection<PhieuXuat> PhieuXuats { get; set; }
        public virtual ICollection<PhieuThuChi> PhieuThuChis { get; set; }
    }
}