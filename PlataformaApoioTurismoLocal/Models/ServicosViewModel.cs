using System.Collections.Generic;

namespace ProjetoFim.Models
{
    public class ServicosViewModel
    {
        public List<Servico> ServicosMaisRequisitados { get; set; }
        public List<Servico> ServicosRecemAdicionados { get; set; }
        public List<Servico> ServicosComDesconto { get; set; }

        public ServicosViewModel()
        {
            ServicosMaisRequisitados = new List<Servico>();
            ServicosRecemAdicionados = new List<Servico>();
            ServicosComDesconto = new List<Servico>();
        }
    }
}