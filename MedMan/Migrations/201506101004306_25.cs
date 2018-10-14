namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _25 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.PhieuThuChis", new[] { "MaNhaThuoc" });
            DropIndex("dbo.PhieuThuChis", new[] { "MaNguoiTao" });
            RenameColumn(table: "dbo.PhieuThuChis", name: "MaNguoiTao", newName: "UserProfile_UserId");
            RenameColumn(table: "dbo.PhieuThuChis", name: "MaNhaThuoc", newName: "NhaThuoc_MaNhaThuoc");
            RenameColumn(table: "dbo.PhieuThuChis", name: "MaKhachHang", newName: "KhachHang_MaKhachHang");
            RenameColumn(table: "dbo.PhieuThuChis", name: "MaNhaCungCap", newName: "NhaCungCap_MaNhaCungCap");
            RenameIndex(table: "dbo.PhieuThuChis", name: "IX_MaKhachHang", newName: "IX_KhachHang_MaKhachHang");
            RenameIndex(table: "dbo.PhieuThuChis", name: "IX_MaNhaCungCap", newName: "IX_NhaCungCap_MaNhaCungCap");
            AddColumn("dbo.PhieuThuChis", "Amount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PhieuThuChis", "Created", c => c.DateTime());
            AddColumn("dbo.PhieuThuChis", "Modified", c => c.DateTime());
            AddColumn("dbo.PhieuThuChis", "CreatedBy_UserId", c => c.Int());
            AddColumn("dbo.PhieuThuChis", "ModifiedBy_UserId", c => c.Int());
            AlterColumn("dbo.PhieuThuChis", "NhaThuoc_MaNhaThuoc", c => c.String(maxLength: 128));
            AlterColumn("dbo.PhieuThuChis", "UserProfile_UserId", c => c.Int());
            CreateIndex("dbo.PhieuThuChis", "CreatedBy_UserId");
            CreateIndex("dbo.PhieuThuChis", "ModifiedBy_UserId");
            CreateIndex("dbo.PhieuThuChis", "NhaThuoc_MaNhaThuoc");
            CreateIndex("dbo.PhieuThuChis", "UserProfile_UserId");
            AddForeignKey("dbo.PhieuThuChis", "CreatedBy_UserId", "dbo.UserProfile", "UserId");
            AddForeignKey("dbo.PhieuThuChis", "ModifiedBy_UserId", "dbo.UserProfile", "UserId");
            DropColumn("dbo.PhieuThuChis", "SoLuong");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PhieuThuChis", "SoLuong", c => c.Single(nullable: false));
            DropForeignKey("dbo.PhieuThuChis", "ModifiedBy_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.PhieuThuChis", "CreatedBy_UserId", "dbo.UserProfile");
            DropIndex("dbo.PhieuThuChis", new[] { "UserProfile_UserId" });
            DropIndex("dbo.PhieuThuChis", new[] { "NhaThuoc_MaNhaThuoc" });
            DropIndex("dbo.PhieuThuChis", new[] { "ModifiedBy_UserId" });
            DropIndex("dbo.PhieuThuChis", new[] { "CreatedBy_UserId" });
            AlterColumn("dbo.PhieuThuChis", "UserProfile_UserId", c => c.Int(nullable: false));
            AlterColumn("dbo.PhieuThuChis", "NhaThuoc_MaNhaThuoc", c => c.String(nullable: false, maxLength: 128));
            DropColumn("dbo.PhieuThuChis", "ModifiedBy_UserId");
            DropColumn("dbo.PhieuThuChis", "CreatedBy_UserId");
            DropColumn("dbo.PhieuThuChis", "Modified");
            DropColumn("dbo.PhieuThuChis", "Created");
            DropColumn("dbo.PhieuThuChis", "Amount");
            RenameIndex(table: "dbo.PhieuThuChis", name: "IX_NhaCungCap_MaNhaCungCap", newName: "IX_MaNhaCungCap");
            RenameIndex(table: "dbo.PhieuThuChis", name: "IX_KhachHang_MaKhachHang", newName: "IX_MaKhachHang");
            RenameColumn(table: "dbo.PhieuThuChis", name: "NhaCungCap_MaNhaCungCap", newName: "MaNhaCungCap");
            RenameColumn(table: "dbo.PhieuThuChis", name: "KhachHang_MaKhachHang", newName: "MaKhachHang");
            RenameColumn(table: "dbo.PhieuThuChis", name: "NhaThuoc_MaNhaThuoc", newName: "MaNhaThuoc");
            RenameColumn(table: "dbo.PhieuThuChis", name: "UserProfile_UserId", newName: "MaNguoiTao");
            CreateIndex("dbo.PhieuThuChis", "MaNguoiTao");
            CreateIndex("dbo.PhieuThuChis", "MaNhaThuoc");
        }
    }
}
