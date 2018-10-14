namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _35 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BacSies", "Active", c => c.Boolean(nullable: false));
            AddColumn("dbo.NhaThuocs", "Active", c => c.Boolean(nullable: false));
            AddColumn("dbo.Thuocs", "Active", c => c.Boolean(nullable: false));
            AddColumn("dbo.PhieuNhaps", "Active", c => c.Boolean(nullable: false));
            AddColumn("dbo.KhachHangs", "Active", c => c.Boolean(nullable: false));
            AddColumn("dbo.NhomKhachHangs", "Active", c => c.Boolean(nullable: false));
            AddColumn("dbo.PhieuThuChis", "Active", c => c.Boolean(nullable: false));
            AddColumn("dbo.NhaCungCaps", "Active", c => c.Boolean(nullable: false));
            AddColumn("dbo.NhomNhaCungCaps", "Active", c => c.Boolean(nullable: false));
            AddColumn("dbo.PhieuXuats", "Active", c => c.Boolean(nullable: false));
            AddColumn("dbo.NhomThuocs", "Active", c => c.Boolean(nullable: false));
            AddColumn("dbo.PhieuKiemKes", "Active", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PhieuKiemKes", "Active");
            DropColumn("dbo.NhomThuocs", "Active");
            DropColumn("dbo.PhieuXuats", "Active");
            DropColumn("dbo.NhomNhaCungCaps", "Active");
            DropColumn("dbo.NhaCungCaps", "Active");
            DropColumn("dbo.PhieuThuChis", "Active");
            DropColumn("dbo.NhomKhachHangs", "Active");
            DropColumn("dbo.KhachHangs", "Active");
            DropColumn("dbo.PhieuNhaps", "Active");
            DropColumn("dbo.Thuocs", "Active");
            DropColumn("dbo.NhaThuocs", "Active");
            DropColumn("dbo.BacSies", "Active");
        }
    }
}
