namespace ProjetoFim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdicionarDescontoAoQuarto : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Quarto", "DescontoPercentagem", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Quarto", "DescontoPercentagem");
        }
    }
}
