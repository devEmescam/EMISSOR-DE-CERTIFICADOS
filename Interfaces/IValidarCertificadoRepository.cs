using System.Data;

namespace EMISSOR_DE_CERTIFICADOS.Interfaces
{
    public interface IValidarCertificadoRepository
    {
        Task<bool> ValidarCodigoCertificado(string codigo);
        Task<DataTable> ObterImagemCertificadoPorCodigo(string codigo);
    }
}
