
namespace EMISSOR_DE_CERTIFICADOS.Interfaces
{
    public interface IUsuarioService
    {
        Task<int> GerarUsuarioAsync(int idPessoa);
        Task<UsuarioSenha> ObterUsuarioESenhaAsync(int idEventoPessoa);

        // Novo método para criar um novo usuário
        Task<bool> CriarNovoUsuarioAsync(string login);
    }
}