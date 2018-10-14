namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _33 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PhieuKiemKes", "DaCanKho", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PhieuKiemKes", "DaCanKho");
        }
    }
}
