using EMISSOR_DE_CERTIFICADOS.Services;

namespace EMISSOR_DE_CERTIFICADOS.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<int> GerarUsuarioAsync(string cpf, string senha);
        Task<bool> UsuarioExisteAsync(string usuario);
        Task<int> RetornarIdAsync(string usuario);
        Task<UsuarioSenha> ObterUsuarioESenhaAsync(int idEventoPessoa);
        Task<int> CriarNovoUsuarioAsync(string login);
        Task<string> ObterSetorUsuarioAsync(int id);
    }
}