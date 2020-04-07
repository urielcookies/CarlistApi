namespace CarlistApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MakeNullableValuesCarStatus : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CarStatus", "PriceSold", c => c.Decimal(storeType: "money"));
            AlterColumn("dbo.CarStatus", "YearSold", c => c.Short());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CarStatus", "YearSold", c => c.Short(nullable: false));
            AlterColumn("dbo.CarStatus", "PriceSold", c => c.Decimal(nullable: false, storeType: "money"));
        }
    }
}
