namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _19 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Thuocs", "MaThuoc", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Thuocs", "MaThuoc", c => c.String());
        }
    }
}
