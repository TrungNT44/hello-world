namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PhieuKiemKes",
                c => new
                    {
                        MaPhieuKiemKe = c.Int(nullable: false, identity: true),
                        NgayTao = c.DateTime(nullable: false),
                        MaNguoiTao = c.Int(nullable: false),
                        MaPhieuNhap = c.Int(),
                        MaPhieuXuat = c.Int(),
                        MaNhaThuoc = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.MaPhieuKiemKe)
                .ForeignKey("dbo.UserProfile", t => t.MaNguoiTao)
                .ForeignKey("dbo.NhaThuocs", t => t.MaNhaThuoc)
                .ForeignKey("dbo.PhieuNhaps", t => t.MaPhieuNhap)
                .ForeignKey("dbo.PhieuXuats", t => t.MaPhieuXuat)
                .Index(t => t.MaNguoiTao)
                .Index(t => t.MaPhieuNhap)
                .Index(t => t.MaPhieuXuat)
                .Index(t => t.MaNhaThuoc);
            
            CreateTable(
                "dbo.PhieuKiemKeChiTiets",
                c => new
                    {
                        MaPhieuKiemKeCt = c.Int(nullable: false, identity: true),
                        ThuocId = c.Int(nullable: false),
                        TonKho = c.Single(nullable: false),
                        ThucTe = c.Single(nullable: false),
                        MaPhieuKiemKe = c.Int(nullable: false),
                        MaNhaThuoc = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.MaPhieuKiemKeCt)
                .ForeignKey("dbo.NhaThuocs", t => t.MaNhaThuoc)
                .ForeignKey("dbo.PhieuKiemKes", t => t.MaPhieuKiemKe)
                .ForeignKey("dbo.Thuocs", t => t.ThuocId)
                .Index(t => t.ThuocId)
                .Index(t => t.MaPhieuKiemKe)
                .Index(t => t.MaNhaThuoc);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PhieuKiemKes", "MaPhieuXuat", "dbo.PhieuXuats");
            DropForeignKey("dbo.PhieuKiemKes", "MaPhieuNhap", "dbo.PhieuNhaps");
            DropForeignKey("dbo.PhieuKiemKeChiTiets", "ThuocId", "dbo.Thuocs");
            DropForeignKey("dbo.PhieuKiemKeChiTiets", "MaPhieuKiemKe", "dbo.PhieuKiemKes");
            DropForeignKey("dbo.PhieuKiemKeChiTiets", "MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.PhieuKiemKes", "MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.PhieuKiemKes", "MaNguoiTao", "dbo.UserProfile");
            DropIndex("dbo.PhieuKiemKeChiTiets", new[] { "MaNhaThuoc" });
            DropIndex("dbo.PhieuKiemKeChiTiets", new[] { "MaPhieuKiemKe" });
            DropIndex("dbo.PhieuKiemKeChiTiets", new[] { "ThuocId" });
            DropIndex("dbo.PhieuKiemKes", new[] { "MaNhaThuoc" });
            DropIndex("dbo.PhieuKiemKes", new[] { "MaPhieuXuat" });
            DropIndex("dbo.PhieuKiemKes", new[] { "MaPhieuNhap" });
            DropIndex("dbo.PhieuKiemKes", new[] { "MaNguoiTao" });
            DropTable("dbo.PhieuKiemKeChiTiets");
            DropTable("dbo.PhieuKiemKes");
        }
    }
}
