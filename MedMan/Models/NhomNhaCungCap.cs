using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using sThuoc.DAL;

namespace sThuoc.Models
{
    public class NhomNhaCungCap:BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaNhomNhaCungCap { get; set; }
        [Display(Name = "Nhóm nhà cung cấp"),Required]
        public string TenNhomNhaCungCap { get; set; }
        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; }
        [Display(Name = "Mã nhà thuốc")]
        public string MaNhaThuoc { get; set; }
        public virtual NhaThuoc NhaThuoc { get; set; }
        public virtual ICollection<NhaCungCap> NhaCungCaps { get; set; }
        public bool? IsDefault { get; set; }
    }
}