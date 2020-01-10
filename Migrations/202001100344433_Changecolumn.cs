namespace CarlistApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Changecolumn : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CarExpenses", "Cost", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CarExpenses", "Cost", c => c.Double(nullable: false));
        }
    }
}
