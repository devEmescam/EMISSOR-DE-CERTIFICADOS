using System.Data;

namespace EMISSOR_DE_CERTIFICADOS.Interfaces
{
    public interface IOrganizadorRepository
    {
        Task<DataTable> BuscarTodosEventosAsync(int userId);
        Task<DataTable> BuscarEventoPorIdAsync(int id);
        Task<int?> InserirEventoAsync(string nome, byte[] imagemCertificado, int idUsuario);
        Task InserirEventoPessoaAsync(int idEvento, int idPessoa, string texto);
        Task AtualizarTextoFrenteEventoPessoaAsync(int idEvento, int idPessoa, string texto);
        Task<byte[]> BuscarBytesDaImagemNoBDAsync(int id);
        Task InserirCertificadoAsync(int idEvento, int idPessoa);
        Task<DataTable> ObterEmailConfigAsync();
    }
}