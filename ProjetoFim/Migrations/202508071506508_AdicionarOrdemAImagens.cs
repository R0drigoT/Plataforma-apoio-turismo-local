namespace ProjetoFim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdicionarOrdemAImagens : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Imagem", "Ordem", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Imagem", "Ordem");
        }
    }
}
