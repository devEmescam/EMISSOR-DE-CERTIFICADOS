using Microsoft.AspNetCore.Mvc;
using EMISSOR_DE_CERTIFICADOS.Models;
using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Helpers;
using System.Data;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    public class LoginController : Controller
    {
        private readonly DBHelpers _dbHelper;
        private readonly ISessao _sessao;
        private readonly ADHelper _adHelper = new ADHelper();                

        public LoginController(DBHelpers databaseHelper, ISessao sessao)
        {
            _dbHelper = databaseHelper;
            _sessao = sessao;            
        }
        public IActionResult Index()
        {
            //Se usuario estiver logado, redirecionar para home
            if (_sessao.BuscarSessaodoUsuario() != null) return RedirectToAction("Index", "Home");

            return View();
        }
        public IActionResult Sair()
        {
            _sessao.RemoverSessaoUsuario();
            return RedirectToAction("Index", "Login");
        }        

        [HttpPost]
        public async Task<IActionResult> LoginOrganizador(LoginModel loginModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Tenta buscar o ID do usuário
                    loginModel.Id = await RetornarIdUsuarioAsync(loginModel.Login, "", true);

                    // Valida se retornou algo
                    if (loginModel.Id > 0)
                    {
                        // Usuário encontrado no banco de dados da aplicação, agora verificar login e senha no AD
                        if (_adHelper.VerificaUsuario(loginModel.Login, loginModel.Senha)) // Assuma que VerificaUsuario também deve ser assíncrono se necessário
                        {
                            _sessao.CriarSessaoDoUsuario(loginModel);

                            // Armazena o Id do usuário na sessão
                            HttpContext.Session.SetInt32("UserId", loginModel.Id);
                            // Armazenar o Login do usuário na sessão
                            HttpContext.Session.SetString("Login", loginModel.Login);
                            // Armazenar a Senha do usuário na sessão
                            HttpContext.Session.SetString("Senha", loginModel.Senha);

                            return RedirectToAction("Index", "Home_Organizador");
                        }
                        else
                        {
                            // Adicionar mensagem de erro ao ModelState
                            ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
                        }
                    }
                    else
                    {
                        // Adicionar mensagem de erro ao ModelState
                        ModelState.AddModelError(string.Empty, "Usuário não localizado.");
                    }
                }
                // Se chegar aqui não foi possível buscar o Usuário, volta para tela de login para nova tentativa
                return View("~/Views/Login_Organizador/Login_organizador.cshtml", loginModel);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [LoginController.LoginOrganizador] Erro: {ex.Message}");
            }
        }       

        [HttpPost]
        public async Task<IActionResult> LoginParticipante(LoginModel loginModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Tenta buscar o ID do usuário
                    loginModel.Id = await RetornarIdUsuarioAsync(loginModel.Login, loginModel.Senha, false);

                    // Valida se retornou algo
                    if (loginModel.Id > 0)
                    {
                        _sessao.CriarSessaoDoUsuario(loginModel);
                        //return RedirectToAction("Login", "Home_Participante");
                        return RedirectToAction("Index", "Home_Participante");
                    }
                    else
                    {
                        // Adicionar mensagem de erro ao ModelState
                        ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
                    }
                }

                // Se chegar aqui não foi possível buscar o Usuário, volta para tela de login para nova tentativa
                return View("~/Views/Login_Participante/Login_participante.cshtml", loginModel);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [LoginController.LoginParticipante] Erro: {ex.Message}");
            }
        }

        #region *** METODOS ***
        private async Task<int> RetornarIdUsuarioAsync(string usuario, string senha, bool administrativo)
        {
            string sSQL = "";
            Int32 Id = 0;
            DataTable oDT = new DataTable();

            try
            {
                if (administrativo)
                {
                    sSQL = $"SELECT ID FROM USUARIO WHERE USUARIO = '{usuario}' AND ADMINISTRATIVO = '{administrativo}'";
                }
                else
                {
                    sSQL = $"SELECT ID FROM USUARIO WHERE USUARIO = '{usuario}' AND SENHA = '{senha}' AND ADMINISTRATIVO = '{administrativo}'";
                }

                oDT = await _dbHelper.ExecuteQueryAsync(sSQL);

                if (oDT != null && oDT.Rows.Count > 0)
                {
                    Id = oDT.Rows[0].Field<int>("ID");
                }

                return Id;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [LoginController.RetornarIdUsuarioAsync] Erro: {ex.Message}");
            }
        }
        #endregion
    }
}
