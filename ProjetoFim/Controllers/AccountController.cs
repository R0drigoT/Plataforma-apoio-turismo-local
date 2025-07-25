using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ProjetoFim.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ProjetoFim.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController() { }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get => _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            private set => _signInManager = value;
        }

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        // POST: /Account/Login (Serve para Utilizadores e Admins)
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dados de login inválidos.";
                return RedirectToAction("Index", "Home");
            }

            var result = await SignInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:
                    var user = await UserManager.FindByNameAsync(model.Username);
                    if (user != null && await UserManager.IsInRoleAsync(user.Id, "Admin"))
                    {
                        return RedirectToAction("Dashboard", "Home");
                    }
                    return RedirectToAction("Homepage", "Home");

                case SignInStatus.Failure:
                default:
                    TempData["ErrorMessage"] = "Tentativa de login inválida.";
                    return RedirectToAction("Index", "Home");
            }
        }

        // POST: /Account/Registar
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Registar(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Username, Email = model.Username, DataDeNascimento = model.DateOfBirth };
                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Atribui a role "Cliente" ao novo utilizador
                    await UserManager.AddToRoleAsync(user.Id, "Cliente");

                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    return Json(new { success = true, redirectUrl = Url.Action("Homepage", "Home") });
                }

                var errors = result.Errors.ToList();
                return Json(new { success = false, errors = errors });
            }

            var modelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, errors = modelErrors });
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;
    }
}