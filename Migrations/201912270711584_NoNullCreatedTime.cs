namespace CarlistApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NoNullCreatedTime : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CarAccesses", "CreatedTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.CarExpenses", "CreatedTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.CarInformations", "CreatedTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.CarStatus", "CreatedTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.UserAccounts", "CreatedTime", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.UserAccounts", "CreatedTime", c => c.DateTime());
            AlterColumn("dbo.CarStatus", "CreatedTime", c => c.DateTime());
            AlterColumn("dbo.CarInformations", "CreatedTime", c => c.DateTime());
            AlterColumn("dbo.CarExpenses", "CreatedTime", c => c.DateTime());
            AlterColumn("dbo.CarAccesses", "CreatedTime", c => c.DateTime());
        }
    }
}
