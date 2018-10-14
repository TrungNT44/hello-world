
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using sThuoc.Repositories;
using WebGrease.Css.Extensions;
using sThuoc.Utils;
using sThuoc.DAL;
using Med.Web.Controllers;
using App.Constants.Enums;

namespace sThuoc.Models.ViewModels
{
    public class TheoBacSyViewModel
    {
        public long? BacSyId { get; set; }
        public string Period { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? From { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? To { get; set; }
        public List<TheoBacSyItemViewModel> Items { get; set; }
        public int Export { get; set; }
        public override string ToString()
        {
            return string.Format("{0}{1}{2}{3}{4}", BacSyId, Period, From, To, Export);
        }
    }

    public class TheoBacSyItemViewModel : BaoCaoItemBase
    {
        public bool HienThi;
        public int MaBacSy { get; set; }
        public TheoBacSyItemViewModel(PhieuXuatChiTiet phieuXuatCT, List<PhieuNhapChiTiet> listPhieuNhapChiTiet, long stt = 0, List<PhieuNhapMoiNhat> list = null)
        {
            var phieuXuat = phieuXuatCT.PhieuXuat;
            if (stt > 0)
                Stt = stt;
            MaPhieu = phieuXuat.MaPhieuXuat;
            MaNhanVien = phieuXuat.CreatedBy.UserId;
            if (phieuXuat.NgayXuat != null) Date = phieuXuat.NgayXuat.Value;
            SoPhieu = phieuXuat.SoPhieuXuat;
            ChietKhau = phieuXuat.PhieuXuatChiTiets.Max(e => e.ChietKhau);
            if (phieuXuat.KhachHang != null)
                TenKhachHang = phieuXuat.KhachHang.TenKhachHang;
            if (phieuXuat.NhaCungCap != null)
                TenKhachHang = phieuXuat.NhaCungCap.TenNhaCungCap;
            TongTien = phieuXuat.TongTien;
            DaTra = phieuXuat.DaTra;
            TenNhanVien = phieuXuat.CreatedBy != null ? phieuXuat.CreatedBy.TenDayDu : "";
            if (phieuXuat.BacSy != null)
            {
                MaBacSy = phieuXuat.BacSy.MaBacSy;
                TenBacSy = phieuXuat.BacSy.TenBacSy;
            }

            if (string.IsNullOrEmpty(phieuXuatCT.Option1))
                LoiNhuan += sThuoc.Utils.Helpers.GetLoiNhuanLoaiThuoc(listPhieuNhapChiTiet, phieuXuatCT.Thuoc, phieuXuatCT.DonViTinh.MaDonViTinh, phieuXuatCT.SoLuong, phieuXuatCT.GiaXuat, phieuXuatCT.ChietKhau, phieuXuatCT.PhieuXuat.VAT, list);
        }

        public TheoBacSyItemViewModel()
        {

        }
    }

    public class BaoCaoTongHopViewModel
    {
        public string Period { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? From { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? To { get; set; }
    }
    public class TheoNhanVienViewModel
    {
        public int? UserId { get; set; }
        public TheoNhanVienTypes Type { get; set; }
        public string Period { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? From { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? To { get; set; }
        public List<TheoNhanVienItemViewModel> Items { get; set; }
        public int Export { get; set; }
        public override string ToString()
        {
            return string.Format("{0}{1}{2}{3}{4}", UserId, Period, From, To, Export);
        }
    }

    public class TheoNhanVienItemViewModel : BaoCaoItemBase
    {

        public TheoNhanVienItemViewModel(PhieuXuatChiTiet item, List<PhieuNhapChiTiet> listPhieuNhapChiTiet, int stt = 0, List<PhieuNhapMoiNhat> list = null)
        {
            if (stt > 0)
                Stt = stt;
            PhieuXuat phieuXuat = item.PhieuXuat;
            MaNhanVien = phieuXuat.CreatedBy.UserId;
            if (phieuXuat.NgayXuat != null) Date = phieuXuat.NgayXuat.Value;
            SoPhieu = phieuXuat.SoPhieuXuat;
            MaPhieu = phieuXuat.MaPhieuXuat;
            MaLoai = phieuXuat.LoaiXuatNhap.MaLoaiXuatNhap;
            ChietKhau = phieuXuat.PhieuXuatChiTiets.Max(e => e.ChietKhau);
            if (phieuXuat.KhachHang != null)
                TenKhachHang = phieuXuat.KhachHang.TenKhachHang;
            if (phieuXuat.NhaCungCap != null)
                TenKhachHang = phieuXuat.NhaCungCap.TenNhaCungCap;
            TongTien = phieuXuat.TongTien;
            DaTra = phieuXuat.DaTra;
            TenNhanVien = phieuXuat.CreatedBy != null ? phieuXuat.CreatedBy.TenDayDu : "";

            if (string.IsNullOrEmpty(item.Option1))
                LoiNhuan += sThuoc.Utils.Helpers.GetLoiNhuanLoaiThuoc(listPhieuNhapChiTiet, item.Thuoc, item.DonViTinh.MaDonViTinh, item.SoLuong, item.GiaXuat, item.ChietKhau, item.PhieuXuat.VAT, list);
        }

        public TheoNhanVienItemViewModel()
        {

        }
    }

    public enum TheoNhanVienTypes
    {
        All,
        Individual
    }
    public class TheoKhachHangViewModel
    {
        public string Type { get; set; }
        public Dictionary<string, string> Types
        {
            get
            {
                var dict = new Dictionary<string, string>
                {
                    {"all", "Tất cả"},
                    {"bygroup", "Theo nhóm"},
                    {"byname", "Theo tên"}
                };
                return dict;
            }
        }
        public long MaKhachHang { get; set; }
        public long MaNhomKhachHang { get; set; }
        public string Period { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? From { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? To { get; set; }
        public List<TheoKhachHangItemViewModel> Items { get; set; }
        public int Export { get; set; }
        public override string ToString()
        {
            return string.Format("{0}{1}{2}{3}{4}{5}{6}", Type, Period, From, To, Export, MaKhachHang, MaNhomKhachHang);
        }
    }
    public class TheoKhachHangItemViewModel : BaoCaoItemBase
    {
        public TheoKhachHangItemViewModel(PhieuXuatChiTiet phieuXuatCt, List<PhieuNhapChiTiet> listPhieuNhapChiTiet, long stt = 0, List<PhieuNhapMoiNhat> list = null)
        {
            if (stt > 0)
                Stt = stt;
            var phieuXuat = phieuXuatCt.PhieuXuat;
            MaPhieu = phieuXuat.MaPhieuXuat;
            MaNhanVien = phieuXuat.CreatedBy.UserId;
            if (phieuXuat.NgayXuat != null) Date = phieuXuat.NgayXuat.Value;
            SoPhieu = phieuXuat.SoPhieuXuat;
            ChietKhau = phieuXuat.PhieuXuatChiTiets.Max(e => e.ChietKhau);
            if (phieuXuat.KhachHang != null)
            {
                TenKhachHang = phieuXuat.KhachHang.TenKhachHang;
                MaKhachHang = phieuXuat.KhachHang.MaKhachHang;
                MaNhomKhachHang = phieuXuat.KhachHang.MaNhomKhachHang;
            }

            if (phieuXuat.NhaCungCap != null)
            {
                TenKhachHang = phieuXuat.NhaCungCap.TenNhaCungCap;
            }

            Date = phieuXuat.NgayXuat.Value;
            TongTien = phieuXuat.TongTien;
            DaTra = phieuXuat.DaTra;
            TenNhanVien = phieuXuat.CreatedBy != null ? phieuXuat.CreatedBy.TenDayDu : "";

            if (string.IsNullOrEmpty(phieuXuatCt.Option1))
                LoiNhuan += sThuoc.Utils.Helpers.GetLoiNhuanLoaiThuoc(listPhieuNhapChiTiet, phieuXuatCt.Thuoc, phieuXuatCt.DonViTinh.MaDonViTinh, phieuXuatCt.SoLuong, phieuXuatCt.GiaXuat, phieuXuatCt.ChietKhau, phieuXuatCt.PhieuXuat.VAT, list);
        }

        public TheoKhachHangItemViewModel()
        {

        }
    }
    public class TheoNhaCungCapViewModel
    {
        public string Type { get; set; }
        public Dictionary<string, string> Types
        {
            get
            {
                var dict = new Dictionary<string, string>
                {
                    {"all", "Tất cả"},
                    {"bygroup", "Theo nhóm"},
                    {"byname", "Theo tên"}
                };
                return dict;
            }
        }
        public long MaNhaCungCap { get; set; }
        public long MaNhomNhaCungCap { get; set; }
        public string Period { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? From { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? To { get; set; }
        public IList<TheoNhaCungCapItemViewModel> Items { get; set; }
        public TheoNhaCungCapItemViewModel Item { get; set; }
        public int Export { get; set; }
        public override string ToString()
        {
            return string.Format("{0}{1}{2}{3}{4}{5}{6}", Type, Period, From, To, Export, MaNhaCungCap, MaNhomNhaCungCap);
        }
    }
    public class TheoNhaCungCapItemViewModel : BaoCaoItemBase
    {
        public int VAT { get; set; }
        public string TenNhaCungCap { get; set; }
        public decimal TongTraSau { get; set; }
        public TheoNhaCungCapItemViewModel(PhieuNhap phieuNhap,decimal tongTraSau, long stt = 0)
        {
            if (stt > 0)
                Stt = stt;
            if (phieuNhap.NgayNhap != null) Date = phieuNhap.NgayNhap.Value;
            SoPhieu = phieuNhap.SoPhieuNhap;
            MaPhieu = phieuNhap.MaPhieuNhap;
            MaLoai = phieuNhap.LoaiXuatNhap.MaLoaiXuatNhap;
            VAT = phieuNhap.VAT;
            MaNhanVien = phieuNhap.CreatedBy.UserId;
            if (phieuNhap.NhaCungCap != null)
                TenNhaCungCap = phieuNhap.NhaCungCap.TenNhaCungCap;
            TongTien = phieuNhap.TongTien;
            DaTra = phieuNhap.DaTra;
            TongTraSau = tongTraSau;
            MaNhaCungCap = phieuNhap.NhaCungCap.MaNhaCungCap;
            MaNhomNhaCungCap = phieuNhap.NhaCungCap.MaNhomNhaCungCap;
        }

        public TheoNhaCungCapItemViewModel()
        {

        }
    }

    public class TheoNgayViewModel
    {
        public List<TheoNgayItemViewModel> Items;
        public int SoLuongKhach;
        public decimal TongLoiNhuan;
        public decimal TongTien;
        public int Export;
        public DateTime Date;
    }

    public class TheoNgayItemViewModel
    {
        public string TenKhachHang;
        public int SoPhieu;
        public int MaPhieu;
        public int MaLoai;
        public decimal TongTien;
        public decimal TienTra;
        public decimal TienNo;
        public decimal LoiNhuan;
        public DateTime Date;
        public int CreatedBy;
        public TheoNgayItemViewModel()
        {

        }

        public TheoNgayItemViewModel(PhieuXuatChiTiet phieuXuatCt, List<PhieuNhapChiTiet> listPhieuNhapChiTiet, List<PhieuNhapMoiNhat> list)
        {
            var item = phieuXuatCt.PhieuXuat;
            MaPhieu = item.MaPhieuXuat;
            SoPhieu = (int)item.SoPhieuXuat;
            MaLoai = item.MaLoaiXuatNhap;
            TenKhachHang = item.KhachHang.TenKhachHang;
            CreatedBy = item.CreatedBy.UserId;
            TongTien = item.TongTien;
            TienTra = item.DaTra;
            TienNo = TongTien - TienTra;
            Date = item.NgayXuat.Value;
            if (string.IsNullOrEmpty(phieuXuatCt.Option1))
                LoiNhuan += sThuoc.Utils.Helpers.GetLoiNhuanLoaiThuoc(listPhieuNhapChiTiet, phieuXuatCt.Thuoc, phieuXuatCt.DonViTinh.MaDonViTinh, phieuXuatCt.SoLuong, phieuXuatCt.GiaXuat, phieuXuatCt.ChietKhau, phieuXuatCt.PhieuXuat.VAT, list);
        }
    }

    public class ChiTietTheoNgayItemViewModel
    {
        public string TenKhachHang;
        public int SoPhieu;
        public int MaPhieu;
        public int MaLoai;
        public string MaThuoc;
        public string TenThuoc;
        public string DonVi;
        public decimal SoLuong;
        public decimal SoLuongTruocXL;
        public decimal DonGia;
        public decimal VAT;
        public decimal ChietKhau;
        public decimal ThanhTien;
        public decimal LoiNhuan;
        public DateTime Date;
        public int CreatedBy;
        public ChiTietTheoNgayItemViewModel()
        {

        }

        public ChiTietTheoNgayItemViewModel(PhieuXuatChiTiet phieuXuatCt, List<PhieuNhapChiTiet> listPhieuNhapChiTiet, List<PhieuNhapMoiNhat> list)
        {
            var thuoc = phieuXuatCt.Thuoc;
            var phieuxuat = phieuXuatCt.PhieuXuat;
            SoLuongTruocXL = !string.IsNullOrEmpty(phieuXuatCt.Option3) ? decimal.Parse(phieuXuatCt.Option3) : phieuXuatCt.SoLuong;
            ThanhTien = phieuXuatCt.GiaXuat * SoLuongTruocXL * (1 + phieuxuat.VAT / 100) * (1 - phieuXuatCt.ChietKhau / 100);
            MaPhieu = phieuxuat.MaPhieuXuat;
            SoPhieu = (int)phieuxuat.SoPhieuXuat;
            MaLoai = phieuxuat.MaLoaiXuatNhap;
            TenKhachHang = phieuxuat.KhachHang.TenKhachHang;
            CreatedBy = phieuxuat.CreatedBy.UserId;
            TenThuoc = thuoc.TenThuoc;
            MaThuoc = thuoc.MaThuoc;
            SoLuong = phieuXuatCt.SoLuong;
            DonGia = phieuXuatCt.GiaXuat;
            VAT = phieuxuat.VAT;
            DonVi = phieuXuatCt.DonViTinh.TenDonViTinh;
            ChietKhau = phieuXuatCt.ChietKhau;
            //ThanhTien = SoLuong * DonGia * (1 - ChietKhau / 100) * (1 + VAT / 100);
            Date = phieuxuat.NgayXuat.Value;
            if (string.IsNullOrEmpty(phieuXuatCt.Option1))
                LoiNhuan += sThuoc.Utils.Helpers.GetLoiNhuanLoaiThuoc(listPhieuNhapChiTiet, phieuXuatCt.Thuoc, phieuXuatCt.DonViTinh.MaDonViTinh, phieuXuatCt.SoLuong, phieuXuatCt.GiaXuat, phieuXuatCt.ChietKhau, phieuXuatCt.PhieuXuat.VAT, list);
        }
    }
    public class ChiTietTheoNgayViewModel
    {
        public List<ChiTietTheoNgayItemViewModel> Items;
        public int SoLuongKhach;
        public decimal TongLoiNhuan;
        public decimal TongTien;
        public DateTime Date;
    }

    public class TheoMatHangViewModel
    {
        public string Type { get; set; }
        public Dictionary<string, string> Types
        {
            get
            {
                var dict = new Dictionary<string, string>
                {
                    {"all", "Tất cả"},
                    {"bygroup", "Theo nhóm"},
                    {"byname", "Theo tên"}
                };
                return dict;
            }
        }
        public long ThuocId { get; set; }
        public string SearchTerm { get; set; }
        public long MaNhomThuoc { get; set; }
        public string Period { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? From { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? To { get; set; }
        public List<TheoMatHangItemViewModel> Items { get; set; }
        public int Export { get; set; }
        public override string ToString()
        {
            return string.Format("{0}{1}{2}{3}{4}{5}{6}", Type, Period, From, To, Export, ThuocId, MaNhomThuoc);
        }
    }
    public class TheoMatHangItemViewModel : BaoCaoItemBase
    {
        public int MaNhomThuoc { get; set; }
        public int ThuocId { get; set; }
        public string MaThuoc { get; set; }
        public string TenThuoc { get; set; }
        public decimal GiaXuat { get; set; }
        public decimal GiaNhap { get; set; }
        public decimal SoLuong { get; set; }
        public decimal SoLuongTruocXL { get; set; }
        public decimal TongTienTruocXL { get; set; }
        public DonViTinh DonViXuatLe { get; set; }
        public string TenDonViTinh { get; set; }
        
        public TheoMatHangItemViewModel(PhieuXuatChiTiet phieuXuatCt, UnitOfWork unitOfWork, List<PhieuNhapChiTiet> list, long stt = 0, List<PhieuNhapMoiNhat> listPhieuNhapMoiNhat = null)
        {
            if (stt > 0)
                Stt = stt;

            using (SecurityContext ctx = new SecurityContext())
            {
                var item = ctx.PhieuXuatChiTiets.Where(c => c.MaPhieuXuatCt == phieuXuatCt.MaPhieuXuatCt).FirstOrDefault();
                SoLuongTruocXL = item.SoLuong;
                TongTienTruocXL = item.GiaXuat * item.SoLuong * (1 + item.PhieuXuat.VAT / 100) * (1 - item.ChietKhau / 100);
            }

            MaNhanVien = phieuXuatCt.PhieuXuat.CreatedBy.UserId;
            DonViXuatLe = phieuXuatCt.Thuoc.DonViXuatLe;
            MaNhomThuoc = phieuXuatCt.Thuoc.NhomThuoc.MaNhomThuoc;
            ThuocId = phieuXuatCt.Thuoc.ThuocId;
            TenDonViTinh = DonViXuatLe.TenDonViTinh;
            SoLuong = phieuXuatCt.SoLuong;
            GiaNhap = phieuXuatCt.Thuoc.GiaNhap;
            GiaXuat = phieuXuatCt.Thuoc.GiaBanLe;
            MaPhieu = phieuXuatCt.PhieuXuat.MaPhieuXuat;
            SoPhieu = phieuXuatCt.PhieuXuat.SoPhieuXuat;
            if (phieuXuatCt.PhieuXuat.NgayXuat != null) Date = phieuXuatCt.PhieuXuat.NgayXuat.Value;
            TongTien = phieuXuatCt.GiaXuat * phieuXuatCt.SoLuong * (1 + phieuXuatCt.PhieuXuat.VAT / 100) * (1 - phieuXuatCt.ChietKhau / 100);
            Date = phieuXuatCt.PhieuXuat.NgayXuat.Value;
            MaThuoc = phieuXuatCt.Thuoc.MaThuoc;
            TenThuoc = phieuXuatCt.Thuoc.TenThuoc;

            // tinh loi nhuan.   
            if (string.IsNullOrEmpty(phieuXuatCt.Option1))
                LoiNhuan += sThuoc.Utils.Helpers.GetLoiNhuanLoaiThuoc(list, phieuXuatCt.Thuoc, phieuXuatCt.DonViTinh.MaDonViTinh, phieuXuatCt.SoLuong, phieuXuatCt.GiaXuat, phieuXuatCt.ChietKhau, phieuXuatCt.PhieuXuat.VAT, listPhieuNhapMoiNhat);
        }

        public TheoMatHangItemViewModel()
        {

        }
    }
    public class TheoKhoHangViewModel
    {
        public string Type { get; set; }
        public Dictionary<string, string> Types
        {
            get
            {
                var dict = new Dictionary<string, string>
                {
                    {"all", "Tất cả"},
                    {"byname", "Theo tên"},
                    {"bygroup", "Theo nhóm"}
                };
                return dict;
            }
        }
        public long ThuocId { get; set; }
        public long MaNhomThuoc { get; set; }
        public string Period { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? From { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? To { get; set; }
        public IList<TheoKhoHangItemViewModel> Items { get; set; }
        public int Export { get; set; }
        public string SearchTerm { get; set; }
        public override string ToString()
        {
            return string.Format("{0}{1}{2}{3}{4}{5}{6}", Type, Period, From, To, Export, ThuocId, MaNhomThuoc);
        }
    }


    public class TheoKhoHangItemViewModel : BaoCaoItemBase
    {
        public string MaThuoc { get; set; }
        public string TenThuoc { get; set; }
        public decimal TonDau { get; set; }
        public decimal TongGiaTriTonDau { get; set; }
        public decimal Nhap { get; set; }
        public decimal TongGiaTriNhap { get; set; }
        public decimal Xuat { get; set; }
        public decimal TongGiaTriXuat { get; set; }
        public decimal TonCuoi { get; set; }
        public decimal TongGiaTriTonCuoi { get; set; }
        public DonViTinh DonViThuNguyen { get; set; }

        public TheoKhoHangItemViewModel(Thuoc thuoc, TheoKhoHangViewModel form)
        {
            DonViThuNguyen = thuoc.DonViThuNguyen;
            TongGiaTriTonDau = thuoc.SoDuDauKy;
            MaThuoc = thuoc.MaThuoc;
            TenThuoc = thuoc.TenThuoc;
            if (form.Period == "period")
            {
                var tongNhap =
                    thuoc.PhieuNhapChiTiets.Where(
                        e => e.PhieuNhap.NgayNhap >= form.From && e.PhieuNhap.NgayNhap <= form.To && e.PhieuNhap.RecordStatusID == (byte)RecordStatus.Activated)
                        .GroupBy(e => e.Thuoc.ThuocId).Select(e =>

                            new
                            {
                                Quantity = e.Sum(g => thuoc.DonViThuNguyen != null && thuoc.HeSo > 0 && g.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh ? g.SoLuong * thuoc.HeSo : g.SoLuong),
                                Value = e.Sum(g => TinhGiaTriChoPhieuNhapChiTiet(g))
                            }
                        ).FirstOrDefault();
                var tongXuat =
                    thuoc.PhieuXuatChiTiets.Where(
                        e => e.PhieuXuat.NgayXuat >= form.From && e.PhieuXuat.NgayXuat <= form.To && e.PhieuXuat.RecordStatusID == (byte)RecordStatus.Activated)
                        .GroupBy(e => e.Thuoc.ThuocId).Select(e =>

                            new
                            {
                                Quantity = e.Sum(g => thuoc.DonViThuNguyen != null && thuoc.HeSo > 0 && g.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh ? g.SoLuong * thuoc.HeSo : g.SoLuong),
                                Value = e.Sum(g => TinhGiaTriChoPhieuXuatChiTiet(g))
                            }
                        ).FirstOrDefault();
                if (tongNhap != null)
                {
                    Nhap = tongNhap.Quantity;
                    TongGiaTriNhap = tongNhap.Value;
                }
                if (tongXuat != null)
                {
                    Xuat = tongXuat.Quantity;
                    TongGiaTriXuat = tongXuat.Value;
                }
            }
            else
            {


                if (thuoc.PhieuXuatChiTiets != null)
                {
                    var tongXuat = thuoc.PhieuXuatChiTiets.Where(
                        e => e.PhieuXuat.RecordStatusID == (byte)RecordStatus.Activated)
                        .GroupBy(e => e.Thuoc.ThuocId).Select(e =>

                            new
                            {
                                Quantity = e.Sum(g => thuoc.DonViThuNguyen != null && thuoc.HeSo > 0 && g.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh ? g.SoLuong * thuoc.HeSo : g.SoLuong),
                                Value = e.Sum(g => TinhGiaTriChoPhieuXuatChiTiet(g))
                            }
                        ).FirstOrDefault();
                    if (tongXuat != null)
                    {
                        Xuat = tongXuat.Quantity;
                        TongGiaTriXuat = tongXuat.Value;
                    }
                }
                if (thuoc.PhieuNhapChiTiets != null)
                {
                    var tongNhap =
                    thuoc.PhieuNhapChiTiets.Where(
                        e => e.PhieuNhap.RecordStatusID == (byte)RecordStatus.Activated)
                        .GroupBy(e => e.Thuoc.ThuocId).Select(e =>

                            new
                            {
                                Quantity = e.Sum(g => thuoc.DonViThuNguyen != null && thuoc.HeSo > 0 && g.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh ? g.SoLuong * thuoc.HeSo : g.SoLuong),
                                Value = e.Sum(g => TinhGiaTriChoPhieuNhapChiTiet(g))
                            }
                        ).FirstOrDefault();
                    if (tongNhap != null)
                    {
                        Nhap = tongNhap.Quantity;
                        TongGiaTriNhap = tongNhap.Value;
                    }
                }


            }

            TonCuoi = Nhap - Xuat + TonDau;
            TongGiaTriTonCuoi = TonCuoi * thuoc.GiaNhap;

        }

        private decimal TinhGiaTriChoPhieuXuatChiTiet(PhieuXuatChiTiet pxct)
        {
            var tong = pxct.SoLuong * pxct.GiaXuat;
            if (pxct.ChietKhau > 0)
                tong -= pxct.ChietKhau * pxct.SoLuong * pxct.GiaXuat / 100;
            if (pxct.PhieuXuat.VAT > 0)
                tong += pxct.PhieuXuat.VAT * pxct.SoLuong * pxct.GiaXuat / 100;
            return tong;
        }

        private decimal TinhGiaTriChoPhieuNhapChiTiet(PhieuNhapChiTiet pnct)
        {
            var tong = pnct.SoLuong * pnct.GiaNhap;
            if (pnct.ChietKhau > 0)
                tong -= pnct.ChietKhau * pnct.SoLuong * pnct.GiaNhap / 100;
            if (pnct.PhieuNhap.VAT > 0)
                tong += pnct.PhieuNhap.VAT * pnct.SoLuong * pnct.GiaNhap / 100;
            return tong;
        }

        public TheoKhoHangItemViewModel()
        {

        }
    }
    public class BaoCaoItemBase
    {
        public long Stt { get; set; }
        public DateTime Date { get; set; }
        public long SoPhieu { get; set; }
        public long MaPhieu { get; set; }
        public int MaLoai { get; set; }
        public decimal ChietKhau { get; set; }
        public string TenKhachHang { get; set; }
        public string TenNhanVien { get; set; }
        public string TenBacSy { get; set; }
        public decimal TongTien { get; set; }
        public decimal DaTra { get; set; }
        public decimal TongTraSau { get; set; }
        public int MaNhanVien { get; set; }
        public int MaKhachHang { get; set; }
        public int MaNhaCungCap { get; set; }
        public int MaNhomKhachHang { get; set; }
        public int MaNhomNhaCungCap { get; set; }
        public decimal TienNo
        {
            get
            {
                // tien no chi lay gia tri DUONG, neu gia tri AM nghia la da tra lai KH = tien mat.
                var tong = TongTien - DaTra;
                //if (tong > 0)
                //    return tong;
                return tong;
            }
        }

        public decimal LoiNhuan { get; set; }
        public BaoCaoItemBase() { }
    }

}