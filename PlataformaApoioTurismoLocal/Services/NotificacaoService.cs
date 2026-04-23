using ProjetoFim.Models;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ProjetoFim.Services 
{
    public class NotificacaoService
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public async Task CriarNotificacaoAsync(string destinatarioId, string mensagem, string url)
        {
            var notificacao = new Notificacao
            {
                DestinatarioId = destinatarioId,
                Mensagem = mensagem,
                Url = url,
                DataCriacao = DateTime.Now,
                Lida = false
            };
            db.Notificacoes.Add(notificacao);
            await db.SaveChangesAsync();
        }
    }
}