using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sThuoc.Models
{
    public class NhaThuocThamSo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Display(Name = "Giá trị"), Required]
        public string GiaTri { get; set; }

        public virtual NhaThuoc NhaThuoc { get; set; }
        public virtual ThamSoNhaThuoc ThamSoNhaThuoc { get; set; }
    }
}