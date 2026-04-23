namespace ProjetoFim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdicionarRelacaoConversaProdutos : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Conversa", "QuartoId", c => c.Int());
            AddColumn("dbo.Conversa", "ServicoId", c => c.Int());
            CreateIndex("dbo.Conversa", "QuartoId");
            CreateIndex("dbo.Conversa", "ServicoId");
            AddForeignKey("dbo.Conversa", "QuartoId", "dbo.Quarto", "Id");
            AddForeignKey("dbo.Conversa", "ServicoId", "dbo.Servico", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Conversa", "ServicoId", "dbo.Servico");
            DropForeignKey("dbo.Conversa", "QuartoId", "dbo.Quarto");
            DropIndex("dbo.Conversa", new[] { "ServicoId" });
            DropIndex("dbo.Conversa", new[] { "QuartoId" });
            DropColumn("dbo.Conversa", "ServicoId");
            DropColumn("dbo.Conversa", "QuartoId");
        }
    }
}
