using Microsoft.AspNet.Identity; 
using ProjetoFim.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Services.Description;
using ProjetoFim.Services;

namespace ProjetoFim.Controllers
{
    [Authorize] 
    public class ReservaController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Criar(int quartoId, DateTime dataInicio, DateTime dataFim)
        {
            var quarto = await db.Quartos.FindAsync(quartoId);
            if (quarto == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ProjetoFim.Resources.Strings.Booking_Error_RoomNotFound);
            }

            var numeroNoites = (dataFim - dataInicio).Days;
            if (numeroNoites <= 0)
            {
                TempData["ErroReserva"] = ProjetoFim.Resources.Strings.Booking_Error_EndBeforeStart;
                return RedirectToAction("DetalhesQuarto", "Home", new { id = quartoId });
            }

            var estadosOcupados = BookingStates.OccupiedStates;
            bool temConflito = await db.DetalhesReservas.AnyAsync(d =>
                d.QuartoId == quartoId &&
                estadosOcupados.Contains(d.Reserva.Estado) &&
                (d.DataInicio < dataFim && d.DataFim > dataInicio)
            );

            if (temConflito)
            {
                TempData["ErroReserva"] = ProjetoFim.Resources.Strings.Booking_Error_DatesUnavailable;
                return RedirectToAction("DetalhesQuarto", "Home", new { id = quartoId });
            }

            var utilizadorId = User.Identity.GetUserId();

            decimal precoPorNoiteFinal = quarto.PrecoPorNoite;
            if (quarto.DescontoPercentagem > 0)
            {
                precoPorNoiteFinal = quarto.PrecoPorNoite * (1 - (quarto.DescontoPercentagem / 100.0m));
            }

            var precoTotal = numeroNoites * precoPorNoiteFinal;

            var reserva = new Reserva
            {
                DataCriacao = DateTime.Now,
                UtilizadorId = utilizadorId,
                ValorTotal = precoTotal,
                Estado = BookingStates.Pending
            };

            var detalheReserva = new DetalhesReserva
            {
                Reserva = reserva,
                QuartoId = quartoId,
                PrecoUnitario = quarto.PrecoPorNoite,
                Quantidade = numeroNoites,
                DataInicio = dataInicio,
                DataFim = dataFim
            };

            db.Reservas.Add(reserva);
            db.DetalhesReservas.Add(detalheReserva);
            await db.SaveChangesAsync();

            return RedirectToAction("Confirmacao", new { id = reserva.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CriarReservaServico(int servicoId, DateTime dataReserva, int numParticipantes)
        {
            var servico = await db.Servicos.FindAsync(servicoId);
            if (servico == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ProjetoFim.Resources.Strings.Service_Error_NotFound);
            }

            var utilizadorId = User.Identity.GetUserId();

            decimal precoUnitarioFinal = servico.Preco;
            if (servico.DescontoPercentagem > 0)
            {
                precoUnitarioFinal = servico.Preco * (1 - (servico.DescontoPercentagem / 100.0m));
            }

            var precoTotal = numParticipantes * precoUnitarioFinal;

            var reserva = new Reserva
            {
                DataCriacao = DateTime.Now,
                UtilizadorId = utilizadorId,
                ValorTotal = precoTotal,
                Estado = BookingStates.Pending
            };

            var detalheReserva = new DetalhesReserva
            {
                Reserva = reserva,
                ServicoId = servicoId,
                PrecoUnitario = precoUnitarioFinal,
                Quantidade = numParticipantes,
                DataInicio = dataReserva,
                DataFim = dataReserva
            };

            db.Reservas.Add(reserva);
            db.DetalhesReservas.Add(detalheReserva);
            await db.SaveChangesAsync();

            return RedirectToAction("Confirmacao", new { id = reserva.Id });
        }

        public async Task<ActionResult> Confirmacao(int id)
         {
            var reserva = await db.Reservas.Include(r => r.DetalhesReserva).FirstOrDefaultAsync(r => r.Id == id);
            if (reserva == null)
            {
                return HttpNotFound();
             }
            return View(reserva); 
            }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Cancelar(int id)
        {
            var utilizadorId = User.Identity.GetUserId();
            var reserva = await db.Reservas.FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null || (reserva.UtilizadorId != utilizadorId && !User.IsInRole("Admin")))
            {
                return Json(new { success = false, message = ProjetoFim.Resources.Strings.Booking_Error_NotFoundOrDenied });
            }

            reserva.Estado = BookingStates.CancellationRequested;
            await db.SaveChangesAsync();

            return Json(new { success = true });
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Detalhes(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var reserva = await db.Reservas
                                  .Include(r => r.Utilizador)
                                  .Include(r => r.DetalhesReserva.Select(d => d.Quarto))
                                  .Include(r => r.DetalhesReserva.Select(d => d.Servico))
                                  .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
            {
                return HttpNotFound();
            }
          
            return View(reserva);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AlterarEstado(int id, string novoEstado)
        {
            var reservaParaAlterar = await db.Reservas.Include(r => r.DetalhesReserva).FirstOrDefaultAsync(r => r.Id == id);
            if (reservaParaAlterar == null)
            {
                return HttpNotFound();
            }

            if (novoEstado == BookingStates.Confirmed)
            {
                var detalhe = reservaParaAlterar.DetalhesReserva.FirstOrDefault();
                if (detalhe?.QuartoId != null)
                {
                    var dataInicio = detalhe.DataInicio.Value;
                    var dataFim = detalhe.DataFim.Value;
                    var quartoId = detalhe.QuartoId.Value;

                    bool temConflito = await db.DetalhesReservas.AnyAsync(d =>
                        d.Reserva.Id != reservaParaAlterar.Id &&
                        d.QuartoId == quartoId &&
                        d.Reserva.Estado == BookingStates.Confirmed &&
                        (d.DataInicio < dataFim && d.DataFim > dataInicio)
                    );

                    if (temConflito)
                    {
                        TempData["ErroGestao"] = ProjetoFim.Resources.Strings.BookingMgmt_Error_CannotConfirm_DatesConflict;
                        return RedirectToAction("Detalhes", new { id = id });
                    }
                }
            }

            if (novoEstado == BookingStates.Cancelled)
            {
                novoEstado = BookingStates.Confirmed;
            }

            var estadosValidos = BookingStates.OccupiedStates;
            if (!estadosValidos.Contains(novoEstado))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ProjetoFim.Resources.Strings.BookingMgmt_Error_InvalidState);
            }

            reservaParaAlterar.Estado = novoEstado;

            var notificacaoService = new NotificacaoService();
            var mensagemNotificacao = string.Format(
                ProjetoFim.Resources.Strings.BookingMgmt_Notification_StatusChanged_Format,
                reservaParaAlterar.Id.ToString("D5"),
                novoEstado
            );
            var urlNotificacao = Url.Action("Reservas", "Home");
            await notificacaoService.CriarNotificacaoAsync(reservaParaAlterar.UtilizadorId, mensagemNotificacao, urlNotificacao);

            await db.SaveChangesAsync();

            TempData["SucessoGestao"] = ProjetoFim.Resources.Strings.BookingMgmt_Success_StateChanged;
            return RedirectToAction("Detalhes", new { id = id });
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