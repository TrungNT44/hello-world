namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _45 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Settings", "MaNhaThuoc", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Settings", "MaNhaThuoc");
        }
    }
}
