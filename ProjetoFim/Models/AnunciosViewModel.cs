using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjetoFim.Models
{
    public class AnunciosViewModel
    {
        public IEnumerable<Quarto> Quartos { get; set; }
        public IEnumerable<Servico> Servicos { get; set; }
    }
}