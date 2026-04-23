using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoFim.Models
{
    public class Mensagem
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Conteudo { get; set; }
        public DateTime DataEnvio { get; set; }
        public bool Lida { get; set; }
        [Required]
        public int ConversaId { get; set; }
        [ForeignKey("ConversaId")]
        public virtual Conversa Conversa { get; set; }
        [Required]
        public string RemetenteId { get; set; }
        [ForeignKey("RemetenteId")]
        public virtual ApplicationUser Remetente { get; set; }
    }
}