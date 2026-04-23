using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using PagedList;
using ProjetoFim.Models;
using ProjetoFim.Resources;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace ProjetoFim.Controllers
{
    public class HomeController : BaseController
    {
        
        private ApplicationDbContext db = new ApplicationDbContext();
        private async Task<HomepageViewModel> PrepararDadosHomepage()
        {
            var viewModel = new HomepageViewModel
            {
                QuartosMelhorAvaliacao = await db.Quartos
                                                 .Include(s => s.Traducoes)
                                                 .Include(q => q.Imagens)
                                                 .OrderByDescending(q => q.AvaliacaoMedia)
                                                 .ThenByDescending(q => q.Id)
                                                 .Take(8)
                                                 .ToListAsync(),

                QuartosRecemAdicionados = await db.Quartos
                                                  .Include(s => s.Traducoes)
                                                  .Include(q => q.Imagens)
                                                  .OrderByDescending(q => q.Id)
                                                  .Take(8)
                                                  .ToListAsync(),

                QuartosComDesconto = await db.Quartos
                                             .Include(s => s.Traducoes)
                                             .Include(q => q.Imagens)
                                             .Where(q => q.DescontoPercentagem > 0)
                                             .OrderByDescending(q => q.DescontoPercentagem)
                                             .Take(8)
                                             .ToListAsync()
            };
            return viewModel;
        }

public async Task<ActionResult> Index()
{
    var viewModel = await PrepararDadosHomepage();
    return View("Homepage", viewModel);
}
[Authorize]
public async Task<ActionResult> Homepage()
{
    var viewModel = await PrepararDadosHomepage();
    var utilizadorId = User.Identity.GetUserId();
    viewModel.FavoritosDoUtilizador = await db.Favoritos
                                              .Where(f => f.UtilizadorId == utilizadorId && f.QuartoId != null)
                                              .Select(f => f.QuartoId.Value)
                                              .ToListAsync();
    return View(viewModel);
}
        public async Task<ActionResult> Servicos()
        {
            var servicos = await db.Servicos.Include(s => s.Imagens).ToListAsync();
            var viewModel = new ServicosViewModel

            {
                ServicosMaisRequisitados = await db.Servicos
                                                   .Include(s => s.Traducoes)
                                                   .OrderByDescending(s => s.AvaliacaoMedia)
                                                   .Take(8)
                                                   .ToListAsync(),

                ServicosRecemAdicionados = await db.Servicos
                                                   .Include(s => s.Traducoes)
                                                   .OrderByDescending(s => s.Id)
                                                   .Take(8)
                                                   .ToListAsync(),

                ServicosComDesconto = await db.Servicos
                                              .Include(s => s.Traducoes)
                                              .Where(s => s.DescontoPercentagem > 0)
                                              .OrderByDescending(s => s.DescontoPercentagem)
                                              .Take(8)
                                              .ToListAsync()
            };

            return View(viewModel);
        }
        public async Task<JsonResult> GetDatasOcupadasServico(int id)
        {
            var estadosOcupados = BookingStates.OccupiedStates;
            var datasReservadas = await db.DetalhesReservas
                                          .Where(d => d.ServicoId == id && estadosOcupados.Contains(d.Reserva.Estado))
                                          .Select(d => d.DataInicio) 
                                          .ToListAsync();
            var datasFormatadas = datasReservadas.Select(d => d.Value.ToString("yyyy-MM-dd"));

            return Json(datasFormatadas, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> DetalhesQuarto(int? id, bool? showAllReviews)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quarto quarto = await db.Quartos
                                    .Include(q => q.Imagens)
                                    .Include(q => q.Comodidades)
                                    .Include(q => q.Avaliacoes.Select(a => a.Utilizador))
                                    .Include(q => q.Traducoes)
                                    .FirstOrDefaultAsync(q => q.Id == id);
            if (quarto == null)
            {
                return HttpNotFound();
            }

            var traducao = TraducaoHelper.GetQuartoTrad(quarto, Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);
            ViewBag.NomeTraduzido = traducao.Nome ?? quarto.Nome;
            ViewBag.DescricaoTraduzida = traducao.Descricao ?? quarto.Descricao;

            string cidade = quarto.Localizacao;
            var partesMorada = quarto.Localizacao.Split(',');
            if (partesMorada.Length >= 4)
            {
                cidade = partesMorada[partesMorada.Length - 4].Trim();
            }
            ViewBag.Cidade = cidade;
            ViewBag.ShowAllReviews = showAllReviews ?? false;
            decimal precoFinal = quarto.PrecoPorNoite;
            if (quarto.DescontoPercentagem > 0)
            {
                precoFinal = quarto.PrecoPorNoite * (1 - (quarto.DescontoPercentagem / 100.0m));
            }
            ViewBag.PrecoFinal = precoFinal;
            bool isFavorito = false;
            if (User.Identity.IsAuthenticated)
            {
                var utilizadorId = User.Identity.GetUserId();
                isFavorito = await db.Favoritos.AnyAsync(f => f.QuartoId == id && f.UtilizadorId == utilizadorId);
            }
            ViewBag.IsFavorito = isFavorito;

            return View(quarto);
        }

        public async Task<ActionResult> ResultadosServicos(string localizacao, DateTime? data, string tipoServico)
        {
            var query = db.Servicos.AsQueryable();

            if (!string.IsNullOrEmpty(localizacao))
            {
                query = query.Where(s => s.Localizacao.Contains(localizacao));
            }
            if (!string.IsNullOrEmpty(tipoServico))
            {
                query = query.Where(s => s.Nome.Contains(tipoServico));
            }
            if (data.HasValue)
            {
            }
            var servicosEncontrados = await query.ToListAsync();
            ViewBag.Localizacao = localizacao;
            ViewBag.Data = data;
            ViewBag.TipoServico = tipoServico;
            return View(servicosEncontrados);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult ListaAvaliacoes(int? pagina)
        {
            var query = db.Avaliacoes
                          .Include(a => a.Utilizador)
                          .Include(a => a.Quarto)
                          .Include(a => a.Servico)
                          .Include(a => a.Quarto.Traducoes)
                          .Include(a => a.Servico.Traducoes)
                          .OrderByDescending(a => a.DataAvaliacao);

            int numeroPagina = (pagina ?? 1);
            int tamanhoPagina = 10; 

            var avaliacoesPaginadas = query.ToPagedList(numeroPagina, tamanhoPagina);

            return View(avaliacoesPaginadas);
        }

        private static decimal? ParseDecimalAny(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            raw = raw.Trim().Replace(" ", "");
            decimal d;
            if (decimal.TryParse(raw, System.Globalization.NumberStyles.Number, new System.Globalization.CultureInfo("pt-PT"), out d))
                return d;
            if (decimal.TryParse(raw, System.Globalization.NumberStyles.Number, new System.Globalization.CultureInfo("en-US"), out d))
                return d;
            raw = raw.Replace(',', '.');
            if (decimal.TryParse(raw, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out d))
                return d;
            return null;
        }

        private void PreencherViewBagTradsQuartoParaView()
        {
            var lst = (Session["NovoQuarto_Trads"] as List<QuartoTrad>)
                      ?? (Session["QuartoEmEdicao_Trads"] as List<QuartoTrad>)
                      ?? new List<QuartoTrad>();

            ViewBag.Nome_en = lst.FirstOrDefault(t => t.Cultura == "en")?.Nome;
            ViewBag.Descricao_en = lst.FirstOrDefault(t => t.Cultura == "en")?.Descricao;
            ViewBag.Nome_es = lst.FirstOrDefault(t => t.Cultura == "es")?.Nome;
            ViewBag.Descricao_es = lst.FirstOrDefault(t => t.Cultura == "es")?.Descricao;
            ViewBag.Nome_fr = lst.FirstOrDefault(t => t.Cultura == "fr")?.Nome;
            ViewBag.Descricao_fr = lst.FirstOrDefault(t => t.Cultura == "fr")?.Descricao;
        }

        private void PreencherViewBagTradsServicoParaView()
        {
            var lst = (Session["NovoServico_Trads"] as List<ServicoTrad>)
                      ?? (Session["ServicoEmEdicao_Trads"] as List<ServicoTrad>)
                      ?? new List<ServicoTrad>();

            ViewBag.S_Nome_en = lst.FirstOrDefault(t => t.Cultura == "en")?.Nome;
            ViewBag.S_Descricao_en = lst.FirstOrDefault(t => t.Cultura == "en")?.Descricao;
            ViewBag.S_Nome_es = lst.FirstOrDefault(t => t.Cultura == "es")?.Nome;
            ViewBag.S_Descricao_es = lst.FirstOrDefault(t => t.Cultura == "es")?.Descricao;
            ViewBag.S_Nome_fr = lst.FirstOrDefault(t => t.Cultura == "fr")?.Nome;
            ViewBag.S_Descricao_fr = lst.FirstOrDefault(t => t.Cultura == "fr")?.Descricao;
        }

        public async Task<JsonResult> GetDatasOcupadas(int id)
        {
            var estadosOcupados = BookingStates.OccupiedStates;
            var datasReservadas = await db.DetalhesReservas
                                          .Where(d => d.QuartoId == id && estadosOcupados.Contains(d.Reserva.Estado))
                                          .Select(d => new
                                          {
                                              from = d.DataInicio,
                                              to = d.DataFim
                                          })
                                          .ToListAsync();
            var datasFormatadas = datasReservadas.Select(r => new {
                from = r.from.Value.ToString("yyyy-MM-dd"),
                to = r.to.Value.ToString("yyyy-MM-dd")
            });

            return Json(datasFormatadas, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public async Task<ActionResult> Reservas(string filtro = "atuais")
        {
            var utilizadorId = User.Identity.GetUserId();
            var hoje = DateTime.Today;
            var query = db.Reservas
                          .Include(r => r.DetalhesReserva) 
                          .Where(r => r.UtilizadorId == utilizadorId);

            if (filtro == "passadas")
            {
                query = query.Where(r => r.DetalhesReserva.Any(d => d.DataFim < hoje));
                ViewBag.FiltroAtivo = "passadas";
            }
            else
            {
                query = query.Where(r => r.DetalhesReserva.Any(d => d.DataFim >= hoje));
                ViewBag.FiltroAtivo = "atuais";
            }
            var listaDeReservas = await query
                                        .Include(r => r.DetalhesReserva.Select(d => d.Quarto.Imagens))
                                        .Include(r => r.DetalhesReserva.Select(d => d.Quarto.Traducoes))
                                        .Include(r => r.DetalhesReserva.Select(d => d.Servico.Imagens))
                                        .Include(r => r.DetalhesReserva.Select(d => d.Servico.Traducoes))
                                        .OrderByDescending(r => r.DataCriacao)
                                        .ToListAsync();

            return View(listaDeReservas);
        }
        [Authorize]
        public async Task<ActionResult> Favoritos()
        {
            var utilizadorId = User.Identity.GetUserId();
            var listaDeFavoritos = await db.Favoritos
                                           .Where(f => f.UtilizadorId == utilizadorId)
                                           .Include(f => f.Quarto.Imagens)
                                           .Include(f => f.Quarto.Traducoes)
                                           .Include(f => f.Servico.Traducoes)
                                           .Include(f => f.Servico.Imagens)
                                           .ToListAsync();

            return View(listaDeFavoritos);
        }
        private string LocalizarEstado(string estado)
        {
            if (string.Equals(estado, BookingStates.Confirmed, StringComparison.OrdinalIgnoreCase))
                return ProjetoFim.Resources.Strings.ListaReservas_estado_confirmada;
            if (string.Equals(estado, BookingStates.Pending, StringComparison.OrdinalIgnoreCase))
                return ProjetoFim.Resources.Strings.ListaReservas_estado_pendente;
            if (string.Equals(estado, BookingStates.Cancelled, StringComparison.OrdinalIgnoreCase))
                return ProjetoFim.Resources.Strings.ListaReservas_estado_cancelada;
            if (string.Equals(estado, BookingStates.Completed, StringComparison.OrdinalIgnoreCase))
                return ProjetoFim.Resources.Strings.ListaReservas_estado_concluida;
            if (string.Equals(estado, BookingStates.CancellationRequested, StringComparison.OrdinalIgnoreCase))
                return ProjetoFim.Resources.Strings.ListaReservas_estado_cancelamento_pedido;

            return estado ?? "";
        }
        [Authorize]
        public async Task<ActionResult> Notificacoes()
        {
            var utilizadorId = User.Identity.GetUserId();

            var lista = await db.Notificacoes
                                .Where(n => n.DestinatarioId == utilizadorId)
                                .OrderByDescending(n => n.DataCriacao)
                                .ToListAsync();

            var regexId = new Regex(@"#(?<id>\d{5})");
            var ids = lista.Select(n => regexId.Match(n.Mensagem))
                           .Where(m => m.Success)
                           .Select(m => int.Parse(m.Groups["id"].Value))
                           .Distinct()
                           .ToList();

            var estadosPorId = await db.Reservas
                                       .Where(r => ids.Contains(r.Id))
                                       .Select(r => new { r.Id, r.Estado })
                                       .ToDictionaryAsync(x => x.Id, x => x.Estado);

            foreach (var n in lista)
            {
                var m = regexId.Match(n.Mensagem);
                if (!m.Success) continue;

                var id = int.Parse(m.Groups["id"].Value);
                if (!estadosPorId.TryGetValue(id, out var estadoAtual)) continue;

                var estadoLocal = LocalizarEstado(estadoAtual); 
                                                                
                n.Mensagem = string.Format(
                    ProjetoFim.Resources.Strings.BookingMgmt_Notification_StatusChanged_Format,
                    "#" + id.ToString("D5"),
                    estadoLocal
                );
            }

            return View(lista);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> MarcarComoLida(int id)
        {
            var utilizadorId = User.Identity.GetUserId();

            var notif = await db.Notificacoes
                .FirstOrDefaultAsync(n => n.Id == id && n.DestinatarioId == utilizadorId);

            if (notif == null)
                return Json(new { success = false, message = ProjetoFim.Resources.Strings.Home_Notificacao_NaoEncontrada });

            db.Notificacoes.Remove(notif);

            await db.SaveChangesAsync();
            return Json(new { success = true });
        }

        private List<QuartoTrad> LerTradsQuartoDoRequest(Quarto q)
        {
            var list = new List<QuartoTrad>();

            void Add(string cultura, string nome, string desc)
            {
                if (!string.IsNullOrWhiteSpace(nome))
                {
                    list.Add(new QuartoTrad
                    {
                        QuartoId = q.Id,          
                        Cultura = cultura,
                        Nome = nome,
                        Descricao = string.IsNullOrWhiteSpace(desc) ? null : desc
                    });
                }
            }

            Add("en", Request["Nome_en"], Request["Descricao_en"]);
            Add("es", Request["Nome_es"], Request["Descricao_es"]);
            Add("fr", Request["Nome_fr"], Request["Descricao_fr"]);
            return list;
        }

        private List<ServicoTrad> LerTradsServicoDoRequest(Servico s)
        {
            var list = new List<ServicoTrad>();

            void Add(string cultura, string nome, string desc)
            {
                if (!string.IsNullOrWhiteSpace(nome))
                {
                    list.Add(new ServicoTrad
                    {
                        ServicoId = s.Id,          
                        Cultura = cultura,
                        Nome = nome,
                        Descricao = string.IsNullOrWhiteSpace(desc) ? null : desc
                    });
                }
            }

            Add("en", Request["S_Nome_en"], Request["S_Descricao_en"]);
            Add("es", Request["S_Nome_es"], Request["S_Descricao_es"]);
            Add("fr", Request["S_Nome_fr"], Request["S_Descricao_fr"]);
            return list;
        }



        [Authorize(Roles = "Admin")]
        public async Task<JsonResult> GetCalendarEvents()
        {

            var todosOsDetalhes = await db.DetalhesReservas
                                          .Include(d => d.Quarto)
                                          .Include(d => d.Servico)
                                          .Include(d => d.Reserva) 
                                          .Where(d => d.Reserva.Estado != BookingStates.Cancelled)
                                          .Where(d => d.DataInicio.HasValue)
                                          .Select(d => new
                                          {
                                              NomeItem = d.Quarto != null ? d.Quarto.Nome : d.Servico.Nome,
                                              TipoItem = d.QuartoId != null ? "Quarto" : "Servico",
                                              Inicio = d.DataInicio.Value,
                                              Fim = d.DataFim.Value
                                          })
                                          .ToListAsync();
            var eventosParaCalendario = todosOsDetalhes.Select(reserva => new
            {
                title = reserva.NomeItem,
                start = reserva.Inicio.ToString("yyyy-MM-dd"),
                end = reserva.TipoItem == "Quarto" ? reserva.Fim.ToString("yyyy-MM-dd") : reserva.Inicio.AddDays(1).ToString("yyyy-MM-dd"),
                backgroundColor = reserva.TipoItem == "Quarto" ? "#007bff" : "#28a745",
                borderColor = reserva.TipoItem == "Quarto" ? "#007bff" : "#28a745",
                allDay = true
            });

            return Json(eventosParaCalendario, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Mensagens()
        {
            return View();
        }
        [Authorize]
        public async Task<ActionResult> Perfil()
        {
            var utilizadorId = User.Identity.GetUserId();
            var utilizador = await db.Users
                                        .Include(u => u.Avaliacoes.Select(a => a.Quarto.Imagens))
                                        .Include(u => u.Avaliacoes.Select(a => a.Servico.Imagens))
                                        .FirstOrDefaultAsync(u => u.Id == utilizadorId);
            if (utilizador == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return RedirectToAction("Index");
            }
            return View(utilizador);
        }
        [Authorize]
        public async Task<ActionResult> EditarPerfil()
        {
            var utilizadorId = User.Identity.GetUserId();
            var utilizador = await db.Users.FirstOrDefaultAsync(u => u.Id == utilizadorId);

            if (utilizador == null)
            {
                return HttpNotFound();
            }

            return View(utilizador);
        }
        public async Task<ActionResult> ResultadosPesquisa(string localizacao, DateTime? checkIn, DateTime? checkOut, int? hospedes)
        {
            var query = db.Quartos.Include(q => q.Imagens).AsQueryable();

            if (!string.IsNullOrEmpty(localizacao))
            {
                query = query.Where(q => q.Localizacao.Contains(localizacao));
            }

            if (hospedes.HasValue && hospedes > 0)
            {
                query = query.Where(q => q.NumeroHospedes >= hospedes.Value);
            }

            if (checkIn.HasValue && checkOut.HasValue)
            {

                var estadosOcupados = BookingStates.OccupiedStates;

                query = query.Where(q =>
                    !db.DetalhesReservas.Any(d =>
                        d.QuartoId == q.Id &&
                        estadosOcupados.Contains(d.Reserva.Estado) &&
                        (d.DataInicio < checkOut.Value && d.DataFim > checkIn.Value)
                    )
                );
            }

            var quartosEncontrados = await query.ToListAsync();

            ViewBag.Localizacao = localizacao;
            ViewBag.CheckIn = checkIn;
            ViewBag.CheckOut = checkOut;
            ViewBag.Hospedes = hospedes;

            return View(quartosEncontrados);
        }
        [HttpPost] 
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditarPerfil(ApplicationUser model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); 
            }
            var utilizadorId = User.Identity.GetUserId();
            var utilizadorNaDb = await db.Users.FirstOrDefaultAsync(u => u.Id == utilizadorId);

            if (utilizadorNaDb == null)
            {
                return HttpNotFound();
            }
            utilizadorNaDb.UserName = model.UserName;
            utilizadorNaDb.Email = model.Email;
            utilizadorNaDb.PhoneNumber = model.PhoneNumber;
            utilizadorNaDb.DataDeNascimento = model.DataDeNascimento;
            await db.SaveChangesAsync();
            return RedirectToAction("Perfil");
        }
        public async Task<ActionResult> DetalhesServico(int? id, bool? showAllReviews)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var servico = await db.Servicos
                         .Include(s => s.Imagens)
                         .Include(s => s.Avaliacoes.Select(a => a.Utilizador))
                         .Include(s => s.Traducoes)
                         .FirstOrDefaultAsync(s => s.Id == id);
           
            if (servico == null)
            {
                return HttpNotFound();
            }
            var lang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            var trad = servico.Traducoes?.FirstOrDefault(t => t.Cultura == lang)
                    ?? servico.Traducoes?.FirstOrDefault(t => t.Cultura == "pt"); 
            ViewBag.NomeTraduzido = trad?.Nome ?? servico.Nome;
            ViewBag.DescricaoTraduzida = trad?.Descricao ?? servico.Descricao;
            string cidade = servico.Localizacao;
            var partesMorada = servico.Localizacao.Split(',');
            if (partesMorada.Length >= 4)
            {
                cidade = partesMorada[partesMorada.Length - 4].Trim();
            }
            ViewBag.Cidade = cidade;
            ViewBag.ShowAllReviews = showAllReviews ?? false;
            bool isFavorito = false;
            if (User.Identity.IsAuthenticated)
            {
                var utilizadorId = User.Identity.GetUserId();
                isFavorito = await db.Favoritos.AnyAsync(f => f.ServicoId == id && f.UtilizadorId == utilizadorId);
            }
            ViewBag.IsFavorito = isFavorito;

            return View(servico);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Cancelar(int id)
        {
            var utilizadorId = User.Identity.GetUserId();

            var reserva = await db.Reservas.FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null || reserva.UtilizadorId != utilizadorId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden); 
            }

            reserva.Estado = BookingStates.Cancelled;

            await db.SaveChangesAsync();

            return RedirectToAction("Reservas", "Home");
        }

        [Authorize]
        public async Task<ActionResult> PaginaPagamento(
    int? id,                 
    int? quartoId,
    int? servicoId,
    DateTime dataInicio,
    DateTime? dataFim,       
    int? numHospedes,        
    int? numParticipantes 
)
        {
           
            if (!quartoId.HasValue && !servicoId.HasValue && id.HasValue)
            {
                if (dataFim.HasValue) quartoId = id; 
                else servicoId = id;                   
            }

            var utilizadorId = User.Identity.GetUserId();
            ViewBag.Utilizador = await db.Users.FirstOrDefaultAsync(u => u.Id == utilizadorId);

            if (quartoId.HasValue)
            {
                var quarto = await db.Quartos.Include(q => q.Imagens)
                                             .FirstOrDefaultAsync(q => q.Id == quartoId.Value);
                if (quarto == null) return HttpNotFound();
                if (!dataFim.HasValue) return new HttpStatusCodeResult(400, "dataFim em falta");

                decimal precoPorNoite = quarto.PrecoPorNoite;
                if (quarto.DescontoPercentagem > 0)
                    precoPorNoite = quarto.PrecoPorNoite * (1 - (quarto.DescontoPercentagem / 100m));

                int noites = (dataFim.Value - dataInicio).Days;
                if (noites <= 0) noites = 1;

                ViewBag.Quarto = quarto;
                ViewBag.Servico = null;
                ViewBag.DataInicio = dataInicio;
                ViewBag.DataFim = dataFim.Value;
                ViewBag.NumHospedes = numHospedes ?? 1;
                ViewBag.NumeroNoites = noites;
                ViewBag.PrecoTotal = noites * precoPorNoite;
                ViewBag.PrecoUnitario = precoPorNoite;

                return View("PaginaPagamento");
            }

            if (servicoId.HasValue)
            {
                var servico = await db.Servicos.Include(s => s.Imagens)
                                               .FirstOrDefaultAsync(s => s.Id == servicoId.Value);
                if (servico == null) return HttpNotFound();

                decimal precoUnit = servico.Preco;
                if (servico.DescontoPercentagem > 0)
                    precoUnit = servico.Preco * (1 - (servico.DescontoPercentagem / 100m));

                int qtd = Math.Max(1, numParticipantes ?? 1);

                ViewBag.Quarto = null;
                ViewBag.Servico = servico;
                ViewBag.DataInicio = dataInicio;
                ViewBag.DataFim = dataInicio;
                ViewBag.NumParticipantes = qtd;
                ViewBag.NumeroNoites = 1;
                ViewBag.PrecoUnitario = precoUnit;    
                ViewBag.PrecoTotal = qtd * precoUnit;

                return View("PaginaPagamento");
            }

            return new HttpStatusCodeResult(400, "quartoId ou servicoId em falta");
        }


        [Authorize] 
        public async Task<ActionResult> MetodosPagamento()
        {

            var utilizadorId = User.Identity.GetUserId();

            var utilizador = await db.Users.FirstOrDefaultAsync(u => u.Id == utilizadorId);

            if (utilizador == null)
            {
                return HttpNotFound();
            }
            return View(utilizador);
        }
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Dashboard()
        {
            var hoje = DateTime.Today;
            var primeiroDiaDoMes = new DateTime(hoje.Year, hoje.Month, 1);
            var ultimoDiaDoMes = primeiroDiaDoMes.AddMonths(1).AddDays(-1);
            var adminId = User.Identity.GetUserId();
            int reservasPendentes = await db.Reservas.CountAsync(r => r.Estado == BookingStates.Pending);

            int checkInsHoje = await db.DetalhesReservas.CountAsync(d =>
                d.DataInicio.HasValue &&
                DbFunctions.TruncateTime(d.DataInicio.Value) == hoje &&
                d.Reserva.Estado == BookingStates.Cancelled
            );

            decimal ganhosMes = await db.Reservas
                                        .Where(r => r.DataCriacao >= primeiroDiaDoMes &&
                                                    r.DataCriacao <= ultimoDiaDoMes &&
                                                    r.Estado == BookingStates.Completed) 
                                        .Select(r => (decimal?)r.ValorTotal)
                                        .SumAsync() ?? 0;

            int mensagensNaoLidas = await db.Conversas
                                            .CountAsync(c => c.Mensagens.Any(m => !m.Lida && m.RemetenteId != adminId));
            var atividades = await db.Reservas
                                     .Include(r => r.Utilizador)
                                     .Include(r => r.DetalhesReserva.Select(d => d.Quarto))
                                     .Include(r => r.DetalhesReserva.Select(d => d.Servico))
                                     .Include(r => r.DetalhesReserva.Select(d => d.Quarto.Traducoes))
                                     .Include(r => r.DetalhesReserva.Select(d => d.Servico.Traducoes))
                                     .OrderByDescending(r => r.DataCriacao)
                                     .Take(5)
                                     .ToListAsync();
            var viewModel = new DashboardViewModel
            {
                ReservasPendentes = reservasPendentes,
                CheckInsParaHoje = checkInsHoje,
                GanhosDoMes = ganhosMes,
                MensagensNaoLidas = mensagensNaoLidas,
                AtividadeRecente = atividades
            };

            return View(viewModel);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult GerirAnuncios(int? paginaQuartos, int? paginaServicos)
        {
            int numPaginaQuartos = paginaQuartos ?? 1;
            int numPaginaServicos = paginaServicos ?? 1;
            int tamanhoPagina = 6;
            var viewModel = new AnunciosViewModel
            {
                Quartos = db.Quartos.Include(q => q.Imagens).OrderBy(q => q.Id).ToPagedList(numPaginaQuartos, tamanhoPagina),
                Servicos = db.Servicos.Include(s => s.Imagens).OrderBy(s => s.Id).ToPagedList(numPaginaServicos, tamanhoPagina)
            };
            return View(viewModel);
        }
        private string NormalizarEstado(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return "ALL";

            switch (valor)
            {
                case BookingStates.Pending:
                case BookingStates.Confirmed:
                case BookingStates.Cancelled:
                case BookingStates.CancellationRequested:
                case BookingStates.Completed:
                case "ALL":
                    return valor;
            }

            if (valor == ProjetoFim.Resources.Strings.ListaReservas_estado_pendente) return BookingStates.Pending;
            if (valor == ProjetoFim.Resources.Strings.ListaReservas_estado_confirmada) return BookingStates.Confirmed;
            if (valor == ProjetoFim.Resources.Strings.ListaReservas_estado_cancelada) return BookingStates.Cancelled;
            if (valor == ProjetoFim.Resources.Strings.ListaReservas_estado_cancelamento_pedido) return BookingStates.CancellationRequested;
            if (valor == ProjetoFim.Resources.Strings.ListaReservas_estado_concluida) return BookingStates.Completed;


            return "ALL";
        }
        [Authorize(Roles = "Admin")]
        private async Task<int> LimparReservasOrfasAsync()
        {

            var detalhesInvalidos = await db.DetalhesReservas
                .Where(d =>
                    (d.QuartoId != null && !db.Quartos.Any(q => q.Id == d.QuartoId)) ||
                    (d.ServicoId != null && !db.Servicos.Any(s => s.Id == d.ServicoId)))
                .ToListAsync();

            var reservaIdsPossivelmenteOrfas = detalhesInvalidos
                .Select(d => d.ReservaId)
                .Distinct()
                .ToList();

            if (detalhesInvalidos.Any())
            {
                db.DetalhesReservas.RemoveRange(detalhesInvalidos);
                await db.SaveChangesAsync();
            }

            var reservasOrfas = await db.Reservas
                .Where(r => !r.DetalhesReserva.Any() || reservaIdsPossivelmenteOrfas.Contains(r.Id))
                .ToListAsync();

            if (!reservasOrfas.Any()) return 0;

            var idsOrfas = reservasOrfas.Select(r => r.Id).ToList();

            var convIds = await db.Conversas
                .Where(c => c.ReservaId.HasValue && idsOrfas.Contains(c.ReservaId.Value))
                .Select(c => c.Id)
                .ToListAsync();

            if (convIds.Any())
            {
                db.Mensagens.RemoveRange(db.Mensagens.Where(m => convIds.Contains(m.ConversaId)));
                db.Conversas.RemoveRange(db.Conversas.Where(c => convIds.Contains(c.Id)));
                await db.SaveChangesAsync();
            }

            db.Reservas.RemoveRange(reservasOrfas);
            return await db.SaveChangesAsync();
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ListaReservas(string pesquisa, string estado, DateTime? data, int? pagina)
        {
            await LimparReservasOrfasAsync();  

            var query = db.Reservas
                          .Include(r => r.Utilizador)
                          .Include(r => r.DetalhesReserva.Select(d => d.Quarto))
                          .Include(r => r.DetalhesReserva.Select(d => d.Servico))
                          .Include(r => r.DetalhesReserva.Select(d => d.Quarto.Traducoes))
                          .Include(r => r.DetalhesReserva.Select(d => d.Servico.Traducoes))
                          .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                query = query.Where(r =>
                    r.Utilizador.UserName.Contains(pesquisa) ||
                    r.DetalhesReserva.Any(d => d.Quarto != null && d.Quarto.Nome.Contains(pesquisa)) ||
                    r.DetalhesReserva.Any(d => d.Servico != null && d.Servico.Nome.Contains(pesquisa)));
            }

            string estadoCanonico = NormalizarEstado(estado);
            if (!string.IsNullOrEmpty(estadoCanonico) && estadoCanonico != "ALL")
                query = query.Where(r => r.Estado == estadoCanonico);

            ViewBag.EstadoAtual = estadoCanonico ?? "ALL";

            if (data.HasValue)
            {
                var dia = data.Value.Date;
                query = query.Where(r =>
                    r.DetalhesReserva.Any(d => DbFunctions.TruncateTime(d.DataInicio) <= dia &&
                                               DbFunctions.TruncateTime(d.DataFim) >= dia));
            }

            int numeroPagina = pagina ?? 1;
            int tamanhoPagina = 15;

            ViewBag.PesquisaAtual = pesquisa;
            ViewBag.DataAtual = data;

            var reservasPaginadas = query
                .OrderByDescending(r => r.DataCriacao)
                .ToPagedList(numeroPagina, tamanhoPagina);  

            return View(reservasPaginadas);
        }


        public ActionResult Calendario()
        {
            return View();
        }
        public ActionResult CaixaDeEntrada()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Estatisticas()
        {
            ViewBag.TotalGanhos = await db.Reservas.Where(r => r.Estado == BookingStates.Completed).SumAsync(r => (decimal?)r.ValorTotal) ?? 0;
            ViewBag.TotalReservas = await db.Reservas.CountAsync();
            ViewBag.MediaPorReserva = await db.Reservas.Where(r => r.Estado == BookingStates.Completed).AverageAsync(r => (decimal?)r.ValorTotal) ?? 0;
            var ganhosMensais = new List<decimal>();
            var labelsMeses = new List<string>();
            for (int i = 5; i >= 0; i--)
            {
                var mesTarget = DateTime.Now.AddMonths(-i);
                var primeiroDia = new DateTime(mesTarget.Year, mesTarget.Month, 1);
                var ultimoDia = primeiroDia.AddMonths(1).AddDays(-1);

                decimal ganho = await db.Reservas
                                      .Where(r => r.DataCriacao >= primeiroDia && r.DataCriacao <= ultimoDia && r.Estado == BookingStates.Completed)
                                      .SumAsync(r => (decimal?)r.ValorTotal) ?? 0;

                ganhosMensais.Add(ganho);
                labelsMeses.Add(primeiroDia.ToString("MMM/yy"));
            }
            ViewBag.GanhosMensaisData = ganhosMensais;
            ViewBag.LabelsMeses = labelsMeses;

            return View();
        }
        [Authorize] 
        public async Task<ActionResult> HistoricoTransacoes(int? pagina)
        {
            var utilizadorId = User.Identity.GetUserId();
            var query = db.Reservas
                          .Include(r => r.Utilizador)
                          .Include(r => r.DetalhesReserva.Select(d => d.Quarto))
                            .Include(r => r.DetalhesReserva.Select(d => d.Quarto.Traducoes))
                          .Include(r => r.DetalhesReserva.Select(d => d.Servico))
                          .Include(r => r.DetalhesReserva.Select(d => d.Servico.Traducoes))
                          .AsQueryable();
            if (!User.IsInRole("Admin"))
            {
                query = query.Where(r => r.UtilizadorId == utilizadorId);
            }

            int numeroPagina = (pagina ?? 1);
            int tamanhoPagina = 15;
            var transacoesPaginadas = query.OrderByDescending(r => r.DataCriacao)
                                  .ToPagedList(numeroPagina, tamanhoPagina);

            return View(transacoesPaginadas);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult AdicionarQuarto_Passo1()
        {
            var quarto = Session["NovoQuarto"] as Quarto ?? new Quarto();
            var trads = (Session["NovoQuarto_Trads"] as List<QuartoTrad>) ?? new List<QuartoTrad>();
            ViewBag.Nome_en = trads.FirstOrDefault(t => t.Cultura == "en")?.Nome;
            ViewBag.Desc_en = trads.FirstOrDefault(t => t.Cultura == "en")?.Descricao;
            ViewBag.Nome_es = trads.FirstOrDefault(t => t.Cultura == "es")?.Nome;
            ViewBag.Desc_es = trads.FirstOrDefault(t => t.Cultura == "es")?.Descricao;
            ViewBag.Nome_fr = trads.FirstOrDefault(t => t.Cultura == "fr")?.Nome;
            ViewBag.Desc_fr = trads.FirstOrDefault(t => t.Cultura == "fr")?.Descricao;
            return View(quarto);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult AdicionarQuarto_Passo1(Quarto quarto)
        {
            ModelState.Remove("PrecoPorNoite");
            ModelState.Remove("NumeroHospedes");
            ModelState.Remove("NumeroCamasCasal");
            ModelState.Remove("NumeroCamasSolteiro");
            ModelState.Remove("NumeroCasasDeBanho");
            ModelState.Remove("TemEstacionamento");
            ModelState.Remove("DescontoPercentagem");
            ModelState.Remove("Latitude");
            ModelState.Remove("Longitude");

            var latStr = Request["Latitude"];
            var lonStr = Request["Longitude"];
            if (quarto.Latitude == null && !string.IsNullOrWhiteSpace(latStr))
            {
                if (double.TryParse(latStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var lat))
                {
                    quarto.Latitude = lat;
                }
            }
            if (quarto.Longitude == null && !string.IsNullOrWhiteSpace(lonStr))
            {
                if (double.TryParse(lonStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var lon))
                {
                    quarto.Longitude = lon;
                }
            }

            if (string.IsNullOrWhiteSpace(quarto.Nome))
                ModelState.AddModelError("Nome", ProjetoFim.Resources.Strings.Home_Validacao_NomeObrigatorio);
            if (string.IsNullOrWhiteSpace(quarto.Localizacao))
                ModelState.AddModelError("Localizacao", ProjetoFim.Resources.Strings.Home_Validacao_LocalizacaoObrigatoria);

            if (!ModelState.IsValid)
            {
                ViewBag.Nome_en = Request["Nome_en"];
                ViewBag.Descricao_en = Request["Descricao_en"];
                ViewBag.Nome_es = Request["Nome_es"];
                ViewBag.Descricao_es = Request["Descricao_es"];
                ViewBag.Nome_fr = Request["Nome_fr"];
                ViewBag.Descricao_fr = Request["Descricao_fr"];
                return View(quarto);
            }

            var qSessao = Session["NovoQuarto"] as Quarto ?? new Quarto();
            qSessao.Nome = quarto.Nome;
            qSessao.Descricao = quarto.Descricao;
            qSessao.Localizacao = quarto.Localizacao;
            qSessao.Latitude = quarto.Latitude;
            qSessao.Longitude = quarto.Longitude;

            qSessao.Traducoes = LerTradsQuartoDoRequest(qSessao);
            var tradsQ = qSessao.Traducoes ?? new List<QuartoTrad>();
            Session["NovoQuarto_Trads"] = tradsQ; 

            Session["NovoQuarto"] = qSessao;

            return RedirectToAction("AdicionarQuarto_Passo2");
        }


        [Authorize(Roles = "Admin")]
        public ActionResult AdicionarQuarto_Passo2()
        {
            var quarto = Session["NovoQuarto"] as Quarto;
            if (quarto == null)
            {
                return RedirectToAction("AdicionarQuarto_Passo1");
            }
            return View(quarto);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AdicionarQuarto_Passo2(Quarto quarto)
        {
            var quartoDaSessao = Session["NovoQuarto"] as Quarto;
            if (quartoDaSessao == null)
            {
                return RedirectToAction("AdicionarQuarto_Passo1");
            }
            quartoDaSessao.NumeroCamasCasal = quarto.NumeroCamasCasal;
            quartoDaSessao.NumeroCamasSolteiro = quarto.NumeroCamasSolteiro;
            quartoDaSessao.NumeroCasasDeBanho = quarto.NumeroCasasDeBanho;
            quartoDaSessao.TemEstacionamento = quarto.TemEstacionamento;
            quartoDaSessao.NumeroHospedes = quarto.NumeroHospedes;

            Session["NovoQuarto"] = quartoDaSessao;
            return RedirectToAction("AdicionarQuarto_Passo3");
        }
        [Authorize(Roles = "Admin")]
        public ActionResult AdicionarQuarto_Passo3()
        {
            var quarto = Session["NovoQuarto"] as Quarto;
            if (quarto == null) return RedirectToAction("AdicionarQuarto_Passo1");

            ViewBag.TodasAsComodidades = db.Comodidades.ToList();
          
            var selecionadas = (Session["ComodidadesSelecionadasIDs"] as int[]) ??
                               quarto.Comodidades?.Select(c => c.Id).ToArray() ??
                               Array.Empty<int>();
            ViewBag.ComodidadesSelecionadas = new HashSet<int>(selecionadas);

            return View(quarto);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AdicionarQuarto_Passo3(int[] comodidadesSelecionadas)
        {
            var quarto = Session["NovoQuarto"] as Quarto;
            if (quarto == null)
            {
                return RedirectToAction("AdicionarQuarto_Passo1");
            }
            Session["ComodidadesSelecionadasIDs"] = comodidadesSelecionadas;
            return RedirectToAction("AdicionarQuarto_Passo4");
        }
        [Authorize(Roles = "Admin")]
        public ActionResult AdicionarQuarto_Passo4()
        {
            var quarto = Session["NovoQuarto"] as Quarto;
            if (quarto == null)
            {
                return RedirectToAction("AdicionarQuarto_Passo1");
            }
            return View(quarto);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AdicionarQuarto_Passo4(IEnumerable<HttpPostedFileBase> files)
        {
            var quarto = Session["NovoQuarto"] as Quarto;
            if (quarto == null) return RedirectToAction("AdicionarQuarto_Passo1");

            if (quarto.Imagens == null) quarto.Imagens = new List<Imagem>();

            if (files != null && files.Any())
            {
                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                        var extension = Path.GetExtension(file.FileName);
                        fileName = $"{fileName}_{Guid.NewGuid()}{extension}";

                        var uploadsDir = Server.MapPath("~/Uploads/Imagens/");
                        Directory.CreateDirectory(uploadsDir);
                        var path = Path.Combine(uploadsDir, fileName);
                        file.SaveAs(path);

                        quarto.Imagens.Add(new Imagem { Url = "/Uploads/Imagens/" + fileName });
                    }
                }
            }

            Session["NovoQuarto"] = quarto;
            return RedirectToAction("AdicionarQuarto_Passo5");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdicionarQuarto_Passo5()
        {
            var quarto = Session["NovoQuarto"] as Quarto;
            if (quarto == null) return RedirectToAction("AdicionarQuarto_Passo1");

            var token = Guid.NewGuid().ToString("N");
            Session["NovoQuarto_Token"] = token;
            ViewBag.FormToken = token;

            return View(quarto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AdicionarQuarto_Passo5(Quarto form)
        {
            var quartoDaSessao = Session["NovoQuarto"] as Quarto;
            if (quartoDaSessao == null) return RedirectToAction("AdicionarQuarto_Passo1");

            var postToken = Request["FormToken"];
            var sessToken = Session["NovoQuarto_Token"] as string;
            if (string.IsNullOrEmpty(postToken) || !string.Equals(postToken, sessToken, StringComparison.Ordinal))
                return RedirectToAction("GerirAnuncios"); 

            var precoRaw = Request["PrecoPorNoite"];
            var preco = ParseDecimalAny(precoRaw);
            if (preco == null || preco <= 0)
            {
                ModelState.AddModelError("PrecoPorNoite", ProjetoFim.Resources.Strings.Home_Validacao_PrecoMaiorZero);
                ViewBag.FormToken = sessToken;
                return View(quartoDaSessao);
            }
            quartoDaSessao.PrecoPorNoite = preco.Value;

            var descRaw = Request["DescontoPercentagem"];
            var desc = ParseDecimalAny(descRaw);
            var descInt = desc.HasValue ? (int)Math.Round(desc.Value, MidpointRounding.AwayFromZero) : 0;
            quartoDaSessao.DescontoPercentagem = descInt;

            var comodidadesIDs = Session["ComodidadesSelecionadasIDs"] as int[];
            var trads = Session["NovoQuarto_Trads"] as List<QuartoTrad> ?? new List<QuartoTrad>();

            using (var db = new ApplicationDbContext())
            using (var tx = db.Database.BeginTransaction())
            {
                quartoDaSessao.Comodidades = new List<Comodidade>();
                if (comodidadesIDs != null)
                {
                    foreach (var id in comodidadesIDs)
                    {
                        var comodidade = db.Comodidades.Find(id);
                        if (comodidade != null) quartoDaSessao.Comodidades.Add(comodidade);
                    }
                }

                db.Quartos.Add(quartoDaSessao);
                db.SaveChanges();

                if (trads.Count > 0)
                {
                    var culturas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var t in trads)
                    {
                        if (t == null || string.IsNullOrWhiteSpace(t.Nome)) continue;
                        if (!culturas.Add(t.Cultura ?? "")) continue;
                        t.QuartoId = quartoDaSessao.Id;
                        db.QuartosTrad.Add(t);
                    }
                    db.SaveChanges();
                }

                tx.Commit();
            }

            Session.Remove("NovoQuarto_Token");
            Session.Remove("NovoQuarto");
            Session.Remove("ComodidadesSelecionadasIDs");
            Session.Remove("NovoQuarto_Trads");

            return RedirectToAction("GerirAnuncios", "Home");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ModificarQuarto_Passo1(int id)
        {
            var db = new ApplicationDbContext();
            var quartoParaEditar = db.Quartos
                                     .Include(q => q.Comodidades)
                                     .Include(q => q.Imagens)
                                     .Include(q => q.Traducoes)
                                     .FirstOrDefault(q => q.Id == id);

            if (quartoParaEditar == null)
            {
                return HttpNotFound();
            }
            Session.Remove("QuartoEmEdicao");
            Session["QuartoEmEdicao"] = quartoParaEditar;

            if (Session["QuartoEmEdicao_Trads"] == null)
                Session["QuartoEmEdicao_Trads"] = (quartoParaEditar.Traducoes ?? new List<QuartoTrad>()).ToList();

            PreencherViewBagTradsQuartoParaView();
            return View("AdicionarQuarto_Passo1", quartoParaEditar);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ModificarQuarto_Passo1(int id, Quarto quarto)
        {
            var quartoDaSessao = Session["QuartoEmEdicao"] as Quarto;
            if (quartoDaSessao == null) return RedirectToAction("GerirAnuncios");

            quartoDaSessao.Nome = quarto.Nome;
            quartoDaSessao.Descricao = quarto.Descricao;
            quartoDaSessao.Localizacao = quarto.Localizacao;
            quartoDaSessao.Latitude = quarto.Latitude;
            quartoDaSessao.Longitude = quarto.Longitude;

            quartoDaSessao.Traducoes = LerTradsQuartoDoRequest(quartoDaSessao);
            var tradsEdit = quartoDaSessao.Traducoes ?? new List<QuartoTrad>();
            Session["QuartoEmEdicao_Trads"] = tradsEdit; 

            Session["QuartoEmEdicao"] = quartoDaSessao;

            return RedirectToAction("ModificarQuarto_Passo2", new { id = id });
        }


        [Authorize(Roles = "Admin")]
        public ActionResult ModificarQuarto_Passo2(int id)
        {
            var quarto = Session["QuartoEmEdicao"] as Quarto;
            if (quarto == null || quarto.Id != id) return RedirectToAction("GerirAnuncios");

            return View("AdicionarQuarto_Passo2", quarto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ModificarQuarto_Passo2(int id, Quarto quarto)
        {
            var quartoDaSessao = Session["QuartoEmEdicao"] as Quarto;
            if (quartoDaSessao == null) return RedirectToAction("GerirAnuncios");

            quartoDaSessao.NumeroCamasCasal = quarto.NumeroCamasCasal;
            quartoDaSessao.NumeroCamasSolteiro = quarto.NumeroCamasSolteiro;
            quartoDaSessao.NumeroCasasDeBanho = quarto.NumeroCasasDeBanho;
            quartoDaSessao.TemEstacionamento = quarto.TemEstacionamento;
            quartoDaSessao.NumeroHospedes = quarto.NumeroHospedes;

            Session["QuartoEmEdicao"] = quartoDaSessao;

            return RedirectToAction("ModificarQuarto_Passo3", new { id = id });
        }
        [Authorize(Roles = "Admin")]
        public ActionResult ModificarQuarto_Passo3(int id)
        {
            var quarto = Session["QuartoEmEdicao"] as Quarto;
            if (quarto == null) return RedirectToAction("GerirAnuncios");
            var db = new ApplicationDbContext();
            ViewBag.TodasAsComodidades = db.Comodidades.ToList();
            return View("AdicionarQuarto_Passo3", quarto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ModificarQuarto_Passo3(int id, int[] comodidadesSelecionadas)
        {
            var quartoParaEditar = Session["QuartoEmEdicao"] as Quarto;
            if (quartoParaEditar == null || quartoParaEditar.Id != id)
            {
                return RedirectToAction("GerirAnuncios");
            }
            Session["ComodidadesSelecionadasIDs"] = comodidadesSelecionadas;
            return RedirectToAction("ModificarQuarto_Passo4", new { id = quartoParaEditar.Id });
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ModificarQuarto_Passo4(int id)
        {
            var quarto = Session["QuartoEmEdicao"] as Quarto;
            if (quarto == null) return RedirectToAction("GerirAnuncios");

            return View("AdicionarQuarto_Passo4", quarto);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ModificarQuarto_Passo4(int id, IEnumerable<HttpPostedFileBase> files, int[] imagensParaApagar, string ordemDasImagens)
        {
            Session["ImagensParaApagarIDs"] = imagensParaApagar;
            Session["OrdemDasImagens"] = ordemDasImagens; 
            if (files != null && files.Any(f => f != null))
            {
                var nomesFicheirosTemporarios = new List<string>();
                var pastaTemporaria = Server.MapPath("~/Uploads/Temp/");
                Directory.CreateDirectory(pastaTemporaria);

                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        var nomeFicheiro = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var caminho = Path.Combine(pastaTemporaria, nomeFicheiro);
                        file.SaveAs(caminho);
                        nomesFicheirosTemporarios.Add(nomeFicheiro);
                    }
                }
                Session["NovasImagensTemp"] = nomesFicheirosTemporarios;
            }

            return RedirectToAction("ModificarQuarto_Passo5", new { id = id });
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult ModificarQuarto_Passo5(int id)
        {
     
            var quartoEmEdicao = Session["QuartoEmEdicao"] as Quarto;
            if (quartoEmEdicao != null && quartoEmEdicao.Id == id)
            {
                return View("AdicionarQuarto_Passo5", quartoEmEdicao);
            }

            using (var db = new ApplicationDbContext())
            {
                var quartoDb = db.Quartos
                    .Include(q => q.Comodidades)
                    .Include(q => q.Imagens)
                    .FirstOrDefault(q => q.Id == id);

                if (quartoDb == null)
                    return RedirectToAction("GerirAnuncios");

                Session["QuartoEmEdicao"] = quartoDb;
                return View("AdicionarQuarto_Passo5", quartoDb);
            }
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ModificarQuarto_Passo5(int id, Quarto quartoDoFormulario)
        {
            var quartoComAlteracoes = Session["QuartoEmEdicao"] as Quarto;
            if (quartoComAlteracoes == null || quartoComAlteracoes.Id != id)
            {
                return RedirectToAction("GerirAnuncios");
            }

            var precoRaw = Request["PrecoPorNoite"];
            var preco = ParseDecimalAny(precoRaw);
            if (preco == null || preco <= 0)
            {
                ModelState.AddModelError("PrecoPorNoite", ProjetoFim.Resources.Strings.Home_Validacao_PrecoMaiorZero);
                return View("AdicionarQuarto_Passo5", quartoComAlteracoes);
            }

            using (var db = new ApplicationDbContext())
            {
                var quartoParaAtualizar = db.Quartos
                    .Include(q => q.Comodidades).Include(q => q.Imagens).Include(q => q.Traducoes)
                    .FirstOrDefault(q => q.Id == id);
                if (quartoParaAtualizar == null)
                {
                    return HttpNotFound();
                }
                quartoParaAtualizar.Nome = quartoComAlteracoes.Nome;
                quartoParaAtualizar.Descricao = quartoComAlteracoes.Descricao;
                quartoParaAtualizar.Localizacao = quartoComAlteracoes.Localizacao;
                quartoParaAtualizar.PrecoPorNoite = preco.Value;

                var descRaw = Request["DescontoPercentagem"];
                var desc = ParseDecimalAny(descRaw);
                var descInt = desc.HasValue ? (int)Math.Round(desc.Value, MidpointRounding.AwayFromZero) : 0;
                quartoParaAtualizar.DescontoPercentagem = descInt;

                quartoParaAtualizar.NumeroCamasCasal = quartoComAlteracoes.NumeroCamasCasal;
                quartoParaAtualizar.NumeroCamasSolteiro = quartoComAlteracoes.NumeroCamasSolteiro;
                quartoParaAtualizar.NumeroCasasDeBanho = quartoComAlteracoes.NumeroCasasDeBanho;
                quartoParaAtualizar.TemEstacionamento = quartoComAlteracoes.TemEstacionamento;
                quartoParaAtualizar.NumeroHospedes = quartoComAlteracoes.NumeroHospedes;

                var comodidadesIDs = Session["ComodidadesSelecionadasIDs"] as int[];
                quartoParaAtualizar.Comodidades.Clear();
                if (comodidadesIDs != null)
                {
                    foreach (var comId in comodidadesIDs)
                    {
                        var comodidade = db.Comodidades.Find(comId);
                        if (comodidade != null) quartoParaAtualizar.Comodidades.Add(comodidade);
                    }
                }
                var imagensParaApagarIDs = Session["ImagensParaApagarIDs"] as int[];
                if (imagensParaApagarIDs != null && imagensParaApagarIDs.Any())
                {
                    foreach (var imagemId in imagensParaApagarIDs)
                    {
                        var imagemParaApagar = db.Imagens.Find(imagemId);
                        if (imagemParaApagar != null)
                        {
                            var caminhoFisico = Server.MapPath(imagemParaApagar.Url);
                            if (System.IO.File.Exists(caminhoFisico))
                            {
                                System.IO.File.Delete(caminhoFisico);
                            }
                            db.Imagens.Remove(imagemParaApagar);
                        }
                    }
                }
                var nomesFicheirosTemporarios = Session["NovasImagensTemp"] as List<string>;
                if (nomesFicheirosTemporarios != null)
                {
                    var pastaTemporaria = Server.MapPath("~/Uploads/Temp/");
                    var pastaFinal = Server.MapPath("~/Uploads/Imagens/");
                    Directory.CreateDirectory(pastaFinal);

                    foreach (var nomeFicheiroTemp in nomesFicheirosTemporarios)
                    {
                        var caminhoOrigem = Path.Combine(pastaTemporaria, nomeFicheiroTemp);
                        var caminhoDestino = Path.Combine(pastaFinal, nomeFicheiroTemp);
                        if (System.IO.File.Exists(caminhoOrigem))
                        {
                            System.IO.File.Move(caminhoOrigem, caminhoDestino);
                            quartoParaAtualizar.Imagens.Add(new Imagem { Url = "/Uploads/Imagens/" + nomeFicheiroTemp });
                        }
                    }
                }
                var ordemDasImagens = Session["OrdemDasImagens"] as string;
                if (!string.IsNullOrEmpty(ordemDasImagens))
                {
                    var idsOrdenados = ordemDasImagens.Split(',').Select(int.Parse).ToList();
                    for (int i = 0; i < idsOrdenados.Count; i++)
                    {
                        var imagemId = idsOrdenados[i];
                        var imagem = db.Imagens.Find(imagemId);
                        if (imagem != null)
                        {
                            imagem.Ordem = i;
                        }
                    }
                }
                db.SaveChanges();

                var trads = Session["QuartoEmEdicao_Trads"] as List<QuartoTrad>;
                if (trads != null)
                {
                    db.QuartosTrad.RemoveRange(quartoParaAtualizar.Traducoes);
                    db.SaveChanges();

                    var culturas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var t in trads)
                    {
                        if (string.IsNullOrWhiteSpace(t?.Nome)) continue;
                        if (!culturas.Add(t.Cultura ?? "")) continue;

                        t.QuartoId = id;
                        db.QuartosTrad.Add(t);
                    }
                    db.SaveChanges();
                }
            }
            Session.Remove("QuartoEmEdicao");
            Session.Remove("ComodidadesSelecionadasIDs");
            Session.Remove("NovasImagensTemp");
            Session.Remove("ImagensParaApagarIDs");
            Session.Remove("OrdemDasImagens");
            Session.Remove("QuartoEmEdicao_Trads");
            return RedirectToAction("GerirAnuncios");
        }

        private static readonly string[] EstadosAtivos = { "Pendente", "Confirmada" };
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult RemoverQuarto(int id)
        {
            using (var db = new ApplicationDbContext())
            using (var tx = db.Database.BeginTransaction())
            {
                var quarto = db.Quartos
                    .Include(q => q.Imagens)
                    .Include(q => q.Comodidades)
                    .FirstOrDefault(q => q.Id == id);

                if (quarto == null) return RedirectToAction("GerirAnuncios");


                bool temReservaAtiva = db.DetalhesReservas
                    .Any(d => d.QuartoId == id && EstadosAtivos.Contains(d.Reserva.Estado));
                if (temReservaAtiva)
                {
                    TempData["Erro"] = "Existem reservas ativas associadas a este quarto. Cancele/termine antes de remover.";
                    return RedirectToAction("GerirAnuncios");
                }

                var reservasIds = db.DetalhesReservas
                    .Where(d => d.QuartoId == id)
                    .Select(d => d.ReservaId)
                    .Distinct()
                    .ToList();

                var convIdsQuarto = db.Conversas.Where(c => c.QuartoId == id).Select(c => c.Id);
                var convIdsReserva = db.Conversas.Where(c => c.ReservaId.HasValue && reservasIds.Contains(c.ReservaId.Value))
                                                 .Select(c => c.Id);
                var convIds = convIdsQuarto.Concat(convIdsReserva).Distinct().ToList();

                db.Mensagens.RemoveRange(db.Mensagens.Where(m => convIds.Contains(m.ConversaId)));
                db.Conversas.RemoveRange(db.Conversas.Where(c => convIds.Contains(c.Id)));

                var detalhes = db.DetalhesReservas.Where(d => d.QuartoId == id).ToList();
                db.DetalhesReservas.RemoveRange(detalhes);

                var reservasOrfas = db.Reservas.Where(r => reservasIds.Contains(r.Id) && !r.DetalhesReserva.Any());
                db.Reservas.RemoveRange(reservasOrfas);

                db.Avaliacoes.RemoveRange(db.Avaliacoes.Where(a => a.QuartoId == id));
                db.Favoritos.RemoveRange(db.Favoritos.Where(f => f.QuartoId == id));

                foreach (var img in quarto.Imagens.ToList())
                {
                    var path = Server.MapPath(img.Url);
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                }
                db.Imagens.RemoveRange(db.Imagens.Where(i => i.QuartoId == id));

                quarto.Comodidades.Clear(); 

                db.Quartos.Remove(quarto);

                db.SaveChanges();
                tx.Commit();
            }

            return RedirectToAction("GerirAnuncios");
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult RemoverServico(int id)
        {
            using (var db = new ApplicationDbContext())
            using (var tx = db.Database.BeginTransaction())
            {
                var servico = db.Servicos
                    .Include(s => s.Imagens)
                    .FirstOrDefault(s => s.Id == id);

                if (servico == null) return RedirectToAction("GerirAnuncios");

  
                bool temReservaAtiva = db.DetalhesReservas
                    .Any(d => d.ServicoId == id && EstadosAtivos.Contains(d.Reserva.Estado));
                if (temReservaAtiva)
                {
                    TempData["Erro"] = "Existem reservas ativas associadas a este serviço. Cancele/termine antes de remover.";
                    return RedirectToAction("GerirAnuncios");
                }

                var reservasIds = db.DetalhesReservas
                    .Where(d => d.ServicoId == id)
                    .Select(d => d.ReservaId)
                    .Distinct()
                    .ToList();


                var convIdsServico = db.Conversas.Where(c => c.ServicoId == id).Select(c => c.Id);
                var convIdsReserva = db.Conversas.Where(c => c.ReservaId.HasValue && reservasIds.Contains(c.ReservaId.Value))
                                                 .Select(c => c.Id);
                var convIds = convIdsServico.Concat(convIdsReserva).Distinct().ToList();

                db.Mensagens.RemoveRange(db.Mensagens.Where(m => convIds.Contains(m.ConversaId)));
                db.Conversas.RemoveRange(db.Conversas.Where(c => convIds.Contains(c.Id)));


                var detalhes = db.DetalhesReservas.Where(d => d.ServicoId == id).ToList();
                db.DetalhesReservas.RemoveRange(detalhes);

 
                var reservasOrfas = db.Reservas.Where(r => reservasIds.Contains(r.Id) && !r.DetalhesReserva.Any());
                db.Reservas.RemoveRange(reservasOrfas);

                db.Avaliacoes.RemoveRange(db.Avaliacoes.Where(a => a.ServicoId == id));
                db.Favoritos.RemoveRange(db.Favoritos.Where(f => f.ServicoId == id));

                foreach (var img in servico.Imagens.ToList())
                {
                    var path = Server.MapPath(img.Url);
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                }
                db.Imagens.RemoveRange(db.Imagens.Where(i => i.ServicoId == id));

 
                db.Servicos.Remove(servico);

                db.SaveChanges();
                tx.Commit();
            }

            return RedirectToAction("GerirAnuncios");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdicionarServico_Passo1()
        {
            var servico = Session["NovoServico"] as Servico ?? new Servico();
            PreencherViewBagTradsServicoParaView();
            return View(servico);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult AdicionarServico_Passo1(Servico servico)
        {
            ModelState.Clear();
            ModelState.Remove("Preco");
            ModelState.Remove("DescontoPercentagem");

            if (string.IsNullOrWhiteSpace(servico.Nome))
                ModelState.AddModelError("Nome", ProjetoFim.Resources.Strings.Home_Validacao_NomeObrigatorio);
            if (string.IsNullOrWhiteSpace(servico.Localizacao))
                ModelState.AddModelError("Localizacao", ProjetoFim.Resources.Strings.Home_Validacao_LocalizacaoObrigatoria);

            if (!ModelState.IsValid)
            {
                ViewBag.S_Nome_en = Request["S_Nome_en"];
                ViewBag.S_Descricao_en = Request["S_Descricao_en"];
                ViewBag.S_Nome_es = Request["S_Nome_es"];
                ViewBag.S_Descricao_es = Request["S_Descricao_es"];
                ViewBag.S_Nome_fr = Request["S_Nome_fr"];
                ViewBag.S_Descricao_fr = Request["S_Descricao_fr"];
                return View(servico);
            }

            var s = Session["NovoServico"] as Servico ?? new Servico();
            s.Nome = servico.Nome;
            s.Descricao = servico.Descricao;
            s.Localizacao = servico.Localizacao;
            s.Latitude = servico.Latitude;
            s.Longitude = servico.Longitude;

            s.Traducoes = LerTradsServicoDoRequest(s);
            var tradsS = s.Traducoes ?? new List<ServicoTrad>();
            Session["NovoServico_Trads"] = tradsS;

            Session["NovoServico"] = s;
            return RedirectToAction("AdicionarServico_Passo2");
        }


        [Authorize(Roles = "Admin")]
        public ActionResult AdicionarServico_Passo2()
        {
            var servico = Session["NovoServico"] as Servico;
            if (servico == null) return RedirectToAction("AdicionarServico_Passo1");
            if (servico.Imagens == null) servico.Imagens = new List<Imagem>();
            return View(servico);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AdicionarServico_Passo2(IEnumerable<HttpPostedFileBase> ficheiros)
        {
            var servico = Session["NovoServico"] as Servico;
            if (servico == null) return RedirectToAction("AdicionarServico_Passo1");

            if (servico.Imagens == null)
                servico.Imagens = new List<Imagem>();

            if (ficheiros != null && ficheiros.Any(f => f != null && f.ContentLength > 0))
            {
                foreach (var file in ficheiros)
                {
                    var nomeFicheiro = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var caminho = Path.Combine(Server.MapPath("~/Uploads/Imagens/"), nomeFicheiro);
                    Directory.CreateDirectory(Server.MapPath("~/Uploads/Imagens/"));
                    file.SaveAs(caminho);

                    servico.Imagens.Add(new Imagem { Url = "/Uploads/Imagens/" + nomeFicheiro });
                }
            }
            Session["NovoServico"] = servico;
            return RedirectToAction("AdicionarServico_Passo3");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdicionarServico_Passo3()
        {
            var servico = Session["NovoServico"] as Servico;
            if (servico == null) return RedirectToAction("AdicionarServico_Passo1");

            var token = Guid.NewGuid().ToString("N");
            Session["NovoServico_Token"] = token;
            ViewBag.FormToken = token;

            return View(servico);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AdicionarServico_Passo3(Servico servico)
        {
            var servicoDaSessao = Session["NovoServico"] as Servico;
            if (servicoDaSessao == null) return RedirectToAction("AdicionarServico_Passo1");

            var postToken = Request["FormToken"];
            var sessToken = Session["NovoServico_Token"] as string;
            if (string.IsNullOrEmpty(postToken) || !string.Equals(postToken, sessToken, StringComparison.Ordinal))
                return RedirectToAction("GerirAnuncios");
            Session.Remove("NovoServico_Token");

            var precoRaw = Request["Preco"];
            var preco = ParseDecimalAny(precoRaw);
            if (preco == null || preco <= 0)
            {
                ModelState.AddModelError("Preco", ProjetoFim.Resources.Strings.Home_Validacao_PrecoMaiorZero);
                return View(servicoDaSessao);
            }
            servicoDaSessao.Preco = preco.Value;

            var descRaw = Request["DescontoPercentagem"];
            var desc = ParseDecimalAny(descRaw);
            var descInt = desc.HasValue ? (int)Math.Round(desc.Value, MidpointRounding.AwayFromZero) : 0;
            servicoDaSessao.DescontoPercentagem = descInt;

            var trads = Session["NovoServico_Trads"] as List<ServicoTrad> ?? new List<ServicoTrad>();
            var maxRaw = Request["MaxParticipantes"];
            int maxPart;
            if (!int.TryParse(maxRaw, out maxPart) || maxPart < 1)
            {
                ModelState.AddModelError("MaxParticipantes", "Indique um número de participantes válido (>= 1).");
                return View(servicoDaSessao);
            }
            servicoDaSessao.MaxParticipantes = maxPart;

            using (var db = new ApplicationDbContext())
            using (var tx = db.Database.BeginTransaction())
            {
                db.Servicos.Add(servicoDaSessao);
                db.SaveChanges();

                if (trads.Count > 0)
                {
                    var culturas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var t in trads)
                    {
                        if (t == null || string.IsNullOrWhiteSpace(t.Nome)) continue;
                        if (!culturas.Add(t.Cultura ?? "")) continue;

                        t.ServicoId = servicoDaSessao.Id;
                        db.ServicosTrad.Add(t);
                    }
                    db.SaveChanges();
                }

                tx.Commit();
            }

            Session.Remove("NovoServico");
            Session.Remove("NovoServico_Trads");

            return RedirectToAction("GerirAnuncios");
        }



        [Authorize(Roles = "Admin")]
        public ActionResult ModificarServico_Passo1(int id)
        {
            var servico = db.Servicos
                            .Include("Imagens")
                            .Include("Traducoes")
                            .FirstOrDefault(s => s.Id == id);

            if (servico == null) return HttpNotFound();

            Session["ServicoEmEdicao"] = servico;

            if (Session["ServicoEmEdicao_Trads"] == null)
                Session["ServicoEmEdicao_Trads"] = (servico.Traducoes ?? new List<ServicoTrad>()).ToList();

            PreencherViewBagTradsServicoParaView();
            return View("AdicionarServico_Passo1", servico);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult ModificarServico_Passo1(Servico servico)
        {
            var sSessao = Session["ServicoEmEdicao"] as Servico;
            if (sSessao == null) return RedirectToAction("GerirAnuncios");

            ModelState.Remove("Latitude");
            ModelState.Remove("Longitude");
            ModelState.Remove("Preco");
            ModelState.Remove("DescontoPercentagem");

            if (string.IsNullOrWhiteSpace(servico.Nome))
                ModelState.AddModelError("Nome", Strings.Home_Validacao_NomeObrigatorio);
            if (string.IsNullOrWhiteSpace(servico.Localizacao))
                ModelState.AddModelError("Localizacao", Strings.Home_Validacao_LocalizacaoObrigatoria);

            if (!ModelState.IsValid)
            {
                ViewBag.S_Nome_en = Request["S_Nome_en"];
                ViewBag.S_Descricao_en = Request["S_Descricao_en"];
                ViewBag.S_Nome_es = Request["S_Nome_es"];
                ViewBag.S_Descricao_es = Request["S_Descricao_es"];
                ViewBag.S_Nome_fr = Request["S_Nome_fr"];
                ViewBag.S_Descricao_fr = Request["S_Descricao_fr"];
                return View("AdicionarServico_Passo1", servico);
            }

            sSessao.Nome = servico.Nome;
            sSessao.Descricao = servico.Descricao;
            sSessao.Localizacao = servico.Localizacao;
            sSessao.Latitude = servico.Latitude;
            sSessao.Longitude = servico.Longitude;

            sSessao.Traducoes = LerTradsServicoDoRequest(sSessao);
            var tradsSEdit = sSessao.Traducoes ?? new List<ServicoTrad>();
            Session["ServicoEmEdicao_Trads"] = tradsSEdit; 

            Session["ServicoEmEdicao"] = sSessao;

            return RedirectToAction("ModificarServico_Passo2", new { id = sSessao.Id });
        }


        [Authorize(Roles = "Admin")]
        public ActionResult ModificarServico_Passo2(int id)
        {
            var servico = Session["ServicoEmEdicao"] as Servico;
            if (servico == null) return RedirectToAction("GerirAnuncios");
            return View("AdicionarServico_Passo2", servico);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult ModificarServico_Passo2(int id, IEnumerable<HttpPostedFileBase> ficheiros, int[] imagensParaApagar, string ordemDasImagens)
        {
            Session["ImagensParaApagarIDs_Servico"] = imagensParaApagar;
            Session["OrdemDasImagens_Servico"] = ordemDasImagens;
            if (ficheiros != null && ficheiros.Any(f => f != null))
            {
                var nomesFicheirosTemporarios = new List<string>();
                var pastaTemporaria = Server.MapPath("~/Uploads/Temp/");
                Directory.CreateDirectory(pastaTemporaria);

                foreach (var file in ficheiros)
                {
                    var nomeFicheiro = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var caminho = Path.Combine(pastaTemporaria, nomeFicheiro);
                    file.SaveAs(caminho);
                    nomesFicheirosTemporarios.Add(nomeFicheiro);
                }
                Session["NovasImagensTemp_Servico"] = nomesFicheirosTemporarios;
            }

            return RedirectToAction("ModificarServico_Passo3", new { id = id });
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ModificarServico_Passo3(int id)
        {
            var servico = Session["ServicoEmEdicao"] as Servico;
            if (servico == null) return RedirectToAction("GerirAnuncios");

            return View("AdicionarServico_Passo3", servico);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ModificarServico_Passo3(int id, Servico servicoDoFormulario)
        {
            var servicoDaSessao = Session["ServicoEmEdicao"] as Servico;
            if (servicoDaSessao == null || servicoDaSessao.Id != id)
            {
                return RedirectToAction("GerirAnuncios");
            }

            var precoRaw = Request["Preco"];
            var preco = ParseDecimalAny(precoRaw);
            if (preco == null || preco <= 0)
            {
                ModelState.AddModelError("Preco", Strings.Home_Validacao_PrecoMaiorZero);
                return View("AdicionarServico_Passo3", servicoDaSessao);
            }

            using (var db = new ApplicationDbContext())
            {
                var servicoNaDb = db.Servicos.Include(s => s.Imagens).Include(s => s.Traducoes).FirstOrDefault(s => s.Id == id);
                if (servicoNaDb == null)
                {
                    return HttpNotFound();
                }
                servicoNaDb.Nome = servicoDaSessao.Nome;
                servicoNaDb.Descricao = servicoDaSessao.Descricao;
                servicoNaDb.Localizacao = servicoDaSessao.Localizacao;
                servicoNaDb.Preco = preco.Value;

                var descRaw = Request["DescontoPercentagem"];
                var desc = ParseDecimalAny(descRaw);
                var descInt = desc.HasValue ? (int)Math.Round(desc.Value, MidpointRounding.AwayFromZero) : 0;
                servicoNaDb.DescontoPercentagem = descInt;
                var maxRaw = Request["MaxParticipantes"];
                int maxPart;
                if (!int.TryParse(maxRaw, out maxPart) || maxPart < 1)
                {
                    ModelState.AddModelError("MaxParticipantes", "Indique um número de participantes válido (>= 1).");
                    return View("AdicionarServico_Passo3", servicoDaSessao);
                }
                servicoNaDb.MaxParticipantes = maxPart;


                var imagensParaApagarIDs = Session["ImagensParaApagarIDs_Servico"] as int[];
                if (imagensParaApagarIDs != null && imagensParaApagarIDs.Any())
                {
                    foreach (var imagemId in imagensParaApagarIDs)
                    {
                        var imagemParaApagar = db.Imagens.Find(imagemId);
                        if (imagemParaApagar != null)
                        {
                            var caminhoFisico = Server.MapPath(imagemParaApagar.Url);
                            if (System.IO.File.Exists(caminhoFisico))
                            {
                                System.IO.File.Delete(caminhoFisico);
                            }
                            db.Imagens.Remove(imagemParaApagar);
                        }
                    }
                }
                var nomesFicheirosTemporarios = Session["NovasImagensTemp_Servico"] as List<string>;
                if (nomesFicheirosTemporarios != null)
                {
                    var pastaTemporaria = Server.MapPath("~/Uploads/Temp/");
                    var pastaFinal = Server.MapPath("~/Uploads/Imagens/");
                    Directory.CreateDirectory(pastaFinal);

                    foreach (var nomeFicheiroTemp in nomesFicheirosTemporarios)
                    {
                        var caminhoOrigem = Path.Combine(pastaTemporaria, nomeFicheiroTemp);
                        var caminhoDestino = Path.Combine(pastaFinal, nomeFicheiroTemp);
                        if (System.IO.File.Exists(caminhoOrigem))
                        {
                            System.IO.File.Move(caminhoOrigem, caminhoDestino);
                            servicoNaDb.Imagens.Add(new Imagem { Url = "/Uploads/Imagens/" + nomeFicheiroTemp });
                        }
                    }
                }
                db.SaveChanges();

                var trads = Session["ServicoEmEdicao_Trads"] as List<ServicoTrad>;
                if (trads != null)
                {
                    db.ServicosTrad.RemoveRange(servicoNaDb.Traducoes);
                    db.SaveChanges();

                    var culturas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var t in trads)
                    {
                        if (string.IsNullOrWhiteSpace(t?.Nome)) continue;
                        if (!culturas.Add(t.Cultura ?? "")) continue;

                        t.ServicoId = id;
                        db.ServicosTrad.Add(t);
                    }
                    db.SaveChanges();
                }
                var ordemDasImagens = Session["OrdemDasImagens_Servico"] as string;
                if (!string.IsNullOrEmpty(ordemDasImagens))
                {
                    var idsOrdenados = ordemDasImagens.Split(',').Select(int.Parse).ToList();
                    for (int i = 0; i < idsOrdenados.Count; i++)
                    {
                        var imagemId = idsOrdenados[i];
                        var imagem = servicoNaDb.Imagens.FirstOrDefault(img => img.Id == imagemId);
                        if (imagem != null)
                        {
                            imagem.Ordem = i;
                        }
                    }
                }

                db.SaveChanges();
            }
            Session.Remove("ServicoEmEdicao");
            Session.Remove("ImagensParaApagarIDs_Servico");
            Session.Remove("OrdemDasImagens_Servico");
            Session.Remove("NovasImagensTemp_Servico");
            Session.Remove("ServicoEmEdicao_Trads");

            return RedirectToAction("GerirAnuncios");
        }


        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PrevisualizarQuarto(int id, bool? showAllReviews)
        {
            var quarto = await db.Quartos
                                 .Include(q => q.Imagens)
                                 .Include(q => q.Comodidades)
                                 .Include(q => q.Avaliacoes.Select(a => a.Utilizador))
                                 .FirstOrDefaultAsync(q => q.Id == id);
            if (quarto == null)
            {
                return HttpNotFound();
            }

            ViewBag.IsPreviewMode = true;
            ViewBag.ShowAllReviews = showAllReviews ?? false;
            return View("DetalhesQuarto", quarto);
        }
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PrevisualizarServico(int id)
        {
            var servico = await db.Servicos
                                  .Include(s => s.Imagens)
                                  .Include(s => s.Avaliacoes.Select(a => a.Utilizador))
                                  .FirstOrDefaultAsync(s => s.Id == id);
            if (servico == null)
            {
                return HttpNotFound();
            }
            ViewBag.IsPreviewMode = true;
            return View("DetalhesServico", servico);
        }
    }
}
