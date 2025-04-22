using EMISSOR_DE_CERTIFICADOS.Models;
using EMISSOR_DE_CERTIFICADOS.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Newtonsoft.Json;
using EMISSOR_DE_CERTIFICADOS.Interfaces;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    // CONTROLLER QUE TRATA OS EVENTOS
    public class Home_OrganizadorController : Controller
    {
        private readonly ISessao _sessao;
        private readonly IOrganizadorService _organizadorService;
        private readonly IUsuarioService _usuarioService;

        // Construtor unificado
        public Home_OrganizadorController(ISessao sessao, IOrganizadorService organizadorService, IUsuarioService usuarioService)
        {
            _organizadorService = organizadorService ?? throw new ArgumentNullException(nameof(organizadorService), "O IOrganizadorService não pode ser nulo.");
            _sessao = sessao ?? throw new ArgumentNullException(nameof(sessao), "O ISessao não pode ser nulo.");
            _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService), "O IUsuarioService não pode ser nulo.");
        }

        #region *** IActionResults ***        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Recupera todos os eventos do banco de dados
                var eventos = await _organizadorService.BuscarTodosEventosAsync();

                // Passa o login para a view através do modelo
                ViewBag.Login = HttpContext.Session.GetString("Login");
                ViewBag.Setor = HttpContext.Session.GetString("Setor");

                return View(eventos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em [Home_OrganizadorController.Index] Erro: {ex.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NovoEvento(string nomeEvento, IFormFile arteCertificadoFile, string tableData)
        {
            EventoModel evento = new EventoModel();

            try
            {
                evento = new EventoModel
                {
                    Nome = nomeEvento,
                    ImagemCertificado = arteCertificadoFile
                };

                if (!string.IsNullOrEmpty(tableData))
                {
                    var tabelaDataList = JsonConvert.DeserializeObject<List<TabelaData>>(tableData);

                    // Registra o evento no banco de dados
                    await _organizadorService.InserirEventoAsync(evento, tabelaDataList);
                }
                else
                {
                    throw new Exception("Não foi possível identificar os registros de participantes.");
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em [Home_OrganizadorController.NovoEvento]. Erro: {ex.Message}");
            }
        }

        [HttpGet]
        // Usado para carregar dados no card que adicionará novas pessoas ao evento registrado em banco de dados
        public async Task<IActionResult> ObterPessoasEvento(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return StatusCode(500, new { success = false, message = "Não foi possível identificar o evento." });
                }
                var eventoPessoas = await _organizadorService.ObterEventoPessoas(id, false);
                return Json(eventoPessoas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em [Home_OrganizadorController.ObterPessoasEvento]");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Adiciona novas pessoas ao evento registrado em banco de dados        
        public async Task<IActionResult> AtualizarPessoasEvento(int id, string tableData)
        {
            try
            {
                if (id <= 0)
                {
                    throw new Exception("Não foi possível identificar o evento.");
                }

                if (string.IsNullOrEmpty(tableData))
                {
                    throw new Exception("Não foi possível identificar os registros de participantes.");
                }

                var tabelaDataList = JsonConvert.DeserializeObject<List<TabelaData>>(tableData);
                await _organizadorService.AtualizarPessoasEventoAsync(id, tabelaDataList);

                // Certifique-se de que o retorno é um JSON válido
                return Json(new { redirectUrl = Url.Action(nameof(Index)) });
            }
            catch (Exception ex)
            {
                // Mesmo em caso de erro, retorne uma resposta JSON
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> VisualizarImagem(int id)
        {
            try
            {
                byte[] imagemBytes = await _organizadorService.BuscarArteCertificadoEmBytesNoBDAsync(id);

                // Retorna a imagem como um arquivo para o navegador
                return File(imagemBytes, "image/jpeg");
            }
            catch (Exception ex)
            {
                // Se ocorrer algum erro, retorna um status 500 (Internal Server Error)                
                return StatusCode(500, $"Erro em [Home_OrganizadorController.VisualizarImagem]. Erro: {ex.Message}");
            }
        }

        [HttpGet("/Home_Organizador/VisualizarCertificadoParticipante/{idEventoPessoa}")]
        public async Task<IActionResult> VisualizarCertificadoParticipante(int idEventoPessoa)
        {
            try
            {                
                byte[] imagemBytes = await _organizadorService.BuscarCertificadoParticipanteEmBytesNoBDAsync(idEventoPessoa);

                // Retorna a imagem como um arquivo para o navegador
                return File(imagemBytes, "image/jpeg");
            }
            catch (Exception ex)
            {
                // Se ocorrer algum erro, retorna um status 500 (Internal Server Error)                
                return StatusCode(500, $"Erro em [Home_OrganizadorController.VisualizarCertificadoParticipante]. Erro: {ex.Message}");
            }
        }


        // Chamado pela ação de tela referente a emissão dos certificados das pessoas do evento        
        public async Task<IActionResult> DetalhesEventoPessoas(int idEvento)
        {
            try
            {
                // Obter os detalhes do evento e das pessoas
                var evento = await _organizadorService.ObterEventoPessoas(idEvento, true);

                // Validação do retorno
                if (evento == null)
                {
                    return NotFound(new { message = "Evento não encontrado." });
                }

                // Retornar os detalhes como JSON
                return Json(evento);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Ocorreu um erro em [Home_OrganizadorController.DetalhesEventoPessoas]. Erro: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Rotina de Emissão de certificados                
        public async Task<IActionResult> EmitirCertificado(int id, List<int> idPessoas)
        {
            try
            {
                if (idPessoas.Count == 0)
                {
                    return StatusCode(500, new { success = false, message = "Nenhuma pessoa selecionada para emissão de certificado." });
                }

                EventoModel evento = await _organizadorService.BuscarEventoPorIdAsync(id);

                if (ModelState.IsValid)
                {
                    await _organizadorService.EmitirCertificadoAsync(evento, idPessoas);
                }
                else
                {
                    return NotFound(new { success = false, message = "Evento não encontrado." });
                }

                // Obter um objeto atualizado com os dados do processo de emissão que foi realizado
                var eventoPessoas = await _organizadorService.ObterEventoPessoas(id, true);

                // Return JSON objeto com status de sucesso
                return Json(new { success = true, data = eventoPessoas });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Ocorreu um erro em Home_OrganizadorController.EmitirCertificado. Erro: {ex.Message}" });
            }
        }

        public async Task<IActionResult> ObterEmailConfig()
        {
            try
            {
                var emailConfig = await _organizadorService.ObterEmailConfigAsync();
                return View(emailConfig);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Ocorreu um erro em Home_OrganizadorController.ObterEmailConfig. Erro: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            try
            {
                // Limpe os dados da sessão para desconectar o usuário
                HttpContext.Session.Clear();
                _sessao.RemoverSessaoUsuario();

                // Redirecionar para a página de login do organizador                
                return View("~/Views/Login_Organizador/Login_organizador.cshtml");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao encerrar a sessão: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult CheckSession()
        {
            var user = _sessao.BuscarSessaodoUsuario();
            if (user == null)
            {
                return Unauthorized();
            }
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CadastrarUsuario(string login, string senha)
        {
            try
            {
                var result = await _usuarioService.CriarNovoUsuarioAsync(login);

                if (result)
                {
                    return Json(new { success = true, message = "Usuário cadastrado com sucesso!" });
                }
                else
                {
                    return Json(new { success = false, message = "Erro ao cadastrar o usuário." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro ao cadastrar o usuário: {ex.Message}");
            }
        }

        #endregion
    }
}
