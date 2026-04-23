using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using ProjetoFim.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ProjetoFim.Controllers
{
    [Authorize]
    public class ManageController : BaseController
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadFotoPerfil(HttpPostedFileBase ficheiroFoto)
        {
            if (ficheiroFoto != null && ficheiroFoto.ContentLength > 0)
            {
                var utilizadorId = User.Identity.GetUserId();
                var utilizador = await UserManager.FindByIdAsync(utilizadorId);
                if (utilizador != null)
                {
                    var nomeFicheiro = Guid.NewGuid().ToString() + Path.GetExtension(ficheiroFoto.FileName);
                    var caminho = Path.Combine(Server.MapPath("~/Uploads/FotosPerfil/"), nomeFicheiro);
                    Directory.CreateDirectory(Server.MapPath("~/Uploads/FotosPerfil/"));
                    ficheiroFoto.SaveAs(caminho);
                    utilizador.CaminhoFotoPerfil = "/Uploads/FotosPerfil/" + nomeFicheiro;
                    await UserManager.UpdateAsync(utilizador);
                }
            }
            return RedirectToAction("Perfil", "Home");
        }
    }
}