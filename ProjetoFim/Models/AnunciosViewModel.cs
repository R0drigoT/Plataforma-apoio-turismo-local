using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace ProjetoFim.Models
{
    public class AnunciosViewModel
    {
        public IPagedList<Quarto> Quartos { get; set; }
        public IPagedList<Servico> Servicos { get; set; }
    }
}