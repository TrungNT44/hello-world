using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using sThuoc.DAL;

namespace sThuoc.Models
{
    public class BacSy:BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaBacSy { get; set; }
        [Required]
        [Display(Name = "Tên bác sỹ")]
        public string TenBacSy { get; set; }
        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }
        [Display(Name = "Số ĐT")]
        public string DienThoai { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [Display(Name = "Mã nhà thuốc")]
        public string MaNhaThuoc { get; set; }
        public virtual NhaThuoc NhaThuoc { get; set; }

        public virtual ICollection<PhieuXuat> PhieuXuats { get; set; }
    }
}