using System.Collections.Generic;

namespace ProjetoFim.Models
{
    public class DashboardViewModel
    {
        public int ReservasPendentes { get; set; }
        public int NovasReservasHoje { get; set; }
        public int CheckInsParaHoje { get; set; }
        public decimal GanhosDoMes { get; set; }
        public int MensagensNaoLidas { get; set; }
        public List<Reserva> AtividadeRecente { get; set; }
        public DashboardViewModel()
        {
            AtividadeRecente = new List<Reserva>();
        }
    }
}