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

        // Chave estrangeira para o Quarto a que esta imagem pertence
        public int QuartoId { get; set; }

        [ForeignKey("QuartoId")]
        public virtual Quarto Quarto { get; set; }
    }
}