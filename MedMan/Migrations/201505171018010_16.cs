namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _16 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.NhomKhachHangs", "Created", c => c.DateTime());
            AlterColumn("dbo.NhomKhachHangs", "Modified", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.NhomKhachHangs", "Modified", c => c.DateTime(nullable: false));
            AlterColumn("dbo.NhomKhachHangs", "Created", c => c.DateTime(nullable: false));
        }
    }
}
