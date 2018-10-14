namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _46 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PhieuNhapChiTiets", "NgayNhap", c => c.DateTime(nullable: false));
            AddColumn("dbo.PhieuXuatChiTiets", "NgayXuat", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PhieuXuatChiTiets", "NgayXuat");
            DropColumn("dbo.PhieuNhapChiTiets", "NgayNhap");
        }
    }
}
