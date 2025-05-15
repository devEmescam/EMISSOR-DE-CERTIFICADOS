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

        [HttpPost("login-organizador")]
        public async Task<IActionResult> LoginOrganizador([FromBody] LoginModel loginModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { status = "error", message = "Requisição inválida", etapa = "validação inicial" });

                Console.WriteLine($"🔍 Iniciando login de ORGANIZADOR: {loginModel.Login}");

                loginModel.Id = await RetornarIdUsuarioAsync(loginModel.Login, "", true);

                if (loginModel.Id > 0)
                {
                    Console.WriteLine($"✅ Usuário localizado no banco de dados. ID = {loginModel.Id}");

                    if (_adHelper.VerificaUsuario(loginModel.Login, loginModel.Senha))
                    {
                        Console.WriteLine("🔐 Login no AD bem-sucedido");
                        _sessao.CriarSessaoDoUsuario(loginModel);
                        return Ok(new { status = "success", userId = loginModel.Id });
                    }
                    else
                    {
                        Console.WriteLine("❌ Falha na autenticação do AD");
                        return Unauthorized(new { status = "error", message = "Usuário ou senha inválidos (AD)", etapa = "verificação AD" });
                    }
                }

                Console.WriteLine("❌ Usuário não encontrado no banco de dados");
                return NotFound(new { status = "error", message = "Usuário não localizado no sistema", etapa = "busca no banco" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 ERRO LoginOrganizador: {ex}");
                return StatusCode(500, new { status = "error", message = ex.Message, etapa = "erro interno" });
            }
        }

        [HttpPost("login-participante")]
        public async Task<IActionResult> LoginParticipante([FromBody] LoginModel loginModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { status = "error", message = "Requisição inválida", etapa = "validação inicial" });

                Console.WriteLine($"🔍 Iniciando login de PARTICIPANTE: {loginModel.Login}");

                loginModel.Id = await RetornarIdUsuarioAsync(loginModel.Login, loginModel.Senha, false);

                if (loginModel.Id > 0)
                {
                    Console.WriteLine($"✅ Participante localizado. ID = {loginModel.Id}");
                    _sessao.CriarSessaoDoUsuario(loginModel);
                    return Ok(new { status = "success", userId = loginModel.Id });
                }

                Console.WriteLine("❌ Usuário ou senha inválidos para participante");
                return Unauthorized(new { status = "error", message = "Usuário ou senha inválidos", etapa = "busca no banco" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 ERRO LoginParticipante: {ex}");
                return StatusCode(500, new { status = "error", message = ex.Message, etapa = "erro interno" });
            }
        }

        #region *** MÉTODO COM LOG DE SQL ***
        private async Task<int> RetornarIdUsuarioAsync(string usuario, string senha, bool administrativo)
        {
            string sSQL = administrativo
                ? $"SELECT ID FROM USUARIO WHERE USUARIO = '{usuario}' AND ADMINISTRATIVO = '{administrativo}'"
                : $"SELECT ID FROM USUARIO WHERE USUARIO = '{usuario}' AND SENHA = '{senha}' AND ADMINISTRATIVO = '{administrativo}'";

            try
            {
                Console.WriteLine($"📄 Executando SQL: {sSQL}");

                var oDT = await _dbHelper.ExecuteQueryAsync(sSQL);

                if (oDT != null && oDT.Rows.Count > 0)
                {
                    Console.WriteLine("📊 Resultado encontrado no banco");
                    return oDT.Rows[0].Field<int>("ID");
                }

                Console.WriteLine("⚠️ Nenhum registro retornado do banco");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 ERRO SQL: {ex.Message}");
                throw new Exception($"Erro em RetornarIdUsuarioAsync: {ex.Message}");
            }
        }
        #endregion

    }
}
