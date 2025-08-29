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
                                    .Include(s => s.Traducoes)
                                    .FirstOrDefaultAsync(q => q.Id == id);
            if (quarto == null)
            {
                return HttpNotFound();
            }
            var lang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            var trad = quarto.Traducoes?.FirstOrDefault(t => t.Cultura == lang)
                    ?? quarto.Traducoes?.FirstOrDefault(t => t.Cultura == "pt"); 

            ViewBag.NomeTraduzido = trad?.Nome ?? quarto.Nome;
            ViewBag.DescricaoTraduzida = trad?.Descricao ?? quarto.Descricao;
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
                          .OrderByDescending(a => a.DataAvaliacao);

            int numeroPagina = (pagina ?? 1);
            int tamanhoPagina = 10; 

            var avaliacoesPaginadas = query.ToPagedList(numeroPagina, tamanhoPagina);

            return View(avaliacoesPaginadas);
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
                                        .Include(r => r.DetalhesReserva.Select(d => d.Servico)) 
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
                                           .Include(f => f.Servico)
                                           .ToListAsync();

            return View(listaDeFavoritos);
        }

        [Authorize]
        [Authorize]
        public async Task<ActionResult> Notificacoes()
        {
            var utilizadorId = User.Identity.GetUserId();
            var notificacoes = await db.Notificacoes
                                       .Where(n => n.DestinatarioId == utilizadorId)
                                       .OrderByDescending(n => n.DataCriacao)
                                       .ToListAsync();
            return View(notificacoes);
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
                                        .Include(u => u.Avaliacoes.Select(a => a.Servico))
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
        public async Task<ActionResult> PaginaPagamento(int id, DateTime dataInicio, DateTime dataFim, int numHospedes)
        {
            var quarto = await db.Quartos.Include(q => q.Imagens).FirstOrDefaultAsync(q => q.Id == id);
            if (quarto == null)
            {
                return HttpNotFound();
            }

            decimal precoPorNoiteFinal = quarto.PrecoPorNoite;
            if (quarto.DescontoPercentagem > 0)
            {
                precoPorNoiteFinal = quarto.PrecoPorNoite * (1 - (quarto.DescontoPercentagem / 100.0m));
            }
            var numeroNoites = (dataFim - dataInicio).Days;
            if (numeroNoites <= 0) numeroNoites = 1; 
            var precoTotalFinal = numeroNoites * precoPorNoiteFinal;

            var utilizadorId = User.Identity.GetUserId();
            var utilizador = await db.Users.FirstOrDefaultAsync(u => u.Id == utilizadorId);
            ViewBag.Utilizador = utilizador;
            ViewBag.Quarto = quarto;
            ViewBag.DataInicio = dataInicio;
            ViewBag.DataFim = dataFim;
            ViewBag.NumHospedes = numHospedes;
            ViewBag.NumeroNoites = numeroNoites;
            ViewBag.PrecoTotal = precoTotalFinal;

            return View();
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

            // Já é canónico?
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

            // Map de rótulos antigos (português/es/…) para canónico
            if (valor == ProjetoFim.Resources.Strings.ListaReservas_estado_pendente) return BookingStates.Pending;
            if (valor == ProjetoFim.Resources.Strings.ListaReservas_estado_confirmada) return BookingStates.Confirmed;
            if (valor == ProjetoFim.Resources.Strings.ListaReservas_estado_cancelada) return BookingStates.Cancelled;
            if (valor == ProjetoFim.Resources.Strings.ListaReservas_estado_cancelamento_pedido) return BookingStates.CancellationRequested;
            if (valor == ProjetoFim.Resources.Strings.ListaReservas_estado_concluida) return BookingStates.Completed;

            // Por omissão
            return "ALL";
        }
        [Authorize(Roles = "Admin")]
        public ActionResult ListaReservas(string pesquisa, string estado, DateTime? data, int? pagina)
        {
            var query = db.Reservas
                          .Include(r => r.Utilizador)
                          .Include(r => r.DetalhesReserva.Select(d => d.Quarto))
                          .Include(r => r.DetalhesReserva.Select(d => d.Servico))
                          .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                query = query.Where(r =>
                    r.Utilizador.UserName.Contains(pesquisa) ||
                    r.DetalhesReserva.Any(d => d.Quarto != null && d.Quarto.Nome.Contains(pesquisa)) ||
                    r.DetalhesReserva.Any(d => d.Servico != null && d.Servico.Nome.Contains(pesquisa))
                );
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
                                               DbFunctions.TruncateTime(d.DataFim) >= dia)
                );
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
                          .Include(r => r.DetalhesReserva.Select(d => d.Servico))
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
                return View(quarto);
            }

            var qSessao = Session["NovoQuarto"] as Quarto ?? new Quarto();
            qSessao.Nome = quarto.Nome;
            qSessao.Descricao = quarto.Descricao;
            qSessao.Localizacao = quarto.Localizacao;
            qSessao.Latitude = quarto.Latitude;
            qSessao.Longitude = quarto.Longitude;

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
            if (quarto == null)
            {
                return RedirectToAction("AdicionarQuarto_Passo1");
            }
            return View(quarto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AdicionarQuarto_Passo5(Quarto form)
        {
            var quartoDaSessao = Session["NovoQuarto"] as Quarto;
            if (quartoDaSessao == null) return RedirectToAction("AdicionarQuarto_Passo1");

            if (form.PrecoPorNoite <= 0)
            {
                ModelState.AddModelError("PrecoPorNoite", ProjetoFim.Resources.Strings.Home_Validacao_PrecoMaiorZero);
                return View(quartoDaSessao);
            }

            quartoDaSessao.PrecoPorNoite = form.PrecoPorNoite;
            quartoDaSessao.DescontoPercentagem = form.DescontoPercentagem;

            var comodidadesIDs = Session["ComodidadesSelecionadasIDs"] as int[];
            using (var db = new ApplicationDbContext())
            {
                quartoDaSessao.Comodidades = new List<Comodidade>();
                if (comodidadesIDs != null)
                {
                    foreach (var id in comodidadesIDs)
                    {
                        var comodidade = db.Comodidades.Find(id);
                        if (comodidade != null)
                            quartoDaSessao.Comodidades.Add(comodidade);
                    }
                }

                db.Quartos.Add(quartoDaSessao);
                db.SaveChanges();
            }

            Session.Remove("NovoQuarto");
            Session.Remove("ComodidadesSelecionadasIDs");

            return RedirectToAction("GerirAnuncios", "Home");
        }


        [Authorize(Roles = "Admin")]
        public ActionResult ModificarQuarto_Passo1(int id)
        {
            var db = new ApplicationDbContext();
            var quartoParaEditar = db.Quartos
                                     .Include(q => q.Comodidades)
                                     .Include(q => q.Imagens)
                                     .FirstOrDefault(q => q.Id == id);

            if (quartoParaEditar == null)
            {
                return HttpNotFound();
            }
            Session.Remove("QuartoEmEdicao");
            Session["QuartoEmEdicao"] = quartoParaEditar;

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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ModificarQuarto_Passo5(int id, Quarto quartoDoFormulario)
        {
            var quartoComAlteracoes = Session["QuartoEmEdicao"] as Quarto;
            if (quartoComAlteracoes == null || quartoComAlteracoes.Id != id)
            {
                return RedirectToAction("GerirAnuncios");
            }
            if (quartoDoFormulario.PrecoPorNoite <= 0)
            {
                ModelState.AddModelError("PrecoPorNoite", ProjetoFim.Resources.Strings.Home_Validacao_PrecoMaiorZero);
                return View("AdicionarQuarto_Passo5", quartoComAlteracoes);
            }
            using (var db = new ApplicationDbContext())
            {
                var quartoParaAtualizar = db.Quartos.Include(q => q.Comodidades).Include(q => q.Imagens).FirstOrDefault(q => q.Id == id);
                if (quartoParaAtualizar == null)
                {
                    return HttpNotFound();
                }
                quartoParaAtualizar.Nome = quartoComAlteracoes.Nome;
                quartoParaAtualizar.Descricao = quartoComAlteracoes.Descricao;
                quartoParaAtualizar.Localizacao = quartoComAlteracoes.Localizacao;
                quartoParaAtualizar.PrecoPorNoite = quartoDoFormulario.PrecoPorNoite;
                quartoParaAtualizar.DescontoPercentagem = quartoDoFormulario.DescontoPercentagem;
                quartoParaAtualizar.NumeroCamasCasal = quartoComAlteracoes.NumeroCamasCasal;
                quartoParaAtualizar.NumeroCamasSolteiro = quartoComAlteracoes.NumeroCamasSolteiro;
                quartoParaAtualizar.NumeroCasasDeBanho = quartoComAlteracoes.NumeroCasasDeBanho;
                quartoParaAtualizar.TemEstacionamento = quartoComAlteracoes.TemEstacionamento;

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
            }
            Session.Remove("QuartoEmEdicao");
            Session.Remove("ComodidadesSelecionadasIDs");
            Session.Remove("NovasImagensTemp");
            Session.Remove("ImagensParaApagarIDs");
            Session.Remove("OrdemDasImagens");
            return RedirectToAction("GerirAnuncios");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult RemoverQuarto(int id)
        {
            using (var db = new ApplicationDbContext())
            {
                var quartoParaRemover = db.Quartos
                                          .Include(q => q.Imagens)
                                          .Include(q => q.Comodidades)
                                          .FirstOrDefault(q => q.Id == id);

                if (quartoParaRemover != null)
                {
                    foreach (var imagem in quartoParaRemover.Imagens.ToList())
                    {
                        var caminhoFisico = Server.MapPath(imagem.Url);
                        if (System.IO.File.Exists(caminhoFisico))
                        {
                            System.IO.File.Delete(caminhoFisico);
                        }
                    }
                    db.Quartos.Remove(quartoParaRemover);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("GerirAnuncios");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult RemoverServico(int id)
        {
            using (var db = new ApplicationDbContext())
            {
                var servicoParaRemover = db.Servicos
                                           .Include(s => s.Imagens)
                                           .FirstOrDefault(s => s.Id == id);

                if (servicoParaRemover != null)
                {
                    foreach (var imagem in servicoParaRemover.Imagens.ToList())
                    {
                        var caminhoFisico = Server.MapPath(imagem.Url);
                        if (System.IO.File.Exists(caminhoFisico))
                        {
                            System.IO.File.Delete(caminhoFisico);
                        }
                    }
                    db.Servicos.Remove(servicoParaRemover);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("GerirAnuncios");
        }
        [Authorize(Roles = "Admin")]
        public ActionResult AdicionarServico_Passo1()
        {
            var servico = Session["NovoServico"] as Servico ?? new Servico();
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

            if (!ModelState.IsValid) return View(servico);

            var s = Session["NovoServico"] as Servico ?? new Servico();
            s.Nome = servico.Nome;
            s.Descricao = servico.Descricao;
            s.Localizacao = servico.Localizacao;
            s.Latitude = servico.Latitude;
            s.Longitude = servico.Longitude;

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
            return View(servico);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AdicionarServico_Passo3(Servico servico)
        {
            var servicoDaSessao = Session["NovoServico"] as Servico;
            if (servicoDaSessao == null) return RedirectToAction("AdicionarServico_Passo1");

            if (servico.Preco <= 0)
            {
                ModelState.AddModelError("Preco", ProjetoFim.Resources.Strings.Home_Validacao_PrecoMaiorZero);
                return View(servicoDaSessao);
            }

            servicoDaSessao.Preco = servico.Preco;
            servicoDaSessao.DescontoPercentagem = servico.DescontoPercentagem;

            db.Servicos.Add(servicoDaSessao);
            db.SaveChanges();

            Session.Remove("NovoServico");
            return RedirectToAction("GerirAnuncios");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ModificarServico_Passo1(int id)
        {
            var servico = db.Servicos.Include("Imagens").FirstOrDefault(s => s.Id == id);
            if (servico == null) return HttpNotFound();

            Session["ServicoEmEdicao"] = servico;

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
                return View("AdicionarServico_Passo1", servico);

            sSessao.Nome = servico.Nome;
            sSessao.Descricao = servico.Descricao;
            sSessao.Localizacao = servico.Localizacao;
            sSessao.Latitude = servico.Latitude;
            sSessao.Longitude = servico.Longitude;

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
            if (servicoDoFormulario.Preco <= 0)
            {
                ModelState.AddModelError("Preco", Strings.Home_Validacao_PrecoMaiorZero);
                return View("AdicionarServico_Passo3", servicoDaSessao);
            }

            using (var db = new ApplicationDbContext())
            {
                var servicoNaDb = db.Servicos.Include(s => s.Imagens).FirstOrDefault(s => s.Id == id);
                if (servicoNaDb == null)
                {
                    return HttpNotFound();
                }
                servicoNaDb.Nome = servicoDaSessao.Nome;
                servicoNaDb.Descricao = servicoDaSessao.Descricao;
                servicoNaDb.Localizacao = servicoDaSessao.Localizacao;
                servicoNaDb.Preco = servicoDoFormulario.Preco;
                servicoNaDb.DescontoPercentagem = servicoDoFormulario.DescontoPercentagem;
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
