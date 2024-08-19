using EMISSOR_DE_CERTIFICADOS.Models;

namespace EMISSOR_DE_CERTIFICADOS.Helpers
{
    public interface ISessao
    {
        void CriarSessaoDoUsuario(LoginModel login);
        void RemoverSessaoUsuario();
        LoginModel BuscarSessaodoUsuario();
        int ObterUsuarioId();
        string ObterUsuarioLogin();
        string ObterUsuarioPassword();
    }
}