using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace sThuoc.Models
{
    public class NhanVienNhaThuoc
    {
        [Key]
        public long Id { get; set; }

        public virtual NhaThuoc NhaThuoc { get; set; }
        public virtual UserProfile User { get; set; }
        public string Role { get; set; }
    }
}