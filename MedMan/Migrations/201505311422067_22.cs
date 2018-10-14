namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _22 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PhieuNhaps", "SoPhieuNhap", c => c.Long(nullable: false));
            AlterColumn("dbo.PhieuXuats", "SoPhieuXuat", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PhieuXuats", "SoPhieuXuat", c => c.String());
            AlterColumn("dbo.PhieuNhaps", "SoPhieuNhap", c => c.String());
        }
    }
}
