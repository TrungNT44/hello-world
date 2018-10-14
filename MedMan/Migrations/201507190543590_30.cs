namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _30 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PhieuThuChis", "NguoiNhan", c => c.String());
            AddColumn("dbo.PhieuThuChis", "DiaChi", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PhieuThuChis", "DiaChi");
            DropColumn("dbo.PhieuThuChis", "NguoiNhan");
        }
    }
}
