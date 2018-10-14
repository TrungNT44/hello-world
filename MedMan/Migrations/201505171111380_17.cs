namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _17 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NhaThuocs", "Created", c => c.DateTime());
            AddColumn("dbo.NhaThuocs", "Modified", c => c.DateTime());
            AddColumn("dbo.NhaThuocs", "CreatedBy_UserId", c => c.Int());
            AddColumn("dbo.NhaThuocs", "ModifiedBy_UserId", c => c.Int());
            AddColumn("dbo.Thuocs", "Created", c => c.DateTime());
            AddColumn("dbo.Thuocs", "Modified", c => c.DateTime());
            AddColumn("dbo.Thuocs", "CreatedBy_UserId", c => c.Int());
            AddColumn("dbo.Thuocs", "ModifiedBy_UserId", c => c.Int());
            AddColumn("dbo.KhachHangs", "Created", c => c.DateTime());
            AddColumn("dbo.KhachHangs", "Modified", c => c.DateTime());
            AddColumn("dbo.KhachHangs", "CreatedBy_UserId", c => c.Int());
            AddColumn("dbo.KhachHangs", "ModifiedBy_UserId", c => c.Int());
            AddColumn("dbo.NhaCungCaps", "Created", c => c.DateTime());
            AddColumn("dbo.NhaCungCaps", "Modified", c => c.DateTime());
            AddColumn("dbo.NhaCungCaps", "CreatedBy_UserId", c => c.Int());
            AddColumn("dbo.NhaCungCaps", "ModifiedBy_UserId", c => c.Int());
            AddColumn("dbo.NhomNhaCungCaps", "Created", c => c.DateTime());
            AddColumn("dbo.NhomNhaCungCaps", "Modified", c => c.DateTime());
            AddColumn("dbo.NhomNhaCungCaps", "CreatedBy_UserId", c => c.Int());
            AddColumn("dbo.NhomNhaCungCaps", "ModifiedBy_UserId", c => c.Int());
            AddColumn("dbo.NhomThuocs", "Created", c => c.DateTime());
            AddColumn("dbo.NhomThuocs", "Modified", c => c.DateTime());
            AddColumn("dbo.NhomThuocs", "CreatedBy_UserId", c => c.Int());
            AddColumn("dbo.NhomThuocs", "ModifiedBy_UserId", c => c.Int());
            CreateIndex("dbo.NhaThuocs", "CreatedBy_UserId");
            CreateIndex("dbo.NhaThuocs", "ModifiedBy_UserId");
            CreateIndex("dbo.Thuocs", "CreatedBy_UserId");
            CreateIndex("dbo.Thuocs", "ModifiedBy_UserId");
            CreateIndex("dbo.KhachHangs", "CreatedBy_UserId");
            CreateIndex("dbo.KhachHangs", "ModifiedBy_UserId");
            CreateIndex("dbo.NhaCungCaps", "CreatedBy_UserId");
            CreateIndex("dbo.NhaCungCaps", "ModifiedBy_UserId");
            CreateIndex("dbo.NhomNhaCungCaps", "CreatedBy_UserId");
            CreateIndex("dbo.NhomNhaCungCaps", "ModifiedBy_UserId");
            CreateIndex("dbo.NhomThuocs", "CreatedBy_UserId");
            CreateIndex("dbo.NhomThuocs", "ModifiedBy_UserId");
            AddForeignKey("dbo.Thuocs", "CreatedBy_UserId", "dbo.UserProfile", "UserId");
            AddForeignKey("dbo.KhachHangs", "CreatedBy_UserId", "dbo.UserProfile", "UserId");
            AddForeignKey("dbo.KhachHangs", "ModifiedBy_UserId", "dbo.UserProfile", "UserId");
            AddForeignKey("dbo.NhaCungCaps", "CreatedBy_UserId", "dbo.UserProfile", "UserId");
            AddForeignKey("dbo.NhaCungCaps", "ModifiedBy_UserId", "dbo.UserProfile", "UserId");
            AddForeignKey("dbo.NhomNhaCungCaps", "CreatedBy_UserId", "dbo.UserProfile", "UserId");
            AddForeignKey("dbo.NhomNhaCungCaps", "ModifiedBy_UserId", "dbo.UserProfile", "UserId");
            AddForeignKey("dbo.Thuocs", "ModifiedBy_UserId", "dbo.UserProfile", "UserId");
            AddForeignKey("dbo.NhomThuocs", "CreatedBy_UserId", "dbo.UserProfile", "UserId");
            AddForeignKey("dbo.NhomThuocs", "ModifiedBy_UserId", "dbo.UserProfile", "UserId");
            AddForeignKey("dbo.NhaThuocs", "CreatedBy_UserId", "dbo.UserProfile", "UserId");
            AddForeignKey("dbo.NhaThuocs", "ModifiedBy_UserId", "dbo.UserProfile", "UserId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NhaThuocs", "ModifiedBy_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.NhaThuocs", "CreatedBy_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.NhomThuocs", "ModifiedBy_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.NhomThuocs", "CreatedBy_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.Thuocs", "ModifiedBy_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.NhomNhaCungCaps", "ModifiedBy_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.NhomNhaCungCaps", "CreatedBy_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.NhaCungCaps", "ModifiedBy_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.NhaCungCaps", "CreatedBy_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.KhachHangs", "ModifiedBy_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.KhachHangs", "CreatedBy_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.Thuocs", "CreatedBy_UserId", "dbo.UserProfile");
            DropIndex("dbo.NhomThuocs", new[] { "ModifiedBy_UserId" });
            DropIndex("dbo.NhomThuocs", new[] { "CreatedBy_UserId" });
            DropIndex("dbo.NhomNhaCungCaps", new[] { "ModifiedBy_UserId" });
            DropIndex("dbo.NhomNhaCungCaps", new[] { "CreatedBy_UserId" });
            DropIndex("dbo.NhaCungCaps", new[] { "ModifiedBy_UserId" });
            DropIndex("dbo.NhaCungCaps", new[] { "CreatedBy_UserId" });
            DropIndex("dbo.KhachHangs", new[] { "ModifiedBy_UserId" });
            DropIndex("dbo.KhachHangs", new[] { "CreatedBy_UserId" });
            DropIndex("dbo.Thuocs", new[] { "ModifiedBy_UserId" });
            DropIndex("dbo.Thuocs", new[] { "CreatedBy_UserId" });
            DropIndex("dbo.NhaThuocs", new[] { "ModifiedBy_UserId" });
            DropIndex("dbo.NhaThuocs", new[] { "CreatedBy_UserId" });
            DropColumn("dbo.NhomThuocs", "ModifiedBy_UserId");
            DropColumn("dbo.NhomThuocs", "CreatedBy_UserId");
            DropColumn("dbo.NhomThuocs", "Modified");
            DropColumn("dbo.NhomThuocs", "Created");
            DropColumn("dbo.NhomNhaCungCaps", "ModifiedBy_UserId");
            DropColumn("dbo.NhomNhaCungCaps", "CreatedBy_UserId");
            DropColumn("dbo.NhomNhaCungCaps", "Modified");
            DropColumn("dbo.NhomNhaCungCaps", "Created");
            DropColumn("dbo.NhaCungCaps", "ModifiedBy_UserId");
            DropColumn("dbo.NhaCungCaps", "CreatedBy_UserId");
            DropColumn("dbo.NhaCungCaps", "Modified");
            DropColumn("dbo.NhaCungCaps", "Created");
            DropColumn("dbo.KhachHangs", "ModifiedBy_UserId");
            DropColumn("dbo.KhachHangs", "CreatedBy_UserId");
            DropColumn("dbo.KhachHangs", "Modified");
            DropColumn("dbo.KhachHangs", "Created");
            DropColumn("dbo.Thuocs", "ModifiedBy_UserId");
            DropColumn("dbo.Thuocs", "CreatedBy_UserId");
            DropColumn("dbo.Thuocs", "Modified");
            DropColumn("dbo.Thuocs", "Created");
            DropColumn("dbo.NhaThuocs", "ModifiedBy_UserId");
            DropColumn("dbo.NhaThuocs", "CreatedBy_UserId");
            DropColumn("dbo.NhaThuocs", "Modified");
            DropColumn("dbo.NhaThuocs", "Created");
        }
    }
}
