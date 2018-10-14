namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _29 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PhieuNhaps", "DaTra", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PhieuXuats", "DaTra", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PhieuXuats", "DaTra", c => c.Int(nullable: false));
            AlterColumn("dbo.PhieuNhaps", "DaTra", c => c.Int(nullable: false));
        }
    }
}
