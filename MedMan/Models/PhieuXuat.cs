using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using sThuoc.DAL;
using sThuoc.Enums;
using App.Constants.Enums;

namespace sThuoc.Models
{
    public class PhieuXuat : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaPhieuXuat { get; set; }
        [Display(Name = "Mã số")]
        public long SoPhieuXuat { get; set; }
        [Display(Name = "Ngày")]
        public DateTime? NgayXuat { get; set; }
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
        [Display(Name = "Loại Xuất Nhập")]
        public int MaLoaiXuatNhap { get; set; }
        public virtual LoaiXuatNhap LoaiXuatNhap { get; set; }
        [Display(Name = "Khách Hàng")]
        public virtual KhachHang KhachHang { get; set; }
        [Display(Name = "Nhà Cung Cấp")]
        public virtual NhaCungCap NhaCungCap { get; set; }
        [Display(Name = "Bác Sỹ")]
        public virtual BacSy BacSy { get; set; }
        public virtual ICollection<ChiNhanh> ChiNhanhs { get; set; }

        public virtual ICollection<PhieuXuatChiTiet> PhieuXuatChiTiets { get; set; }

        public bool? Locked { get; set; }
        public bool? IsDebt { get; set; }
        public DateTime? PreNoteDate { get; set; }
        public byte RecordStatusID { get; set; }
    }
    public sealed class PhieuXuatEditModel
    {
        public int? MaPhieuXuat { get; set; }
        [Display(Name = "Số Phiếu Xuất")]
        public long SoPhieuXuat { get; set; }
        [DataType(DataType.DateTime), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày Xuất")]
        public DateTime? NgayXuat { get; set; }
        public int VAT { get; set; }
        [Display(Name = "Diễn Giải")]
        public string DienGiai { get; set; }
        [Display(Name = "Tổng Tiền")]
        public decimal TongTien { get; set; }
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
        public int? MaBacSy { get; set; }
        public string TenBacSy { get; set; }
        public string NguoiLapPhieu { get; set; }
        public DateTime? NgayLapPhieu { get; set; }
        public IList<PhieuXuatChiTietEditModel> PhieuXuatChiTiets { get; set; }
        public EBillType BillType { get; set; }
        public string JsonDrugOderItems { get; set; }

        public bool? Locked { get; set; }
        public bool CanTransitWarehouse { get; set; }
        public PhieuXuatEditModel()
        {
            BillType = EBillType.Manual;
            JsonDrugOderItems = string.Empty;
        }
        public PhieuXuatEditModel(PhieuXuat phieuXuat)
        {
            BillType = EBillType.Manual;
            JsonDrugOderItems = string.Empty;
            VAT = phieuXuat.VAT;
            Xoa = phieuXuat.Xoa;
            Locked = phieuXuat.Locked;
            TongTien = phieuXuat.TongTien;
            SoPhieuXuat = phieuXuat.SoPhieuXuat;
            NgayXuat = phieuXuat.NgayXuat;
            MaPhieuXuat = phieuXuat.MaPhieuXuat;
            MaNhaThuoc = phieuXuat.NhaThuoc.MaNhaThuoc;
            NguoiLapPhieu = phieuXuat.CreatedBy.TenDayDu;
            NgayLapPhieu = phieuXuat.Created;
            if (phieuXuat.NhaCungCap != null)
            {
                MaNhaCungCap = phieuXuat.NhaCungCap.MaNhaCungCap;
                TenNhaCungCap = phieuXuat.NhaCungCap.TenNhaCungCap;
            }
            if (phieuXuat.KhachHang != null)
            {
                MaKhachHang = phieuXuat.KhachHang.MaKhachHang;
                TenKhachHang = phieuXuat.KhachHang.TenKhachHang;
            }
            MaLoaiXuatNhap = phieuXuat.LoaiXuatNhap.MaLoaiXuatNhap;
            if (phieuXuat.BacSy != null)
            {
                MaBacSy = phieuXuat.BacSy.MaBacSy;
                TenBacSy = phieuXuat.BacSy.TenBacSy;
            }
            DienGiai = phieuXuat.DienGiai;
            DaTra = phieuXuat.DaTra;
            if (phieuXuat.PhieuXuatChiTiets.Any())
            {
                PhieuXuatChiTiets = phieuXuat.PhieuXuatChiTiets.Where(i => i.RecordStatusID == (byte)RecordStatus.Activated).Select(e => new PhieuXuatChiTietEditModel()
                {
                    ChietKhau = ((double)e.ChietKhau).ToString(),
                    GiaXuat = e.GiaXuat,
                    HeSo = e.Thuoc.HeSo,
                    MaDonViTinh = e.DonViTinh.MaDonViTinh,
                    MaDonViTinhLe = e.Thuoc.DonViXuatLe.MaDonViTinh,
                    MaDonViTinhThuNguyen = e.Thuoc.DonViThuNguyen != null ? e.Thuoc.DonViThuNguyen.MaDonViTinh : 0,
                    MaPhieuXuat = e.PhieuXuat.MaPhieuXuat,
                    MaThuoc = e.Thuoc.MaThuoc,
                    ThuocId = e.Thuoc.ThuocId,
                    SoLuong = e.SoLuong,
                    TenDonViTinh = e.DonViTinh.TenDonViTinh,
                    TenThuoc = e.Thuoc.TenThuoc,
                    MaPhieuXuatCt = e.MaPhieuXuatCt


                }).ToList();
            }

        }
    }
    public class PhieuXuatChiTietEditModel
    {
        public int? MaPhieuXuatCt { get; set; }
        [Display(Name = "Thuốc")]
        public int ThuocId { get; set; }
        public string MaThuoc { get; set; }
        public string TenThuoc { get; set; }
        public int HeSo { get; set; }


        public int MaPhieuXuat { get; set; }
        public string MaNhaThuoc { get; set; }
        [Display(Name = "Đơn Vị Tính")]
        public int MaDonViTinh { get; set; }
        public int MaDonViTinhLe { get; set; }
        public int MaDonViTinhThuNguyen { get; set; }
        public string TenDonViTinh { get; set; }
        [Display(Name = "Chiết Khấu")]
        public string ChietKhau { get; set; }
        [Display(Name = "Giá Xuất")]
        public decimal GiaXuat { get; set; }
        [Display(Name = "Số Lượng")]
        public decimal SoLuong { get; set; }
    }
}