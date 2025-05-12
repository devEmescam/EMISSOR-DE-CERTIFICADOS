using Microsoft.AspNetCore.Mvc;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    [ApiController]
    [Route("api/participante-controller")]
    public class ParticipanteController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] object loginData)
        {
            // Substitua a lógica abaixo pela validação e autenticação real
            return Ok(new { Message = "Login realizado com sucesso", Data = loginData });
        }
    }
}
