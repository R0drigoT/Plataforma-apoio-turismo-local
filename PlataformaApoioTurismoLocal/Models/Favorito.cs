using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoFim.Models
{
    public class Favorito
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UtilizadorId { get; set; }
        [ForeignKey("UtilizadorId")]
        public virtual ApplicationUser Utilizador { get; set; }       
        public int? QuartoId { get; set; }
        [ForeignKey("QuartoId")]
        public virtual Quarto Quarto { get; set; }
        public int? ServicoId { get; set; }
        [ForeignKey("ServicoId")]
        public virtual Servico Servico { get; set; }
    }
}