namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _3 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.PhieuXuats", new[] { "MaBacSy" });
            AlterColumn("dbo.PhieuXuats", "MaBacSy", c => c.Int());
            CreateIndex("dbo.PhieuXuats", "MaBacSy");
        }
        
        public override void Down()
        {
            DropIndex("dbo.PhieuXuats", new[] { "MaBacSy" });
            AlterColumn("dbo.PhieuXuats", "MaBacSy", c => c.Int(nullable: false));
            CreateIndex("dbo.PhieuXuats", "MaBacSy");
        }
    }
}
