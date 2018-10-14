namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _41 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PhieuKiemKeChiTiets", "TonKho", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PhieuKiemKeChiTiets", "ThucTe", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PhieuKiemKeChiTiets", "ThucTe", c => c.Single(nullable: false));
            AlterColumn("dbo.PhieuKiemKeChiTiets", "TonKho", c => c.Single(nullable: false));
        }
    }
}
