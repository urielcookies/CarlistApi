namespace CarlistApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeDatatypeDoubleToDecimalCarStatus : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CarStatus", "PriceSold", c => c.Decimal(nullable: false, storeType: "money"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CarStatus", "PriceSold", c => c.Double(nullable: false));
        }
    }
}
