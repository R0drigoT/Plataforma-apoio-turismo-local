using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjetoFim.Models
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime DataDeNascimento { get; set; }
        public string CaminhoFotoPerfil { get; set; }
        public virtual ICollection<Avaliacao> Avaliacoes { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("AlojamentoContext", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create() => new ApplicationDbContext();

        public DbSet<Quarto> Quartos { get; set; }
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<Comodidade> Comodidades { get; set; }
        public DbSet<Imagem> Imagens { get; set; }
        public DbSet<Notificacao> Notificacoes { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<DetalhesReserva> DetalhesReservas { get; set; }
        public DbSet<Favorito> Favoritos { get; set; }
        public DbSet<Conversa> Conversas { get; set; }
        public DbSet<Mensagem> Mensagens { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }
        public DbSet<QuartoTrad> QuartosTrad { get; set; }
        public DbSet<ServicoTrad> ServicosTrad { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<Quarto>()
                .HasMany(q => q.Comodidades)
                .WithMany(c => c.Quartos)
                .Map(m =>
                {
                    m.ToTable("QuartoComodidade");
                    m.MapLeftKey("QuartoId");
                    m.MapRightKey("ComodidadeId");
                });

            modelBuilder.Entity<Conversa>()
                .HasRequired(c => c.Utilizador)
                .WithMany()
                .HasForeignKey(c => c.UtilizadorId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Mensagem>()
                .HasRequired(m => m.Remetente)
                .WithMany()
                .HasForeignKey(m => m.RemetenteId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<QuartoTrad>()
                .HasRequired(t => t.Quarto)
                .WithMany(q => q.Traducoes)
                .HasForeignKey(t => t.QuartoId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ServicoTrad>()
                .HasRequired(t => t.Servico)
                .WithMany(s => s.Traducoes)
                .HasForeignKey(t => t.ServicoId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<QuartoTrad>()
                .Property(t => t.Cultura)
                .HasMaxLength(5);

            modelBuilder.Entity<ServicoTrad>()
                .Property(t => t.Cultura)
                .HasMaxLength(5);
        }
    }
}
