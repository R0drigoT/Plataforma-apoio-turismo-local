namespace ProjetoFim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdicionarFotoDePerfilAoUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "CaminhoFotoPerfil", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "CaminhoFotoPerfil");
        }
    }
}
