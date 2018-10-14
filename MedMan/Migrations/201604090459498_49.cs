namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _49 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TongKetKies",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        KyTinhToan = c.DateTime(nullable: false),
                        TonCuoiKy_SoLuong = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TonCuoiKy_GiaTri = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TongDoanhThu = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TongLoiNhuan = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TrangThai = c.Boolean(nullable: false),
                        NhaThuoc_MaNhaThuoc = c.String(maxLength: 128),
                        Thuoc_ThuocId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NhaThuocs", t => t.NhaThuoc_MaNhaThuoc)
                .ForeignKey("dbo.Thuocs", t => t.Thuoc_ThuocId)
                .Index(t => t.NhaThuoc_MaNhaThuoc)
                .Index(t => t.Thuoc_ThuocId);
            
            CreateTable(
                "dbo.TongKetKyChiTiets",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SoLuong = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Gia = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Date = c.DateTime(nullable: false),
                        NhaThuoc_MaNhaThuoc = c.String(maxLength: 128),
                        Thuoc_ThuocId = c.Int(),
                        TongKetKy_Id = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NhaThuocs", t => t.NhaThuoc_MaNhaThuoc)
                .ForeignKey("dbo.Thuocs", t => t.Thuoc_ThuocId)
                .ForeignKey("dbo.TongKetKies", t => t.TongKetKy_Id)
                .Index(t => t.NhaThuoc_MaNhaThuoc)
                .Index(t => t.Thuoc_ThuocId)
                .Index(t => t.TongKetKy_Id);
            
            AddColumn("dbo.PhieuNhapChiTiets", "Option1", c => c.String());
            AddColumn("dbo.PhieuNhapChiTiets", "Option2", c => c.String());
            AddColumn("dbo.PhieuNhapChiTiets", "Option3", c => c.String());
            AddColumn("dbo.PhieuNhapChiTiets", "Option4", c => c.String());
            AddColumn("dbo.PhieuNhapChiTiets", "Option5", c => c.String());
            AddColumn("dbo.PhieuXuatChiTiets", "Option1", c => c.String());
            AddColumn("dbo.PhieuXuatChiTiets", "Option2", c => c.String());
            AddColumn("dbo.PhieuXuatChiTiets", "Option3", c => c.String());
            AddColumn("dbo.PhieuXuatChiTiets", "Option4", c => c.String());
            AddColumn("dbo.PhieuXuatChiTiets", "Option5", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TongKetKyChiTiets", "TongKetKy_Id", "dbo.TongKetKies");
            DropForeignKey("dbo.TongKetKyChiTiets", "Thuoc_ThuocId", "dbo.Thuocs");
            DropForeignKey("dbo.TongKetKyChiTiets", "NhaThuoc_MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.TongKetKies", "Thuoc_ThuocId", "dbo.Thuocs");
            DropForeignKey("dbo.TongKetKies", "NhaThuoc_MaNhaThuoc", "dbo.NhaThuocs");
            DropIndex("dbo.TongKetKyChiTiets", new[] { "TongKetKy_Id" });
            DropIndex("dbo.TongKetKyChiTiets", new[] { "Thuoc_ThuocId" });
            DropIndex("dbo.TongKetKyChiTiets", new[] { "NhaThuoc_MaNhaThuoc" });
            DropIndex("dbo.TongKetKies", new[] { "Thuoc_ThuocId" });
            DropIndex("dbo.TongKetKies", new[] { "NhaThuoc_MaNhaThuoc" });
            DropColumn("dbo.PhieuXuatChiTiets", "Option5");
            DropColumn("dbo.PhieuXuatChiTiets", "Option4");
            DropColumn("dbo.PhieuXuatChiTiets", "Option3");
            DropColumn("dbo.PhieuXuatChiTiets", "Option2");
            DropColumn("dbo.PhieuXuatChiTiets", "Option1");
            DropColumn("dbo.PhieuNhapChiTiets", "Option5");
            DropColumn("dbo.PhieuNhapChiTiets", "Option4");
            DropColumn("dbo.PhieuNhapChiTiets", "Option3");
            DropColumn("dbo.PhieuNhapChiTiets", "Option2");
            DropColumn("dbo.PhieuNhapChiTiets", "Option1");
            DropTable("dbo.TongKetKyChiTiets");
            DropTable("dbo.TongKetKies");
        }
    }
}
