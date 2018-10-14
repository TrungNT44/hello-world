using sThuoc.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sThuoc.Models
{
    [Serializable]
    public class HetHang
    {
        public int ThuocId;
        public string MaThuoc;
        public string TenThuoc;
        public string SoLuongCanhBao;
        public string TonKho;
    }

    [Serializable]
    public class CanhBaoDuTru : HetHang
    {
        public int DuTru;
        public decimal DonGia;
        public decimal ThanhTien;
        public string DonViTinh;
    }

    [Serializable]
    public class CanhBaoHetHan
    {
        public int ThuocId;
        public string MaThuoc;
        public string TenThuoc;
        public string HangItGiaoDich;
        public string SoLuong;
        public string DonVi;
        public string Han;
    }

    [Serializable]
    public class CanhBaoLoiNhuanAm
    {
        public string MaThuoc;
        public string TenThuoc;
        public decimal LoiNhuan;
        public string PhieuNhaps;
        public int MaPhieuXuat;
        public long SoPhieuXuat;
        public DateTime Date;
        public List<long> ListSoPhieuNhap;
        public List<long> ListMaPhieuNhap;
        public CanhBaoLoiNhuanAm(PhieuXuatChiTiet phieuXuatCt, List<PhieuNhapChiTiet> list, List<PhieuNhapMoiNhat> listPhieuNhapMoiNhats)
        {
            MaThuoc = phieuXuatCt.Thuoc.MaThuoc;
            TenThuoc = phieuXuatCt.Thuoc.TenThuoc;
            Date = phieuXuatCt.PhieuXuat.NgayXuat.Value;
            MaPhieuXuat = phieuXuatCt.PhieuXuat.MaPhieuXuat;
            SoPhieuXuat = phieuXuatCt.PhieuXuat.SoPhieuXuat;
            // tinh loi nhuan.
            List<long> listPhieuNhap = new List<long>();
            List<long> listMaPhieuNhap = new List<long>();
            LoiNhuan = sThuoc.Utils.Helpers.GetLoiNhuanAm(list, phieuXuatCt.Thuoc, phieuXuatCt.DonViTinh.MaDonViTinh, phieuXuatCt.SoLuong, 
                phieuXuatCt.GiaXuat, phieuXuatCt.ChietKhau, phieuXuatCt.PhieuXuat.VAT, listPhieuNhap, listMaPhieuNhap, listPhieuNhapMoiNhats);
            if (LoiNhuan < 0)
            {
                PhieuNhaps = string.Join(", ", listPhieuNhap);
            }
            else
            {
                listPhieuNhap.Clear();
                listMaPhieuNhap.Clear();
            }
            ListSoPhieuNhap = listPhieuNhap;
            ListMaPhieuNhap = listMaPhieuNhap;
        }
    }

    [Serializable]
    public class PhieuNhapMoiNhat
    {
        public decimal GiaNhap;
        public int VAT;
        public decimal ChietKhau;
        public int ThuocId;
    }
}