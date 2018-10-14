using System;
using System.ComponentModel.DataAnnotations;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;

namespace sThuoc.Models
{
    public class TongKetKyChiTiet
    {
        [Key]
        public int Id { get; set; }
        public decimal SoLuong { get; set; }
        public decimal Gia { get; set; }
        public DateTime Date { get; set; }
        public virtual TongKetKy TongKetKy { get; set; }
        public virtual NhaThuoc NhaThuoc { get; set; }
        public virtual Thuoc Thuoc { get; set; }
    }
}