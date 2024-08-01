namespace EMISSOR_DE_CERTIFICADOS.Interfaces
{
    public interface ICertificadosRepository
    {
        Task InserirAsync(int idEventoPessoa, byte[] certificadoBytes, string codigoCertificado);
    }
}
