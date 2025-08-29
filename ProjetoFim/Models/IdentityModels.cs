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
        public DbSet<Quarto> Quartos { get; set; }
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<Comodidade> Comodidades { get; set; }
        public DbSet<Imagem> Imagens { get; set; }
        public System.Data.Entity.DbSet<Notificacao> Notificacoes { get; set; }
        public System.Data.Entity.DbSet<Reserva> Reservas { get; set; }
        public System.Data.Entity.DbSet<DetalhesReserva> DetalhesReservas { get; set; }
        public System.Data.Entity.DbSet<Favorito> Favoritos { get; set; }
        public System.Data.Entity.DbSet<Conversa> Conversas { get; set; }
        public System.Data.Entity.DbSet<Mensagem> Mensagens { get; set; }
        public System.Data.Entity.DbSet<Avaliacao> Avaliacoes { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

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
        }
    }
}