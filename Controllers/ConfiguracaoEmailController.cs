using EMISSOR_DE_CERTIFICADOS.Models;
using EMISSOR_DE_CERTIFICADOS.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    public class ConfiguracaoEmailController : Controller
    {
        private readonly EmailConfigRepository _emailConfigRepo;
        public ConfiguracaoEmailController(EmailConfigRepository emailConfigRepo) 
        {
            _emailConfigRepo = emailConfigRepo;
        }

        #region *** IActionResults ***        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var configuracao = await BuscarConfiguracaoAsync();
                //TO DO: definir direcionamento correto
                return View(configuracao);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em [ConfiguracaoEmail.Index] Erro: {ex.Message}");                
            }            
        }
        #endregion

        #region *** Metodo ***        
        private async Task<EmailConfigModel> BuscarConfiguracaoAsync() 
        {
            var emailConfigModel = new EmailConfigModel();

            try
            {
                return emailConfigModel = await _emailConfigRepo.CarregarDadosAsync();                
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [ConfiguracaoEmail.BuscarConfiguracaoAsync] Erro: {ex.Message}");
            }
        }
        #endregion
    }
}
