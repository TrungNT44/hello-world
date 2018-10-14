namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _43 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Settings", "Value", c => c.String(nullable: false));
            AlterColumn("dbo.Settings", "Active", c => c.Boolean());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Settings", "Active", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Settings", "Value", c => c.String());
        }
    }
}
