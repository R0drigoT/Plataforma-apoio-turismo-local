using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoFim.Models
{
    public class DetalhesReserva
    {
        [Key]
        public int Id { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        [Required]
        public int ReservaId { get; set; }
        [ForeignKey("ReservaId")]
        public virtual Reserva Reserva { get; set; }
        public int? QuartoId { get; set; } 
        [ForeignKey("QuartoId")]
        public virtual Quarto Quarto { get; set; }
        public int? ServicoId { get; set; } 
        [ForeignKey("ServicoId")]
        public virtual Servico Servico { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
    }
}