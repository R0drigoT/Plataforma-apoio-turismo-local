namespace ProjetoFim.Models
{
    public class ServicoViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string Localizacao { get; set; }
        public string ImagemUrl { get; set; }
        public decimal Preco { get; set; }
        public int DescontoPercentagem { get; set; }
        public decimal? PrecoComDesconto { get; set; }
    }
}
