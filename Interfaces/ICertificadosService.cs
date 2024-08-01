namespace EMISSOR_DE_CERTIFICADOS.Interfaces
{
    public interface ICertificadosService
    {
        Task<bool> GerarCertificadoAsync(int idEvento_Pessoa, int idPessoa, string textoOriginal, IFormFile imagem);
    }
}