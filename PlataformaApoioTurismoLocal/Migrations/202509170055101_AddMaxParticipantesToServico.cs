namespace ProjetoFim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMaxParticipantesToServico : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Servico", "MaxParticipantes", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Servico", "MaxParticipantes");
        }
    }
}
