using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using sThuoc.DAL;

namespace sThuoc.Models
{
    public class PhieuThuChi:BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaPhieu { get; set; }
        [DisplayFormat(DataFormatString = "{0: #,##0} VND")]
        public decimal Amount { get; set; }
        public int SoPhieu { get; set; }
        public string DienGiai { get; set; }
        [DataType(DataType.DateTime), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime NgayTao { get; set; }
        public int LoaiPhieu { get; set; }
        public virtual NhaThuoc NhaThuoc { get; set; }
        public virtual KhachHang KhachHang { get; set; }
        public virtual NhaCungCap NhaCungCap { get; set; }
        public string NguoiNhan { get; set; }
        public string DiaChi { get; set; }
        public virtual ICollection<ChiNhanh> ChiNhanhs { get; set; }
        public virtual LoaiThuChi LoaiThuChi { get; set; }
    }

    public class PhieuThuChiEditModel
    {
        public int MaPhieu { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public int SoPhieu { get; set; }
        public string DienGiai { get; set; }
        public DateTime NgayTao { get; set; }
        public int LoaiPhieu { get; set; }
        public string MaNhaThuoc { get; set; }
        //public NhaThuoc NhaThuoc { get; set; }
        public int MaKhachHang { get; set; }
        public int MaNhaCungCap { get; set; }
        public string NguoiNhan { get; set; }
        public string DiaChi { get; set; }
        public string NguoiLapPhieu { get; set; }

        public PhieuThuChiEditModel()
        {

        }

        public PhieuThuChiEditModel(PhieuThuChi phieuthuchi)
        {
            MaPhieu = phieuthuchi.MaPhieu;
            Amount = phieuthuchi.Amount;
            SoPhieu = phieuthuchi.SoPhieu;
            DienGiai = phieuthuchi.DienGiai;
            NgayTao = phieuthuchi.NgayTao;
            LoaiPhieu = phieuthuchi.LoaiPhieu;
            MaNhaThuoc = phieuthuchi.NhaThuoc.MaNhaThuoc;
            if(phieuthuchi.KhachHang != null)
            {
                MaKhachHang = phieuthuchi.KhachHang.MaKhachHang;
            }
            if(phieuthuchi.NhaCungCap != null)
            {
                MaNhaCungCap = phieuthuchi.NhaCungCap.MaNhaCungCap;
            }
            NguoiNhan = phieuthuchi.NguoiNhan;
            DiaChi = phieuthuchi.DienGiai;
            NguoiLapPhieu = phieuthuchi.CreatedBy.TenDayDu;           

        }
    }
    
}