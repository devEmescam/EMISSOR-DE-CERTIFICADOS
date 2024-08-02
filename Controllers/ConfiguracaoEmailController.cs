using EMISSOR_DE_CERTIFICADOS.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    public class ConfiguracaoEmailController : Controller
    {
        private readonly IEmailConfigService _emailConfigService;
        public ConfiguracaoEmailController(IEmailConfigService emailConfigService) 
        {
            _emailConfigService = emailConfigService;
        }

        #region *** IActionResults ***        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var configuracao = await _emailConfigService.CarregarDadosAsync();
                return View(configuracao);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em [ConfiguracaoEmailController.Index] Erro: {ex.Message}");                
            }            
        }
        #endregion        
    }
}
