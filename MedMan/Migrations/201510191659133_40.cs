namespace sThuoc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _40 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.NhaThuocs", "SoKinhDoanh", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.NhaThuocs", "SoKinhDoanh", c => c.Int());
        }
    }
}
