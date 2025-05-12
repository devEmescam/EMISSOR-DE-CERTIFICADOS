using EMISSOR_DE_CERTIFICADOS.Models;
using EMISSOR_DE_CERTIFICADOS.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Newtonsoft.Json;
using EMISSOR_DE_CERTIFICADOS.Interfaces;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    [ApiController]
    [Route("api/home-organizador")]
    public class HomeOrganizadorController : ControllerBase
    {
        private readonly ISessao _sessao;
        private readonly IOrganizadorService _organizadorService;
        private readonly IUsuarioService _usuarioService;

        public HomeOrganizadorController(ISessao sessao, IOrganizadorService organizadorService, IUsuarioService usuarioService)
        {
            _organizadorService = organizadorService ?? throw new ArgumentNullException(nameof(organizadorService), "O IOrganizadorService não pode ser nulo.");
            _sessao = sessao ?? throw new ArgumentNullException(nameof(sessao), "O ISessao não pode ser nulo.");
            _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService), "O IUsuarioService não pode ser nulo.");
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var eventos = await _organizadorService.BuscarTodosEventosAsync();
                return Ok(eventos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em [HomeOrganizadorController.Index] Erro: {ex.Message}");
            }
        }

        [HttpPost("NovoEvento")]
        public async Task<IActionResult> NovoEvento(string nomeEvento, IFormFile arteCertificadoFile, string tableData)
        {
            try
            {
                if (string.IsNullOrEmpty(tableData))
                {
                    throw new Exception("Não foi possível identificar os registros de participantes.");
                }

                var tabelaDataList = JsonConvert.DeserializeObject<List<TabelaData>>(tableData);
                var evento = new EventoModel
                {
                    Nome = nomeEvento,
                    ImagemCertificado = arteCertificadoFile
                };

                await _organizadorService.InserirEventoAsync(evento, tabelaDataList);
                return Ok(new { success = true, message = "Evento criado com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em [HomeOrganizadorController.NovoEvento]. Erro: {ex.Message}");
            }
        }

        [HttpGet("ObterPessoasEvento/{id}")]
        public async Task<IActionResult> ObterPessoasEvento(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "Não foi possível identificar o evento." });
                }

                var eventoPessoas = await _organizadorService.ObterEventoPessoas(id, false);
                return Ok(eventoPessoas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em [HomeOrganizadorController.ObterPessoasEvento]. Erro: {ex.Message}");
            }
        }

        [HttpPost("AtualizarPessoasEvento/{id}")]
        public async Task<IActionResult> AtualizarPessoasEvento(int id, string tableData)
        {
            try
            {
                if (id <= 0 || string.IsNullOrEmpty(tableData))
                {
                    throw new Exception("Dados inválidos para atualização.");
                }

                var tabelaDataList = JsonConvert.DeserializeObject<List<TabelaData>>(tableData);
                await _organizadorService.AtualizarPessoasEventoAsync(id, tabelaDataList);

                return Ok(new { success = true, message = "Pessoas atualizadas com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("VisualizarImagem/{id}")]
        public async Task<IActionResult> VisualizarImagem(int id)
        {
            try
            {
                byte[] imagemBytes = await _organizadorService.BuscarBytesDaImagemNoBDAsync(id);
                return File(imagemBytes, "image/jpeg");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro em [HomeOrganizadorController.VisualizarImagem]. Erro: {ex.Message}");
            }
        }

        [HttpGet("DetalhesEventoPessoas/{idEvento}")]
        public async Task<IActionResult> DetalhesEventoPessoas(int idEvento)
        {
            try
            {
                var evento = await _organizadorService.ObterEventoPessoas(idEvento, true);
                if (evento == null)
                {
                    return NotFound(new { message = "Evento não encontrado." });
                }

                return Ok(evento);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Ocorreu um erro em [HomeOrganizadorController.DetalhesEventoPessoas]. Erro: {ex.Message}" });
            }
        }

        [HttpPost("EmitirCertificado/{id}")]
        public async Task<IActionResult> EmitirCertificado(int id, List<int> idPessoas)
        {
            try
            {
                if (idPessoas.Count == 0)
                {
                    return BadRequest(new { success = false, message = "Nenhuma pessoa selecionada para emissão de certificado." });
                }

                var evento = await _organizadorService.BuscarEventoPorIdAsync(id);
                if (evento == null)
                {
                    return NotFound(new { success = false, message = "Evento não encontrado." });
                }

                await _organizadorService.EmitirCertificadoAsync(evento, idPessoas);
                var eventoPessoas = await _organizadorService.ObterEventoPessoas(id, true);

                return Ok(new { success = true, data = eventoPessoas });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Ocorreu um erro em HomeOrganizadorController.EmitirCertificado. Erro: {ex.Message}" });
            }
        }

        [HttpGet("ObterEmailConfig")]
        public async Task<IActionResult> ObterEmailConfig()
        {
            try
            {
                var emailConfig = await _organizadorService.ObterEmailConfigAsync();
                return Ok(emailConfig);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Ocorreu um erro em HomeOrganizadorController.ObterEmailConfig. Erro: {ex.Message}" });
            }
        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            try
            {
                HttpContext.Session.Clear();
                _sessao.RemoverSessaoUsuario();
                return Ok(new { success = true, message = "Logout realizado com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao encerrar a sessão: {ex.Message}");
            }
        }

        [HttpGet("CheckSession")]
        public IActionResult CheckSession()
        {
            var user = _sessao.BuscarSessaodoUsuario();
            if (user == null)
            {
                return Unauthorized();
            }
            return Ok();
        }

        [HttpPost("CadastrarUsuario")]
        public async Task<IActionResult> CadastrarUsuario(string login, string senha)
        {
            try
            {
                var result = await _usuarioService.CriarNovoUsuarioAsync(login);
                if (result)
                {
                    return Ok(new { success = true, message = "Usuário cadastrado com sucesso!" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Erro ao cadastrar o usuário." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro ao cadastrar o usuário: {ex.Message}");
            }
        }
    }
}
