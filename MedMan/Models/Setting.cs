using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace sThuoc.Models
{
    public class Setting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Key { get; set; }        
        [Required]
        public string Value { get; set; }
        public string MaNhaThuoc { get; set; }
        public bool? Active { get; set; }
        [NotMapped]
        public string TuNgay { get; set; }
        [NotMapped]
        public string DenNgay { get; set; }
    }
}