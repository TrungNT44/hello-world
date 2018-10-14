namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _15 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserPermissions", "Controller", c => c.String());
            DropColumn("dbo.UserPermissions", "View");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UserPermissions", "View", c => c.String());
            DropColumn("dbo.UserPermissions", "Controller");
        }
    }
}
