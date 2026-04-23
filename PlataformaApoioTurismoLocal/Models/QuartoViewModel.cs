using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjetoFim.Models 
{
    public class QuartoViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string Localizacao { get; set; }
        public int Rating { get; set; }
        public string ImagemUrl { get; set; }
        public decimal? PrecoOriginal { get; set; }
        public decimal? PrecoComDesconto { get; set; }
    }
}