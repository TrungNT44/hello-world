namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _18 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BacSies", "Created", c => c.DateTime());
            AddColumn("dbo.BacSies", "Modified", c => c.DateTime());
            AddColumn("dbo.BacSies", "CreatedBy_UserId", c => c.Int());
            AddColumn("dbo.BacSies", "ModifiedBy_UserId", c => c.Int());
            CreateIndex("dbo.BacSies", "CreatedBy_UserId");
            CreateIndex("dbo.BacSies", "ModifiedBy_UserId");
            AddForeignKey("dbo.BacSies", "CreatedBy_UserId", "dbo.UserProfile", "UserId");
            AddForeignKey("dbo.BacSies", "ModifiedBy_UserId", "dbo.UserProfile", "UserId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BacSies", "ModifiedBy_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.BacSies", "CreatedBy_UserId", "dbo.UserProfile");
            DropIndex("dbo.BacSies", new[] { "ModifiedBy_UserId" });
            DropIndex("dbo.BacSies", new[] { "CreatedBy_UserId" });
            DropColumn("dbo.BacSies", "ModifiedBy_UserId");
            DropColumn("dbo.BacSies", "CreatedBy_UserId");
            DropColumn("dbo.BacSies", "Modified");
            DropColumn("dbo.BacSies", "Created");
        }
    }
}
