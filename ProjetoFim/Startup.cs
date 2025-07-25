using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using ProjetoFim.Models;

// Esta linha é crucial e garante que esta classe é executada no arranque.
[assembly: OwinStartup(typeof(ProjetoFim.Startup))]

namespace ProjetoFim
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Configura o DbContext, UserManager e SignInManager para serem usados uma vez por pedido
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Ativa a autenticação por cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Home/Index")
            });
        }
    }
}