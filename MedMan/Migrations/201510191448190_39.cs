namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _39 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.UserPermissions", "Object");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UserPermissions", "Object", c => c.String());
        }
    }
}
