using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ProjetoFim.Models
{
    public class QuartoTrad
    {
        public int Id { get; set; }

        [Required] public int QuartoId { get; set; }
        [Required, MaxLength(5)] public string Cultura { get; set; } 
        [Required, MaxLength(200)] public string Nome { get; set; }
        public string Descricao { get; set; }

        public virtual Quarto Quarto { get; set; }

    }

}