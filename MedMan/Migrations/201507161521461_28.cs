namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _28 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Nuocs", "MaNhaThuoc", "dbo.NhaThuocs");
            DropIndex("dbo.Nuocs", new[] { "MaNhaThuoc" });
            RenameColumn(table: "dbo.Nuocs", name: "MaNhaThuoc", newName: "NhaThuoc_MaNhaThuoc");
            AddColumn("dbo.Thuocs", "BarCode", c => c.String());
            AddColumn("dbo.Thuocs", "HoatDong", c => c.Boolean(nullable: false));
            AddColumn("dbo.Nuocs", "Code", c => c.String(nullable: false));
            AlterColumn("dbo.Nuocs", "NhaThuoc_MaNhaThuoc", c => c.String(maxLength: 128));
            CreateIndex("dbo.Nuocs", "NhaThuoc_MaNhaThuoc");
            AddForeignKey("dbo.Nuocs", "NhaThuoc_MaNhaThuoc", "dbo.NhaThuocs", "MaNhaThuoc");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Nuocs", "NhaThuoc_MaNhaThuoc", "dbo.NhaThuocs");
            DropIndex("dbo.Nuocs", new[] { "NhaThuoc_MaNhaThuoc" });
            AlterColumn("dbo.Nuocs", "NhaThuoc_MaNhaThuoc", c => c.String(nullable: false, maxLength: 128));
            DropColumn("dbo.Nuocs", "Code");
            DropColumn("dbo.Thuocs", "HoatDong");
            DropColumn("dbo.Thuocs", "BarCode");
            RenameColumn(table: "dbo.Nuocs", name: "NhaThuoc_MaNhaThuoc", newName: "MaNhaThuoc");
            CreateIndex("dbo.Nuocs", "MaNhaThuoc");
            AddForeignKey("dbo.Nuocs", "MaNhaThuoc", "dbo.NhaThuocs", "MaNhaThuoc", cascadeDelete: true);
        }
    }
}
