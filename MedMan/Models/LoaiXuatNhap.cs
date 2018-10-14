using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sThuoc.Models
{
    public class LoaiXuatNhap
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaLoaiXuatNhap { get; set; }
        [Display(Name = "Tên Loại Xuất Nhập"),Required]
        public string TenLoaiXuatNhap { get; set; }
        public bool? IsHidden { get; set; }
    }
}