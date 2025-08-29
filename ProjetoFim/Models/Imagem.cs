using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoFim.Models
{
    public class Imagem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Url { get; set; }
        public int Ordem { get; set; }
        public int? QuartoId { get; set; } 
        [ForeignKey("QuartoId")]
        public virtual Quarto Quarto { get; set; }
        public int? ServicoId { get; set; }
        [ForeignKey("ServicoId")]
        public virtual Servico Servico { get; set; }
    }
}