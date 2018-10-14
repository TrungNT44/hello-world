namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _4 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Thuocs", "MaDangBaoChe", "dbo.DangBaoChes");
            DropForeignKey("dbo.Thuocs", "MaNuoc", "dbo.Nuocs");
            DropIndex("dbo.Thuocs", new[] { "MaNuoc" });
            DropIndex("dbo.Thuocs", new[] { "MaDangBaoChe" });
            CreateTable(
                "dbo.PhieuThuChis",
                c => new
                    {
                        MaPhieu = c.Int(nullable: false, identity: true),
                        SoLuong = c.Single(nullable: false),
                        SoPhieu = c.Int(nullable: false),
                        DienGiai = c.String(),
                        NgayTao = c.DateTime(nullable: false),
                        LoaiPhieu = c.Boolean(nullable: false),
                        MaNhaThuoc = c.String(nullable: false, maxLength: 128),
                        MaKhachHang = c.Int(),
                        MaNhaCungCap = c.Int(),
                        MaNguoiTao = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MaPhieu)
                .ForeignKey("dbo.KhachHangs", t => t.MaKhachHang)
                .ForeignKey("dbo.UserProfile", t => t.MaNguoiTao)
                .ForeignKey("dbo.NhaCungCaps", t => t.MaNhaCungCap)
                .ForeignKey("dbo.NhaThuocs", t => t.MaNhaThuoc)
                .Index(t => t.MaNhaThuoc)
                .Index(t => t.MaKhachHang)
                .Index(t => t.MaNhaCungCap)
                .Index(t => t.MaNguoiTao);
            
            AlterColumn("dbo.NhaThuocs", "SoKinhDoanh", c => c.Int());
            AlterColumn("dbo.Thuocs", "MaNuoc", c => c.Int());
            AlterColumn("dbo.Thuocs", "MaDangBaoChe", c => c.Int());
            CreateIndex("dbo.Thuocs", "MaNuoc");
            CreateIndex("dbo.Thuocs", "MaDangBaoChe");
            AddForeignKey("dbo.Thuocs", "MaDangBaoChe", "dbo.DangBaoChes", "MaDangBaoChe");
            AddForeignKey("dbo.Thuocs", "MaNuoc", "dbo.Nuocs", "MaNuoc");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Thuocs", "MaNuoc", "dbo.Nuocs");
            DropForeignKey("dbo.Thuocs", "MaDangBaoChe", "dbo.DangBaoChes");
            DropForeignKey("dbo.PhieuThuChis", "MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.PhieuThuChis", "MaNhaCungCap", "dbo.NhaCungCaps");
            DropForeignKey("dbo.PhieuThuChis", "MaNguoiTao", "dbo.UserProfile");
            DropForeignKey("dbo.PhieuThuChis", "MaKhachHang", "dbo.KhachHangs");
            DropIndex("dbo.PhieuThuChis", new[] { "MaNguoiTao" });
            DropIndex("dbo.PhieuThuChis", new[] { "MaNhaCungCap" });
            DropIndex("dbo.PhieuThuChis", new[] { "MaKhachHang" });
            DropIndex("dbo.PhieuThuChis", new[] { "MaNhaThuoc" });
            DropIndex("dbo.Thuocs", new[] { "MaDangBaoChe" });
            DropIndex("dbo.Thuocs", new[] { "MaNuoc" });
            AlterColumn("dbo.Thuocs", "MaDangBaoChe", c => c.Int(nullable: false));
            AlterColumn("dbo.Thuocs", "MaNuoc", c => c.Int(nullable: false));
            AlterColumn("dbo.NhaThuocs", "SoKinhDoanh", c => c.Int(nullable: false));
            DropTable("dbo.PhieuThuChis");
            CreateIndex("dbo.Thuocs", "MaDangBaoChe");
            CreateIndex("dbo.Thuocs", "MaNuoc");
            AddForeignKey("dbo.Thuocs", "MaNuoc", "dbo.Nuocs", "MaNuoc", cascadeDelete: true);
            AddForeignKey("dbo.Thuocs", "MaDangBaoChe", "dbo.DangBaoChes", "MaDangBaoChe", cascadeDelete: true);
        }
    }
}
