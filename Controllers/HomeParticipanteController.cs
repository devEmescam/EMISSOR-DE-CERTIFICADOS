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

        public Home_ParticipanteController(ISessao sessao, IPessoaEventosRepository pessoaEventosRepository, IPessoaService pessoaService)
        {     
            _sessao = sessao ?? throw new ArgumentNullException(nameof(sessao), "O ISessao não pode ser nulo.");
            _pessoaEventosRepository = pessoaEventosRepository ?? throw new ArgumentNullException(nameof(pessoaEventosRepository), "O PessoaEventosRepository não pode ser nulo.");
            _pessoaService = pessoaService ?? throw new ArgumentNullException(nameof(pessoaService), "O IPessoaService não pode ser nulo.");
        }

        #region *** IActionResults ***     
        [HttpGet]
        public async Task<IActionResult> Index()
        {            
            try
            {               
                // Recupera todos os eventos do banco de dados
                var eventos = await BuscarTodosCeritificadosAsync();
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
                HttpContext.Session.Clear();
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

        #region *** METODOS ***
        private async Task<IEnumerable<EventoPessoa>> BuscarTodosCeritificadosAsync()
        {
            string cpf = string.Empty;
            int idPessoa = -1;           

            try
            {
                // Usuario participante loga com cpf
                cpf = HttpContext.Session.GetString("Login");

                if (cpf == null)
                {
                    throw new Exception("Login do usuário não encontrado na sessão.");
                }

                //Cria uma instancia de pessoaController para chamar os metodos contidos nessa classe                

                idPessoa = await _pessoaService.ObterIdPessoaPorCPFAsync(cpf);
                var eventos = await _pessoaEventosRepository.CarregarEventosPessoa(idPessoa, -1, false);
                return eventos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [Home_ParticipanteController.BuscarTodosCeritificadosAsync]: {ex.Message}");
            }
        }
        #endregion
    }
}