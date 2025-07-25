namespace ProjetoFim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdicionarComodidadesEImagens : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Quartos", newName: "Quarto");
            CreateTable(
                "dbo.Comodidade",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nome = c.String(nullable: false),
                        IconeCss = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Imagem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Url = c.String(nullable: false),
                        QuartoId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quarto", t => t.QuartoId, cascadeDelete: true)
                .Index(t => t.QuartoId);
            
            CreateTable(
                "dbo.QuartoComodidade",
                c => new
                    {
                        QuartoId = c.Int(nullable: false),
                        ComodidadeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.QuartoId, t.ComodidadeId })
                .ForeignKey("dbo.Quarto", t => t.QuartoId, cascadeDelete: true)
                .ForeignKey("dbo.Comodidade", t => t.ComodidadeId, cascadeDelete: true)
                .Index(t => t.QuartoId)
                .Index(t => t.ComodidadeId);
            
            AddColumn("dbo.Quarto", "NumeroCamasCasal", c => c.Int(nullable: false));
            AddColumn("dbo.Quarto", "NumeroCamasSolteiro", c => c.Int(nullable: false));
            AddColumn("dbo.Quarto", "NumeroCasasDeBanho", c => c.Int(nullable: false));
            AddColumn("dbo.Quarto", "TemEstacionamento", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Imagem", "QuartoId", "dbo.Quarto");
            DropForeignKey("dbo.QuartoComodidade", "ComodidadeId", "dbo.Comodidade");
            DropForeignKey("dbo.QuartoComodidade", "QuartoId", "dbo.Quarto");
            DropIndex("dbo.QuartoComodidade", new[] { "ComodidadeId" });
            DropIndex("dbo.QuartoComodidade", new[] { "QuartoId" });
            DropIndex("dbo.Imagem", new[] { "QuartoId" });
            DropColumn("dbo.Quarto", "TemEstacionamento");
            DropColumn("dbo.Quarto", "NumeroCasasDeBanho");
            DropColumn("dbo.Quarto", "NumeroCamasSolteiro");
            DropColumn("dbo.Quarto", "NumeroCamasCasal");
            DropTable("dbo.QuartoComodidade");
            DropTable("dbo.Imagem");
            DropTable("dbo.Comodidade");
            RenameTable(name: "dbo.Quarto", newName: "Quartos");
        }
    }
}
