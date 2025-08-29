namespace ProjetoFim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddQuartoServicoTrad : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ServicoTrad",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ServicoId = c.Int(nullable: false),
                        Cultura = c.String(nullable: false, maxLength: 5),
                        Nome = c.String(nullable: false, maxLength: 200),
                        Descricao = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Servico", t => t.ServicoId, cascadeDelete: true)
                .Index(t => t.ServicoId);
            
            CreateTable(
                "dbo.QuartoTrad",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuartoId = c.Int(nullable: false),
                        Cultura = c.String(nullable: false, maxLength: 5),
                        Nome = c.String(nullable: false, maxLength: 200),
                        Descricao = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quarto", t => t.QuartoId, cascadeDelete: true)
                .Index(t => t.QuartoId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuartoTrad", "QuartoId", "dbo.Quarto");
            DropForeignKey("dbo.ServicoTrad", "ServicoId", "dbo.Servico");
            DropIndex("dbo.QuartoTrad", new[] { "QuartoId" });
            DropIndex("dbo.ServicoTrad", new[] { "ServicoId" });
            DropTable("dbo.QuartoTrad");
            DropTable("dbo.ServicoTrad");
        }
    }
}
