namespace CarlistApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMoneyAnnotation : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CarExpenses", "Cost", c => c.Decimal(nullable: false, storeType: "money"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CarExpenses", "Cost", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
