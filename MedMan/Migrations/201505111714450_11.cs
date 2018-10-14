namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _11 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.NhanVienNhaThuocs", "NhaThuoc_MaNhaThuoc", "dbo.NhaThuocs");
            DropIndex("dbo.NhanVienNhaThuocs", new[] { "NhaThuoc_MaNhaThuoc" });
            AlterColumn("dbo.NhanVienNhaThuocs", "NhaThuoc_MaNhaThuoc", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.NhanVienNhaThuocs", "NhaThuoc_MaNhaThuoc");
            AddForeignKey("dbo.NhanVienNhaThuocs", "NhaThuoc_MaNhaThuoc", "dbo.NhaThuocs", "MaNhaThuoc", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NhanVienNhaThuocs", "NhaThuoc_MaNhaThuoc", "dbo.NhaThuocs");
            DropIndex("dbo.NhanVienNhaThuocs", new[] { "NhaThuoc_MaNhaThuoc" });
            AlterColumn("dbo.NhanVienNhaThuocs", "NhaThuoc_MaNhaThuoc", c => c.String(maxLength: 128));
            CreateIndex("dbo.NhanVienNhaThuocs", "NhaThuoc_MaNhaThuoc");
            AddForeignKey("dbo.NhanVienNhaThuocs", "NhaThuoc_MaNhaThuoc", "dbo.NhaThuocs", "MaNhaThuoc");
        }
    }
}
