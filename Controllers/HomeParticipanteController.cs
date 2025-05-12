using EMISSOR_DE_CERTIFICADOS.Helpers;
using EMISSOR_DE_CERTIFICADOS.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    [ApiController]
    [Route("api/home-participante")]
    public class HomeParticipanteController : ControllerBase
    {
        private readonly ISessao _sessao;
        private readonly IPessoaEventosRepository _pessoaEventosRepository;
        private readonly IPessoaService _pessoaService;
        private readonly IParticipanteService _participanteService;

        public HomeParticipanteController(ISessao sessao, IPessoaEventosRepository pessoaEventosRepository, IPessoaService pessoaService, IParticipanteService participanteService)
        {
            _sessao = sessao ?? throw new ArgumentNullException(nameof(sessao), "O ISessao não pode ser nulo.");
            _pessoaEventosRepository = pessoaEventosRepository ?? throw new ArgumentNullException(nameof(pessoaEventosRepository), "O PessoaEventosRepository não pode ser nulo.");
            _pessoaService = pessoaService ?? throw new ArgumentNullException(nameof(pessoaService), "O IPessoaService não pode ser nulo.");
            _participanteService = participanteService ?? throw new ArgumentNullException(nameof(participanteService), "O IParticipanteService não pode ser nulo.");
        }

        #region *** API Endpoints ***
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var eventos = await _participanteService.BuscarTodosCertificadosAsync();

                var cpf = HttpContext.Session.GetString("Login");
                if (string.IsNullOrEmpty(cpf))
                {
                    return Unauthorized(new { mensagem = "Usuário não autenticado." });
                }

                string usuario = await _pessoaService.ObterNomePorCPFAsync(cpf);
                return Ok(new { usuario, eventos });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Ocorreu um erro em [HomeParticipanteController.Index] Erro: {ex.Message}" });
            }
        }

        [HttpGet("ObterImagemCertificado/{idEventoPessoa}")]
        public async Task<IActionResult> ObterImagemCertificado(int idEventoPessoa)
        {
            try
            {
                var cpf = HttpContext.Session.GetString("Login");
                if (string.IsNullOrEmpty(cpf))
                {
                    return Unauthorized(new { mensagem = "Usuário não autenticado." });
                }

                var idPessoa = await _pessoaService.ObterIdPessoaPorCPFAsync(cpf);
                var eventos = await _pessoaEventosRepository.CarregarEventosPessoaAsync(idPessoa, idPessoa, false);
                var evento = eventos?.FirstOrDefault(e => e.IdEventoPessoa == idEventoPessoa);

                if (evento != null && !string.IsNullOrEmpty(evento.ImagemCertificadoBase64))
                {
                    return Ok(new { sucesso = true, imagemBase64 = evento.ImagemCertificadoBase64 });
                }
                return NotFound(new { sucesso = false, mensagem = "Certificado não encontrado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao obter imagem do certificado: {ex.Message}" });
            }
        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            try
            {
                _sessao.RemoverSessaoUsuario();
                return Ok(new { mensagem = "Sessão encerrada com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao encerrar a sessão: {ex.Message}" });
            }
        }

        [HttpGet("CheckSession")]
        public IActionResult CheckSession()
        {
            var user = _sessao.BuscarSessaodoUsuario();
            if (user == null)
            {
                return Unauthorized(new { mensagem = "Sessão não encontrada." });
            }
            return Ok(new { mensagem = "Sessão ativa." });
        }
        #endregion
    }
}
