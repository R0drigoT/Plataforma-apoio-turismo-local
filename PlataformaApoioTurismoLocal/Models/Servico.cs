using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjetoFim.Models
{
    public class Servico
    {
        public Servico()
        {
            this.Avaliacoes = new HashSet<Avaliacao>();
            this.Traducoes = new HashSet<ServicoTrad>();
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Strings),
                  ErrorMessageResourceName = "Servico_Nome_Obrigatorio")]
        [Display(Name = "Servico_Nome", ResourceType = typeof(Resources.Strings))]
        public string Nome { get; set; }

        [Display(Name = "Servico_Desconto", ResourceType = typeof(Resources.Strings))]
        public int DescontoPercentagem { get; set; }

        [StringLength(1000,
            ErrorMessageResourceType = typeof(Resources.Strings),
            ErrorMessageResourceName = "Servico_Descricao_Limite")]
        [Display(Name = "Servico_Descricao", ResourceType = typeof(Resources.Strings))]
        public string Descricao { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Strings),
                  ErrorMessageResourceName = "Servico_Localizacao_Obrigatoria")]
        [Display(Name = "Servico_Localizacao", ResourceType = typeof(Resources.Strings))]
        public string Localizacao { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Strings),
                  ErrorMessageResourceName = "Servico_Preco_Obrigatorio")]
        [Display(Name = "Servico_Preco", ResourceType = typeof(Resources.Strings))]
        public decimal Preco { get; set; }

        [Range(1, 1000)]
        [Display(Name = "Servico_MaxParticipantes", ResourceType = typeof(Resources.Strings))]
        public int MaxParticipantes { get; set; } = 1;

        [Display(Name = "Servico_Latitude", ResourceType = typeof(Resources.Strings))]
        public double? Latitude { get; set; }

        [Display(Name = "Servico_Longitude", ResourceType = typeof(Resources.Strings))]
        public double? Longitude { get; set; }

        public double AvaliacaoMedia { get; set; }

        public virtual ICollection<Avaliacao> Avaliacoes { get; set; }
        public virtual ICollection<Imagem> Imagens { get; set; }
        public virtual ICollection<ServicoTrad> Traducoes { get; set; } = new List<ServicoTrad>();
    }
}
