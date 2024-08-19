namespace EMISSOR_DE_CERTIFICADOS.Interfaces
{
    public interface IValidarCertificadoService
    {
        Task<bool> Validar(string codigo);
        Task<IFormFile> ObterImagemCertificadoPorCodigo(string codigo);
    }
}
