namespace ProjetoFim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TornarFavoritosPolimorficos : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Favorito", "QuartoId", "dbo.Quarto");
            DropIndex("dbo.Favorito", new[] { "QuartoId" });
            AlterColumn("dbo.Favorito", "QuartoId", c => c.Int());
            CreateIndex("dbo.Favorito", "QuartoId");
            AddForeignKey("dbo.Favorito", "QuartoId", "dbo.Quarto", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Favorito", "QuartoId", "dbo.Quarto");
            DropIndex("dbo.Favorito", new[] { "QuartoId" });
            AlterColumn("dbo.Favorito", "QuartoId", c => c.Int(nullable: false));
            CreateIndex("dbo.Favorito", "QuartoId");
            AddForeignKey("dbo.Favorito", "QuartoId", "dbo.Quarto", "Id", cascadeDelete: true);
        }
    }
}
