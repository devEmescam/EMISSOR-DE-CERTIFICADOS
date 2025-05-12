using Microsoft.AspNetCore.Mvc;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    [ApiController]
    [Route("api/home-validar-certificado")]
    public class HomeValidarCertificadoController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { Message = "API funcionando corretamente" });
        }
    }
}
