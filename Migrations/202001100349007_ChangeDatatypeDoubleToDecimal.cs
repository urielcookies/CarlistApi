namespace CarlistApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeDatatypeDoubleToDecimal : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CarInformations", "Cost", c => c.Decimal(storeType: "money"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CarInformations", "Cost", c => c.Double());
        }
    }
}
