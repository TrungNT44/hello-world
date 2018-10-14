namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _32 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserPermissions", "User_UserId", "dbo.UserProfile");
            DropIndex("dbo.UserPermissions", new[] { "User_UserId" });
            AlterColumn("dbo.UserPermissions", "User_UserId", c => c.Int(nullable: false));
            CreateIndex("dbo.UserPermissions", "User_UserId");
            AddForeignKey("dbo.UserPermissions", "User_UserId", "dbo.UserProfile", "UserId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserPermissions", "User_UserId", "dbo.UserProfile");
            DropIndex("dbo.UserPermissions", new[] { "User_UserId" });
            AlterColumn("dbo.UserPermissions", "User_UserId", c => c.Int());
            CreateIndex("dbo.UserPermissions", "User_UserId");
            AddForeignKey("dbo.UserPermissions", "User_UserId", "dbo.UserProfile", "UserId");
        }
    }
}
