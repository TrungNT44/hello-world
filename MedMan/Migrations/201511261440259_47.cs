namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _47 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.PhieuNhapChiTiets", "NgayNhap");
            DropColumn("dbo.PhieuXuatChiTiets", "NgayXuat");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PhieuXuatChiTiets", "NgayXuat", c => c.DateTime(nullable: false));
            AddColumn("dbo.PhieuNhapChiTiets", "NgayNhap", c => c.DateTime(nullable: false));
        }
    }
}
