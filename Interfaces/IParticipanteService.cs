namespace EMISSOR_DE_CERTIFICADOS.Interfaces
{
    public interface IParticipanteService
    {
        Task<IEnumerable<EventoPessoa>> BuscarTodosCertificadosAsync();
    }
}
