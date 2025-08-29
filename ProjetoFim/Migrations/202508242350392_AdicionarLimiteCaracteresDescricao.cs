namespace ProjetoFim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdicionarLimiteCaracteresDescricao : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Avaliacao", "Comentario", c => c.String(maxLength: 500));
            AlterColumn("dbo.Quarto", "Descricao", c => c.String(maxLength: 1000));
            AlterColumn("dbo.Servico", "Descricao", c => c.String(maxLength: 1000));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Servico", "Descricao", c => c.String());
            AlterColumn("dbo.Quarto", "Descricao", c => c.String());
            AlterColumn("dbo.Avaliacao", "Comentario", c => c.String());
        }
    }
}
