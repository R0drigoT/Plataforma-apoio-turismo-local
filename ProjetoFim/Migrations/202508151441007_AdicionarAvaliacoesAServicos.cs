namespace ProjetoFim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdicionarAvaliacoesAServicos : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Avaliacao", "ServicoId", c => c.Int());
            AddColumn("dbo.Servico", "AvaliacaoMedia", c => c.Double(nullable: false));
            CreateIndex("dbo.Avaliacao", "ServicoId");
            AddForeignKey("dbo.Avaliacao", "ServicoId", "dbo.Servico", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Avaliacao", "ServicoId", "dbo.Servico");
            DropIndex("dbo.Avaliacao", new[] { "ServicoId" });
            DropColumn("dbo.Servico", "AvaliacaoMedia");
            DropColumn("dbo.Avaliacao", "ServicoId");
        }
    }
}
