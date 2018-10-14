namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _13 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.NhomKhachHangs", name: "MaNhaThuoc", newName: "NhaThuoc_MaNhaThuoc");
            RenameIndex(table: "dbo.NhomKhachHangs", name: "IX_MaNhaThuoc", newName: "IX_NhaThuoc_MaNhaThuoc");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.NhomKhachHangs", name: "IX_NhaThuoc_MaNhaThuoc", newName: "IX_MaNhaThuoc");
            RenameColumn(table: "dbo.NhomKhachHangs", name: "NhaThuoc_MaNhaThuoc", newName: "MaNhaThuoc");
        }
    }
}
