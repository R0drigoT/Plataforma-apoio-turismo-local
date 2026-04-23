namespace ProjetoFim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdicionarFavoritosDeServico : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Favorito", "ServicoId", c => c.Int());
            CreateIndex("dbo.Favorito", "ServicoId");
            AddForeignKey("dbo.Favorito", "ServicoId", "dbo.Servico", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Favorito", "ServicoId", "dbo.Servico");
            DropIndex("dbo.Favorito", new[] { "ServicoId" });
            DropColumn("dbo.Favorito", "ServicoId");
        }
    }
}
