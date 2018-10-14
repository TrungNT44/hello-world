namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _12 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NhomKhachHangs", "Created", c => c.DateTime(nullable: false));
            AddColumn("dbo.NhomKhachHangs", "Modified", c => c.DateTime(nullable: false));
            AddColumn("dbo.NhomKhachHangs", "CreatedBy_UserId", c => c.Int());
            AddColumn("dbo.NhomKhachHangs", "ModifiedBy_UserId", c => c.Int());
            CreateIndex("dbo.NhomKhachHangs", "CreatedBy_UserId");
            CreateIndex("dbo.NhomKhachHangs", "ModifiedBy_UserId");
            AddForeignKey("dbo.NhomKhachHangs", "CreatedBy_UserId", "dbo.UserProfile", "UserId");
            AddForeignKey("dbo.NhomKhachHangs", "ModifiedBy_UserId", "dbo.UserProfile", "UserId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NhomKhachHangs", "ModifiedBy_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.NhomKhachHangs", "CreatedBy_UserId", "dbo.UserProfile");
            DropIndex("dbo.NhomKhachHangs", new[] { "ModifiedBy_UserId" });
            DropIndex("dbo.NhomKhachHangs", new[] { "CreatedBy_UserId" });
            DropColumn("dbo.NhomKhachHangs", "ModifiedBy_UserId");
            DropColumn("dbo.NhomKhachHangs", "CreatedBy_UserId");
            DropColumn("dbo.NhomKhachHangs", "Modified");
            DropColumn("dbo.NhomKhachHangs", "Created");
        }
    }
}
