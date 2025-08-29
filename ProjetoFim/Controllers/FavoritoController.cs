using Microsoft.AspNet.Identity;
using ProjetoFim.Models;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ProjetoFim.Controllers
{
    [Authorize] 
    public class FavoritoController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ToggleFavorito(int quartoId)
        {
            var utilizadorId = User.Identity.GetUserId();
            var favoritoExistente = await db.Favoritos
                .FirstOrDefaultAsync(f => f.QuartoId == quartoId && f.UtilizadorId == utilizadorId);

            bool isFavoritoAgora;

            if (favoritoExistente != null)
            {
                db.Favoritos.Remove(favoritoExistente);
                isFavoritoAgora = false; 
            }
            else
            {
                var novoFavorito = new Favorito
                {
                    QuartoId = quartoId,
                    UtilizadorId = utilizadorId
                };
                db.Favoritos.Add(novoFavorito);
                isFavoritoAgora = true; 
            }
            await db.SaveChangesAsync();
            return Json(new { success = true, isFavorito = isFavoritoAgora });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ToggleFavoritoServico(int servicoId)
        {
            var utilizadorId = User.Identity.GetUserId();

            var favoritoExistente = await db.Favoritos
                .FirstOrDefaultAsync(f => f.ServicoId == servicoId && f.UtilizadorId == utilizadorId);

            bool isFavoritoAgora;

            if (favoritoExistente != null)
            {
                db.Favoritos.Remove(favoritoExistente);
                isFavoritoAgora = false;
            }
            else
            {
                var novoFavorito = new Favorito
                {
                    ServicoId = servicoId,
                    UtilizadorId = utilizadorId
                };
                db.Favoritos.Add(novoFavorito);
                isFavoritoAgora = true;
            }

            await db.SaveChangesAsync();
            return Json(new { success = true, isFavorito = isFavoritoAgora });
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}