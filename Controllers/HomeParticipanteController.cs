using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Helpers;
using EMISSOR_DE_CERTIFICADOS.Interfaces;
using EMISSOR_DE_CERTIFICADOS.Repositories;
using Microsoft.AspNetCore.Mvc;
using static EMISSOR_DE_CERTIFICADOS.Repositories.PessoaEventosRepository;

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
                // Passa o login para a view através do modelo
                ViewBag.Login = HttpContext.Session.GetString("Login");
                return View(eventos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em [Home_ParticipanteController.Index] Erro: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Limpe os dados da sessão para desconectar o usuário                
                _sessao.RemoverSessaoUsuario();

                // Redirecionar para a página de login do organizador         
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