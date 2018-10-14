namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _44 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.Settings");
            AddColumn("dbo.Settings", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Settings", "Key", c => c.String(nullable: false));
            AddPrimaryKey("dbo.Settings", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.Settings");
            AlterColumn("dbo.Settings", "Key", c => c.String(nullable: false, maxLength: 128));
            DropColumn("dbo.Settings", "Id");
            AddPrimaryKey("dbo.Settings", "Key");
        }
    }
}
