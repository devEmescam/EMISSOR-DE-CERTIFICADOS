using EMISSOR_DE_CERTIFICADOS.Helpers;
using EMISSOR_DE_CERTIFICADOS.Interfaces;
using static EMISSOR_DE_CERTIFICADOS.Repositories.PessoaEventosRepository;

namespace EMISSOR_DE_CERTIFICADOS.Services
{
    public class ParticipanteService
    {
        private readonly ISessao _sessao;
        private readonly IPessoaService _pessoaService;
        private readonly IPessoaEventosRepository _pessoaEventosRepository;
        public ParticipanteService(ISessao sessao, IPessoaService pessoaService, IPessoaEventosRepository pessoaEventosRepository) 
        {
            _sessao = sessao;
            _pessoaService = pessoaService;
            _pessoaEventosRepository = pessoaEventosRepository;
        }

        public async Task<IEnumerable<EventoPessoa>> BuscarTodosCeritificadosAsync()
        {
            string cpf = string.Empty;
            int idPessoa = -1;

            try
            {
                // Usuario participante loga com cpf                
                cpf = _sessao.ObterUsuarioLogin();
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
    }
}