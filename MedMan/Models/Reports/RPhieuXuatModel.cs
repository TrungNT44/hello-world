using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sThuoc.Models.Reports
{
    public class RPhieuXuatModel
    {
        public int MaPhieuXuat;
        public string LoaiPhieu;
        public string NgayXuat;
        public string SoPhieu;
        public string KhachHang;
        public string BacSy;
        public string DienGiai;
        public string DiaChi;
        public string NhanVien;
        public string CongTienHang;
        public string VAT;
        public string NoCu;
        public string TongTien;
        public string Tra;
        public string ConNo;
        public List<RPhieuXuatItem> Items;
    }

    public class RPhieuXuatItem
    {
        public int MaPhieuXuat;
        public string STT;
        public string TenHang;
        public string DVT;
        public string SoLuong;
        public string DonGia ;
        public string ChietKhau;
        public string ThanhTien;        
    }
}