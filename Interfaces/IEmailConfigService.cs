using EMISSOR_DE_CERTIFICADOS.Models;

namespace EMISSOR_DE_CERTIFICADOS.Interfaces
{
    public interface IEmailConfigService
    {
        Task<EmailConfigModel> CarregarDadosAsync();
    }
}
