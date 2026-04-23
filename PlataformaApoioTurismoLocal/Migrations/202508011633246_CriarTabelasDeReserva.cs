namespace ProjetoFim.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CriarTabelasDeReserva : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DetalhesReserva",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Quantidade = c.Int(nullable: false),
                        PrecoUnitario = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ReservaId = c.Int(nullable: false),
                        QuartoId = c.Int(),
                        ServicoId = c.Int(),
                        DataInicio = c.DateTime(),
                        DataFim = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quarto", t => t.QuartoId)
                .ForeignKey("dbo.Reserva", t => t.ReservaId, cascadeDelete: true)
                .ForeignKey("dbo.Servico", t => t.ServicoId)
                .Index(t => t.ReservaId)
                .Index(t => t.QuartoId)
                .Index(t => t.ServicoId);
            
            CreateTable(
                "dbo.Reserva",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DataCriacao = c.DateTime(nullable: false),
                        ValorTotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Estado = c.String(),
                        UtilizadorId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UtilizadorId, cascadeDelete: true)
                .Index(t => t.UtilizadorId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DetalhesReserva", "ServicoId", "dbo.Servico");
            DropForeignKey("dbo.Reserva", "UtilizadorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DetalhesReserva", "ReservaId", "dbo.Reserva");
            DropForeignKey("dbo.DetalhesReserva", "QuartoId", "dbo.Quarto");
            DropIndex("dbo.Reserva", new[] { "UtilizadorId" });
            DropIndex("dbo.DetalhesReserva", new[] { "ServicoId" });
            DropIndex("dbo.DetalhesReserva", new[] { "QuartoId" });
            DropIndex("dbo.DetalhesReserva", new[] { "ReservaId" });
            DropTable("dbo.Reserva");
            DropTable("dbo.DetalhesReserva");
        }
    }
}
