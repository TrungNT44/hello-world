namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _21 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PhieuNhaps", "MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.PhieuNhapChiTiets", "MaDonViTinh", "dbo.DonViTinhs");
            DropIndex("dbo.PhieuNhapChiTiets", new[] { "MaPhieuNhap" });
            DropIndex("dbo.PhieuNhapChiTiets", new[] { "MaNhaThuoc" });
            DropIndex("dbo.PhieuNhapChiTiets", new[] { "ThuocId" });
            DropIndex("dbo.PhieuNhapChiTiets", new[] { "MaDonViTinh" });
            DropIndex("dbo.PhieuNhaps", new[] { "MaLoaiXuatNhap" });
            DropIndex("dbo.PhieuNhaps", new[] { "MaNguoiTao" });
            RenameColumn(table: "dbo.PhieuNhaps", name: "MaNguoiTao", newName: "UserProfile_UserId");
            RenameColumn(table: "dbo.PhieuNhapChiTiets", name: "MaNhaThuoc", newName: "NhaThuoc_MaNhaThuoc");
            RenameColumn(table: "dbo.PhieuNhaps", name: "MaNhaThuoc", newName: "NhaThuoc_MaNhaThuoc");
            RenameColumn(table: "dbo.PhieuNhapChiTiets", name: "ThuocId", newName: "Thuoc_ThuocId");
            RenameColumn(table: "dbo.PhieuNhapChiTiets", name: "MaDonViTinh", newName: "DonViTinh_MaDonViTinh");
            RenameColumn(table: "dbo.PhieuNhapChiTiets", name: "MaPhieuNhap", newName: "PhieuNhap_MaPhieuNhap");
            RenameColumn(table: "dbo.PhieuNhaps", name: "MaKhachHang", newName: "KhachHang_MaKhachHang");
            RenameColumn(table: "dbo.PhieuNhaps", name: "MaLoaiXuatNhap", newName: "LoaiXuatNhap_MaLoaiXuatNhap");
            RenameColumn(table: "dbo.PhieuNhaps", name: "MaNhaCungCap", newName: "NhaCungCap_MaNhaCungCap");
            RenameIndex(table: "dbo.PhieuNhaps", name: "IX_MaKhachHang", newName: "IX_KhachHang_MaKhachHang");
            RenameIndex(table: "dbo.PhieuNhaps", name: "IX_MaNhaCungCap", newName: "IX_NhaCungCap_MaNhaCungCap");
            RenameIndex(table: "dbo.PhieuNhaps", name: "IX_MaNhaThuoc", newName: "IX_NhaThuoc_MaNhaThuoc");
            AddColumn("dbo.PhieuNhaps", "Created", c => c.DateTime());
            AddColumn("dbo.PhieuNhaps", "Modified", c => c.DateTime());
            AddColumn("dbo.PhieuNhaps", "CreatedBy_UserId", c => c.Int());
            AddColumn("dbo.PhieuNhaps", "ModifiedBy_UserId", c => c.Int());
            AlterColumn("dbo.PhieuNhapChiTiets", "PhieuNhap_MaPhieuNhap", c => c.Int());
            AlterColumn("dbo.PhieuNhapChiTiets", "NhaThuoc_MaNhaThuoc", c => c.String(maxLength: 128));
            AlterColumn("dbo.PhieuNhapChiTiets", "Thuoc_ThuocId", c => c.Int());
            AlterColumn("dbo.PhieuNhapChiTiets", "DonViTinh_MaDonViTinh", c => c.Int());
            AlterColumn("dbo.PhieuNhapChiTiets", "ChietKhau", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PhieuNhapChiTiets", "GiaNhap", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PhieuNhaps", "LoaiXuatNhap_MaLoaiXuatNhap", c => c.Int());
            AlterColumn("dbo.PhieuNhaps", "UserProfile_UserId", c => c.Int());
            CreateIndex("dbo.PhieuNhapChiTiets", "DonViTinh_MaDonViTinh");
            CreateIndex("dbo.PhieuNhapChiTiets", "NhaThuoc_MaNhaThuoc");
            CreateIndex("dbo.PhieuNhapChiTiets", "PhieuNhap_MaPhieuNhap");
            CreateIndex("dbo.PhieuNhapChiTiets", "Thuoc_ThuocId");
            CreateIndex("dbo.PhieuNhaps", "CreatedBy_UserId");
            CreateIndex("dbo.PhieuNhaps", "LoaiXuatNhap_MaLoaiXuatNhap");
            CreateIndex("dbo.PhieuNhaps", "ModifiedBy_UserId");
            CreateIndex("dbo.PhieuNhaps", "UserProfile_UserId");
            AddForeignKey("dbo.PhieuNhaps", "CreatedBy_UserId", "dbo.UserProfile", "UserId");
            AddForeignKey("dbo.PhieuNhaps", "ModifiedBy_UserId", "dbo.UserProfile", "UserId");
            AddForeignKey("dbo.PhieuNhaps", "NhaThuoc_MaNhaThuoc", "dbo.NhaThuocs", "MaNhaThuoc", cascadeDelete: true);
            AddForeignKey("dbo.PhieuNhapChiTiets", "DonViTinh_MaDonViTinh", "dbo.DonViTinhs", "MaDonViTinh");
            DropColumn("dbo.PhieuNhaps", "NgayTao");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PhieuNhaps", "NgayTao", c => c.DateTime(nullable: false));
            DropForeignKey("dbo.PhieuNhapChiTiets", "DonViTinh_MaDonViTinh", "dbo.DonViTinhs");
            DropForeignKey("dbo.PhieuNhaps", "NhaThuoc_MaNhaThuoc", "dbo.NhaThuocs");
            DropForeignKey("dbo.PhieuNhaps", "ModifiedBy_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.PhieuNhaps", "CreatedBy_UserId", "dbo.UserProfile");
            DropIndex("dbo.PhieuNhaps", new[] { "UserProfile_UserId" });
            DropIndex("dbo.PhieuNhaps", new[] { "ModifiedBy_UserId" });
            DropIndex("dbo.PhieuNhaps", new[] { "LoaiXuatNhap_MaLoaiXuatNhap" });
            DropIndex("dbo.PhieuNhaps", new[] { "CreatedBy_UserId" });
            DropIndex("dbo.PhieuNhapChiTiets", new[] { "Thuoc_ThuocId" });
            DropIndex("dbo.PhieuNhapChiTiets", new[] { "PhieuNhap_MaPhieuNhap" });
            DropIndex("dbo.PhieuNhapChiTiets", new[] { "NhaThuoc_MaNhaThuoc" });
            DropIndex("dbo.PhieuNhapChiTiets", new[] { "DonViTinh_MaDonViTinh" });
            AlterColumn("dbo.PhieuNhaps", "UserProfile_UserId", c => c.Int(nullable: false));
            AlterColumn("dbo.PhieuNhaps", "LoaiXuatNhap_MaLoaiXuatNhap", c => c.Int(nullable: false));
            AlterColumn("dbo.PhieuNhapChiTiets", "GiaNhap", c => c.Int(nullable: false));
            AlterColumn("dbo.PhieuNhapChiTiets", "ChietKhau", c => c.Int(nullable: false));
            AlterColumn("dbo.PhieuNhapChiTiets", "DonViTinh_MaDonViTinh", c => c.Int(nullable: false));
            AlterColumn("dbo.PhieuNhapChiTiets", "Thuoc_ThuocId", c => c.Int(nullable: false));
            AlterColumn("dbo.PhieuNhapChiTiets", "NhaThuoc_MaNhaThuoc", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.PhieuNhapChiTiets", "PhieuNhap_MaPhieuNhap", c => c.Int(nullable: false));
            DropColumn("dbo.PhieuNhaps", "ModifiedBy_UserId");
            DropColumn("dbo.PhieuNhaps", "CreatedBy_UserId");
            DropColumn("dbo.PhieuNhaps", "Modified");
            DropColumn("dbo.PhieuNhaps", "Created");
            RenameIndex(table: "dbo.PhieuNhaps", name: "IX_NhaThuoc_MaNhaThuoc", newName: "IX_MaNhaThuoc");
            RenameIndex(table: "dbo.PhieuNhaps", name: "IX_NhaCungCap_MaNhaCungCap", newName: "IX_MaNhaCungCap");
            RenameIndex(table: "dbo.PhieuNhaps", name: "IX_KhachHang_MaKhachHang", newName: "IX_MaKhachHang");
            RenameColumn(table: "dbo.PhieuNhaps", name: "NhaCungCap_MaNhaCungCap", newName: "MaNhaCungCap");
            RenameColumn(table: "dbo.PhieuNhaps", name: "LoaiXuatNhap_MaLoaiXuatNhap", newName: "MaLoaiXuatNhap");
            RenameColumn(table: "dbo.PhieuNhaps", name: "KhachHang_MaKhachHang", newName: "MaKhachHang");
            RenameColumn(table: "dbo.PhieuNhapChiTiets", name: "PhieuNhap_MaPhieuNhap", newName: "MaPhieuNhap");
            RenameColumn(table: "dbo.PhieuNhapChiTiets", name: "DonViTinh_MaDonViTinh", newName: "MaDonViTinh");
            RenameColumn(table: "dbo.PhieuNhapChiTiets", name: "Thuoc_ThuocId", newName: "ThuocId");
            RenameColumn(table: "dbo.PhieuNhaps", name: "NhaThuoc_MaNhaThuoc", newName: "MaNhaThuoc");
            RenameColumn(table: "dbo.PhieuNhapChiTiets", name: "NhaThuoc_MaNhaThuoc", newName: "MaNhaThuoc");
            RenameColumn(table: "dbo.PhieuNhaps", name: "UserProfile_UserId", newName: "MaNguoiTao");
            CreateIndex("dbo.PhieuNhaps", "MaNguoiTao");
            CreateIndex("dbo.PhieuNhaps", "MaLoaiXuatNhap");
            CreateIndex("dbo.PhieuNhapChiTiets", "MaDonViTinh");
            CreateIndex("dbo.PhieuNhapChiTiets", "ThuocId");
            CreateIndex("dbo.PhieuNhapChiTiets", "MaNhaThuoc");
            CreateIndex("dbo.PhieuNhapChiTiets", "MaPhieuNhap");
            AddForeignKey("dbo.PhieuNhapChiTiets", "MaDonViTinh", "dbo.DonViTinhs", "MaDonViTinh", cascadeDelete: true);
            AddForeignKey("dbo.PhieuNhaps", "MaNhaThuoc", "dbo.NhaThuocs", "MaNhaThuoc");
        }
    }
}
