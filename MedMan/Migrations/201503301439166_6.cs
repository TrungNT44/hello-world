namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _6 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PhieuThuChis", "LoaiPhieu", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PhieuThuChis", "LoaiPhieu", c => c.Boolean(nullable: false));
        }
    }
}
