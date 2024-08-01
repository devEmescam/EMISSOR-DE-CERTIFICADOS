using EMISSOR_DE_CERTIFICADOS.Repositories;

namespace EMISSOR_DE_CERTIFICADOS.Interfaces
{
    public interface IEventoPessoasRepository
    {
        Task<Evento> CarregarDadosAsync(int idEvento, bool emitirCertificado);           
    }
}