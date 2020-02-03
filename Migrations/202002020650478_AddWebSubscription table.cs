namespace CarlistApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddWebSubscriptiontable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.WebSubscriptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserAccountId = c.Int(nullable: false),
                        Endpoint = c.String(),
                        P256dh = c.String(),
                        Auth = c.String(),
                        CreatedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.WebSubscriptions");
        }
    }
}
