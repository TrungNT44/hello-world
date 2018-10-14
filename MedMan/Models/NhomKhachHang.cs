using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using sThuoc.DAL;

namespace sThuoc.Models
{
    public class NhomKhachHang:BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaNhomKhachHang { get; set; }
        [Display(Name = "Nhóm khách hàng"),Required]
        public string TenNhomKhachHang { get; set; }
        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; }
        [Display(Name = "Mã nhà thuốc")]
        public virtual NhaThuoc NhaThuoc { get; set; }
        public virtual ICollection<KhachHang> KhachHangs { get; set; }
    }
}