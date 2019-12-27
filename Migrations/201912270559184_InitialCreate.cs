namespace CarlistApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CarAccesses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserAccountId = c.Int(nullable: false),
                        CarInformationId = c.Int(nullable: false),
                        Write = c.Boolean(),
                        CreatedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CarExpenses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserAccountId = c.Int(nullable: false),
                        CarInformationId = c.Int(nullable: false),
                        Expense = c.String(),
                        Cost = c.Double(nullable: false),
                        CreatedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CarInformations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Year = c.Short(),
                        Brand = c.String(),
                        Model = c.String(),
                        Cost = c.Double(),
                        CleanTitle = c.Boolean(),
                        Notes = c.String(),
                        UserAccountId = c.Int(nullable: false),
                        Partner = c.String(),
                        CreatedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CarStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserAccountId = c.Int(nullable: false),
                        CarInformationId = c.Int(nullable: false),
                        Sold = c.Boolean(nullable: false),
                        PriceSold = c.Double(nullable: false),
                        YearSold = c.Short(nullable: false),
                        CreatedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CarStatus");
            DropTable("dbo.CarInformations");
            DropTable("dbo.CarExpenses");
            DropTable("dbo.CarAccesses");
        }
    }
}
