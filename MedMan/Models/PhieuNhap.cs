using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using OfficeOpenXml.FormulaParsing.Utilities;
using PagedList;
using sThuoc.DAL;
using App.Constants.Enums;

namespace sThuoc.Models
{
    public class PhieuNhap : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaPhieuNhap { get; set; }
        [Display(Name = "Mã số")]
        public long SoPhieuNhap { get; set; }
        [DataType(DataType.DateTime), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày")]
        public DateTime? NgayNhap { get; set; }
        public int VAT { get; set; }
        [Display(Name = "Diễn giải")]
        public string DienGiai { get; set; }
        [Display(Name = "Tổng tiền")]
        public decimal TongTien { get; set; }
        [Display(Name = "Đã trả")]
        public decimal DaTra { get; set; }
        [Display(Name = "Đã xóa")]
        [NotMapped]
        public bool Xoa { get { return RecordStatusID == (byte)RecordStatus.Deleted; } }
        public virtual NhaThuoc NhaThuoc { get; set; }
        public virtual LoaiXuatNhap LoaiXuatNhap { get; set; }
        public virtual NhaCungCap NhaCungCap { get; set; }
        public virtual KhachHang KhachHang { get; set; }
        public virtual ICollection<ChiNhanh> ChiNhanhs { get; set; }
        public virtual IList<PhieuNhapChiTiet> PhieuNhapChiTiets { get; set; }
        public bool? Locked { get; set; }
        public bool? IsDebt { get; set; }
        public DateTime? PreNoteDate { get; set; }
        public byte RecordStatusID { get; set; }
    }

    public class GetThuocsResultSet
    {
        public int value { get; set; }

        public string label { get; set; }

        public string desc { get; set; }
        public double? soLuong { get; set; }
        public string maThuoc { get; set; }
        public int heSo { get; set; }
        public int donViXuat { get; set; }
        public int donViTN { get; set; }
        public int giaBan { get; set; }
        public int giaBuon { get; set; }
        public int giaNhap { get; set; }
    }

    public class PhieusModel
    {
        public PhieusModel()
        {
            PhieuNhaps = new PagedList<PhieuNhap>(new List<PhieuNhap>(), 1, 1);
            PhieuXuats = new PagedList<PhieuXuat>(new List<PhieuXuat>(), 1, 1);
        }
        public IPagedList<PhieuNhap> PhieuNhaps { get; set; }
        public IPagedList<PhieuXuat> PhieuXuats { get; set; }
    }

    public class PhieuNhapEditModel
    {
        public int? MaPhieuNhap { get; set; }
        [Display(Name = "Số Phiếu Nhập")]
        public long SoPhieuNhap { get; set; }
        [DataType(DataType.DateTime), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày Nhập")]
        public DateTime? NgayNhap { get; set; }
        public int VAT { get; set; }
        [Display(Name = "Diễn Giải")]
        public string DienGiai { get; set; }
        [Display(Name = "Tổng Tiền")]
        public decimal TongTien { get; set; }
        [NotMapped]
        public decimal TongTien_Chua_VAT { get; set; }
        [Display(Name = "Đã Trả")]
        public decimal DaTra { get; set; }
        [Display(Name = "Đã Xóa")]
        public bool Xoa { get; set; }
        public string MaNhaThuoc { get; set; }
        public int MaLoaiXuatNhap { get; set; }
        public int MaNhaCungCap { get; set; }
        public string TenNhaCungCap { get; set; }
        public int MaKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public string NguoiLapPhieu { get; set; }
        public DateTime? NgayLapPhieu { get; set; }
        public IList<PhieuNhapChiTietEditModel> PhieuNhapChiTiets { get; set; }

        public bool? Locked { get; set; }
        public PhieuNhapEditModel()
        {

        }
        public PhieuNhapEditModel(PhieuNhap phieuNhap)
        {
            VAT = phieuNhap.VAT;
            Xoa = phieuNhap.Xoa;
            Locked = phieuNhap.Locked;
            TongTien = phieuNhap.TongTien;
            SoPhieuNhap = phieuNhap.SoPhieuNhap;
            NgayNhap = phieuNhap.NgayNhap;
            MaPhieuNhap = phieuNhap.MaPhieuNhap;
            MaNhaThuoc = phieuNhap.NhaThuoc.MaNhaThuoc;
            NguoiLapPhieu = phieuNhap.CreatedBy.TenDayDu;
            NgayLapPhieu = phieuNhap.Created;
            if (phieuNhap.NhaCungCap != null)
            {
                MaNhaCungCap = phieuNhap.NhaCungCap.MaNhaCungCap;
                TenNhaCungCap = phieuNhap.NhaCungCap.TenNhaCungCap;
            }
            if (phieuNhap.KhachHang != null)
            {
                MaKhachHang = phieuNhap.KhachHang.MaKhachHang;
                TenKhachHang = phieuNhap.KhachHang.TenKhachHang;
            }
            MaLoaiXuatNhap = phieuNhap.LoaiXuatNhap.MaLoaiXuatNhap;

            DienGiai = phieuNhap.DienGiai;
            DaTra = phieuNhap.DaTra;
            if (phieuNhap.PhieuNhapChiTiets.Any())
            {                     
                PhieuNhapChiTiets = phieuNhap.PhieuNhapChiTiets.Where(i => i.RecordStatusID == (byte)RecordStatus.Activated).Select(e => new PhieuNhapChiTietEditModel()
                {
                    ChietKhau = ((double)e.ChietKhau).ToString(),
                    GiaNhap = e.GiaNhap,
                    GiaBan = e.DonViTinh.MaDonViTinh == e.Thuoc.DonViXuatLe.MaDonViTinh ? e.Thuoc.GiaBanLe : e.Thuoc.GiaBanLe * e.Thuoc.HeSo,
                    HeSo = e.Thuoc.HeSo,
                    MaDonViTinh = e.DonViTinh.MaDonViTinh,
                    MaDonViTinhLe = e.Thuoc.DonViXuatLe.MaDonViTinh,
                    MaDonViTinhThuNguyen = e.Thuoc.DonViThuNguyen != null ? e.Thuoc.DonViThuNguyen.MaDonViTinh : 0,
                    MaPhieuNhap = e.PhieuNhap.MaPhieuNhap,
                    MaThuoc = e.Thuoc.MaThuoc,
                    ThuocId = e.Thuoc.ThuocId,
                    SoLuong = e.SoLuong,
                    TenDonViTinh = e.DonViTinh.TenDonViTinh,
                    TenThuoc = e.Thuoc.TenThuoc,
                    MaPhieuNhapCt = e.MaPhieuNhapCt,
                    SoLo = e.SoLo,
                    HanDung = e.HanDung

                }).ToList();
            }
            decimal tong_tien = 0;
            foreach(PhieuNhapChiTietEditModel item in PhieuNhapChiTiets)
            {
                tong_tien += (item.GiaNhap * item.SoLuong);
            }
            TongTien_Chua_VAT = tong_tien;
        }
    }

    public class PhieuNhapChiTietEditModel
    {
        public int? MaPhieuNhapCt { get; set; }
        [Display(Name = "Thuốc")]
        public int ThuocId { get; set; }
        public string MaThuoc { get; set; }
        public string TenThuoc { get; set; }
        public int HeSo { get; set; }


        public int MaPhieuNhap { get; set; }
        public string MaNhaThuoc { get; set; }
        [Display(Name = "Đơn Vị Tính")]
        public int MaDonViTinh { get; set; }
        public int MaDonViTinhLe { get; set; }
        public int MaDonViTinhThuNguyen { get; set; }
        public string TenDonViTinh { get; set; }
        [Display(Name = "Chiết Khấu")]
        public string ChietKhau { get; set; }
        //public string SChietKhau { get; set; }
        [Display(Name = "Giá Nhập")]
        public decimal GiaNhap { get; set; }
        [Display(Name = "Giá Bán")]
        public decimal GiaBan { get; set; }
        [Display(Name = "Số Lượng")]
        public decimal SoLuong { get; set; }
        [Display(Name = "Số Lô")]
        public string SoLo { get; set; }
        [Display(Name = "Hạn dùng")]
        public DateTime? HanDung { get; set; }
        

    }
}