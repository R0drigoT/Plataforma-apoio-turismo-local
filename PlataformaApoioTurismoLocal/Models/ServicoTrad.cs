using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProjetoFim.Models
{
    public class ServicoTrad
    {
        public int Id { get; set; }

        [Required] public int ServicoId { get; set; }
        [Required, MaxLength(5)] public string Cultura { get; set; }
        [Required, MaxLength(200)] public string Nome { get; set; }
        public string Descricao { get; set; }

        public virtual Servico Servico { get; set; }

    }
}