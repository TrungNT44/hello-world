using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using sThuoc.DAL;

namespace sThuoc.Models
{
    public class NhomThuoc:BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaNhomThuoc { get; set; }
        [Display(Name = "Tên nhóm thuốc"),Required]
        public string TenNhomThuoc { get; set; }
        [Display(Name = "Ghi chú")]
        public string KyHieuNhomThuoc { get; set; }
        [Display(Name = "Mã nhà thuốc")]
        public string MaNhaThuoc { get; set; }
        public virtual NhaThuoc NhaThuoc { get; set; }

        public virtual ICollection<Thuoc> Thuocs { get; set; }
    }
}