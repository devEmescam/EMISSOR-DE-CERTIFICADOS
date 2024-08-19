using static EMISSOR_DE_CERTIFICADOS.Repositories.PessoaEventosRepository;

namespace EMISSOR_DE_CERTIFICADOS.Interfaces
{
    public interface IPessoaEventosRepository
    {
        Task<List<Pessoa>> CarregarDadosAsync(string termo, int idUsuario, bool visaoOrganizador);
        Task<List<EventoPessoa>> CarregarEventosPessoaAsync(int idPessoa, int idUsuario, bool visaoOrganizador);
    }
}