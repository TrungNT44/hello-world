namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _8 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PhieuNhapChiTiets", "SoLuong", c => c.Single(nullable: false));
            AlterColumn("dbo.PhieuNhaps", "TongTien", c => c.Single(nullable: false));
            AlterColumn("dbo.PhieuXuats", "TongTien", c => c.Single(nullable: false));
            AlterColumn("dbo.PhieuXuatChiTiets", "SoLuong", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PhieuXuatChiTiets", "SoLuong", c => c.Int(nullable: false));
            AlterColumn("dbo.PhieuXuats", "TongTien", c => c.Int(nullable: false));
            AlterColumn("dbo.PhieuNhaps", "TongTien", c => c.Int(nullable: false));
            AlterColumn("dbo.PhieuNhapChiTiets", "SoLuong", c => c.Int(nullable: false));
        }
    }
}
