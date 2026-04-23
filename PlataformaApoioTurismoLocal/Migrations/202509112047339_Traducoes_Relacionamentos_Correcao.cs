using System.Data.Entity.Migrations;

namespace ProjetoFim.Migrations
{
    public partial class Traducoes_Relacionamentos_Correcao : DbMigration
    {
        public override void Up()
        {

            Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes 
               WHERE name = 'IX_QuartoId' AND object_id = OBJECT_ID('dbo.QuartoTrad'))
    CREATE INDEX [IX_QuartoId] ON [dbo].[QuartoTrad]([QuartoId]);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys 
               WHERE parent_object_id = OBJECT_ID('dbo.QuartoTrad')
                 AND referenced_object_id = OBJECT_ID('dbo.Quarto'))
BEGIN
    ALTER TABLE [dbo].[QuartoTrad]  WITH CHECK 
    ADD CONSTRAINT [FK_QuartoTrad_Quarto_QuartoId]
        FOREIGN KEY([QuartoId]) REFERENCES [dbo].[Quarto]([Id]) ON DELETE CASCADE;
END
");


            Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes 
               WHERE name = 'IX_ServicoId' AND object_id = OBJECT_ID('dbo.ServicoTrad'))
    CREATE INDEX [IX_ServicoId] ON [dbo].[ServicoTrad]([ServicoId]);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys 
               WHERE parent_object_id = OBJECT_ID('dbo.ServicoTrad')
                 AND referenced_object_id = OBJECT_ID('dbo.Servico'))
BEGIN
    ALTER TABLE [dbo].[ServicoTrad]  WITH CHECK 
    ADD CONSTRAINT [FK_ServicoTrad_Servico_ServicoId]
        FOREIGN KEY([ServicoId]) REFERENCES [dbo].[Servico]([Id]) ON DELETE CASCADE;
END
");
        }

        public override void Down()
        {

            Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys 
           WHERE name = 'FK_ServicoTrad_Servico_ServicoId')
    ALTER TABLE [dbo].[ServicoTrad] DROP CONSTRAINT [FK_ServicoTrad_Servico_ServicoId];

IF EXISTS (SELECT 1 FROM sys.indexes 
           WHERE name = 'IX_ServicoId' AND object_id = OBJECT_ID('dbo.ServicoTrad'))
    DROP INDEX [IX_ServicoId] ON [dbo].[ServicoTrad];
");

            Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys 
           WHERE name = 'FK_QuartoTrad_Quarto_QuartoId')
    ALTER TABLE [dbo].[QuartoTrad] DROP CONSTRAINT [FK_QuartoTrad_Quarto_QuartoId];

IF EXISTS (SELECT 1 FROM sys.indexes 
           WHERE name = 'IX_QuartoId' AND object_id = OBJECT_ID('dbo.QuartoTrad'))
    DROP INDEX [IX_QuartoId] ON [dbo].[QuartoTrad];
");
        }
    }
}
