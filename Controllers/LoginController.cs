using Microsoft.AspNetCore.Mvc;
using EMISSOR_DE_CERTIFICADOS.Models;
using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Helpers;
using System.Data;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    [ApiController]
    [Route("api/login")]
    public class LoginController : ControllerBase
    {
        private readonly IDBHelpers _dbHelper;
        private readonly ISessao _sessao;
        private readonly ADHelper _adHelper = new ADHelper();

        public LoginController(IDBHelpers databaseHelper, ISessao sessao)
        {
            _dbHelper = databaseHelper;
            _sessao = sessao;
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            var usuario = _sessao.BuscarSessaodoUsuario();
            if (usuario != null)
            {
                return Ok(new { message = "Usuário logado", usuario });
            }
            return Unauthorized(new { message = "Usuário não está logado" });
        }

        [HttpPost("logout")]
        public IActionResult Sair()
        {
            _sessao.RemoverSessaoUsuario();
            return Ok(new { message = "Usuário desconectado com sucesso" });
        }

        [HttpPost("login-organizador")]
        public async Task<IActionResult> LoginOrganizador([FromBody] LoginModel loginModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    loginModel.Id = await RetornarIdUsuarioAsync(loginModel.Login, "", true);

                    if (loginModel.Id > 0)
                    {
                        if (_adHelper.VerificaUsuario(loginModel.Login, loginModel.Senha))
                        {
                            _sessao.CriarSessaoDoUsuario(loginModel);
                            return Ok(new { message = "Login realizado com sucesso", usuario = loginModel });
                        }
                        else
                        {
                            return Unauthorized(new { message = "Usuário ou senha inválidos" });
                        }
                    }
                    else
                    {
                        return NotFound(new { message = "Usuário não localizado" });
                    }
                }
                return BadRequest(new { message = "Dados inválidos" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro interno: {ex.Message}" });
            }
        }

        [HttpPost("login-participante")]
        public async Task<IActionResult> LoginParticipante([FromBody] LoginModel loginModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    loginModel.Id = await RetornarIdUsuarioAsync(loginModel.Login, loginModel.Senha, false);

                    if (loginModel.Id > 0)
                    {
                        _sessao.CriarSessaoDoUsuario(loginModel);
                        return Ok(new { message = "Login realizado com sucesso", usuario = loginModel });
                    }
                    else
                    {
                        return Unauthorized(new { message = "Usuário ou senha inválidos" });
                    }
                }
                return BadRequest(new { message = "Dados inválidos" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro interno: {ex.Message}" });
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
