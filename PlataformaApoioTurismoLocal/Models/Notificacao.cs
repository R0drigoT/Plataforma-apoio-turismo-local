using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoFim.Models 
{
    public class Notificacao
    {
        public int Id { get; set; }
        public string Mensagem { get; set; }
        public bool Lida { get; set; }
        public DateTime DataCriacao { get; set; }
        public string Url { get; set; }

        public string DestinatarioId { get; set; }
        [ForeignKey("DestinatarioId")]
        public virtual ApplicationUser Destinatario { get; set; }
    }
}