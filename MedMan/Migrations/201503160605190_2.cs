namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _2 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.PhieuNhaps", new[] { "MaNhaCungCap" });
            DropIndex("dbo.PhieuXuats", new[] { "MaKhachHang" });
            AlterColumn("dbo.PhieuNhaps", "MaNhaCungCap", c => c.Int());
            AlterColumn("dbo.PhieuXuats", "MaKhachHang", c => c.Int());
            CreateIndex("dbo.PhieuNhaps", "MaNhaCungCap");
            CreateIndex("dbo.PhieuXuats", "MaKhachHang");
        }
        
        public override void Down()
        {
            DropIndex("dbo.PhieuXuats", new[] { "MaKhachHang" });
            DropIndex("dbo.PhieuNhaps", new[] { "MaNhaCungCap" });
            AlterColumn("dbo.PhieuXuats", "MaKhachHang", c => c.Int(nullable: false));
            AlterColumn("dbo.PhieuNhaps", "MaNhaCungCap", c => c.Int(nullable: false));
            CreateIndex("dbo.PhieuXuats", "MaKhachHang");
            CreateIndex("dbo.PhieuNhaps", "MaNhaCungCap");
        }
    }
}
