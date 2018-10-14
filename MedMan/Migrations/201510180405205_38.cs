namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _38 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.KhachHangs", "NoDauKy", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.KhachHangs", "NoDauKy", c => c.Int());
        }
    }
}
