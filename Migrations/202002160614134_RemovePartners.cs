namespace CarlistApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovePartners : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.CarInformations", "Partner");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CarInformations", "Partner", c => c.String());
        }
    }
}
