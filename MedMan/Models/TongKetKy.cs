using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace sThuoc.Models
{
    public class TongKetKy
    {
        [Key]
        public long Id { get; set; }
        public DateTime KyTinhToan { get; set; }
        public decimal TonCuoiKy_SoLuong { get; set; }
        public decimal TonCuoiKy_GiaTri { get; set; }
        public decimal TongDoanhThu { get; set; }
        public decimal TongLoiNhuan { get; set; }
        public bool TrangThai { get; set; }
        public virtual NhaThuoc NhaThuoc { get; set; }
        public virtual Thuoc Thuoc { get; set; }
        public virtual ICollection<TongKetKyChiTiet> TongKetKyChiTiets { get; set; }
    }
}