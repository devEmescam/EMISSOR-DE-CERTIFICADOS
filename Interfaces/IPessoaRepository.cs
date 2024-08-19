using EMISSOR_DE_CERTIFICADOS.Models;

namespace EMISSOR_DE_CERTIFICADOS.Interfaces
{
    public interface IPessoaRepository
    {
        Task<IEnumerable<PessoaModel>> BuscarTodasPessoasAsync();
        Task<PessoaModel> BuscarPessoaPorIdAsync(int id);
        Task InserirPessoaAsync(PessoaModel pessoa, int idUsuarioAdministrativo);
        Task AtualizarPessoaAsync(PessoaModel pessoa);
        Task DeletarPessoaAsync(int id);
        Task<bool> ExistePessoaComCPFAsync(string cpf, int? userId = null);
        Task<int> ObterIdPessoaPorCPFAsync(string cpf);
        Task<string> ObterCPFPorIdPessoaAsync(int id);
        Task<string> ObterNomePorIdPessoaAsync(int id);
        Task<string> ObterNomePorCPFAsync(string cpf);
    }
}