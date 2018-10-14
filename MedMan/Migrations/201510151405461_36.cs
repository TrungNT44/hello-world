namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _36 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.BacSies", "Active", c => c.Boolean());
            AlterColumn("dbo.NhaThuocs", "Active", c => c.Boolean());
            AlterColumn("dbo.Thuocs", "Active", c => c.Boolean());
            AlterColumn("dbo.PhieuNhaps", "Active", c => c.Boolean());
            AlterColumn("dbo.KhachHangs", "Active", c => c.Boolean());
            AlterColumn("dbo.NhomKhachHangs", "Active", c => c.Boolean());
            AlterColumn("dbo.PhieuThuChis", "Active", c => c.Boolean());
            AlterColumn("dbo.NhaCungCaps", "Active", c => c.Boolean());
            AlterColumn("dbo.NhomNhaCungCaps", "Active", c => c.Boolean());
            AlterColumn("dbo.PhieuXuats", "Active", c => c.Boolean());
            AlterColumn("dbo.NhomThuocs", "Active", c => c.Boolean());
            AlterColumn("dbo.PhieuKiemKes", "Active", c => c.Boolean());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PhieuKiemKes", "Active", c => c.Boolean(nullable: false));
            AlterColumn("dbo.NhomThuocs", "Active", c => c.Boolean(nullable: false));
            AlterColumn("dbo.PhieuXuats", "Active", c => c.Boolean(nullable: false));
            AlterColumn("dbo.NhomNhaCungCaps", "Active", c => c.Boolean(nullable: false));
            AlterColumn("dbo.NhaCungCaps", "Active", c => c.Boolean(nullable: false));
            AlterColumn("dbo.PhieuThuChis", "Active", c => c.Boolean(nullable: false));
            AlterColumn("dbo.NhomKhachHangs", "Active", c => c.Boolean(nullable: false));
            AlterColumn("dbo.KhachHangs", "Active", c => c.Boolean(nullable: false));
            AlterColumn("dbo.PhieuNhaps", "Active", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Thuocs", "Active", c => c.Boolean(nullable: false));
            AlterColumn("dbo.NhaThuocs", "Active", c => c.Boolean(nullable: false));
            AlterColumn("dbo.BacSies", "Active", c => c.Boolean(nullable: false));
        }
    }
}
