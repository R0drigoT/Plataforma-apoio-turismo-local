namespace ProjetoFim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdicionarSistemaDeAvaliacoes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Avaliacao",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Classificacao = c.Int(nullable: false),
                        Comentario = c.String(),
                        DataAvaliacao = c.DateTime(nullable: false),
                        QuartoId = c.Int(nullable: false),
                        UtilizadorId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quarto", t => t.QuartoId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UtilizadorId, cascadeDelete: true)
                .Index(t => t.QuartoId)
                .Index(t => t.UtilizadorId);
            
            AddColumn("dbo.Quarto", "AvaliacaoMedia", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Avaliacao", "UtilizadorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Avaliacao", "QuartoId", "dbo.Quarto");
            DropIndex("dbo.Avaliacao", new[] { "UtilizadorId" });
            DropIndex("dbo.Avaliacao", new[] { "QuartoId" });
            DropColumn("dbo.Quarto", "AvaliacaoMedia");
            DropTable("dbo.Avaliacao");
        }
    }
}
