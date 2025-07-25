using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjetoFim.Models // O namespace deve corresponder ao nome do teu projeto
{
    public class QuartoViewModel
    {
        // Propriedades básicas
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Localizacao { get; set; }
        public int Rating { get; set; }
        public string ImagemUrl { get; set; }

        // Propriedades para a secção de descontos
        public decimal? PrecoOriginal { get; set; }
        public decimal? PrecoComDesconto { get; set; }
    }
}