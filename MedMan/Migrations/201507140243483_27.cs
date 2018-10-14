namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _27 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserProfile", "SoCMT", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserProfile", "SoCMT");
        }
    }
}
