namespace ProjetoFim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdicionarTabelaFavoritos : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Favorito",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UtilizadorId = c.String(nullable: false, maxLength: 128),
                        QuartoId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quarto", t => t.QuartoId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UtilizadorId, cascadeDelete: true)
                .Index(t => t.UtilizadorId)
                .Index(t => t.QuartoId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Favorito", "UtilizadorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Favorito", "QuartoId", "dbo.Quarto");
            DropIndex("dbo.Favorito", new[] { "QuartoId" });
            DropIndex("dbo.Favorito", new[] { "UtilizadorId" });
            DropTable("dbo.Favorito");
        }
    }
}
