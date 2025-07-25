using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjetoFim.Models
{
    // Pode adicionar dados de perfil extra para o utilizador aqui
    public class ApplicationUser : IdentityUser
    {
        public DateTime DataDeNascimento { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }

    // Este é o nosso NOVO contexto principal da base de dados
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("AlojamentoContext", throwIfV1Schema: false)
        {
            // Já não precisamos do SetInitializer(null) aqui
        }

        // As nossas tabelas do projeto vêm para aqui
        public DbSet<Quarto> Quartos { get; set; }
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<Comodidade> Comodidades { get; set; }
        public DbSet<Imagem> Imagens { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Muito importante chamar a base primeiro
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // A nossa configuração de relação de tabelas vem para aqui
            modelBuilder.Entity<Quarto>()
                .HasMany(q => q.Comodidades)
                .WithMany(c => c.Quartos)
                .Map(m =>
                {
                    m.ToTable("QuartoComodidade");
                    m.MapLeftKey("QuartoId");
                    m.MapRightKey("ComodidadeId");
                });
        }
    }
}