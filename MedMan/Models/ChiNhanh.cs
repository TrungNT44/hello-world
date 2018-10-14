using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sThuoc.Models
{
    public class ChiNhanh
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaChiNhanh { get; set; }
        [Display(Name = "Tên Chi Nhánh"),Required]
        public string TenChiNhanh { get; set; }

        [Display(Name = "Người phụ trách")]
        public string NguoiPhuTrach { get; set; }

        [Display(Name = "Địa chỉ chi nhánh")]
        public string DiaChiChiNhanh { get; set; }
        public virtual NhaThuoc NhaThuoc { get; set; }        
    }
}