using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ProjetoFim.Models
{
    public class Servico
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do serviço é obrigatório.")]
        public string Nome { get; set; }

        public string Descricao { get; set; }

        [Required(ErrorMessage = "A localização é obrigatória.")]
        public string Localizacao { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório.")]
        [DisplayName("Preço")]
        public decimal Preco { get; set; }
    }
}