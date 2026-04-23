using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoFim.Models
{
    public class Avaliacao
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Range(1, 5)]
        public int Classificacao { get; set; }
        [StringLength(500, ErrorMessage = "O comentário não pode exceder os 500 caracteres.")]
        public string Comentario { get; set; }
        public DateTime DataAvaliacao { get; set; }
        public int? QuartoId { get; set; }
        [ForeignKey("QuartoId")]
        public virtual Quarto Quarto { get; set; }
        [Required] 
        public string UtilizadorId { get; set; }
        [ForeignKey("UtilizadorId")]
        public virtual ApplicationUser Utilizador { get; set; }
        public int? ServicoId { get; set; }
        [ForeignKey("ServicoId")]
        public virtual Servico Servico { get; set; }

    }
}