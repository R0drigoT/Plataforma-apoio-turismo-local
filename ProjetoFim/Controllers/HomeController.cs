using System.Web.Mvc;
using ProjetoFim.Models;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Web;
using System;
using System.Data.Entity;

namespace ProjetoFinal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Homepage()
        {
            return View();
        }
        public ActionResult Servicos()
        {
            return View();
        }
        public ActionResult DetalhesQuarto()
        {
            return View();
        }
        public ActionResult ResultadosPesquisa()
        {
            return View();
        }
        public ActionResult ResultadosServicos()
        {
            return View();
        }

        public ActionResult Reservas()
        {
            return View();
        }

        public ActionResult Favoritos()
        {
            return View();
        }

        public ActionResult Notificacoes()
        {
            return View();
        }

        public ActionResult Mensagens()
        {
            return View();
        }
        public ActionResult Perfil()
        {
            return View();
        }

        public ActionResult EditarPerfil()
        {
            return View();
        }
        public ActionResult DetalhesServico()
        {
            return View();
        }
        public ActionResult PaginaReserva()
        {
            return View();
        }
        public ActionResult PaginaPagamento()
        {
            return View();
        }
        public ActionResult MetodosPagamento()
        {
            return View();
        }
        public ActionResult Dashboard()
        {
            return View();
        }
        public ActionResult GerirAnuncios()
        {
            var db = new ApplicationDbContext();

            // Cria o nosso ViewModel
            var viewModel = new AnunciosViewModel
            {
                // Vai buscar a lista de quartos
                Quartos = db.Quartos.ToList(),
                // Vai buscar a lista de serviços
                Servicos = db.Servicos.ToList()
            };

            // Envia o ViewModel (que contém as duas listas) para a View
            return View(viewModel);
        }
        public ActionResult ListaReservas()
        {
            return View();
        }
        public ActionResult Calendario()
        {
            return View();
        }
        public ActionResult CaixaDeEntrada()
        {
            return View();
        }
        public ActionResult Estatisticas()
        {
            return View();
        }
        public ActionResult HistoricoTransacoes()
        {
            return View();
        }
        public ActionResult AdicionarQuarto_Passo1()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AdicionarQuarto_Passo1(Quarto quarto)
        {
            if (ModelState.IsValid)
            {
                // Guardamos o objeto "quarto" parcialmente preenchido na Sessão do utilizador
                // para o podermos usar nos próximos passos do assistente.
                Session["NovoQuarto"] = quarto;

                // Redirecionamos para o próximo passo do assistente
                return RedirectToAction("AdicionarQuarto_Passo2");
            }

            // Se houver um erro de validação, mostramos o formulário novamente.
            return View(quarto);
        }
        public ActionResult AdicionarQuarto_Passo2()
        {
            var quarto = Session["NovoQuarto"] as Quarto;
            if (quarto == null)
            {
                return RedirectToAction("AdicionarQuarto_Passo1");
            }
            return View(quarto);
        }

        // POST: Home/AdicionarQuarto_Passo2
        [HttpPost]
        public ActionResult AdicionarQuarto_Passo2(Quarto quarto)
        {
            var quartoDaSessao = Session["NovoQuarto"] as Quarto;
            if (quartoDaSessao == null)
            {
                return RedirectToAction("AdicionarQuarto_Passo1");
            }

            // Atualiza o objeto da sessão com os novos dados
            quartoDaSessao.NumeroCamasCasal = quarto.NumeroCamasCasal;
            quartoDaSessao.NumeroCamasSolteiro = quarto.NumeroCamasSolteiro;
            quartoDaSessao.NumeroCasasDeBanho = quarto.NumeroCasasDeBanho;
            quartoDaSessao.TemEstacionamento = quarto.TemEstacionamento;

            Session["NovoQuarto"] = quartoDaSessao;

            // Redireciona para o próximo passo (que pertence ao HomeController)
            return RedirectToAction("AdicionarQuarto_Passo3");
        }

        public ActionResult AdicionarQuarto_Passo3()
        {
            var quarto = Session["NovoQuarto"] as Quarto;
            if (quarto == null)
            {
                return RedirectToAction("AdicionarQuarto_Passo1");
            }

            // Vai buscar todas as comodidades disponíveis na base de dados
            var db = new ApplicationDbContext();
            ViewBag.TodasAsComodidades = db.Comodidades.ToList();

            return View(quarto);
        }

        // POST: Home/AdicionarQuarto_Passo3
        // ESTE MÉTODO FOI CORRIGIDO
        [HttpPost]
        public ActionResult AdicionarQuarto_Passo3(int[] comodidadesSelecionadas)
        {
            var quarto = Session["NovoQuarto"] as Quarto;
            if (quarto == null)
            {
                return RedirectToAction("AdicionarQuarto_Passo1");
            }

            // Em vez de guardarmos os objetos, guardamos apenas os IDs numa outra variável de sessão.
            // Isto evita o erro de múltiplos contextos.
            Session["ComodidadesSelecionadasIDs"] = comodidadesSelecionadas;

            // Redireciona para o próximo passo, como antes.
            return RedirectToAction("AdicionarQuarto_Passo4");
        }
        // GET: Home/AdicionarQuarto_Passo4
        public ActionResult AdicionarQuarto_Passo4()
        {
            var quarto = Session["NovoQuarto"] as Quarto;
            if (quarto == null)
            {
                return RedirectToAction("AdicionarQuarto_Passo1");
            }
            return View(quarto);
        }

        // POST: Home/AdicionarQuarto_Passo4
        [HttpPost]
        public ActionResult AdicionarQuarto_Passo4(IEnumerable<HttpPostedFileBase> files)
        {
            var quarto = Session["NovoQuarto"] as Quarto;
            if (quarto == null)
            {
                return RedirectToAction("AdicionarQuarto_Passo1");
            }

            // Lógica para guardar os ficheiros no servidor
            if (files != null && files.Any())
            {
                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        // Gera um nome de ficheiro único para evitar conflitos
                        var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                        var extension = Path.GetExtension(file.FileName);
                        fileName = $"{fileName}_{Guid.NewGuid()}{extension}";

                        // Define o caminho onde a imagem será guardada (ex: ~/Uploads/Imagens/)
                        var path = Path.Combine(Server.MapPath("~/Uploads/Imagens/"), fileName);

                        // Cria a pasta se ela não existir
                        Directory.CreateDirectory(Server.MapPath("~/Uploads/Imagens/"));

                        // Guarda o ficheiro no servidor
                        file.SaveAs(path);

                        // Adiciona a referência da imagem ao nosso objeto Quarto
                        quarto.Imagens.Add(new Imagem { Url = "/Uploads/Imagens/" + fileName });
                    }
                }
            }

            Session["NovoQuarto"] = quarto;

            // Redireciona para o próximo passo
            return RedirectToAction("AdicionarQuarto_Passo5");
        }
        // Adicione estes dois métodos ao seu HomeController.cs

        // GET: Home/AdicionarQuarto_Passo5
        public ActionResult AdicionarQuarto_Passo5()
        {
            var quarto = Session["NovoQuarto"] as Quarto;
            if (quarto == null)
            {
                return RedirectToAction("AdicionarQuarto_Passo1");
            }
            return View(quarto);
        }

        // POST: Home/AdicionarQuarto_Passo5
        [HttpPost]
        public ActionResult AdicionarQuarto_Passo5(Quarto quarto)
        {
            var quartoDaSessao = Session["NovoQuarto"] as Quarto;
            if (quartoDaSessao == null)
            {
                return RedirectToAction("AdicionarQuarto_Passo1");
            }

            // Atualiza o objeto com os últimos dados do formulário
            quartoDaSessao.PrecoPorNoite = quarto.PrecoPorNoite;

            // Recupera os IDs das comodidades da sessão
            var comodidadesIDs = Session["ComodidadesSelecionadasIDs"] as int[];

            // Agora, dentro de UMA ÚNICA LIGAÇÃO 'db', juntamos tudo
            using (var db = new ApplicationDbContext())
            {
                // Limpa a coleção para garantir que está vazia
                quartoDaSessao.Comodidades = new List<Comodidade>();

                // Adiciona as comodidades selecionadas ao quarto
                if (comodidadesIDs != null)
                {
                    foreach (var id in comodidadesIDs)
                    {
                        var comodidadeParaAdicionar = db.Comodidades.Find(id);
                        if (comodidadeParaAdicionar != null)
                        {
                            quartoDaSessao.Comodidades.Add(comodidadeParaAdicionar);
                        }
                    }
                }

                // Adicionamos o quarto completo ao contexto
                db.Quartos.Add(quartoDaSessao);

                // Salvamos todas as alterações de uma vez
                db.SaveChanges();
            }

            // Limpamos as sessões para poder adicionar um novo quarto de seguida
            Session.Remove("NovoQuarto");
            Session.Remove("ComodidadesSelecionadasIDs");

            // Redirecionamos para a lista de anúncios, onde o novo quarto deverá aparecer
            return RedirectToAction("GerirAnuncios", "Home"); // Corrigido para apontar para o HomeController
        }
        public ActionResult ModificarQuarto_Passo1(int id)
        {
            var db = new ApplicationDbContext();
            // Vai buscar o quarto e as suas relações (Comodidades, Imagens) à base de dados
            var quartoParaEditar = db.Quartos
                                     .Include(q => q.Comodidades)
                                     .Include(q => q.Imagens)
                                     .FirstOrDefault(q => q.Id == id);

            if (quartoParaEditar == null)
            {
                return HttpNotFound();
            }

            // Limpa sessões antigas e inicia uma nova com o objeto completo
            Session.Remove("QuartoEmEdicao");
            Session["QuartoEmEdicao"] = quartoParaEditar;

            return View("AdicionarQuarto_Passo1", quartoParaEditar);
        }

        // POST: Home/ModificarQuarto_Passo1
        [HttpPost]
        public ActionResult ModificarQuarto_Passo1(int id, Quarto quarto)
        {
            var quartoDaSessao = Session["QuartoEmEdicao"] as Quarto;
            if (quartoDaSessao == null) return RedirectToAction("GerirAnuncios");

            // Atualiza apenas os dados deste passo
            quartoDaSessao.Nome = quarto.Nome;
            quartoDaSessao.Descricao = quarto.Descricao;
            quartoDaSessao.Localizacao = quarto.Localizacao;

            Session["QuartoEmEdicao"] = quartoDaSessao;

            return RedirectToAction("ModificarQuarto_Passo2", new { id = id });
        }

        // GET: Home/ModificarQuarto_Passo2/5
        public ActionResult ModificarQuarto_Passo2(int id)
        {
            var quarto = Session["QuartoEmEdicao"] as Quarto;
            if (quarto == null || quarto.Id != id) return RedirectToAction("GerirAnuncios");

            return View("AdicionarQuarto_Passo2", quarto);
        }

        // POST: Home/ModificarQuarto_Passo2
        [HttpPost]
        public ActionResult ModificarQuarto_Passo2(int id, Quarto quarto)
        {
            var quartoDaSessao = Session["QuartoEmEdicao"] as Quarto;
            if (quartoDaSessao == null) return RedirectToAction("GerirAnuncios");

            quartoDaSessao.NumeroCamasCasal = quarto.NumeroCamasCasal;
            quartoDaSessao.NumeroCamasSolteiro = quarto.NumeroCamasSolteiro;
            quartoDaSessao.NumeroCasasDeBanho = quarto.NumeroCasasDeBanho;
            quartoDaSessao.TemEstacionamento = quarto.TemEstacionamento;

            Session["QuartoEmEdicao"] = quartoDaSessao;

            return RedirectToAction("ModificarQuarto_Passo3", new { id = id });
        }

        // GET: Home/ModificarQuarto_Passo3/5
        public ActionResult ModificarQuarto_Passo3(int id)
        {
            var quarto = Session["QuartoEmEdicao"] as Quarto;
            if (quarto == null) return RedirectToAction("GerirAnuncios");

            var db = new ApplicationDbContext();
            ViewBag.TodasAsComodidades = db.Comodidades.ToList();

            return View("AdicionarQuarto_Passo3", quarto);
        }

        /// No seu HomeController.cs

        // POST: Home/ModificarQuarto_Passo3
        [HttpPost]
        public ActionResult ModificarQuarto_Passo3(int id, int[] comodidadesSelecionadas)
        {
            var quartoParaEditar = Session["QuartoEmEdicao"] as Quarto;
            if (quartoParaEditar == null || quartoParaEditar.Id != id)
            {
                return RedirectToAction("GerirAnuncios");
            }

            // Apenas guardamos os IDs selecionados numa variável de sessão separada
            Session["ComodidadesSelecionadasIDs"] = comodidadesSelecionadas;

            // Redireciona para o próximo passo da edição
            return RedirectToAction("ModificarQuarto_Passo4", new { id = quartoParaEditar.Id });
        }

        // GET: Home/ModificarQuarto_Passo4/5
        public ActionResult ModificarQuarto_Passo4(int id)
        {
            var quarto = Session["QuartoEmEdicao"] as Quarto;
            if (quarto == null) return RedirectToAction("GerirAnuncios");

            return View("AdicionarQuarto_Passo4", quarto);
        }

        // POST: Home/ModificarQuarto_Passo4
        [HttpPost]
        public ActionResult ModificarQuarto_Passo4(int id, IEnumerable<HttpPostedFileBase> files)
        {
            // Se o utilizador selecionou novos ficheiros, guardamo-los na sessão para usar no passo final
            if (files != null && files.Any(f => f != null))
            {
                Session["NovasImagens"] = files.ToList(); // Guardar a lista de ficheiros
            }

            // Redireciona para o próximo passo da edição
            return RedirectToAction("ModificarQuarto_Passo5", new { id = id });
        }

        // GET: Home/ModificarQuarto_Passo5/5
        public ActionResult ModificarQuarto_Passo5(int id)
        {
            var quarto = Session["QuartoEmEdicao"] as Quarto;
            if (quarto == null || quarto.Id != id) return RedirectToAction("GerirAnuncios");

            return View("AdicionarQuarto_Passo5", quarto);
        }

        // POST: Home/ModificarQuarto_Passo5
        // No seu HomeController.cs

        // No seu HomeController.cs

        [HttpPost]
        public ActionResult ModificarQuarto_Passo5(int id, Quarto quartoDoFormulario)
        {
            var quartoComAlteracoes = Session["QuartoEmEdicao"] as Quarto;
            if (quartoComAlteracoes == null || quartoComAlteracoes.Id != id)
            {
                return RedirectToAction("GerirAnuncios");
            }

            using (var db = new ApplicationDbContext())
            {
                var quartoParaAtualizar = db.Quartos.Include(q => q.Comodidades).Include(q => q.Imagens).FirstOrDefault(q => q.Id == id);
                if (quartoParaAtualizar == null)
                {
                    return HttpNotFound();
                }

                // 1. ATUALIZA OS DADOS BÁSICOS (Passos 1, 2 e 5)
                quartoParaAtualizar.Nome = quartoComAlteracoes.Nome;
                quartoParaAtualizar.Descricao = quartoComAlteracoes.Descricao;
                quartoParaAtualizar.Localizacao = quartoComAlteracoes.Localizacao;
                quartoParaAtualizar.NumeroCamasCasal = quartoComAlteracoes.NumeroCamasCasal;
                quartoParaAtualizar.NumeroCamasSolteiro = quartoComAlteracoes.NumeroCamasSolteiro;
                quartoParaAtualizar.NumeroCasasDeBanho = quartoComAlteracoes.NumeroCasasDeBanho;
                quartoParaAtualizar.TemEstacionamento = quartoComAlteracoes.TemEstacionamento;
                quartoParaAtualizar.PrecoPorNoite = quartoDoFormulario.PrecoPorNoite;

                // 2. ATUALIZA AS COMODIDADES (Passo 3)
                var comodidadesIDs = Session["ComodidadesSelecionadasIDs"] as int[];
                quartoParaAtualizar.Comodidades.Clear();
                if (comodidadesIDs != null)
                {
                    foreach (var comId in comodidadesIDs)
                    {
                        var comodidade = db.Comodidades.Find(comId);
                        if (comodidade != null) quartoParaAtualizar.Comodidades.Add(comodidade);
                    }
                }

                // 3. ATUALIZA AS IMAGENS (Passo 4)
                var novasImagens = Session["NovasImagens"] as List<HttpPostedFileBase>;
                if (novasImagens != null)
                {
                    // Apaga as imagens antigas
                    foreach (var imgAntiga in quartoParaAtualizar.Imagens.ToList())
                    {
                        var caminhoFisico = Server.MapPath(imgAntiga.Url);
                        if (System.IO.File.Exists(caminhoFisico))
                        {
                            System.IO.File.Delete(caminhoFisico);
                        }
                        db.Imagens.Remove(imgAntiga);
                    }

                    // Adiciona as imagens novas
                    foreach (var file in novasImagens)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            var fileName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + Guid.NewGuid() + Path.GetExtension(file.FileName);
                            var path = Path.Combine(Server.MapPath("~/Uploads/Imagens/"), fileName);
                            Directory.CreateDirectory(Server.MapPath("~/Uploads/Imagens/"));
                            file.SaveAs(path);
                            quartoParaAtualizar.Imagens.Add(new Imagem { Url = "/Uploads/Imagens/" + fileName, QuartoId = quartoParaAtualizar.Id });
                        }
                    }
                }

                // 4. GUARDA TUDO NA BASE DE DADOS
                db.SaveChanges();
            }

            // Limpa todas as sessões usadas no assistente
            Session.Remove("QuartoEmEdicao");
            Session.Remove("ComodidadesSelecionadasIDs");
            Session.Remove("NovasImagens");

            return RedirectToAction("GerirAnuncios");
        }
        // Adicione este método ao seu HomeController.cs

        [HttpPost]
        public ActionResult RemoverQuarto(int id)
        {
            using (var db = new ApplicationDbContext())
            {
                // Encontra o quarto na base de dados, incluindo as suas imagens e comodidades
                var quartoParaRemover = db.Quartos
                                          .Include(q => q.Imagens)
                                          .Include(q => q.Comodidades)
                                          .FirstOrDefault(q => q.Id == id);

                if (quartoParaRemover != null)
                {
                    // Apaga os ficheiros de imagem do servidor
                    foreach (var imagem in quartoParaRemover.Imagens.ToList())
                    {
                        var caminhoFisico = Server.MapPath(imagem.Url);
                        if (System.IO.File.Exists(caminhoFisico))
                        {
                            System.IO.File.Delete(caminhoFisico);
                        }
                    }

                    // Remove o quarto da base de dados (o Entity Framework trata de remover
                    // as imagens e as relações com as comodidades automaticamente)
                    db.Quartos.Remove(quartoParaRemover);
                    db.SaveChanges();
                }
            }
            // Redireciona de volta para a lista atualizada
            return RedirectToAction("GerirAnuncios");
        }
        // Adicione estes dois métodos ao seu HomeController.cs

        // GET: Home/ModificarServico/5
        // Mostra o formulário de edição pré-preenchido com os dados do serviço
        public ActionResult ModificarServico(int id)
        {
            var db = new ApplicationDbContext();
            var servico = db.Servicos.Find(id);
            if (servico == null)
            {
                return HttpNotFound();
            }
            return View(servico);
        }

        // POST: Home/ModificarServico/5
        // Recebe os dados alterados e guarda-os na base de dados
        [HttpPost]
        public ActionResult ModificarServico(Servico servico)
        {
            if (ModelState.IsValid)
            {
                using (var db = new ApplicationDbContext())
                {
                    // Diz ao Entity Framework que este objeto foi modificado
                    db.Entry(servico).State = EntityState.Modified;
                    db.SaveChanges();
                }
                // Redireciona de volta para a lista de anúncios
                return RedirectToAction("GerirAnuncios");
            }
            // Se a validação falhar, volta a mostrar o formulário com os dados
            return View(servico);
        }
        // Adicione este método ao seu HomeController.cs

        [HttpPost]
        public ActionResult RemoverServico(int id)
        {
            using (var db = new ApplicationDbContext())
            {
                // Encontra o serviço na base de dados pelo ID
                var servicoParaRemover = db.Servicos.Find(id);

                if (servicoParaRemover != null)
                {
                    // Remove o serviço da base de dados
                    db.Servicos.Remove(servicoParaRemover);
                    db.SaveChanges();
                }
            }
            // Redireciona de volta para a lista de anúncios atualizada
            return RedirectToAction("GerirAnuncios");
        }

        public ActionResult AdicionarServico()
        {
            return View();
        }

        // POST: Home/AdicionarServico
        // Esta ação recebe os dados do formulário e guarda na base de dados.
        [HttpPost]
        public ActionResult AdicionarServico(Servico servico)
        {
            if (ModelState.IsValid)
            {
                using (var db = new ApplicationDbContext())
                {
                    db.Servicos.Add(servico);
                    db.SaveChanges();
                }
                return RedirectToAction("GerirAnuncios");
            }

            // Se a validação falhar, volta a mostrar o formulário.
            return View(servico);
        }
    }
}