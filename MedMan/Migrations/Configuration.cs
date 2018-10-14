using System.Collections.Generic;
using System.Web.Security;
using sThuoc.Filter;
using sThuoc.Models;
using WebMatrix.WebData;

namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<sThuoc.DAL.SecurityContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(sThuoc.DAL.SecurityContext context)
        {
            //if(context.LoaiXuatNhaps.Any())
            //    return ;
            ////Security Import
            //WebMatrix.WebData.WebSecurity.InitializeDatabaseConnection("SimpleSecurityConnection",
            //   "UserProfile", "UserId", "UserName", autoCreateTables: true);
            //var roles = (SimpleRoleProvider)Roles.Provider;
            //var membership = (SimpleMembershipProvider)Membership.Provider;
            ////Security
            //if (!roles.RoleExists("Admin"))
            //{
            //    roles.CreateRole("Admin");
            //}
            //if (!roles.RoleExists("User"))
            //{
            //    roles.CreateRole("User");
            //}
            //if (!roles.RoleExists("SuperUser"))
            //{
            //    roles.CreateRole("SuperUser");
            //}
            //if (!Filter.WebSecurity.FoundUser("bangadmin"))
            //{
            //    Filter.WebSecurity.CreateUserAndAccount("bangadmin", "nothing", "Nguyễn Công Bằng", "bangnc@gmail.com", "01692999997","0000000000");
            //}
            //if (!roles.GetRolesForUser("bangadmin").Contains("Admin"))
            //{
            //    roles.AddUsersToRoles(new[] { "bangadmin" }, new[] { "admin" });
            //}

            //if (!Filter.WebSecurity.FoundUser("bangowner"))
            //{
            //    Filter.WebSecurity.CreateUserAndAccount("bangowner", "nothing", "Nguyễn Văn Trung", "trungvt@gmail.com", "01692999998", "nt1");
            //}
            //if (!roles.GetRolesForUser("bangowner").Contains("SuperUser"))
            //{
            //    roles.AddUsersToRoles(new[] { "bangowner" }, new[] { "SuperUser" });
            //}

            //if (!Filter.WebSecurity.FoundUser("bangowner2"))
            //{
            //    Filter.WebSecurity.CreateUserAndAccount("bangowner2", "nothing", "Nguyễn Thanh Huyền", "huyenth@gmail.com", "01692999999", "nt2");
            //}
            //if (!roles.GetRolesForUser("bangowner2").Contains("SuperUser"))
            //{
            //    roles.AddUsersToRoles(new[] { "bangowner2" }, new[] { "SuperUser" });
            //}

            //if (!Filter.WebSecurity.FoundUser("banguser"))
            //{
            //    Filter.WebSecurity.CreateUserAndAccount("banguser", "nothing", "Nguyễn Chí Bình", "binhnc@gmail.com", "0987564123", "nt1");
            //}
            //if (!roles.GetRolesForUser("banguser").Contains("User"))
            //{
            //    roles.AddUsersToRoles(new[] { "banguser" }, new[] { "User" });
            //}

            //if (!Filter.WebSecurity.FoundUser("banguser2"))
            //{
            //    Filter.WebSecurity.CreateUserAndAccount("banguser2", "nothing", "Nguyễn Chí Thanh", "thanhnc@gmail.com", "0987564123", "nt2");
            //}
            //if (!roles.GetRolesForUser("banguser2").Contains("User"))
            //{
            //    roles.AddUsersToRoles(new[] { "banguser2" }, new[] { "User" });
            //}

            ////Function
            //const Operations basicOperations = Operations.Create | Operations.Read | Operations.Modify | Operations.Import | Operations.Export;
            //const Operations advOperations = Operations.Create | Operations.Read | Operations.Modify | Operations.Import | Operations.Export | Operations.Execute | Operations.History | Operations.Delete;

            //var func1 = new Function()
            //{
            //    MaFunction = 1,
            //    Name = "Danh Mục Nhóm Thuốc",
            //    Operations = basicOperations
            //};
            //var func2 = new Function()
            //{
            //    MaFunction = 2,
            //    Name = "Danh Mục Thuốc",
            //    Operations = basicOperations
            //};
            //var func3 = new Function()
            //{
            //    MaFunction = 3,
            //    Name = "Danh Mục Đơn Vị Tính",
            //    Operations = basicOperations
            //};
            //var func4 = new Function()
            //{
            //    MaFunction = 4,
            //    Name = "Danh Mục Nhóm Khách Hàng",
            //    Operations = basicOperations
            //};
            //var func5 = new Function()
            //{
            //    MaFunction = 5,
            //    Name = "Danh Mục Nhóm Nhà Cung Cấp",
            //    Operations = basicOperations
            //};
            //var func6 = new Function()
            //{
            //    MaFunction = 6,
            //    Name = "Danh Mục Nhà Cung Cấp",
            //    Operations = basicOperations
            //};
            //var func7 = new Function()
            //{
            //    MaFunction = 7,
            //    Name = "Danh Mục Bác Sỹ",
            //    Operations = basicOperations
            //};
            //var func8 = new Function()
            //{
            //    MaFunction = 8,
            //    Name = "Danh Mục Nhân Viên",
            //    Operations = basicOperations
            //};
            //var func9 = new Function()
            //{
            //    MaFunction = 9,
            //    Name = "Bán Hàng",
            //    Operations = advOperations
            //};
            //var func10 = new Function()
            //{
            //    MaFunction = 10,
            //    Name = "Nhập Hàng",
            //    Operations = advOperations
            //};
            //var func11 = new Function()
            //{
            //    MaFunction = 11,
            //    Name = "Khách Trả Lại Hàng",
            //    Operations = advOperations
            //};
            //var func12 = new Function()
            //{
            //    MaFunction = 12,
            //    Name = "Hàng Trả Lại Cho Nhà Cung Cấp",
            //    Operations = advOperations
            //};
            //var func13 = new Function()
            //{
            //    MaFunction = 13,
            //    Name = "Viết Phiếu Thu Cho Khách",
            //    Operations = advOperations
            //};
            //var func14 = new Function()
            //{
            //    MaFunction = 14,
            //    Name = "Viết Phiếu Chi Cho Nhà Cung Cấp",
            //    Operations = advOperations
            //};
            //var func15 = new Function()
            //{
            //    MaFunction = 15,
            //    Name = "Viết Phiếu Thu/Chi Khác",
            //    Operations = advOperations
            //};
            //var func16 = new Function()
            //{
            //    MaFunction = 16,
            //    Name = "Báo Cáo",
            //    Operations = basicOperations
            //};
            //var func17 = new Function()
            //{
            //    MaFunction = 17,
            //    Name = "Dạng Bào Chế",
            //    Operations = basicOperations
            //};
            //var func18 = new Function()
            //{
            //    MaFunction = 18,
            //    Name = "Nước",
            //    Operations = basicOperations
            //};
            //var func19 = new Function()
            //{
            //    MaFunction = 19,
            //    Name = "Khách Hàng",
            //    Operations = basicOperations
            //};
            //context.Functions.AddOrUpdate(x => x.MaFunction, func1, func2, func3, func4, func5, func6, func7, func8, func9, func10, func11, func12, func13, func14, func15, func16, func17, func18, func19);
            //context.SaveChanges();
            //NhaThuoc
           // var nt1 = new NhaThuoc
           // {
           //     MaNhaThuoc = "nt1",
           //     TenNhaThuoc = "Nha Thuoc 1",
           //     SoKinhDoanh = 1234321,
           //     DienThoai = "0437844534",
           //     NguoiDaiDien = "Nguyen Cong Bang",
           //     Email = "bangnguyencong@gmail.com",
           //     Mobile = "0983654789",
           //     DuocSy = "Doan Duoc Sy"
           // };
           // var
           //nt2 = new NhaThuoc
           //{
           //    MaNhaThuoc = "nt2",
           //    TenNhaThuoc = "Nha Thuoc 2",
           //    SoKinhDoanh = 1234321,
           //    DienThoai = "0457894247",
           //    NguoiDaiDien = "Tran Danh Tung",
           //    Email = "Tungtd@gmail.com",
           //    Mobile = "0983654789",
           //    DuocSy = "Pham Sy Luan"
           //};
           // var nt3 = new NhaThuoc
           // {
           //     MaNhaThuoc = "nt3",
           //     TenNhaThuoc = "Nha Thuoc 3",
           //     SoKinhDoanh = 1234321,
           //     DienThoai = "0445785471",
           //     NguoiDaiDien = "Hoang Van Ha",
           //     Email = "Havh@gmail.com",
           //     Mobile = "0972123478",
           //     DuocSy = "Tran Thi Duoc"
           // };

           // context.NhaThuocs.AddOrUpdate(x => x.MaNhaThuoc, nt1, nt2, nt3);
           // context.SaveChanges();

           // //Bac Sy
           // var bs1 = new BacSy()
           // {
           //     TenBacSy = "Bac Sy 1",
           //     DiaChi = "Dia Chi Bac Sy 1",
           //     DienThoai = "Dien Thaoai Bac Sy 1",
           //     Email = "bacsy1@gmail.com",
           //     NhaThuoc = nt1
           // };
           // var bs2 = new BacSy()
           // {
           //     TenBacSy = "Bac Sy 2",
           //     DiaChi = "Dia Chi Bac Sy 2",
           //     DienThoai = "Dien Thaoai Bac Sy 2",
           //     Email = "bacsy2@gmail.com",
           //     NhaThuoc = nt2
           // };
           // context.BacSys.AddOrUpdate(x => x.MaBacSy, bs1, bs2);
           // context.SaveChanges();

           // //Dang Bao Che
           // var dbc1 = new DangBaoChe()
           // {
           //     TenDangBaoChe = "Dang Bao Che 1",
           //     NhaThuoc = nt1
           // };
           // var dbc2 = new DangBaoChe()
           // {
           //     TenDangBaoChe = "Dang Bao Che 2",
           //     NhaThuoc = nt1
           // };
           // context.DangBaoChes.AddOrUpdate(x => x.MaDangBaoChe, dbc1, dbc2);
           // context.SaveChanges();

           // //Don Vi Tinh
           // var dvt1 = new DonViTinh()
           // {
           //     TenDonViTinh = "Viên",
           //     NhaThuoc = nt1
           // };
           // var dvt2 = new DonViTinh()
           // {
           //     TenDonViTinh = "Vỉ",
           //     NhaThuoc = nt1
           // };
           // var dvt3 = new DonViTinh()
           // {
           //     TenDonViTinh = "Hộp",
           //     NhaThuoc = nt1
           // };
           // context.DonViTinhs.AddOrUpdate(x => x.MaDonViTinh, dvt1, dvt2, dvt3);
           // context.SaveChanges();

           // //Nhom Khach Hang
           // var nkh1 = new NhomKhachHang()
           // {
           //     TenNhomKhachHang = "Nhom Khach Hang 1",
           //     NhaThuoc = nt1
           // };
           // var nkh2 = new NhomKhachHang()
           // {
           //     TenNhomKhachHang = "Nhom Khach Hang 2",
           //     NhaThuoc = nt1
           // };
           // context.NhomKhachHangs.AddOrUpdate(x => x.MaNhomKhachHang, nkh1, nkh2);
           // context.SaveChanges();

           // //Nhom Nha Cung Cap
           // var nncc1 = new NhomNhaCungCap()
           // {
           //     TenNhomNhaCungCap = "Nhom Nha Cung Cap 1",
           //     NhaThuoc = nt1
           // };
           // var nncc2 = new NhomNhaCungCap()
           // {
           //     TenNhomNhaCungCap = "Nhom Nha Cung Cap 2",
           //     NhaThuoc = nt1
           // };
           // context.NhomNhaCungCaps.AddOrUpdate(x => x.MaNhomNhaCungCap, nncc1, nncc2);
           // context.SaveChanges();

           // var nthuoc1 = new NhomThuoc()
           // {
           //     TenNhomThuoc = "Nhom Thuoc 1",
           //     KyHieuNhomThuoc = "Ky Hieu 1",
           //     NhaThuoc = nt1
           // };
           // var nthuoc2 = new NhomThuoc()
           // {
           //     TenNhomThuoc = "Nhom Thuoc 2",
           //     KyHieuNhomThuoc = "Ky Hieu 2",
           //     NhaThuoc = nt1
           // };
           // context.NhomThuocs.AddOrUpdate(x => x.MaNhomThuoc, nthuoc1, nthuoc2);
           // context.SaveChanges();

           // //Nuoc
           // var n1 = new Nuoc()
           // {
           //     TenNuoc = "My"
           //     //,
           //     //NhaThuoc = nt1
           // };
           // var n2 = new Nuoc()
           // {
           //     TenNuoc = "Duc"
           //     //,
           //     //NhaThuoc = nt1
           // };
           // context.Nuocs.AddOrUpdate(x => x.MaNuoc, n1, n2);
           // context.SaveChanges();

           // //Nha Cung Cap
           // var ncc1 = new NhaCungCap()
           // {
           //     TenNhaCungCap = "Nha Cung Cap 1",
           //     DiaChi = "Dia Chi Nha Cung Cap 1",
           //     SoDienThoai = "019293823",
           //     SoFax = "13-45-346",
           //     MaSoThue = "121246",
           //     NguoiDaiDien = "Nguyen Duc Van",
           //     NguoiLienHe = "Tran Duc Dung",
           //     Email = "nhacungcap1@gmail.com",
           //     NoDauKy = 8124,
           //     NhaThuoc = nt1,
           //     NhomNhaCungCap = nncc1
           // };
           // var ncc2 = new NhaCungCap()
           // {
           //     TenNhaCungCap = "Nha Cung Cap 2",
           //     DiaChi = "Dia Chi Nha Cung Cap 2",
           //     SoDienThoai = "034693823",
           //     SoFax = "25-45-456",
           //     MaSoThue = "4767",
           //     NguoiDaiDien = "Nguyen Thanh Hai",
           //     NguoiLienHe = "Tran Van Ha",
           //     Email = "nhacungcap2@gmail.com",
           //     NoDauKy = 5547,
           //     NhaThuoc = nt1,
           //     NhomNhaCungCap = nncc2
           // };
           // context.NhaCungCaps.AddOrUpdate(x => x.MaNhaCungCap, ncc1, ncc2);
           // context.SaveChanges();

           // //Khach Hang
           // var kh1 = new KhachHang()
           // {
           //     TenKhachHang = "Khach Hang 1",
           //     DiaChi = "Dia Chi Khach Hang 1",
           //     SoDienThoai = "2645756",
           //     DonViCongTac = "Don Vi Cong Tac Khach Hang 1",
           //     Email = "khachhang1@gmail.com",
           //     GhiChu = "Ghi Chu Khach Hang 1",
           //     NoDauKy = 1456,
           //     NhaThuoc = nt1,
           //     NhomKhachHang = nkh1
           // };
           // var kh2 = new KhachHang()
           // {
           //     TenKhachHang = "Khach Hang 2",
           //     DiaChi = "Dia Chi Khach Hang 2",
           //     SoDienThoai = "2346257",
           //     DonViCongTac = "Don Vi Cong Tac Khach Hang 2",
           //     Email = "khachhang2@gmail.com",
           //     GhiChu = "Ghi Chu Khach Hang 2",
           //     NoDauKy = 4579,
           //     NhaThuoc = nt1,
           //     NhomKhachHang = nkh2
           // };
           // context.KhachHangs.AddOrUpdate(x => x.MaKhachHang, kh1, kh2);
           // context.SaveChanges();

           // //Loai Xuat Nhap
           // var lxn1 = new LoaiXuatNhap()
           // {
           //     TenLoaiXuatNhap = "Nhập Kho"
           // };
           // var lxn2 = new LoaiXuatNhap()
           // {
           //     TenLoaiXuatNhap = "Xuất Bán"
           // };
           // var lxn3 = new LoaiXuatNhap()
           // {
           //     TenLoaiXuatNhap = "Nhập Lại Từ Khách Hàng"
           // };
           // var lxn4 = new LoaiXuatNhap()
           // {
           //     TenLoaiXuatNhap = "Xuất Về Nhà Cung Cấp"
           // };
           // context.LoaiXuatNhaps.AddOrUpdate(x => x.MaLoaiXuatNhap, lxn1, lxn2, lxn3, lxn4);
           // context.SaveChanges();

           // //Thuoc
           // var thuoc1 = new Thuoc()
           // {
           //     MaThuoc = "th1",
           //     TenThuoc = "Pagant Havast Talont",
           //     ThongTin = "200mg Ahtos",
           //     NhaThuoc = nt1,
           //     NhomThuoc = nthuoc1,
           //     Nuoc = n1,
           //     DangBaoChe = dbc1,
           //     DonViXuatLe = dvt1,
           //     DonViThuNguyen = dvt2,
           //     HeSo = 12,
           //     GiaNhap = 125,
           //     GiaBanBuon = 200,
           //     GiaBanLe = 250,
           //     SoDuDauKy = 20,
           //     GiaDauKy = 90,
           //     GioiHan = 20
           // };
           // var thuoc2 = new Thuoc()
           // {
           //     MaThuoc = "th2",
           //     TenThuoc = "Contour Harchid Marvel Alou",
           //     ThongTin = "500mg Halo",
           //     NhaThuoc = nt1,
           //     NhomThuoc = nthuoc1,
           //     Nuoc = n2,
           //     DangBaoChe = dbc2,
           //     DonViXuatLe = dvt2,
           //     DonViThuNguyen = dvt3,
           //     HeSo = 30,
           //     GiaNhap = 300,
           //     GiaBanBuon = 400,
           //     GiaBanLe = 500,
           //     SoDuDauKy = 50,
           //     GiaDauKy = 250,
           //     GioiHan = 30
           // };
           // context.Thuocs.AddOrUpdate(x => x.MaThuoc, thuoc1, thuoc2);
           // context.SaveChanges();

           // //Phieu Nhap
           // var pn1 = new PhieuNhap()
           // {
           //     NhaThuoc = nt1,
           //     SoPhieuNhap = 12,
           //     LoaiXuatNhap = lxn1,
           //     NgayNhap = DateTime.Today.AddDays(-5),
           //     VAT = 10,
           //     NhaCungCap = ncc1,
           //     DienGiai = "Phieu Nhap 1",
           //     //MaNguoiTao = 4,
           //     //NgayTao = DateTime.Today,
           //     TongTien = 0,
           //     DaTra = 0,
           //     Xoa = false,
           // };

           // pn1.PhieuNhapChiTiets = new List<PhieuNhapChiTiet>()
           // {
           //     new PhieuNhapChiTiet()
           //     {
           //         PhieuNhap = pn1,
           //         //NhaThuoc = nt1,
           //         Thuoc = thuoc1,
           //         DonViTinh = dvt2,
           //         //HanDung = DateTime.Today.AddYears(3),
           //         SoLuong = 15,
           //         ChietKhau = 5,
           //         GiaNhap = 200                   
           //     },
           //     new PhieuNhapChiTiet()
           //     {
           //         PhieuNhap = pn1,
           //        // NhaThuoc = nt1,
           //         Thuoc = thuoc1,
           //         DonViTinh = dvt2,
           //         //HanDung = DateTime.Today.AddYears(5),
           //         SoLuong = 56,
           //         ChietKhau = 10,
           //         GiaNhap = 160                   
           //     },
           //     new PhieuNhapChiTiet()
           //     {
           //         PhieuNhap = pn1,
           //         //NhaThuoc = nt1,
           //         Thuoc = thuoc2,
           //         DonViTinh = dvt3,
           //         //HanDung = DateTime.Today.AddYears(2),
           //         SoLuong = 15,
           //         ChietKhau = 0,
           //         GiaNhap = 400                   
           //     }
           // };
           // context.PhieuNhaps.AddOrUpdate(x => x.MaPhieuNhap, pn1);
           // context.SaveChanges();

           // //Phieu Xuat
           // var px1 = new PhieuXuat()
           // {
           //     NhaThuoc = nt1,
           //     SoPhieuXuat = 34,
           //     LoaiXuatNhap = lxn2,
           //     NgayXuat = DateTime.Today.AddDays(-3),
           //     VAT = 10,
           //     KhachHang = kh1,
           //     DienGiai = "Phieu Xuat 1",
           //     BacSy = bs1,
           //     //MaNguoiTao = 4,
           //     Created = DateTime.Today,
           //     TongTien = 0,
           //     DaTra = 0,
           //     Xoa = false
           // };

           // px1.PhieuXuatChiTiets = new List<PhieuXuatChiTiet>()
           // {
           //     new PhieuXuatChiTiet()
           //     {
           //         PhieuXuat = px1,
           //         //NhaThuoc = nt1,
           //         Thuoc = thuoc1,
           //         DonViTinh = dvt2,
           //         //HanDung = DateTime.Today.AddYears(3),
           //         SoLuong = 10,
           //         ChietKhau = 0,
           //         GiaXuat = 300                    
           //     },
           //     new PhieuXuatChiTiet()
           //     {
           //         PhieuXuat = px1,
           //         //NhaThuoc = nt1,
           //         Thuoc = thuoc1,
           //         DonViTinh = dvt2,
           //         //HanDung = DateTime.Today.AddYears(5),
           //         SoLuong = 30,
           //         ChietKhau = 5,
           //         GiaXuat = 320                   
           //     },
           //     new PhieuXuatChiTiet()
           //     {
           //         PhieuXuat = px1,
           //         //NhaThuoc = nt1,
           //         Thuoc = thuoc2,
           //         DonViTinh = dvt3,
           //         //HanDung = DateTime.Today.AddYears(2),
           //         SoLuong = 5,
           //         ChietKhau = 0,
           //         GiaXuat = 500                   
           //     }
           // };

           // context.PhieuXuats.AddOrUpdate(x => x.MaPhieuXuat, px1);
           // context.SaveChanges();

           // //Map the resource/operation to the roles
           // FunctionsService.InsertOrUpdateOperationToRole(Filter.WebSecurity.GetUserId("banguser"), FunctionResource.BacSy, Operations.Read | Operations.Create, "User", nt1.MaNhaThuoc);
        }
    }
}
