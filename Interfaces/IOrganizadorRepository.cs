using System.Data;
using System.Threading.Tasks; // Certifique-se de que este namespace está incluído.

namespace EMISSOR_DE_CERTIFICADOS.Interfaces
{
    public interface IOrganizadorRepository
    {
        // Remover a versão sem username
        Task<DataTable> BuscarTodosEventosAsync(int userId, string username);
        Task<DataTable> BuscarEventoPorIdAsync(int id);
        Task<int?> InserirEventoAsync(string nome, byte[] imagemCertificado, int idUsuario);
        Task InserirEventoPessoaAsync(int idEvento, int idPessoa, string texto);
        Task AtualizarTextoFrenteEventoPessoaAsync(int idEvento, int idPessoa, string texto);
        Task<byte[]> BuscarBytesDaImagemNoBDAsync(int id);
        Task<DataTable> ObterEmailConfigAsync();
        Task<DataTable> BuscarPessoasEventoAsync(int eventoId, string idPessoas);
        Task AtualizarCertificadoEmitidoAsync(int idEventoPessoa, bool certificadoEmitido, string mensagemRetorno);
    }
}
