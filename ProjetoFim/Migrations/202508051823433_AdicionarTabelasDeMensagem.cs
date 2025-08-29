namespace ProjetoFim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdicionarTabelasDeMensagem : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Conversa",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Assunto = c.String(nullable: false),
                        DataCriacao = c.DateTime(nullable: false),
                        UtilizadorId = c.String(nullable: false, maxLength: 128),
                        ReservaId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Reserva", t => t.ReservaId)
                .ForeignKey("dbo.AspNetUsers", t => t.UtilizadorId)
                .Index(t => t.UtilizadorId)
                .Index(t => t.ReservaId);
            
            CreateTable(
                "dbo.Mensagem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Conteudo = c.String(nullable: false),
                        DataEnvio = c.DateTime(nullable: false),
                        Lida = c.Boolean(nullable: false),
                        ConversaId = c.Int(nullable: false),
                        RemetenteId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Conversa", t => t.ConversaId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.RemetenteId)
                .Index(t => t.ConversaId)
                .Index(t => t.RemetenteId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Conversa", "UtilizadorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Conversa", "ReservaId", "dbo.Reserva");
            DropForeignKey("dbo.Mensagem", "RemetenteId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Mensagem", "ConversaId", "dbo.Conversa");
            DropIndex("dbo.Mensagem", new[] { "RemetenteId" });
            DropIndex("dbo.Mensagem", new[] { "ConversaId" });
            DropIndex("dbo.Conversa", new[] { "ReservaId" });
            DropIndex("dbo.Conversa", new[] { "UtilizadorId" });
            DropTable("dbo.Mensagem");
            DropTable("dbo.Conversa");
        }
    }
}
