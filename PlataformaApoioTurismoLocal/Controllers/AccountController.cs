using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ProjetoFim.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ProjetoFim.Resources;

namespace ProjetoFim.Controllers
{
    public class AccountController : BaseController
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

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["LoginError"] = Strings.Account_Login_InvalidData; 
                return RedirectToAction("Index", "Home");
            }

            var result = await SignInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, shouldLockout: false);

            if (result == SignInStatus.Success)
            {
                var user = await UserManager.FindByNameAsync(model.Username);

                if (!await UserManager.IsInRoleAsync(user.Id, "Admin"))
                {
                    return RedirectToAction("Homepage", "Home");
                }
                else
                {
                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                    return RedirectToAction("Index", "Home");
                }
            }
            TempData["LoginError"] = Strings.Account_Login_InvalidAttempt; 
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Registar(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var erros = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                        ? e.Exception?.Message
                        : e.ErrorMessage)
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .ToList();

                if (erros.Count == 0)
                    erros.Add(Resources.Strings.Aviso_erro_inesperado_tente_novamente);

                return Json(new { success = false, errors = erros });
            }

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                DataDeNascimento = model.DateOfBirth
            };

            IdentityResult result;
            try
            {
                result = await UserManager.CreateAsync(user, model.Password);
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, errors = new[] { ex.Message } });
            }

            if (result.Succeeded)
            {
                if (!await UserManager.IsInRoleAsync(user.Id, "Cliente"))
                    await UserManager.AddToRoleAsync(user.Id, "Cliente");

                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                return Json(new { success = true, redirectUrl = Url.Action("Homepage", "Home") });
            }

            var identityErrors = result.Errors?.ToList() ?? new System.Collections.Generic.List<string>();
            if (identityErrors.Count == 0)
                identityErrors.Add(Resources.Strings.Aviso_erro_inesperado_tente_novamente);

            return Json(new { success = false, errors = identityErrors });
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdminLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["LoginError"] = Strings.Account_Login_InvalidData; 
                return RedirectToAction("Index", "Home");
            }
            var result = await SignInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, shouldLockout: false);

            if (result == SignInStatus.Success)
            {
                var user = await UserManager.FindByNameAsync(model.Username);
                if (await UserManager.IsInRoleAsync(user.Id, "Admin"))
                {
                    return RedirectToAction("Dashboard", "Home");
                }
                else
                {
                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                    TempData["LoginError"] = Strings.Account_Admin_Only; 
                    return RedirectToAction("Index", "Home");
                }
            }

            TempData["LoginError"] = Strings.Account_Admin_InvalidAttempt; 
            return RedirectToAction("Index", "Home");
        }

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
