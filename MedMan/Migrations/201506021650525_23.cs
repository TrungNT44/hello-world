namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _23 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PhieuXuats", "MaLoaiXuatNhap", "dbo.LoaiXuatNhaps");
            DropIndex("dbo.PhieuXuats", new[] { "MaNhaThuoc" });
            DropIndex("dbo.PhieuXuats", new[] { "MaNguoiTao" });
            DropIndex("dbo.PhieuXuatChiTiets", new[] { "MaPhieuXuat" });
            DropIndex("dbo.PhieuXuatChiTiets", new[] { "MaNhaThuoc" });
            DropIndex("dbo.PhieuXuatChiTiets", new[] { "ThuocId" });
            RenameColumn(table: "dbo.PhieuXuats", name: "MaBacSy", newName: "BacSy_MaBacSy");
            RenameColumn(table: "dbo.PhieuXuats", name: "MaNguoiTao", newName: "UserProfile_UserId");
            RenameColumn(table: "dbo.PhieuXuatChiTiets", name: "MaNhaThuoc", newName: "NhaThuoc_MaNhaThuoc");
            RenameColumn(table: "dbo.PhieuXuats", name: "MaNhaThuoc", newName: "NhaThuoc_MaNhaThuoc");
            RenameColumn(table: "dbo.PhieuXuatChiTiets", name: "ThuocId", newName: "Thuoc_ThuocId");
            RenameColumn(table: "dbo.PhieuXuatChiTiets", name: "MaDonViTinh", newName: "DonViTinh_MaDonViTinh");
            RenameColumn(table: "dbo.PhieuXuats", name: "MaKhachHang", newName: "KhachHang_MaKhachHang");
            RenameColumn(table: "dbo.PhieuXuats", name: "MaNhaCungCap", newName: "NhaCungCap_MaNhaCungCap");
            RenameColumn(table: "dbo.PhieuXuatChiTiets", name: "MaPhieuXuat", newName: "PhieuXuat_MaPhieuXuat");
            RenameIndex(table: "dbo.PhieuXuats", name: "IX_MaBacSy", newName: "IX_BacSy_MaBacSy");
            RenameIndex(table: "dbo.PhieuXuats", name: "IX_MaKhachHang", newName: "IX_KhachHang_MaKhachHang");
            RenameIndex(table: "dbo.PhieuXuats", name: "IX_MaNhaCungCap", newName: "IX_NhaCungCap_MaNhaCungCap");
            RenameIndex(table: "dbo.PhieuXuatChiTiets", name: "IX_MaDonViTinh", newName: "IX_DonViTinh_MaDonViTinh");
            AddColumn("dbo.PhieuXuats", "Created", c => c.DateTime());
            AddColumn("dbo.PhieuXuats", "Modified", c => c.DateTime());
            AddColumn("dbo.PhieuXuats", "CreatedBy_UserId", c => c.Int());
            AddColumn("dbo.PhieuXuats", "ModifiedBy_UserId", c => c.Int());
            AlterColumn("dbo.PhieuXuats", "NhaThuoc_MaNhaThuoc", c => c.String(maxLength: 128));
            AlterColumn("dbo.PhieuXuats", "UserProfile_UserId", c => c.Int());
            AlterColumn("dbo.PhieuXuatChiTiets", "PhieuXuat_MaPhieuXuat", c => c.Int());
            AlterColumn("dbo.PhieuXuatChiTiets", "NhaThuoc_MaNhaThuoc", c => c.String(maxLength: 128));
            AlterColumn("dbo.PhieuXuatChiTiets", "Thuoc_ThuocId", c => c.Int());
            CreateIndex("dbo.PhieuXuats", "CreatedBy_UserId");
            CreateIndex("dbo.PhieuXuats", "ModifiedBy_UserId");
            CreateIndex("dbo.PhieuXuats", "NhaThuoc_MaNhaThuoc");
            CreateIndex("dbo.PhieuXuats", "UserProfile_UserId");
            CreateIndex("dbo.PhieuXuatChiTiets", "NhaThuoc_MaNhaThuoc");
            CreateIndex("dbo.PhieuXuatChiTiets", "PhieuXuat_MaPhieuXuat");
            CreateIndex("dbo.PhieuXuatChiTiets", "Thuoc_ThuocId");
            AddForeignKey("dbo.PhieuXuats", "CreatedBy_UserId", "dbo.UserProfile", "UserId");
            AddForeignKey("dbo.PhieuXuats", "ModifiedBy_UserId", "dbo.UserProfile", "UserId");
            AddForeignKey("dbo.PhieuXuats", "MaLoaiXuatNhap", "dbo.LoaiXuatNhaps", "MaLoaiXuatNhap", cascadeDelete: true);
            DropColumn("dbo.PhieuXuats", "NgayTao");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PhieuXuats", "NgayTao", c => c.DateTime(nullable: false));
            DropForeignKey("dbo.PhieuXuats", "MaLoaiXuatNhap", "dbo.LoaiXuatNhaps");
            DropForeignKey("dbo.PhieuXuats", "ModifiedBy_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.PhieuXuats", "CreatedBy_UserId", "dbo.UserProfile");
            DropIndex("dbo.PhieuXuatChiTiets", new[] { "Thuoc_ThuocId" });
            DropIndex("dbo.PhieuXuatChiTiets", new[] { "PhieuXuat_MaPhieuXuat" });
            DropIndex("dbo.PhieuXuatChiTiets", new[] { "NhaThuoc_MaNhaThuoc" });
            DropIndex("dbo.PhieuXuats", new[] { "UserProfile_UserId" });
            DropIndex("dbo.PhieuXuats", new[] { "NhaThuoc_MaNhaThuoc" });
            DropIndex("dbo.PhieuXuats", new[] { "ModifiedBy_UserId" });
            DropIndex("dbo.PhieuXuats", new[] { "CreatedBy_UserId" });
            AlterColumn("dbo.PhieuXuatChiTiets", "Thuoc_ThuocId", c => c.Int(nullable: false));
            AlterColumn("dbo.PhieuXuatChiTiets", "NhaThuoc_MaNhaThuoc", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.PhieuXuatChiTiets", "PhieuXuat_MaPhieuXuat", c => c.Int(nullable: false));
            AlterColumn("dbo.PhieuXuats", "UserProfile_UserId", c => c.Int(nullable: false));
            AlterColumn("dbo.PhieuXuats", "NhaThuoc_MaNhaThuoc", c => c.String(nullable: false, maxLength: 128));
            DropColumn("dbo.PhieuXuats", "ModifiedBy_UserId");
            DropColumn("dbo.PhieuXuats", "CreatedBy_UserId");
            DropColumn("dbo.PhieuXuats", "Modified");
            DropColumn("dbo.PhieuXuats", "Created");
            RenameIndex(table: "dbo.PhieuXuatChiTiets", name: "IX_DonViTinh_MaDonViTinh", newName: "IX_MaDonViTinh");
            RenameIndex(table: "dbo.PhieuXuats", name: "IX_NhaCungCap_MaNhaCungCap", newName: "IX_MaNhaCungCap");
            RenameIndex(table: "dbo.PhieuXuats", name: "IX_KhachHang_MaKhachHang", newName: "IX_MaKhachHang");
            RenameIndex(table: "dbo.PhieuXuats", name: "IX_BacSy_MaBacSy", newName: "IX_MaBacSy");
            RenameColumn(table: "dbo.PhieuXuatChiTiets", name: "PhieuXuat_MaPhieuXuat", newName: "MaPhieuXuat");
            RenameColumn(table: "dbo.PhieuXuats", name: "NhaCungCap_MaNhaCungCap", newName: "MaNhaCungCap");
            RenameColumn(table: "dbo.PhieuXuats", name: "KhachHang_MaKhachHang", newName: "MaKhachHang");
            RenameColumn(table: "dbo.PhieuXuatChiTiets", name: "DonViTinh_MaDonViTinh", newName: "MaDonViTinh");
            RenameColumn(table: "dbo.PhieuXuatChiTiets", name: "Thuoc_ThuocId", newName: "ThuocId");
            RenameColumn(table: "dbo.PhieuXuats", name: "NhaThuoc_MaNhaThuoc", newName: "MaNhaThuoc");
            RenameColumn(table: "dbo.PhieuXuatChiTiets", name: "NhaThuoc_MaNhaThuoc", newName: "MaNhaThuoc");
            RenameColumn(table: "dbo.PhieuXuats", name: "UserProfile_UserId", newName: "MaNguoiTao");
            RenameColumn(table: "dbo.PhieuXuats", name: "BacSy_MaBacSy", newName: "MaBacSy");
            CreateIndex("dbo.PhieuXuatChiTiets", "ThuocId");
            CreateIndex("dbo.PhieuXuatChiTiets", "MaNhaThuoc");
            CreateIndex("dbo.PhieuXuatChiTiets", "MaPhieuXuat");
            CreateIndex("dbo.PhieuXuats", "MaNguoiTao");
            CreateIndex("dbo.PhieuXuats", "MaNhaThuoc");
            AddForeignKey("dbo.PhieuXuats", "MaLoaiXuatNhap", "dbo.LoaiXuatNhaps", "MaLoaiXuatNhap");
        }
    }
}
