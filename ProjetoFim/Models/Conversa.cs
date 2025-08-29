using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Services.Description;

namespace ProjetoFim.Models
{
    public class Conversa
    {
        public Conversa()
        {
            this.Mensagens = new HashSet<Mensagem>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Assunto { get; set; }

        public DateTime DataCriacao { get; set; }

        [Required]
        public string UtilizadorId { get; set; }
        [ForeignKey("UtilizadorId")]
        public virtual ApplicationUser Utilizador { get; set; }

        public int? ReservaId { get; set; }
        [ForeignKey("ReservaId")]
        public virtual Reserva Reserva { get; set; }


        public virtual ICollection<Mensagem> Mensagens { get; set; }

        public int? QuartoId { get; set; }
        [ForeignKey("QuartoId")]
        public virtual Quarto Quarto { get; set; }

        public int? ServicoId { get; set; }
        [ForeignKey("ServicoId")]
        public virtual Servico Servico { get; set; }
    }
}