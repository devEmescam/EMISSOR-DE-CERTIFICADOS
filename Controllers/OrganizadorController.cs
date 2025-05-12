using Microsoft.AspNetCore.Mvc;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    [ApiController]
    [Route("api/organizador-controller")]
    public class OrganizadorController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Simulação de validação de login
            if (request.Username == "admin" && request.Password == "password")
            {
                return Ok(new { Message = "Login realizado com sucesso!" });
            }

            return Unauthorized(new { Message = "Credenciais inválidas." });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
