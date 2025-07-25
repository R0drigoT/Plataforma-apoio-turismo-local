namespace ProjetoFim.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using ProjetoFim.Models;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ProjetoFim.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ProjetoFim.Models.ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            // Cria a Role "Admin" se não existir
            if (!roleManager.RoleExists("Admin"))
            {
                roleManager.Create(new IdentityRole("Admin"));
            }

            // Cria a Role "Cliente" se não existir
            if (!roleManager.RoleExists("Cliente"))
            {
                roleManager.Create(new IdentityRole("Cliente"));
            }

            // Cria o utilizador "admin" se não existir
            if (userManager.FindByName("admin") == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@exemplo.com", // O Email é obrigatório
                    DataDeNascimento = System.DateTime.Now
                };
                var result = userManager.Create(adminUser, "Admin123!"); // Password forte

                if (result.Succeeded)
                {
                    // Atribui a Role "Admin" ao utilizador "admin"
                    userManager.AddToRole(adminUser.Id, "Admin");
                }
            }

            if (!context.Quartos.Any())
            {
                context.Quartos.AddOrUpdate(
                    q => q.Nome,
                    new Quarto { Nome = "Apartamento Vista Mar", Descricao = "Um belo apartamento com vista para o mar.", Localizacao = "Algarve", PrecoPorNoite = 120.00m, NumeroHospedes = 4 },
                    new Quarto { Nome = "Estúdio Moderno", Descricao = "Um estúdio no centro da cidade.", Localizacao = "Lisboa", PrecoPorNoite = 85.50m, NumeroHospedes = 2 },
                    new Quarto { Nome = "Casa de Campo", Descricao = "Uma casa rústica para relaxar.", Localizacao = "Gerês", PrecoPorNoite = 95.00m, NumeroHospedes = 6 }
                );
            }

            // Adiciona comodidades de exemplo se a tabela estiver vazia
            if (!context.Comodidades.Any())
            {
                context.Comodidades.AddOrUpdate(
                    c => c.Nome,
                    new Comodidade { Nome = "Televisão", IconeCss = "fas fa-tv" },
                    new Comodidade { Nome = "Wifi", IconeCss = "fas fa-wifi" },
                    new Comodidade { Nome = "Máquina de Lavar", IconeCss = "fas fa-washer" },
                    new Comodidade { Nome = "Ar Condicionado", IconeCss = "fas fa-snowflake" },
                    new Comodidade { Nome = "Cozinha", IconeCss = "fas fa-utensils" },
                    new Comodidade { Nome = "Permitido animais", IconeCss = "fas fa-paw" }
                );
            }

            // Adiciona serviços de exemplo se a tabela estiver vazia
            if (!context.Servicos.Any())
            {
                context.Servicos.AddOrUpdate(
                    s => s.Nome,
                    new Servico { Nome = "Passeio de Barco no Douro", Descricao = "Um relaxante passeio de 2 horas pelo rio Douro.", Localizacao = "Porto", Preco = 35.00m },
                    new Servico { Nome = "Tour a Sintra", Descricao = "Visita guiada de dia inteiro aos palácios de Sintra.", Localizacao = "Lisboa", Preco = 60.00m },
                    new Servico { Nome = "Aula de Surf", Descricao = "Aprenda a surfar com os melhores instrutores.", Localizacao = "Peniche", Preco = 40.00m }
                );
            }
        }
    }
}