using EMISSOR_DE_CERTIFICADOS.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Autenticador.Controllers
{
    public class AutenticadorController : Controller
    {
        private readonly IValidarCertificadoService _validarCertificadoService;

        public AutenticadorController(IValidarCertificadoService validarCertificadoService) 
        {
            _validarCertificadoService = validarCertificadoService;
        }        
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ValidarCertificado(string codigo) 
        {
            try
            {
                if( !string.IsNullOrEmpty(codigo))
                {
                    if (await _validarCertificadoService.Validar(codigo))
                    {
                        ViewBag.Validacao = "Certificado Autêntico!";
                        var imagemCertificado = await _validarCertificadoService.ObterImagemCertificadoPorCodigo(codigo);
                        return View(imagemCertificado);
                    }
                    else 
                    {
                        ViewBag.Validacao = "Certificado não encontrado para o código fornecido";
                        return View();
                    }
                }
                
                return StatusCode(500, $"Não foi possivel identificar o código. Verifique.");                

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em [AutenticadorController.ValidarCertificado] Erro: {ex.Message}");
            }
        }


        public async Task<IActionResult> RenderCertificadoImage(string codigo)
        {
            try
            {
                var imagemCertificado = await _validarCertificadoService.ObterImagemCertificadoPorCodigo(codigo);
                if (imagemCertificado != null)
                {
                    return File(imagemCertificado.OpenReadStream(), imagemCertificado.ContentType);
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao renderizar a imagem do certificado. Erro: {ex.Message}");
            }
        }


    }
}
