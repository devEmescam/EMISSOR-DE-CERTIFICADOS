
namespace EMISSOR_DE_CERTIFICADOS.Interfaces
{
    public interface IUsuarioService
    {
        Task<int> GerarUsuarioAsync(int idPessoa);
        Task<UsuarioSenha> ObterUsuarioESenhaAsync(int idEventoPessoa);        
        Task<bool> CriarNovoUsuarioAsync(string login);
        Task<string> RetornarSetorUsuarioAsync(int id);
    }
}