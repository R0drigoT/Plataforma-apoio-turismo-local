using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjetoFim.Models
{
    public class Comodidade
    {
        public Comodidade()
        {
            this.Quartos = new HashSet<Quarto>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        public string IconeCss { get; set; } // Ex: "fas fa-wifi"

        // Propriedade de navegação: Uma comodidade pode estar em vários quartos
        public virtual ICollection<Quarto> Quartos { get; set; }
    }
}