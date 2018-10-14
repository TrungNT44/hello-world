using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sThuoc.Models
{
    public class DonViTinh
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaDonViTinh { get; set; }
        [Display(Name = "Tên Đơn Vị Tính"),Required]
        public string TenDonViTinh { get; set; }
        [Display(Name = "Mã Nhà Thuốc")]
        public string MaNhaThuoc { get; set; }
        public virtual NhaThuoc NhaThuoc { get; set; }

        
        public virtual ICollection<PhieuNhapChiTiet> PhieuNhapChiTiets { get; set; }
        public virtual ICollection<PhieuXuatChiTiet> PhieuXuatChiTiets { get; set; }
    }
}