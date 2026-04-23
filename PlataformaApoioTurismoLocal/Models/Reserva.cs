using ProjetoFim.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoFim.Models 
{
    public class Reserva
    {
        public Reserva()
        {
            this.DetalhesReserva = new HashSet<DetalhesReserva>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime DataCriacao { get; set; }

        [Required]
        public decimal ValorTotal { get; set; }

        public string Estado { get; set; }

        [Required]
        public string UtilizadorId { get; set; }
        [ForeignKey("UtilizadorId")]
        public virtual ApplicationUser Utilizador { get; set; }

        public virtual ICollection<DetalhesReserva> DetalhesReserva { get; set; }
    }
} 