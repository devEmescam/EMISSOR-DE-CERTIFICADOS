using EMISSOR_DE_CERTIFICADOS.Models;
using EMISSOR_DE_CERTIFICADOS.Repositories;

namespace EMISSOR_DE_CERTIFICADOS.Interfaces
{
    public interface IOrganizadorService
    {
        Task<IEnumerable<EventoModel>> BuscarTodosEventosAsync();
        Task<EventoModel> BuscarEventoPorIdAsync(int id);
        Task InserirEventoAsync(EventoModel evento, List<TabelaData>? dadosTabela);
        Task AtualizarPessoasEventoAsync(int id, List<TabelaData>? dadosTabela);
        Task<byte[]> BuscarBytesDaImagemNoBDAsync(int id);
        Task<Evento> ObterEventoPessoas(int idEvento, bool emitirCertificado);
        Task EmitirCertificadoAsync(EventoModel evento, List<int> listaIdPessoas);
        Task<EmailConfigModel> ObterEmailConfigAsync();
    }
}
