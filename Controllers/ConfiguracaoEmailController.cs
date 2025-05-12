using EMISSOR_DE_CERTIFICADOS.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    [ApiController]
    [Route("api/configuracao-email")]
    public class ConfiguracaoEmailController : ControllerBase
    {
        private readonly IEmailConfigService _emailConfigService;

        public ConfiguracaoEmailController(IEmailConfigService emailConfigService)
        {
            _emailConfigService = emailConfigService;
        }

        /// <summary>
        /// Retorna as configurações de e-mail do sistema.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetConfiguracoes()
        {
            try
            {
                var configuracao = await _emailConfigService.CarregarDadosAsync();
                return Ok(configuracao);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao obter configurações de e-mail: {ex.Message}");
            }
        }
    }
}
