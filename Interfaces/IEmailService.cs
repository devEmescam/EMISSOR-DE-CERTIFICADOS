namespace EMISSOR_DE_CERTIFICADOS.Interfaces
{
    public interface IEmailService
    {
        Task<(bool success, string retorno)> EnviarEmailAsync(string login, string senha, int idEventoPessoa);       
    }
}
