using EMISSOR_DE_CERTIFICADOS.Helpers;
using EMISSOR_DE_CERTIFICADOS.Interfaces;

namespace EMISSOR_DE_CERTIFICADOS.Services
{
    internal class ParticipanteService : IParticipanteService
    {
        private readonly ISessao _sessao;
        private readonly IPessoaRepository _pessoaRepository;
        private readonly IPessoaEventosRepository _pessoaEventosRepository;
        public ParticipanteService(ISessao sessao, IPessoaRepository pessoaRepositry, IPessoaEventosRepository pessoaEventosRepository) 
        {
            _sessao = sessao;
            _pessoaRepository = pessoaRepositry;
            _pessoaEventosRepository = pessoaEventosRepository;
        }

        public async Task<IEnumerable<EventoPessoa>> BuscarTodosCertificadosAsync()
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
                idPessoa = await _pessoaRepository.ObterIdPessoaPorCPFAsync(cpf);
                var eventos = await _pessoaEventosRepository.CarregarEventosPessoaAsync(idPessoa, -1, false);

                // Garante que nunca retorne null
                return eventos ?? new List<EventoPessoa>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [ParticipanteService.BuscarTodosCeritificadosAsync]: {ex.Message}");
            }
        }
    }
}