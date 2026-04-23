using System.Collections.Generic;

namespace ProjetoFim.Models 
{
    public class HomepageViewModel
    {
        public List<Quarto> QuartosMelhorAvaliacao { get; set; }
        public List<Quarto> QuartosRecemAdicionados { get; set; }
        public List<Quarto> QuartosComDesconto { get; set; }
        public List<int> FavoritosDoUtilizador { get; set; }
        public HomepageViewModel()
        {
            QuartosMelhorAvaliacao = new List<Quarto>();
            QuartosRecemAdicionados = new List<Quarto>();
            QuartosComDesconto = new List<Quarto>();
            FavoritosDoUtilizador = new List<int>();
        }
    }
}