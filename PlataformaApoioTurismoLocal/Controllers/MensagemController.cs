using Microsoft.AspNet.Identity;
using ProjetoFim.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ProjetoFim.Controllers
{
    [Authorize]
    public class MensagemController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private string GetNomeTraduzido(Quarto q, Servico s)
        {
            var lang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            if (q != null)
            {
                var t = (q.Traducoes ?? new System.Collections.Generic.List<QuartoTrad>())
                            .FirstOrDefault(x => x.Cultura == lang)
                     ?? (q.Traducoes ?? new System.Collections.Generic.List<QuartoTrad>())
                            .FirstOrDefault(x => x.Cultura == "pt");
                return t?.Nome ?? q.Nome;
            }
            if (s != null)
            {
                var t = (s.Traducoes ?? new System.Collections.Generic.List<ServicoTrad>())
                            .FirstOrDefault(x => x.Cultura == lang)
                     ?? (s.Traducoes ?? new System.Collections.Generic.List<ServicoTrad>())
                            .FirstOrDefault(x => x.Cultura == "pt");
                return t?.Nome ?? s.Nome;
            }
            return null;
        }
        private string BuildAssuntoDinamico(Conversa c)
        {
            if (c.ReservaId.HasValue)
                return string.Format(ProjetoFim.Resources.Strings.Messages_Subject_AboutReservation_Format,
                                     c.ReservaId.Value.ToString("D5"));

            var nome = GetNomeTraduzido(c.Quarto, c.Servico);
            if (!string.IsNullOrWhiteSpace(nome))
                return string.Format(ProjetoFim.Resources.Strings.Messages_Subject_QuestionAbout_Format, nome);

            return ProjetoFim.Resources.Strings.Messages_Subject_GeneralContact;
        }
        public async Task<ActionResult> Index(string pesquisa, string filtro = "todas")
        {
            var utilizadorId = User.Identity.GetUserId();
            var query = db.Conversas
                          .Where(c => c.UtilizadorId == utilizadorId)
                          .Include(c => c.Quarto.Imagens)
                          .Include(c => c.Servico.Imagens)
                          .Include(c => c.Quarto.Traducoes)
                          .Include(c => c.Servico.Traducoes)
                          .AsQueryable();
            if (!string.IsNullOrEmpty(pesquisa))
            {
                query = query.Where(c => c.Assunto.Contains(pesquisa) || c.Mensagens.Any(m => m.Conteudo.Contains(pesquisa)));
            }
            if (filtro == "nao-lidas")
            {
                query = query.Where(c => c.Mensagens.Any(m => !m.Lida && m.RemetenteId != utilizadorId));
            }
            ViewBag.FiltroAtivo = filtro;
            var conversas = await query.OrderByDescending(c => c.DataCriacao).ToListAsync();
            foreach (var c in conversas)
                c.Assunto = BuildAssuntoDinamico(c);
            return View(conversas);
        }
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CaixaDeEntrada(string pesquisa, string filtro = "todas")
        {
            var adminId = User.Identity.GetUserId();

            var query = db.Conversas
                          .Include(c => c.Utilizador)
                          .Include(c => c.Quarto.Imagens)
                          .Include(c => c.Quarto.Traducoes)   
                          .Include(c => c.Servico.Imagens)
                          .Include(c => c.Servico.Traducoes); 

            if (!string.IsNullOrEmpty(pesquisa))
                query = query.Where(c => c.Assunto.Contains(pesquisa) ||
                                         c.Utilizador.UserName.Contains(pesquisa));

            if (filtro == "nao-lidas")
                query = query.Where(c => c.Mensagens.Any(m => !m.Lida && m.RemetenteId != adminId));

            ViewBag.FiltroAtivo = filtro;

            var conversas = await query.OrderByDescending(c => c.DataCriacao).ToListAsync();

            foreach (var c in conversas)
                c.Assunto = BuildAssuntoDinamico(c);

            return View(conversas);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> EnviarMensagem(int conversaId, string conteudo)
        {
            if (string.IsNullOrWhiteSpace(conteudo))
            {
                return Json(new { success = false, message = ProjetoFim.Resources.Strings.Messages_Error_EmptyMessage });
            }

            var remetenteId = User.Identity.GetUserId();
            var novaMensagem = new Mensagem
            {
                ConversaId = conversaId,
                Conteudo = conteudo,
                DataEnvio = DateTime.Now,
                RemetenteId = remetenteId,
                Lida = false
            };
            db.Mensagens.Add(novaMensagem);
            await db.SaveChangesAsync();
            return Json(new { success = true });
        }
        [HttpGet]
        public async Task<ActionResult> VerConversa(int id)
        {
            var utilizadorId = User.Identity.GetUserId();

            var conversa = await db.Conversas
                .Include(c => c.Utilizador)                        
                .Include(c => c.Quarto.Traducoes)                   
                .Include(c => c.Servico.Traducoes)                  
                .Include(c => c.Mensagens.Select(m => m.Remetente)) 
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conversa == null)
                return HttpNotFound();

            if (conversa.UtilizadorId != utilizadorId && !User.IsInRole("Admin"))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);

            conversa.Assunto = BuildAssuntoDinamico(conversa);

            var porLer = conversa.Mensagens.Where(m => !m.Lida && m.RemetenteId != utilizadorId).ToList();
            foreach (var m in porLer) m.Lida = true;
            if (porLer.Count > 0) await db.SaveChangesAsync();

            return PartialView("_MensagensDaConversa", conversa);
        }

        public async Task<ActionResult> VerOuCriarConversa(int reservaId)
        {
            var utilizadorId = User.Identity.GetUserId();
            var conversa = await db.Conversas.FirstOrDefaultAsync(c => c.ReservaId == reservaId);
            if (conversa == null)
            {
                var reserva = await db.Reservas.Include(r => r.Utilizador).FirstOrDefaultAsync(r => r.Id == reservaId);
                if (reserva == null) return HttpNotFound();

                conversa = new Conversa
                {
                    Assunto = string.Format(ProjetoFim.Resources.Strings.Messages_Subject_AboutReservation_Format, reserva.Id.ToString("D5")),
                    DataCriacao = DateTime.Now,
                    UtilizadorId = reserva.UtilizadorId,
                    ReservaId = reservaId
                };
                db.Conversas.Add(conversa);
                await db.SaveChangesAsync();
            }
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("CaixaDeEntrada");
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> IniciarConversa(int? quartoId, int? servicoId, string conteudo)
        {
            if (string.IsNullOrWhiteSpace(conteudo))
            {
                return Json(new { success = false, message = ProjetoFim.Resources.Strings.Messages_Error_EmptyMessage });
            }
            var utilizadorId = User.Identity.GetUserId();

            string assunto = ProjetoFim.Resources.Strings.Messages_Subject_GeneralContact;

            if (quartoId.HasValue)
            {
                var quarto = await db.Quartos.FindAsync(quartoId.Value);
                if (quarto != null)
                    assunto = string.Format(ProjetoFim.Resources.Strings.Messages_Subject_QuestionAbout_Format, quarto.Nome);
            }
            else if (servicoId.HasValue)
            {
                var servico = await db.Servicos.FindAsync(servicoId.Value);
                if (servico != null)
                    assunto = string.Format(ProjetoFim.Resources.Strings.Messages_Subject_QuestionAbout_Format, servico.Nome);
            }

            var novaConversa = new Conversa
            {
                Assunto = assunto,
                DataCriacao = DateTime.Now,
                UtilizadorId = utilizadorId,
                QuartoId = quartoId,
                ServicoId = servicoId
            };

            var primeiraMensagem = new Mensagem
            {
                Conversa = novaConversa,
                Conteudo = conteudo,
                DataEnvio = DateTime.Now,
                RemetenteId = utilizadorId,
                Lida = false
            };

            db.Conversas.Add(novaConversa);
            db.Mensagens.Add(primeiraMensagem);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = ProjetoFim.Resources.Strings.Messages_Success_Sent });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Responder(int conversaId, string conteudo)
        {
            if (string.IsNullOrWhiteSpace(conteudo))
            {
                return Json(new { success = false, message = ProjetoFim.Resources.Strings.Messages_Error_EmptyMessage });
            }
            var remetenteId = User.Identity.GetUserId();
            var novaMensagem = new Mensagem
            {
                ConversaId = conversaId,
                Conteudo = conteudo,
                DataEnvio = DateTime.Now,
                RemetenteId = remetenteId,
                Lida = false
            };
            db.Mensagens.Add(novaMensagem);
            await db.SaveChangesAsync();
            return Json(new { success = true, message = RenderRazorViewToString("_SingleMensagem", novaMensagem) });
        }
        private string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new System.IO.StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
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