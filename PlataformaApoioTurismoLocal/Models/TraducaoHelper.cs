using System.Globalization;
using System.Linq;

namespace ProjetoFim.Models
{
    public static class TraducaoHelper
    {
        public static (string Nome, string Descricao) GetQuartoTrad(Quarto q, string cultura = null)
        {
            if (cultura == null)
                cultura = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

            var t = q.Traducoes.FirstOrDefault(x => x.Cultura == cultura)
                    ?? q.Traducoes.FirstOrDefault(x => x.Cultura == "pt");

            return (t != null ? t.Nome : q.Nome, t != null ? t.Descricao : q.Descricao);
        }

        public static (string Nome, string Descricao) GetServicoTrad(Servico s, string cultura = null)
        {
            if (cultura == null)
                cultura = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

            var t = s.Traducoes.FirstOrDefault(x => x.Cultura == cultura)
                    ?? s.Traducoes.FirstOrDefault(x => x.Cultura == "pt");

            return (t != null ? t.Nome : s.Nome, t != null ? t.Descricao : s.Descricao);
        }
    }
}
