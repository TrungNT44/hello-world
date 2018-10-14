namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _48 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PhieuKiemKes", "SoPhieu", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PhieuKiemKes", "SoPhieu");
        }
    }
}
