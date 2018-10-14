namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _10 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NhanVienNhaThuocs",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Role = c.String(),
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
            DropForeignKey("dbo.NhanVienNhaThuocs", "User_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.NhanVienNhaThuocs", "NhaThuoc_MaNhaThuoc", "dbo.NhaThuocs");
            DropIndex("dbo.NhanVienNhaThuocs", new[] { "User_UserId" });
            DropIndex("dbo.NhanVienNhaThuocs", new[] { "NhaThuoc_MaNhaThuoc" });
            DropTable("dbo.NhanVienNhaThuocs");
        }
    }
}
