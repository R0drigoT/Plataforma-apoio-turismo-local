namespace ProjetoFim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdicionarTabelaNotificacao : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Notificacao",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Mensagem = c.String(),
                        Lida = c.Boolean(nullable: false),
                        DataCriacao = c.DateTime(nullable: false),
                        Url = c.String(),
                        DestinatarioId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.DestinatarioId)
                .Index(t => t.DestinatarioId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Notificacao", "DestinatarioId", "dbo.AspNetUsers");
            DropIndex("dbo.Notificacao", new[] { "DestinatarioId" });
            DropTable("dbo.Notificacao");
        }
    }
}
