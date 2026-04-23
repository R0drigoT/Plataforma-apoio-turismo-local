namespace ProjetoFim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdicionarDescontoAServicos : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Servico", "DescontoPercentagem", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Servico", "DescontoPercentagem");
        }
    }
}
