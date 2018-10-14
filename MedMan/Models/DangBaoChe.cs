using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sThuoc.Models
{
    public class DangBaoChe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaDangBaoChe { get; set; }
        [Display(Name = "Tên Dạng Bào Chế"),Required]
        public string TenDangBaoChe { get; set; }
        [Display(Name = "Mã Nhà Thuốc")]
        public string MaNhaThuoc { get; set; }
        public virtual NhaThuoc NhaThuoc { get; set; }

        public virtual ICollection<Thuoc> Thuocs { get; set; }
    }
}