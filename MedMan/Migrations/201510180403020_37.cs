namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _37 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.NhaCungCaps", "NoDauKy", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.NhaCungCaps", "NoDauKy", c => c.Int());
        }
    }
}
