using EMISSOR_DE_CERTIFICADOS.Helpers;
using EMISSOR_DE_CERTIFICADOS.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    public class Home_ParticipanteController : Controller
    {
        private readonly ISessao _sessao;
        private readonly IPessoaEventosRepository _pessoaEventosRepository;
        private readonly IPessoaService _pessoaService;
        private readonly IParticipanteService _participanteService;

        public Home_ParticipanteController(ISessao sessao, IPessoaEventosRepository pessoaEventosRepository, IPessoaService pessoaService, IParticipanteService participanteService)
        {
            _sessao = sessao ?? throw new ArgumentNullException(nameof(sessao), "O ISessao não pode ser nulo.");
            _pessoaEventosRepository = pessoaEventosRepository ?? throw new ArgumentNullException(nameof(pessoaEventosRepository), "O PessoaEventosRepository não pode ser nulo.");
            _pessoaService = pessoaService ?? throw new ArgumentNullException(nameof(pessoaService), "O IPessoaService não pode ser nulo.");
            _participanteService = participanteService ?? throw new ArgumentNullException(nameof(participanteService), "O IParticipanteService não pode ser nulo.");
        }

        #region *** IActionResults ***
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Recupera todos os eventos do banco de dados
                var eventos = await _participanteService.BuscarTodosCertificadosAsync();

                // Busca o nome do usuario para passar para a view
                var cpf = HttpContext.Session.GetString("Login");
                if (string.IsNullOrEmpty(cpf))
                {
                    return RedirectToAction("Logout");
                }

                string usuario = await _pessoaService.ObterNomePorCPFAsync(cpf);
                ViewBag.Login = usuario;

                return View(eventos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em [Home_ParticipanteController.Index] Erro: {ex.Message}");
            }
        }

        // Nova rota para buscar a imagem do certificado
        [HttpGet]
        public async Task<IActionResult> ObterImagemCertificado(int idEventoPessoa)
        {
            try
            {
                var cpf = HttpContext.Session.GetString("Login");
                if (string.IsNullOrEmpty(cpf))
                {
                    return Json(new { sucesso = false, mensagem = "Usuário não autenticado." });
                }

                var idPessoa = await _pessoaService.ObterIdPessoaPorCPFAsync(cpf);
                var eventos = await _pessoaEventosRepository.CarregarEventosPessoaAsync(idPessoa, idPessoa, false);
                var evento = eventos?.FirstOrDefault(e => e.IdEventoPessoa == idEventoPessoa);

                if (evento != null && !string.IsNullOrEmpty(evento.ImagemCertificadoBase64))
                {
                    return Json(new { sucesso = true, imagemBase64 = evento.ImagemCertificadoBase64 });
                }
                return Json(new { sucesso = false, mensagem = "Certificado não encontrado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao obter imagem do certificado: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                _sessao.RemoverSessaoUsuario();
                return View("~/Views/Login_Participante/Login_Participante.cshtml");
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
        #endregion
    }
}
