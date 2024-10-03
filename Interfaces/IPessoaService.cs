using EMISSOR_DE_CERTIFICADOS.Models;

namespace EMISSOR_DE_CERTIFICADOS.Interfaces
{
    public interface IPessoaService
    {
        Task<IEnumerable<Pessoa>> BuscarPorNomeCpfEmailAsync(string termo);
        Task<IEnumerable<EventoPessoa>> BuscarEventosPessoaAsync(int id);
        Task<bool> ReenviarInstrucoesAsync(int idEventoPessoa);
        Task<IEnumerable<PessoaModel>> BuscarTodasPessoasAsync();
        Task<PessoaModel> BuscarPessoaPorIdAsync(int id);
        Task<string> InserirPessoaAsync(PessoaModel pessoa);
        Task AtualizarPessoaAsync(PessoaModel pessoa);
        Task AtualizarEmailAsync(int id, string email);
        Task DeletarPessoaAsync(int id);
        Task<bool> ExistePessoaComCPFAsync(string cpf, int? userId = null);
        Task<int> ObterIdPessoaPorCPFAsync(string cpf);
        Task<string> ObterCPFPorIdPessoaAsync(int id);
        Task<string> ObterNomePorIdPessoaAsync(int id);
        Task<string> ObterNomePorCPFAsync(string cpf);

    }
}