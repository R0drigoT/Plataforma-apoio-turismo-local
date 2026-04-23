namespace ProjetoFim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdicionarImagensAServicos : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Imagem", "QuartoId", "dbo.Quarto");
            DropIndex("dbo.Imagem", new[] { "QuartoId" });
            AddColumn("dbo.Imagem", "ServicoId", c => c.Int());
            AlterColumn("dbo.Imagem", "QuartoId", c => c.Int());
            CreateIndex("dbo.Imagem", "QuartoId");
            CreateIndex("dbo.Imagem", "ServicoId");
            AddForeignKey("dbo.Imagem", "ServicoId", "dbo.Servico", "Id");
            AddForeignKey("dbo.Imagem", "QuartoId", "dbo.Quarto", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Imagem", "QuartoId", "dbo.Quarto");
            DropForeignKey("dbo.Imagem", "ServicoId", "dbo.Servico");
            DropIndex("dbo.Imagem", new[] { "ServicoId" });
            DropIndex("dbo.Imagem", new[] { "QuartoId" });
            AlterColumn("dbo.Imagem", "QuartoId", c => c.Int(nullable: false));
            DropColumn("dbo.Imagem", "ServicoId");
            CreateIndex("dbo.Imagem", "QuartoId");
            AddForeignKey("dbo.Imagem", "QuartoId", "dbo.Quarto", "Id", cascadeDelete: true);
        }
    }
}
