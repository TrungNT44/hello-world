namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _31 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.PhieuKiemKes", name: "NguoiTao_UserId", newName: "UserProfile_UserId");
            RenameColumn(table: "dbo.PhieuKiemKes", name: "MaNhaThuoc", newName: "NhaThuoc_MaNhaThuoc");
            RenameColumn(table: "dbo.PhieuKiemKeChiTiets", name: "MaPhieuKiemKe", newName: "PhieuKiemKe_MaPhieuKiemKe");
            RenameColumn(table: "dbo.PhieuKiemKes", name: "MaPhieuNhap", newName: "PhieuNhap_MaPhieuNhap");
            RenameColumn(table: "dbo.PhieuKiemKes", name: "MaPhieuXuat", newName: "PhieuXuat_MaPhieuXuat");
            RenameIndex(table: "dbo.PhieuKiemKeChiTiets", name: "IX_MaPhieuKiemKe", newName: "IX_PhieuKiemKe_MaPhieuKiemKe");
            RenameIndex(table: "dbo.PhieuKiemKes", name: "IX_MaPhieuNhap", newName: "IX_PhieuNhap_MaPhieuNhap");
            RenameIndex(table: "dbo.PhieuKiemKes", name: "IX_MaPhieuXuat", newName: "IX_PhieuXuat_MaPhieuXuat");
            RenameIndex(table: "dbo.PhieuKiemKes", name: "IX_MaNhaThuoc", newName: "IX_NhaThuoc_MaNhaThuoc");
            RenameIndex(table: "dbo.PhieuKiemKes", name: "IX_NguoiTao_UserId", newName: "IX_UserProfile_UserId");
            AddColumn("dbo.PhieuKiemKes", "Created", c => c.DateTime());
            AddColumn("dbo.PhieuKiemKes", "Modified", c => c.DateTime());
            AddColumn("dbo.PhieuKiemKes", "CreatedBy_UserId", c => c.Int());
            AddColumn("dbo.PhieuKiemKes", "ModifiedBy_UserId", c => c.Int());
            CreateIndex("dbo.PhieuKiemKes", "CreatedBy_UserId");
            CreateIndex("dbo.PhieuKiemKes", "ModifiedBy_UserId");
            AddForeignKey("dbo.PhieuKiemKes", "CreatedBy_UserId", "dbo.UserProfile", "UserId");
            AddForeignKey("dbo.PhieuKiemKes", "ModifiedBy_UserId", "dbo.UserProfile", "UserId");
            DropColumn("dbo.PhieuKiemKes", "NgayTao");
            DropColumn("dbo.PhieuKiemKes", "MaNguoiTao");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PhieuKiemKes", "MaNguoiTao", c => c.Int(nullable: false));
            AddColumn("dbo.PhieuKiemKes", "NgayTao", c => c.DateTime(nullable: false));
            DropForeignKey("dbo.PhieuKiemKes", "ModifiedBy_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.PhieuKiemKes", "CreatedBy_UserId", "dbo.UserProfile");
            DropIndex("dbo.PhieuKiemKes", new[] { "ModifiedBy_UserId" });
            DropIndex("dbo.PhieuKiemKes", new[] { "CreatedBy_UserId" });
            DropColumn("dbo.PhieuKiemKes", "ModifiedBy_UserId");
            DropColumn("dbo.PhieuKiemKes", "CreatedBy_UserId");
            DropColumn("dbo.PhieuKiemKes", "Modified");
            DropColumn("dbo.PhieuKiemKes", "Created");
            RenameIndex(table: "dbo.PhieuKiemKes", name: "IX_UserProfile_UserId", newName: "IX_NguoiTao_UserId");
            RenameIndex(table: "dbo.PhieuKiemKes", name: "IX_NhaThuoc_MaNhaThuoc", newName: "IX_MaNhaThuoc");
            RenameIndex(table: "dbo.PhieuKiemKes", name: "IX_PhieuXuat_MaPhieuXuat", newName: "IX_MaPhieuXuat");
            RenameIndex(table: "dbo.PhieuKiemKes", name: "IX_PhieuNhap_MaPhieuNhap", newName: "IX_MaPhieuNhap");
            RenameIndex(table: "dbo.PhieuKiemKeChiTiets", name: "IX_PhieuKiemKe_MaPhieuKiemKe", newName: "IX_MaPhieuKiemKe");
            RenameColumn(table: "dbo.PhieuKiemKes", name: "PhieuXuat_MaPhieuXuat", newName: "MaPhieuXuat");
            RenameColumn(table: "dbo.PhieuKiemKes", name: "PhieuNhap_MaPhieuNhap", newName: "MaPhieuNhap");
            RenameColumn(table: "dbo.PhieuKiemKeChiTiets", name: "PhieuKiemKe_MaPhieuKiemKe", newName: "MaPhieuKiemKe");
            RenameColumn(table: "dbo.PhieuKiemKes", name: "NhaThuoc_MaNhaThuoc", newName: "MaNhaThuoc");
            RenameColumn(table: "dbo.PhieuKiemKes", name: "UserProfile_UserId", newName: "NguoiTao_UserId");
        }
    }
}
