using EMISSOR_DE_CERTIFICADOS.Models;
using Newtonsoft.Json;

namespace EMISSOR_DE_CERTIFICADOS.Helpers
{
    public class Sessao : ISessao
    {
        private readonly IHttpContextAccessor _httpContext;
        public Sessao(IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext;
        }
        public LoginModel BuscarSessaodoUsuario()
        {
            try
            {
                string sessaoUsuario = _httpContext.HttpContext.Session.GetString("sessaoUsuarioLogado");

                if (string.IsNullOrEmpty(sessaoUsuario)) return null;

                return JsonConvert.DeserializeObject<LoginModel>(sessaoUsuario);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em Sessao.BuscarSessaodoUsuario. Erro: {ex.Message}");
            }

        }
        public void CriarSessaoDoUsuario(LoginModel login)
        {
            try
            {
                string valor = JsonConvert.SerializeObject(login);

                _httpContext.HttpContext.Session.SetString("sessaoUsuarioLogado", valor);                
                _httpContext.HttpContext.Session.SetInt32("UserId", login.Id);
                _httpContext.HttpContext.Session.SetString("Login", login.Login.ToLower());
                _httpContext.HttpContext.Session.SetString("Senha", login.Senha);
                if (login.Administrativo)
                {
                    _httpContext.HttpContext.Session.SetString("Tipo", "organizador");
                }
                else 
                {
                    _httpContext.HttpContext.Session.SetString("Tipo", "nao_definido");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em Sessao.CriarSessaoDoUsuario. Erro: {ex.Message}");
            }
        }
        public void RemoverSessaoUsuario()
        {
            _httpContext.HttpContext.Session.Remove("sessaoUsuarioLogado");
            _httpContext.HttpContext.Session.Remove("UserId");
            _httpContext.HttpContext.Session.Remove("Login");
            _httpContext.HttpContext.Session.Remove("Senha");
            _httpContext.HttpContext.Session.Remove("Tipo");
        }
        public int ObterUsuarioId() 
        {
            try
            {
                 return _httpContext.HttpContext.Session.GetInt32("UserId") ?? 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em Sessao.ObterUsuarioId. Erro: {ex.Message}");
            }
        }
    }
}
