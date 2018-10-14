using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sThuoc.Models
{
    public class Nuoc
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaNuoc { get; set; }
        [Display(Name = "Tên Nước"),Required]
        public string TenNuoc { get; set; }
        [Display(Name = "Mã Nước"), Required]
        public string Code { get; set; }

        public virtual ICollection<Thuoc> Thuocs { get; set; }
    }
}