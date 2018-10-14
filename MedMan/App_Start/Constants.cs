using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MedMan.App_Start
{
    public static class Constants
    {
        public static class Security
        {
            private static Dictionary<string, Dictionary<string, string>> _features = new Dictionary<string, Dictionary<string, string>>();
            public static class Roles
            {
                public static Dictionary<string, string> RoleTexts = new Dictionary<string, string>()
                {
                    {"SuperUser","Hệ Thống"},
                    {"Admin","Quản Lý"},
                    {"User","Nhân Viên"}
                };
                public static class SuperUser
                {
                    public static string Text = "Hệ Thống";
                    public static string Value = "SuperUser";
                }
                public static class Admin
                {
                    public static string Text = "Quản Lý";
                    public static string Value = "Admin";
                }
                public static class User
                {
                    public static string Text = "Nhân Viên";
                    public static string Value = "User";
                }
            }
        }
        public static class Settings
        {
            public const int DeleteStoreAfterDays = 7;
            public const string SoNgayHetHan = "Số ngày sắp hết hạn";
            public const string SoNgayHetHan_Value = "240";
            public const string CapNhatGiaPN = "Tự cập nhật giá nhập/xuất trên phiếu nhập";
            public const string CapNhatGiaPN_value = "Có";
            public const string SoNgayKhongCoGiaoDich = "Số ngày không có giao dịch";
            public const string SoNgayKhongCoGiaoDich_Value = "180";
            //public const string TuDongKhoiTaoHanDung = "Tự động khởi tạo hạn khi có biến động xuất hàng";
            //public const string TuDongKhoiTaoHanDung_Value = "Có";
            //public const string TuDongKhoiTaoDuTru = "Tự động khởi tạo dự trù cho hàng hóa sắp hết";
            //public const string TuDongKhoiTaoDuTru_Value = "Có";
            public const string TuDongTaoMaThuoc = "Tự động tạo mã thuốc";            
            public const string TuDongTaoMaThuoc_Value = "Có";
            public const string TuDongTaoMaVachThuoc = "Tự động tạo mã vạch thuốc";
            public const string TuDongTaoMaVachThuoc_Value = "Có";
            public const string CanhBaoHangLoiNhuanAm = "Cảnh báo hàng lợi nhuận âm";
        }

        public static class Default
        {
            public static class ConstantEntities
            {
                public static string[] NhomKhachHangs = new[] { "Mặc định" };
                public static string[] KhachHangs = { "Khách hàng lẻ", "Điều chỉnh sau kiểm kê" };
                public static string[] NhomNhaCungCaps = { "Mặc định" };
                public static string[] NhaCungCaps = { "Điều chỉnh sau kiểm kê", "Hàng nhập lẻ" };
                public static string KhachHangKiemKe = "Điều chỉnh sau kiểm kê";
                public static string NhaCungCapKiemKe = "Điều chỉnh sau kiểm kê";
                public static string LoaiXuatNhapKiemKe = "Điều chỉnh kiểm kê";
                public static string KhachHangLe = "Khách hàng lẻ";
            }
        }
        public static string MaNhaThuocMapping = "0012";
        public static class Params
        {
            public const string msgOk = "ok";
        }

        public static class LoaiPhieu
        {
            public const string PhieuNhap = "Phiếu nhập";
            public const string PhieuXuat = "Phiếu xuất";
        }

        public static class LoaiPhieuXuatNhap
        {
            public const string NhapKho = "Nhập Kho";
            public const string XuatBan = "Xuất Bán";
            public const string NhapLaiTuKhachHang = "Nhập Lại Từ Khách Hàng";
            public const string XuatVeNhaCungCap = "Xuất Về Nhà Cung Cấp";
            public const string DieuChinhKiemKe = "Điều chỉnh kiểm kê";
        }

        public static class LoaiPhieuThuChi
        {
            public const int PhieuThu = 1;
            public const int PhieuChi = 2;
            public const int PhieuThuKhac = 3;
            public const int PhieuChiKhac = 4;
            public const int PhieuChiPhiKinhDoanh = 5;
        }

        public static class LoaiPhieuXuatIn
        {
            public const int InKhachQuen = 1;
            public const int InKhachLeA5 = 2;
            public const int InKhachLe80mm = 3;
            public const int InKhachLe58mm = 4;
            
        }

        public static class InMaVachOption
        {
            public const string TenGia = "In thêm tên/giá";
            public const string Gia = "In thêm giá";
            public const string Ten = "In thêm tên";            
        }
    }
}