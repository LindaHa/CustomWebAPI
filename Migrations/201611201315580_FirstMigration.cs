namespace CustomWebAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FirstMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Tokens",
                c => new
                    {
                        Code = c.String(nullable: false, maxLength: 128),
                        UserID = c.Int(nullable: false),
                        Expiration = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Code);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Tokens");
        }
    }
}
