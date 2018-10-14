using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sThuoc.Models
{
    public class ThuocChiNhanh
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaChiNhanh { get; set; }
        [Display(Name = "Giá Nhập"), Required(ErrorMessage = "Giá nhập không được bỏ trống")]
        public decimal GiaNhap { get; set; }
        [Display(Name = "Giá bán buôn"), Required(ErrorMessage = "Gái bán buôn không được bỏ trống")]
        public decimal GiaBanBuon { get; set; }
        [Display(Name = "Giá bán lẻ"), Required(ErrorMessage = "Gái bán lẻ không được bỏ trống")]
        public decimal GiaBanLe { get; set; }
        [Display(Name = "Số dư đầu kỳ")]
        public decimal SoDuDauKy { get; set; }
        [Display(Name = "Giá đầu kỳ")]
        public decimal GiaDauKy { get; set; }
        public DateTime? HanDung { get; set; }
        [Display(Name = "Dự trù")]
        public int DuTru { get; set; }
        [Display(Name = "Hoạt động")]
        public bool HoatDong { get; set; }

        public string DiaChiChiNhanh { get; set; }
        public virtual NhaThuoc NhaThuoc { get; set; }
        public virtual ChiNhanh ChiNhanh { get; set; }
        public virtual Thuoc Thuoc { get; set; }  
    }
}