using EMISSOR_DE_CERTIFICADOS.Models;
using static EMISSOR_DE_CERTIFICADOS.Repositories.PessoaEventosRepository;

namespace EMISSOR_DE_CERTIFICADOS.Interfaces
{
    public interface IPessoaService
    {
        Task<IEnumerable<Pessoa>> BuscarPorNomeCpfEmailAsync(string termo);
        Task<IEnumerable<PessoaModel>> BuscarTodasPessoasAsync();
        Task<PessoaModel> BuscarPessoaPorIdAsync(int id);
        Task<string> InserirPessoaAsync(PessoaModel pessoa);
        Task AtualizarPessoaAsync(PessoaModel pessoa);
        Task DeletarPessoaAsync(int id);
        Task<bool> ExistePessoaComCPFAsync(string cpf, int? userId = null);
        Task<int> ObterIdPessoaPorCPFAsync(string cpf);
        Task<string> ObterCPFPorIdPessoaAsync(int id);
        Task<string> ObterNomePorIdPessoaAsync(int id);
    }
}