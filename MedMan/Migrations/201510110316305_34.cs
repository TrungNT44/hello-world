namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _34 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Nuocs", "NhaThuoc_MaNhaThuoc", "dbo.NhaThuocs");
            DropIndex("dbo.Nuocs", new[] { "NhaThuoc_MaNhaThuoc" });
            CreateTable(
                "dbo.ChiNhanhs",
                c => new
                    {
                        MaChiNhanh = c.Int(nullable: false, identity: true),
                        TenChiNhanh = c.String(nullable: false),
                        NguoiPhuTrach = c.String(),
                        DiaChiChiNhanh = c.String(),
                        NhaThuoc_MaNhaThuoc = c.String(maxLength: 128),
                        PhieuNhap_MaPhieuNhap = c.Int(),
                        PhieuThuChi_MaPhieu = c.Int(),
                        PhieuXuat_MaPhieuXuat = c.Int(),
                        PhieuKiemKe_MaPhieuKiemKe = c.Int(),
                    })
                .PrimaryKey(t => t.MaChiNhanh)
                .ForeignKey("dbo.NhaThuocs", t => t.NhaThuoc_MaNhaThuoc)
                .ForeignKey("dbo.PhieuNhaps", t => t.PhieuNhap_MaPhieuNhap)
                .ForeignKey("dbo.PhieuThuChis", t => t.PhieuThuChi_MaPhieu)
                .ForeignKey("dbo.PhieuXuats", t => t.PhieuXuat_MaPhieuXuat)
                .ForeignKey("dbo.PhieuKiemKes", t => t.PhieuKiemKe_MaPhieuKiemKe)
                .Index(t => t.NhaThuoc_MaNhaThuoc)
                .Index(t => t.PhieuNhap_MaPhieuNhap)
                .Index(t => t.PhieuThuChi_MaPhieu)
                .Index(t => t.PhieuXuat_MaPhieuXuat)
                .Index(t => t.PhieuKiemKe_MaPhieuKiemKe);
            
            CreateTable(
                "dbo.LoaiThuChis",
                c => new
                    {
                        MaLoaiPhieu = c.Int(nullable: false, identity: true),
                        TenLoaiThuChi = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.MaLoaiPhieu);
            
            CreateTable(
                "dbo.NhaThuocThamSoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GiaTri = c.String(nullable: false),
                        NhaThuoc_MaNhaThuoc = c.String(maxLength: 128),
                        ThamSoNhaThuoc_MaThamSoNhaThuoc = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NhaThuocs", t => t.NhaThuoc_MaNhaThuoc)
                .ForeignKey("dbo.ThamSoNhaThuocs", t => t.ThamSoNhaThuoc_MaThamSoNhaThuoc)
                .Index(t => t.NhaThuoc_MaNhaThuoc)
                .Index(t => t.ThamSoNhaThuoc_MaThamSoNhaThuoc);
            
            CreateTable(
                "dbo.ThamSoNhaThuocs",
                c => new
                    {
                        MaThamSoNhaThuoc = c.Int(nullable: false, identity: true),
                        MoTaThamSo = c.String(nullable: false),
                        MoTaGiaTri = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.MaThamSoNhaThuoc);
            
            CreateTable(
                "dbo.ThamSoNguoiDungs",
                c => new
                    {
                        MaThamSoNguoiDung = c.Int(nullable: false, identity: true),
                        MoTaThamSo = c.String(nullable: false),
                        MoTaGiaTri = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.MaThamSoNguoiDung);
            
            CreateTable(
                "dbo.ThuocChiNhanhs",
                c => new
                    {
                        MaChiNhanh = c.Int(nullable: false, identity: true),
                        GiaNhap = c.Decimal(nullable: false, precision: 18, scale: 2),
                        GiaBanBuon = c.Decimal(nullable: false, precision: 18, scale: 2),
                        GiaBanLe = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SoDuDauKy = c.Decimal(nullable: false, precision: 18, scale: 2),
                        GiaDauKy = c.Decimal(nullable: false, precision: 18, scale: 2),
                        HanDung = c.DateTime(),
                        DuTru = c.Int(nullable: false),
                        HoatDong = c.Boolean(nullable: false),
                        DiaChiChiNhanh = c.String(),
                        ChiNhanh_MaChiNhanh = c.Int(),
                        NhaThuoc_MaNhaThuoc = c.String(maxLength: 128),
                        Thuoc_ThuocId = c.Int(),
                    })
                .PrimaryKey(t => t.MaChiNhanh)
                .ForeignKey("dbo.ChiNhanhs", t => t.ChiNhanh_MaChiNhanh)
                .ForeignKey("dbo.NhaThuocs", t => t.NhaThuoc_MaNhaThuoc)
                .ForeignKey("dbo.Thuocs", t => t.Thuoc_ThuocId)
                .Index(t => t.ChiNhanh_MaChiNhanh)
                .Index(t => t.NhaThuoc_MaNhaThuoc)
                .Index(t => t.Thuoc_ThuocId);
            
            CreateTable(
                "dbo.UserRoles",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Controller = c.String(),
                        Action = c.String(),
                        UserPermission_Id = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserPermissions", t => t.UserPermission_Id)
                .Index(t => t.UserPermission_Id);
            
            AddColumn("dbo.Thuocs", "HanDung", c => c.DateTime());
            AddColumn("dbo.Thuocs", "DuTru", c => c.Int(nullable: false));
            AddColumn("dbo.PhieuThuChis", "LoaiThuChi_MaLoaiPhieu", c => c.Int());
            CreateIndex("dbo.PhieuThuChis", "LoaiThuChi_MaLoaiPhieu");
            AddForeignKey("dbo.PhieuThuChis", "LoaiThuChi_MaLoaiPhieu", "dbo.LoaiThuChis", "MaLoaiPhieu");
            DropColumn("dbo.Nuocs", "NhaThuoc_MaNhaThuoc");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Nuocs", "NhaThuoc_MaNhaThuoc", c => c.String(maxLength: 128));
            DropForeignKey("dbo.UserRoles", "UserPermission_Id", "dbo.UserPermissions");
            DropForeignKey("dbo.ThuocChiNhanhs", "Thuoc_ThuocId", "dbo.Thuocs");
            DropForeignKey("dbo.ThuocChiNhanhs", "NhaThuoc_MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.ThuocChiNhanhs", "ChiNhanh_MaChiNhanh", "dbo.ChiNhanhs");
            DropForeignKey("dbo.NhaThuocThamSoes", "ThamSoNhaThuoc_MaThamSoNhaThuoc", "dbo.ThamSoNhaThuocs");
            DropForeignKey("dbo.NhaThuocThamSoes", "NhaThuoc_MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.ChiNhanhs", "PhieuKiemKe_MaPhieuKiemKe", "dbo.PhieuKiemKes");
            DropForeignKey("dbo.ChiNhanhs", "PhieuXuat_MaPhieuXuat", "dbo.PhieuXuats");
            DropForeignKey("dbo.PhieuThuChis", "LoaiThuChi_MaLoaiPhieu", "dbo.LoaiThuChis");
            DropForeignKey("dbo.ChiNhanhs", "PhieuThuChi_MaPhieu", "dbo.PhieuThuChis");
            DropForeignKey("dbo.ChiNhanhs", "PhieuNhap_MaPhieuNhap", "dbo.PhieuNhaps");
            DropForeignKey("dbo.ChiNhanhs", "NhaThuoc_MaNhaThuoc", "dbo.NhaThuocs");
            DropIndex("dbo.UserRoles", new[] { "UserPermission_Id" });
            DropIndex("dbo.ThuocChiNhanhs", new[] { "Thuoc_ThuocId" });
            DropIndex("dbo.ThuocChiNhanhs", new[] { "NhaThuoc_MaNhaThuoc" });
            DropIndex("dbo.ThuocChiNhanhs", new[] { "ChiNhanh_MaChiNhanh" });
            DropIndex("dbo.NhaThuocThamSoes", new[] { "ThamSoNhaThuoc_MaThamSoNhaThuoc" });
            DropIndex("dbo.NhaThuocThamSoes", new[] { "NhaThuoc_MaNhaThuoc" });
            DropIndex("dbo.PhieuThuChis", new[] { "LoaiThuChi_MaLoaiPhieu" });
            DropIndex("dbo.ChiNhanhs", new[] { "PhieuKiemKe_MaPhieuKiemKe" });
            DropIndex("dbo.ChiNhanhs", new[] { "PhieuXuat_MaPhieuXuat" });
            DropIndex("dbo.ChiNhanhs", new[] { "PhieuThuChi_MaPhieu" });
            DropIndex("dbo.ChiNhanhs", new[] { "PhieuNhap_MaPhieuNhap" });
            DropIndex("dbo.ChiNhanhs", new[] { "NhaThuoc_MaNhaThuoc" });
            DropColumn("dbo.PhieuThuChis", "LoaiThuChi_MaLoaiPhieu");
            DropColumn("dbo.Thuocs", "DuTru");
            DropColumn("dbo.Thuocs", "HanDung");
            DropTable("dbo.UserRoles");
            DropTable("dbo.ThuocChiNhanhs");
            DropTable("dbo.ThamSoNguoiDungs");
            DropTable("dbo.ThamSoNhaThuocs");
            DropTable("dbo.NhaThuocThamSoes");
            DropTable("dbo.LoaiThuChis");
            DropTable("dbo.ChiNhanhs");
            CreateIndex("dbo.Nuocs", "NhaThuoc_MaNhaThuoc");
            AddForeignKey("dbo.Nuocs", "NhaThuoc_MaNhaThuoc", "dbo.NhaThuocs", "MaNhaThuoc");
        }
    }
}
