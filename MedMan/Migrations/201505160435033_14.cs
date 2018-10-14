namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _14 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserPermissions",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Title = c.String(),
                        Action = c.String(),
                        View = c.String(),
                        NhaThuoc_MaNhaThuoc = c.String(maxLength: 128),
                        User_UserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NhaThuocs", t => t.NhaThuoc_MaNhaThuoc)
                .ForeignKey("dbo.UserProfile", t => t.User_UserId)
                .Index(t => t.NhaThuoc_MaNhaThuoc)
                .Index(t => t.User_UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserPermissions", "User_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.UserPermissions", "NhaThuoc_MaNhaThuoc", "dbo.NhaThuocs");
            DropIndex("dbo.UserPermissions", new[] { "User_UserId" });
            DropIndex("dbo.UserPermissions", new[] { "NhaThuoc_MaNhaThuoc" });
            DropTable("dbo.UserPermissions");
        }
    }
}
