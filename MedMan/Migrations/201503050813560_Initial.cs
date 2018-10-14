namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BacSies",
                c => new
                    {
                        MaBacSy = c.Int(nullable: false, identity: true),
                        TenBacSy = c.String(nullable: false),
                        DiaChi = c.String(),
                        DienThoai = c.String(),
                        Email = c.String(),
                        MaNhaThuoc = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.MaBacSy)
                .ForeignKey("dbo.NhaThuocs", t => t.MaNhaThuoc)
                .Index(t => t.MaNhaThuoc);
            
            CreateTable(
                "dbo.NhaThuocs",
                c => new
                    {
                        MaNhaThuoc = c.String(nullable: false, maxLength: 128),
                        TenNhaThuoc = c.String(),
                        DiaChi = c.String(),
                        SoKinhDoanh = c.Int(nullable: false),
                        DienThoai = c.String(),
                        NguoiDaiDien = c.String(),
                        Email = c.String(),
                        Mobile = c.String(),
                        DuocSy = c.String(),
                    })
                .PrimaryKey(t => t.MaNhaThuoc);
            
            CreateTable(
                "dbo.DangBaoChes",
                c => new
                    {
                        MaDangBaoChe = c.Int(nullable: false, identity: true),
                        TenDangBaoChe = c.String(),
                        MaNhaThuoc = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.MaDangBaoChe)
                .ForeignKey("dbo.NhaThuocs", t => t.MaNhaThuoc)
                .Index(t => t.MaNhaThuoc);
            
            CreateTable(
                "dbo.Thuocs",
                c => new
                    {
                        ThuocId = c.Int(nullable: false, identity: true),
                        MaThuoc = c.String(),
                        TenThuoc = c.String(),
                        ThongTin = c.String(),
                        HeSo = c.Int(nullable: false),
                        GiaNhap = c.Int(nullable: false),
                        GiaBanBuon = c.Int(nullable: false),
                        GiaBanLe = c.Int(nullable: false),
                        SoDuDauKy = c.Single(),
                        GiaDauKy = c.Int(),
                        GioiHan = c.Int(),
                        MaNhaThuoc = c.String(maxLength: 128),
                        MaNhomThuoc = c.Int(nullable: false),
                        MaNuoc = c.Int(nullable: false),
                        MaDangBaoChe = c.Int(nullable: false),
                        MaDonViXuat = c.Int(nullable: false),
                        MaDonViThuNguyen = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ThuocId)
                .ForeignKey("dbo.DangBaoChes", t => t.MaDangBaoChe, cascadeDelete: true)
                .ForeignKey("dbo.DonViTinhs", t => t.MaDonViThuNguyen)
                .ForeignKey("dbo.DonViTinhs", t => t.MaDonViXuat)
                .ForeignKey("dbo.NhaThuocs", t => t.MaNhaThuoc)
                .ForeignKey("dbo.NhomThuocs", t => t.MaNhomThuoc, cascadeDelete: true)
                .ForeignKey("dbo.Nuocs", t => t.MaNuoc, cascadeDelete: true)
                .Index(t => t.MaNhaThuoc)
                .Index(t => t.MaNhomThuoc)
                .Index(t => t.MaNuoc)
                .Index(t => t.MaDangBaoChe)
                .Index(t => t.MaDonViXuat)
                .Index(t => t.MaDonViThuNguyen);
            
            CreateTable(
                "dbo.DonViTinhs",
                c => new
                    {
                        MaDonViTinh = c.Int(nullable: false, identity: true),
                        TenDonViTinh = c.String(),
                        MaNhaThuoc = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.MaDonViTinh)
                .ForeignKey("dbo.NhaThuocs", t => t.MaNhaThuoc)
                .Index(t => t.MaNhaThuoc);
            
            CreateTable(
                "dbo.PhieuNhapChiTiets",
                c => new
                    {
                        MaPhieuNhapCt = c.Int(nullable: false, identity: true),
                        MaPhieuNhap = c.Int(nullable: false),
                        MaNhaThuoc = c.String(nullable: false, maxLength: 128),
                        ThuocId = c.Int(nullable: false),
                        MaDonViTinh = c.Int(nullable: false),
                        HanDung = c.DateTime(nullable: false),
                        ChietKhau = c.Int(nullable: false),
                        GiaNhap = c.Int(nullable: false),
                        SoLuong = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MaPhieuNhapCt)
                .ForeignKey("dbo.DonViTinhs", t => t.MaDonViTinh, cascadeDelete: true)
                .ForeignKey("dbo.NhaThuocs", t => t.MaNhaThuoc)
                .ForeignKey("dbo.PhieuNhaps", t => t.MaPhieuNhap)
                .ForeignKey("dbo.Thuocs", t => t.ThuocId)
                .Index(t => t.MaPhieuNhap)
                .Index(t => t.MaNhaThuoc)
                .Index(t => t.ThuocId)
                .Index(t => t.MaDonViTinh);
            
            CreateTable(
                "dbo.PhieuNhaps",
                c => new
                    {
                        MaPhieuNhap = c.Int(nullable: false, identity: true),
                        SoPhieuNhap = c.String(),
                        NgayNhap = c.DateTime(),
                        VAT = c.Int(),
                        DienGiai = c.String(),
                        NgayTao = c.DateTime(nullable: false),
                        TongTien = c.Int(nullable: false),
                        DaTra = c.Int(),
                        Xoa = c.Boolean(nullable: false),
                        MaNhaThuoc = c.String(nullable: false, maxLength: 128),
                        MaLoaiXuatNhap = c.Int(nullable: false),
                        MaNhaCungCap = c.Int(nullable: false),
                        MaKhachHang = c.Int(),
                        MaNguoiTao = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MaPhieuNhap)
                .ForeignKey("dbo.KhachHangs", t => t.MaKhachHang)
                .ForeignKey("dbo.LoaiXuatNhaps", t => t.MaLoaiXuatNhap)
                .ForeignKey("dbo.UserProfile", t => t.MaNguoiTao)
                .ForeignKey("dbo.NhaCungCaps", t => t.MaNhaCungCap)
                .ForeignKey("dbo.NhaThuocs", t => t.MaNhaThuoc)
                .Index(t => t.MaNhaThuoc)
                .Index(t => t.MaLoaiXuatNhap)
                .Index(t => t.MaNhaCungCap)
                .Index(t => t.MaKhachHang)
                .Index(t => t.MaNguoiTao);
            
            CreateTable(
                "dbo.KhachHangs",
                c => new
                    {
                        MaKhachHang = c.Int(nullable: false, identity: true),
                        TenKhachHang = c.String(),
                        DiaChi = c.String(),
                        SoDienThoai = c.String(),
                        NoDauKy = c.Int(),
                        DonViCongTac = c.String(),
                        Email = c.String(),
                        GhiChu = c.String(),
                        MaNhaThuoc = c.String(maxLength: 128),
                        MaNhomKhachHang = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MaKhachHang)
                .ForeignKey("dbo.NhaThuocs", t => t.MaNhaThuoc)
                .ForeignKey("dbo.NhomKhachHangs", t => t.MaNhomKhachHang, cascadeDelete: true)
                .Index(t => t.MaNhaThuoc)
                .Index(t => t.MaNhomKhachHang);
            
            CreateTable(
                "dbo.NhomKhachHangs",
                c => new
                    {
                        MaNhomKhachHang = c.Int(nullable: false, identity: true),
                        TenNhomKhachHang = c.String(),
                        GhiChu = c.String(),
                        MaNhaThuoc = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.MaNhomKhachHang)
                .ForeignKey("dbo.NhaThuocs", t => t.MaNhaThuoc)
                .Index(t => t.MaNhaThuoc);
            
            CreateTable(
                "dbo.PhieuXuats",
                c => new
                    {
                        MaPhieuXuat = c.Int(nullable: false, identity: true),
                        SoPhieuXuat = c.String(),
                        NgayXuat = c.DateTime(),
                        VAT = c.Int(nullable: false),
                        DienGiai = c.String(),
                        NgayTao = c.DateTime(nullable: false),
                        TongTien = c.Int(nullable: false),
                        DaTra = c.Int(nullable: false),
                        Xoa = c.Boolean(nullable: false),
                        MaNhaThuoc = c.String(nullable: false, maxLength: 128),
                        MaLoaiXuatNhap = c.Int(nullable: false),
                        MaKhachHang = c.Int(nullable: false),
                        MaNhaCungCap = c.Int(),
                        MaBacSy = c.Int(nullable: false),
                        MaNguoiTao = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MaPhieuXuat)
                .ForeignKey("dbo.BacSies", t => t.MaBacSy)
                .ForeignKey("dbo.KhachHangs", t => t.MaKhachHang)
                .ForeignKey("dbo.LoaiXuatNhaps", t => t.MaLoaiXuatNhap)
                .ForeignKey("dbo.UserProfile", t => t.MaNguoiTao)
                .ForeignKey("dbo.NhaCungCaps", t => t.MaNhaCungCap)
                .ForeignKey("dbo.NhaThuocs", t => t.MaNhaThuoc)
                .Index(t => t.MaNhaThuoc)
                .Index(t => t.MaLoaiXuatNhap)
                .Index(t => t.MaKhachHang)
                .Index(t => t.MaNhaCungCap)
                .Index(t => t.MaBacSy)
                .Index(t => t.MaNguoiTao);
            
            CreateTable(
                "dbo.LoaiXuatNhaps",
                c => new
                    {
                        MaLoaiXuatNhap = c.Int(nullable: false, identity: true),
                        TenLoaiXuatNhap = c.String(),
                        MaNhaThuoc = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.MaLoaiXuatNhap)
                .ForeignKey("dbo.NhaThuocs", t => t.MaNhaThuoc)
                .Index(t => t.MaNhaThuoc);
            
            CreateTable(
                "dbo.UserProfile",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        UserName = c.String(),
                        TenDayDu = c.String(),
                        Email = c.String(),
                        SoDienThoai = c.String(),
                        MaNhaThuoc = c.String(),
                        HoatDong = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "dbo.NhaCungCaps",
                c => new
                    {
                        MaNhaCungCap = c.Int(nullable: false, identity: true),
                        TenNhaCungCap = c.String(),
                        DiaChi = c.String(),
                        SoDienThoai = c.String(),
                        SoFax = c.String(),
                        MaSoThue = c.String(),
                        NguoiDaiDien = c.String(),
                        NguoiLienHe = c.String(),
                        Email = c.String(),
                        NoDauKy = c.Int(),
                        MaNhaThuoc = c.String(maxLength: 128),
                        MaNhomNhaCungCap = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MaNhaCungCap)
                .ForeignKey("dbo.NhaThuocs", t => t.MaNhaThuoc)
                .ForeignKey("dbo.NhomNhaCungCaps", t => t.MaNhomNhaCungCap, cascadeDelete: true)
                .Index(t => t.MaNhaThuoc)
                .Index(t => t.MaNhomNhaCungCap);
            
            CreateTable(
                "dbo.NhomNhaCungCaps",
                c => new
                    {
                        MaNhomNhaCungCap = c.Int(nullable: false, identity: true),
                        TenNhomNhaCungCap = c.String(),
                        GhiChu = c.String(),
                        MaNhaThuoc = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.MaNhomNhaCungCap)
                .ForeignKey("dbo.NhaThuocs", t => t.MaNhaThuoc)
                .Index(t => t.MaNhaThuoc);
            
            CreateTable(
                "dbo.PhieuXuatChiTiets",
                c => new
                    {
                        MaPhieuXuatCt = c.Int(nullable: false, identity: true),
                        MaPhieuXuat = c.Int(nullable: false),
                        MaNhaThuoc = c.String(nullable: false, maxLength: 128),
                        ThuocId = c.Int(nullable: false),
                        MaDonViTinh = c.Int(nullable: false),
                        HanDung = c.DateTime(nullable: false),
                        ChietKhau = c.Int(nullable: false),
                        GiaXuat = c.Int(nullable: false),
                        SoLuong = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MaPhieuXuatCt)
                .ForeignKey("dbo.DonViTinhs", t => t.MaDonViTinh, cascadeDelete: true)
                .ForeignKey("dbo.NhaThuocs", t => t.MaNhaThuoc)
                .ForeignKey("dbo.PhieuXuats", t => t.MaPhieuXuat)
                .ForeignKey("dbo.Thuocs", t => t.ThuocId)
                .Index(t => t.MaPhieuXuat)
                .Index(t => t.MaNhaThuoc)
                .Index(t => t.ThuocId)
                .Index(t => t.MaDonViTinh);
            
            CreateTable(
                "dbo.NhomThuocs",
                c => new
                    {
                        MaNhomThuoc = c.Int(nullable: false, identity: true),
                        TenNhomThuoc = c.String(),
                        KyHieuNhomThuoc = c.String(),
                        MaNhaThuoc = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.MaNhomThuoc)
                .ForeignKey("dbo.NhaThuocs", t => t.MaNhaThuoc)
                .Index(t => t.MaNhaThuoc);
            
            CreateTable(
                "dbo.Nuocs",
                c => new
                    {
                        MaNuoc = c.Int(nullable: false, identity: true),
                        TenNuoc = c.String(),
                        MaNhaThuoc = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.MaNuoc)
                .ForeignKey("dbo.NhaThuocs", t => t.MaNhaThuoc)
                .Index(t => t.MaNhaThuoc);
            
            CreateTable(
                "dbo.Functions",
                c => new
                    {
                        MaFunction = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Operations = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MaFunction);
            
            CreateTable(
                "dbo.OperationsToRoles",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        FunctionId = c.Int(nullable: false),
                        RoleName = c.String(nullable: false, maxLength: 128),
                        MaNhaThuoc = c.String(nullable: false, maxLength: 128),
                        Operations = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.FunctionId, t.RoleName, t.MaNhaThuoc });
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Thuocs", "MaNuoc", "dbo.Nuocs");
            DropForeignKey("dbo.Nuocs", "MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.Thuocs", "MaNhomThuoc", "dbo.NhomThuocs");
            DropForeignKey("dbo.NhomThuocs", "MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.Thuocs", "MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.Thuocs", "MaDonViXuat", "dbo.DonViTinhs");
            DropForeignKey("dbo.Thuocs", "MaDonViThuNguyen", "dbo.DonViTinhs");
            DropForeignKey("dbo.PhieuNhapChiTiets", "ThuocId", "dbo.Thuocs");
            DropForeignKey("dbo.PhieuNhapChiTiets", "MaPhieuNhap", "dbo.PhieuNhaps");
            DropForeignKey("dbo.PhieuNhaps", "MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.PhieuNhaps", "MaNhaCungCap", "dbo.NhaCungCaps");
            DropForeignKey("dbo.PhieuNhaps", "MaNguoiTao", "dbo.UserProfile");
            DropForeignKey("dbo.PhieuNhaps", "MaLoaiXuatNhap", "dbo.LoaiXuatNhaps");
            DropForeignKey("dbo.PhieuNhaps", "MaKhachHang", "dbo.KhachHangs");
            DropForeignKey("dbo.PhieuXuatChiTiets", "ThuocId", "dbo.Thuocs");
            DropForeignKey("dbo.PhieuXuatChiTiets", "MaPhieuXuat", "dbo.PhieuXuats");
            DropForeignKey("dbo.PhieuXuatChiTiets", "MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.PhieuXuatChiTiets", "MaDonViTinh", "dbo.DonViTinhs");
            DropForeignKey("dbo.PhieuXuats", "MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.PhieuXuats", "MaNhaCungCap", "dbo.NhaCungCaps");
            DropForeignKey("dbo.NhomNhaCungCaps", "MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.NhaCungCaps", "MaNhomNhaCungCap", "dbo.NhomNhaCungCaps");
            DropForeignKey("dbo.NhaCungCaps", "MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.PhieuXuats", "MaNguoiTao", "dbo.UserProfile");
            DropForeignKey("dbo.PhieuXuats", "MaLoaiXuatNhap", "dbo.LoaiXuatNhaps");
            DropForeignKey("dbo.LoaiXuatNhaps", "MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.PhieuXuats", "MaKhachHang", "dbo.KhachHangs");
            DropForeignKey("dbo.PhieuXuats", "MaBacSy", "dbo.BacSies");
            DropForeignKey("dbo.NhomKhachHangs", "MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.KhachHangs", "MaNhomKhachHang", "dbo.NhomKhachHangs");
            DropForeignKey("dbo.KhachHangs", "MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.PhieuNhapChiTiets", "MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.PhieuNhapChiTiets", "MaDonViTinh", "dbo.DonViTinhs");
            DropForeignKey("dbo.DonViTinhs", "MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.Thuocs", "MaDangBaoChe", "dbo.DangBaoChes");
            DropForeignKey("dbo.DangBaoChes", "MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.BacSies", "MaNhaThuoc", "dbo.NhaThuocs");
            DropIndex("dbo.Nuocs", new[] { "MaNhaThuoc" });
            DropIndex("dbo.NhomThuocs", new[] { "MaNhaThuoc" });
            DropIndex("dbo.PhieuXuatChiTiets", new[] { "MaDonViTinh" });
            DropIndex("dbo.PhieuXuatChiTiets", new[] { "ThuocId" });
            DropIndex("dbo.PhieuXuatChiTiets", new[] { "MaNhaThuoc" });
            DropIndex("dbo.PhieuXuatChiTiets", new[] { "MaPhieuXuat" });
            DropIndex("dbo.NhomNhaCungCaps", new[] { "MaNhaThuoc" });
            DropIndex("dbo.NhaCungCaps", new[] { "MaNhomNhaCungCap" });
            DropIndex("dbo.NhaCungCaps", new[] { "MaNhaThuoc" });
            DropIndex("dbo.LoaiXuatNhaps", new[] { "MaNhaThuoc" });
            DropIndex("dbo.PhieuXuats", new[] { "MaNguoiTao" });
            DropIndex("dbo.PhieuXuats", new[] { "MaBacSy" });
            DropIndex("dbo.PhieuXuats", new[] { "MaNhaCungCap" });
            DropIndex("dbo.PhieuXuats", new[] { "MaKhachHang" });
            DropIndex("dbo.PhieuXuats", new[] { "MaLoaiXuatNhap" });
            DropIndex("dbo.PhieuXuats", new[] { "MaNhaThuoc" });
            DropIndex("dbo.NhomKhachHangs", new[] { "MaNhaThuoc" });
            DropIndex("dbo.KhachHangs", new[] { "MaNhomKhachHang" });
            DropIndex("dbo.KhachHangs", new[] { "MaNhaThuoc" });
            DropIndex("dbo.PhieuNhaps", new[] { "MaNguoiTao" });
            DropIndex("dbo.PhieuNhaps", new[] { "MaKhachHang" });
            DropIndex("dbo.PhieuNhaps", new[] { "MaNhaCungCap" });
            DropIndex("dbo.PhieuNhaps", new[] { "MaLoaiXuatNhap" });
            DropIndex("dbo.PhieuNhaps", new[] { "MaNhaThuoc" });
            DropIndex("dbo.PhieuNhapChiTiets", new[] { "MaDonViTinh" });
            DropIndex("dbo.PhieuNhapChiTiets", new[] { "ThuocId" });
            DropIndex("dbo.PhieuNhapChiTiets", new[] { "MaNhaThuoc" });
            DropIndex("dbo.PhieuNhapChiTiets", new[] { "MaPhieuNhap" });
            DropIndex("dbo.DonViTinhs", new[] { "MaNhaThuoc" });
            DropIndex("dbo.Thuocs", new[] { "MaDonViThuNguyen" });
            DropIndex("dbo.Thuocs", new[] { "MaDonViXuat" });
            DropIndex("dbo.Thuocs", new[] { "MaDangBaoChe" });
            DropIndex("dbo.Thuocs", new[] { "MaNuoc" });
            DropIndex("dbo.Thuocs", new[] { "MaNhomThuoc" });
            DropIndex("dbo.Thuocs", new[] { "MaNhaThuoc" });
            DropIndex("dbo.DangBaoChes", new[] { "MaNhaThuoc" });
            DropIndex("dbo.BacSies", new[] { "MaNhaThuoc" });
            DropTable("dbo.OperationsToRoles");
            DropTable("dbo.Functions");
            DropTable("dbo.Nuocs");
            DropTable("dbo.NhomThuocs");
            DropTable("dbo.PhieuXuatChiTiets");
            DropTable("dbo.NhomNhaCungCaps");
            DropTable("dbo.NhaCungCaps");
            DropTable("dbo.UserProfile");
            DropTable("dbo.LoaiXuatNhaps");
            DropTable("dbo.PhieuXuats");
            DropTable("dbo.NhomKhachHangs");
            DropTable("dbo.KhachHangs");
            DropTable("dbo.PhieuNhaps");
            DropTable("dbo.PhieuNhapChiTiets");
            DropTable("dbo.DonViTinhs");
            DropTable("dbo.Thuocs");
            DropTable("dbo.DangBaoChes");
            DropTable("dbo.NhaThuocs");
            DropTable("dbo.BacSies");
        }
    }
}
