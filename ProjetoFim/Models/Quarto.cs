using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjetoFim.Models
{
    public class Quarto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Strings),
                  ErrorMessageResourceName = "Quarto_Nome_Obrigatorio")]
        [Display(Name = "Quarto_Nome", ResourceType = typeof(Resources.Strings))]
        public string Nome { get; set; }

        [StringLength(1000,
            ErrorMessageResourceType = typeof(Resources.Strings),
            ErrorMessageResourceName = "Quarto_Descricao_Limite")]
        [Display(Name = "Quarto_Descricao", ResourceType = typeof(Resources.Strings))]
        public string Descricao { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Strings),
                  ErrorMessageResourceName = "Quarto_Localizacao_Obrigatoria")]
        [Display(Name = "Quarto_Localizacao", ResourceType = typeof(Resources.Strings))]
        public string Localizacao { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Strings),
                  ErrorMessageResourceName = "Quarto_Preco_Obrigatorio")]
        [Display(Name = "Quarto_PrecoPorNoite", ResourceType = typeof(Resources.Strings))]
        public decimal PrecoPorNoite { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Strings),
                  ErrorMessageResourceName = "Quarto_Hospedes_Obrigatorio")]
        [Display(Name = "Quarto_NumeroHospedes", ResourceType = typeof(Resources.Strings))]
        public int NumeroHospedes { get; set; }

        [Display(Name = "Quarto_CamasCasal", ResourceType = typeof(Resources.Strings))]
        public int NumeroCamasCasal { get; set; }

        [Display(Name = "Quarto_CamasSolteiro", ResourceType = typeof(Resources.Strings))]
        public int NumeroCamasSolteiro { get; set; }

        [Display(Name = "Quarto_CasasDeBanho", ResourceType = typeof(Resources.Strings))]
        public int NumeroCasasDeBanho { get; set; }

        [Display(Name = "Quarto_Desconto", ResourceType = typeof(Resources.Strings))]
        public int DescontoPercentagem { get; set; }

        [Display(Name = "Quarto_Estacionamento", ResourceType = typeof(Resources.Strings))]
        public bool TemEstacionamento { get; set; }

        [Display(Name = "Quarto_Latitude", ResourceType = typeof(Resources.Strings))]
        public double? Latitude { get; set; }

        [Display(Name = "Quarto_Longitude", ResourceType = typeof(Resources.Strings))]
        public double? Longitude { get; set; }

        public double AvaliacaoMedia { get; set; }

        public virtual ICollection<Avaliacao> Avaliacoes { get; set; }
        public virtual ICollection<Imagem> Imagens { get; set; }
        public virtual ICollection<Comodidade> Comodidades { get; set; }
        public virtual ICollection<QuartoTrad> Traducoes { get; set; }
    }
}
