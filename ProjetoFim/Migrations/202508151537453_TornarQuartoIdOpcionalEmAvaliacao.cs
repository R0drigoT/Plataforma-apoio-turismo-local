namespace ProjetoFim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TornarQuartoIdOpcionalEmAvaliacao : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Avaliacao", "QuartoId", "dbo.Quarto");
            DropIndex("dbo.Avaliacao", new[] { "QuartoId" });
            AlterColumn("dbo.Avaliacao", "QuartoId", c => c.Int());
            CreateIndex("dbo.Avaliacao", "QuartoId");
            AddForeignKey("dbo.Avaliacao", "QuartoId", "dbo.Quarto", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Avaliacao", "QuartoId", "dbo.Quarto");
            DropIndex("dbo.Avaliacao", new[] { "QuartoId" });
            AlterColumn("dbo.Avaliacao", "QuartoId", c => c.Int(nullable: false));
            CreateIndex("dbo.Avaliacao", "QuartoId");
            AddForeignKey("dbo.Avaliacao", "QuartoId", "dbo.Quarto", "Id", cascadeDelete: true);
        }
    }
}
