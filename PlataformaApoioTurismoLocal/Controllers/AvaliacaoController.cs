using Microsoft.AspNet.Identity;
using ProjetoFim.Models;
using ProjetoFim.Resources;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ProjetoFim.Controllers
{
    [Authorize]
    public class AvaliacaoController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Criar(int quartoId, int classificacao, string comentario)
        {
            var utilizadorId = User.Identity.GetUserId();
            var hoje = DateTime.Today;

            bool temReservaConcluida = await db.DetalhesReservas.AnyAsync(d =>
                d.QuartoId == quartoId &&
                d.Reserva.UtilizadorId == utilizadorId &&
                d.Reserva.Estado == "Concluída"
            );
            if (!temReservaConcluida)
            {
                TempData["ErroAvaliacao"] = Strings.Avaliacao_Error_MustStayBeforeReview;
                return RedirectToAction("DetalhesQuarto", "Home", new { id = quartoId });
            }

            bool jaAvaliou = await db.Avaliacoes.AnyAsync(a => a.QuartoId == quartoId && a.UtilizadorId == utilizadorId);
            if (jaAvaliou)
            {
                TempData["ErroAvaliacao"] = Strings.Avaliacao_Error_AlreadyReviewed;
                return RedirectToAction("DetalhesQuarto", "Home", new { id = quartoId });
            }

            var novaAvaliacao = new Avaliacao
            {
                QuartoId = quartoId,
                UtilizadorId = utilizadorId,
                Classificacao = classificacao,
                Comentario = comentario,
                DataAvaliacao = DateTime.Now
            };
            db.Avaliacoes.Add(novaAvaliacao);
            await db.SaveChangesAsync();

            var avaliacoesDoQuarto = db.Avaliacoes.Where(a => a.QuartoId == quartoId);
            if (avaliacoesDoQuarto.Any())
            {
                var media = await avaliacoesDoQuarto.AverageAsync(a => a.Classificacao);
                var quarto = await db.Quartos.FindAsync(quartoId);
                quarto.AvaliacaoMedia = Math.Round(media, 1);
                await db.SaveChangesAsync();
            }

            TempData["SucessoAvaliacao"] = Strings.Avaliacao_Success_Submitted;
            return RedirectToAction("DetalhesQuarto", "Home", new { id = quartoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CriarParaServico(int servicoId, int classificacao, string comentario)
        {
            var utilizadorId = User.Identity.GetUserId();

            bool temReservaConcluida = await db.DetalhesReservas.AnyAsync(d =>
                d.ServicoId == servicoId &&
                d.Reserva.UtilizadorId == utilizadorId &&
                d.Reserva.Estado == "Concluída"
            );
            if (!temReservaConcluida)
            {
                TempData["ErroAvaliacao"] = Strings.AvaliacaoService_Error_MustStayBeforeReview;
                return RedirectToAction("DetalhesServico", "Home", new { id = servicoId });
            }

            bool jaAvaliou = await db.Avaliacoes.AnyAsync(a => a.ServicoId == servicoId && a.UtilizadorId == utilizadorId);
            if (jaAvaliou)
            {
                TempData["ErroAvaliacao"] = Strings.AvaliacaoService_Error_AlreadyReviewed;
                return RedirectToAction("DetalhesServico", "Home", new { id = servicoId });
            }

            var novaAvaliacao = new Avaliacao
            {
                ServicoId = servicoId,
                UtilizadorId = utilizadorId,
                Classificacao = classificacao,
                Comentario = comentario,
                DataAvaliacao = DateTime.Now
            };
            db.Avaliacoes.Add(novaAvaliacao);
            await db.SaveChangesAsync();

            var servico = await db.Servicos.FindAsync(servicoId);
            if (servico != null)
            {
                var media = await db.Avaliacoes.Where(a => a.ServicoId == servicoId).AverageAsync(a => a.Classificacao);
                servico.AvaliacaoMedia = Math.Round(media, 1);
                await db.SaveChangesAsync();
            }

            TempData["SucessoAvaliacao"] = Strings.Avaliacao_Success_Submitted;
            return RedirectToAction("DetalhesServico", "Home", new { id = servicoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Apagar(int id)
        {
            var utilizadorId = User.Identity.GetUserId();
            var avaliacao = await db.Avaliacoes
                                    .Include(a => a.Quarto)
                                    .Include(a => a.Servico)
                                    .FirstOrDefaultAsync(a => a.Id == id);

            if (avaliacao == null || (avaliacao.UtilizadorId != utilizadorId && !User.IsInRole("Admin")))
            {
                return Json(new { success = false, message = Strings.Avaliacao_Delete_NotAllowed });
            }

            if (avaliacao.QuartoId.HasValue)
            {
                var quartoId = avaliacao.QuartoId.Value;
                db.Avaliacoes.Remove(avaliacao);
                await db.SaveChangesAsync();

                var quarto = await db.Quartos.FindAsync(quartoId);
                if (quarto != null)
                {
                    var avaliacoesRestantes = db.Avaliacoes.Where(a => a.QuartoId == quartoId);
                    quarto.AvaliacaoMedia = avaliacoesRestantes.Any()
                        ? await avaliacoesRestantes.AverageAsync(a => a.Classificacao)
                        : 0;
                    await db.SaveChangesAsync();
                }
            }
            else if (avaliacao.ServicoId.HasValue)
            {
                var servicoId = avaliacao.ServicoId.Value;
                db.Avaliacoes.Remove(avaliacao);
                await db.SaveChangesAsync();

                var servico = await db.Servicos.FindAsync(servicoId);
                if (servico != null)
                {
                    var avaliacoesRestantes = db.Avaliacoes.Where(a => a.ServicoId == servicoId);
                    servico.AvaliacaoMedia = avaliacoesRestantes.Any()
                        ? await avaliacoesRestantes.AverageAsync(a => a.Classificacao)
                        : 0;
                    await db.SaveChangesAsync();
                }
            }

            return Json(new { success = true });
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
