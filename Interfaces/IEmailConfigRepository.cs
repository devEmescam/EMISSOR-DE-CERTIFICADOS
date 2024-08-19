using EMISSOR_DE_CERTIFICADOS.Models;
using System.Data;

namespace EMISSOR_DE_CERTIFICADOS.Interfaces
{
    public interface IEmailConfigRepository
    {
        Task<DataTable> CarregarDadosDTAsync();
    }
}
