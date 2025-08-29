namespace ProjetoFim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdicionarCoordenadasAosAnuncios : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Quarto", "Latitude", c => c.Double());
            AddColumn("dbo.Quarto", "Longitude", c => c.Double());
            AddColumn("dbo.Servico", "Latitude", c => c.Double());
            AddColumn("dbo.Servico", "Longitude", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Servico", "Longitude");
            DropColumn("dbo.Servico", "Latitude");
            DropColumn("dbo.Quarto", "Longitude");
            DropColumn("dbo.Quarto", "Latitude");
        }
    }
}
