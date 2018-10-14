using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sThuoc.Models
{
    public class LoaiThuChi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaLoaiPhieu { get; set; }
        [Display(Name = "Tên Loại Thu Chi"), Required]
        public string TenLoaiThuChi { get; set; }        
    }
}