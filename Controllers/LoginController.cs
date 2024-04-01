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
        public IActionResult LoginOrganizador(LoginModel loginModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Consultar o banco de dados local
                    var sSQL = $"SELECT ID FROM USUARIO WHERE USUARIO = '{loginModel.Login}'";
                    var returnDataTable = _dbHelper.ExecuteQuery(sSQL, "CertificadoConnection");

                    if (returnDataTable != null && returnDataTable.Rows.Count > 0)
                    {
                        // Atribuir o ID retornado da consulta SQL ao atributo ID do loginModel
                        loginModel.Id = returnDataTable.Rows[0].Field<int>("ID");

                        //Usuário encontrado no banco de dados da aplicação, agora verificar login e senha no AD
                        if (_adHelper.VerificaUsuario(loginModel.Login, loginModel.Senha))
                        {
                            _sessao.CriarSessaoDoUsuario(loginModel);
                            return RedirectToAction("Login", "Home");
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
                        ModelState.AddModelError(string.Empty, "Usuário não localilzado.");
                    }
                }
                // Retornar para a view de LoginOrganizador, onde as mensagens de erro serão exibidas
                return View("~/Views/Login_Organizador/Login_organizador.cshtml", loginModel);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em LoginController.Login. Erro: {ex.Message}");
            }
        }

        [HttpPost]  
        public IActionResult LoginParticipante(LoginModel loginModel) 
        {
            // Retornar para a view de LoginParticipante, onde as mensagens de erro serão exibidas
            return View("~/Views/Login_Participante/Login_participante.cshtml", loginModel);
        }
    }
}
