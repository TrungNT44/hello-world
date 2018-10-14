namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _7 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.PhieuNhapChiTiets", "HanDung");
            DropColumn("dbo.PhieuXuatChiTiets", "HanDung");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PhieuXuatChiTiets", "HanDung", c => c.DateTime(nullable: false));
            AddColumn("dbo.PhieuNhapChiTiets", "HanDung", c => c.DateTime(nullable: false));
        }
    }
}
