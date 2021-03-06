﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sThuoc.Models
{
    public class ThamSoNguoiDung
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaThamSoNguoiDung { get; set; }
        [Display(Name = "Mô tả tham số"),Required]
        public string MoTaThamSo { get; set; }

        [Display(Name = "Mô tả giá trị"), Required]
        public string MoTaGiaTri { get; set; }
    }
}