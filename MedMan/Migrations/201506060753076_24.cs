namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _24 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PhieuXuatChiTiets", "ChietKhau", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PhieuXuatChiTiets", "GiaXuat", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PhieuXuatChiTiets", "GiaXuat", c => c.Int(nullable: false));
            AlterColumn("dbo.PhieuXuatChiTiets", "ChietKhau", c => c.Int(nullable: false));
        }
    }
}
