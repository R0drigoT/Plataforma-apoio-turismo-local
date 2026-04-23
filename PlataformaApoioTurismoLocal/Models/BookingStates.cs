using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjetoFim.Models
{
    public static class BookingStates
    {
        public const string Pending = "Pendente";
        public const string Confirmed = "Confirmada";
        public const string Cancelled = "Cancelada";
        public const string Completed = "Concluída";
        public const string CancellationRequested = "Cancelamento Pedido";

        public static readonly string[] OccupiedStates = { Pending, Confirmed };
    }
}
