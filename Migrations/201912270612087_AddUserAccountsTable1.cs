namespace CarlistApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserAccountsTable1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserAccounts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(),
                        Password = c.String(),
                        Email = c.String(),
                        CreatedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UserAccounts");
        }
    }
}
