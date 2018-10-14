namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Thuocs", "MaNhomThuoc", "dbo.NhomThuocs");
            DropIndex("dbo.Thuocs", new[] { "MaNhomThuoc" });
            DropIndex("dbo.Thuocs", new[] { "MaDonViXuat" });
            DropIndex("dbo.Thuocs", new[] { "MaDonViThuNguyen" });
            RenameColumn(table: "dbo.Thuocs", name: "MaNhaThuoc", newName: "NhaThuoc_MaNhaThuoc");
            RenameColumn(table: "dbo.Thuocs", name: "MaDangBaoChe", newName: "DangBaoChe_MaDangBaoChe");
            RenameColumn(table: "dbo.Thuocs", name: "MaDonViThuNguyen", newName: "DonViThuNguyen_MaDonViTinh");
            RenameColumn(table: "dbo.Thuocs", name: "MaDonViXuat", newName: "DonViXuatLe_MaDonViTinh");
            RenameColumn(table: "dbo.Thuocs", name: "MaNhomThuoc", newName: "NhomThuoc_MaNhomThuoc");
            RenameColumn(table: "dbo.Thuocs", name: "MaNuoc", newName: "Nuoc_MaNuoc");
            RenameIndex(table: "dbo.Thuocs", name: "IX_MaDangBaoChe", newName: "IX_DangBaoChe_MaDangBaoChe");
            RenameIndex(table: "dbo.Thuocs", name: "IX_MaNhaThuoc", newName: "IX_NhaThuoc_MaNhaThuoc");
            RenameIndex(table: "dbo.Thuocs", name: "IX_MaNuoc", newName: "IX_Nuoc_MaNuoc");
            AlterColumn("dbo.Thuocs", "GiaNhap", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Thuocs", "GiaBanBuon", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Thuocs", "GiaBanLe", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Thuocs", "SoDuDauKy", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Thuocs", "GiaDauKy", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Thuocs", "NhomThuoc_MaNhomThuoc", c => c.Int());
            AlterColumn("dbo.Thuocs", "DonViXuatLe_MaDonViTinh", c => c.Int());
            AlterColumn("dbo.Thuocs", "DonViThuNguyen_MaDonViTinh", c => c.Int());
            AlterColumn("dbo.PhieuNhapChiTiets", "SoLuong", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PhieuNhaps", "TongTien", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PhieuXuats", "TongTien", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PhieuXuatChiTiets", "SoLuong", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            CreateIndex("dbo.Thuocs", "DonViThuNguyen_MaDonViTinh");
            CreateIndex("dbo.Thuocs", "DonViXuatLe_MaDonViTinh");
            CreateIndex("dbo.Thuocs", "NhomThuoc_MaNhomThuoc");
            AddForeignKey("dbo.Thuocs", "NhomThuoc_MaNhomThuoc", "dbo.NhomThuocs", "MaNhomThuoc");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Thuocs", "NhomThuoc_MaNhomThuoc", "dbo.NhomThuocs");
            DropIndex("dbo.Thuocs", new[] { "NhomThuoc_MaNhomThuoc" });
            DropIndex("dbo.Thuocs", new[] { "DonViXuatLe_MaDonViTinh" });
            DropIndex("dbo.Thuocs", new[] { "DonViThuNguyen_MaDonViTinh" });
            AlterColumn("dbo.PhieuXuatChiTiets", "SoLuong", c => c.Single(nullable: false));
            AlterColumn("dbo.PhieuXuats", "TongTien", c => c.Single(nullable: false));
            AlterColumn("dbo.PhieuNhaps", "TongTien", c => c.Single(nullable: false));
            AlterColumn("dbo.PhieuNhapChiTiets", "SoLuong", c => c.Single(nullable: false));
            AlterColumn("dbo.Thuocs", "DonViThuNguyen_MaDonViTinh", c => c.Int(nullable: false));
            AlterColumn("dbo.Thuocs", "DonViXuatLe_MaDonViTinh", c => c.Int(nullable: false));
            AlterColumn("dbo.Thuocs", "NhomThuoc_MaNhomThuoc", c => c.Int(nullable: false));
            AlterColumn("dbo.Thuocs", "GiaDauKy", c => c.Int());
            AlterColumn("dbo.Thuocs", "SoDuDauKy", c => c.Int());
            AlterColumn("dbo.Thuocs", "GiaBanLe", c => c.Int(nullable: false));
            AlterColumn("dbo.Thuocs", "GiaBanBuon", c => c.Int(nullable: false));
            AlterColumn("dbo.Thuocs", "GiaNhap", c => c.Int(nullable: false));
            RenameIndex(table: "dbo.Thuocs", name: "IX_Nuoc_MaNuoc", newName: "IX_MaNuoc");
            RenameIndex(table: "dbo.Thuocs", name: "IX_NhaThuoc_MaNhaThuoc", newName: "IX_MaNhaThuoc");
            RenameIndex(table: "dbo.Thuocs", name: "IX_DangBaoChe_MaDangBaoChe", newName: "IX_MaDangBaoChe");
            RenameColumn(table: "dbo.Thuocs", name: "Nuoc_MaNuoc", newName: "MaNuoc");
            RenameColumn(table: "dbo.Thuocs", name: "NhomThuoc_MaNhomThuoc", newName: "MaNhomThuoc");
            RenameColumn(table: "dbo.Thuocs", name: "DonViXuatLe_MaDonViTinh", newName: "MaDonViXuat");
            RenameColumn(table: "dbo.Thuocs", name: "DonViThuNguyen_MaDonViTinh", newName: "MaDonViThuNguyen");
            RenameColumn(table: "dbo.Thuocs", name: "DangBaoChe_MaDangBaoChe", newName: "MaDangBaoChe");
            RenameColumn(table: "dbo.Thuocs", name: "NhaThuoc_MaNhaThuoc", newName: "MaNhaThuoc");
            CreateIndex("dbo.Thuocs", "MaDonViThuNguyen");
            CreateIndex("dbo.Thuocs", "MaDonViXuat");
            CreateIndex("dbo.Thuocs", "MaNhomThuoc");
            AddForeignKey("dbo.Thuocs", "MaNhomThuoc", "dbo.NhomThuocs", "MaNhomThuoc", cascadeDelete: true);
        }
    }
}
