using EMISSOR_DE_CERTIFICADOS.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Autenticador.Controllers
{
    [ApiController]
    [Route("api/autenticador")]
    public class AutenticadorController : ControllerBase
    {
        private readonly IValidarCertificadoService _validarCertificadoService;

        public AutenticadorController(IValidarCertificadoService validarCertificadoService)
        {
            _validarCertificadoService = validarCertificadoService ?? throw new ArgumentNullException(nameof(validarCertificadoService));
        }

        /// <summary>
        /// Verifica a autenticidade de um certificado com base no código fornecido.
        /// Retorna uma mensagem de validação e a URL para acessar a imagem do certificado, caso seja válido.
        /// </summary>
        /// <param name="codigo">Código único do certificado a ser validado</param>
        [HttpGet("validar")]
        public async Task<IActionResult> ValidarCertificado([FromQuery] string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return BadRequest(new { mensagem = "Código não informado." });

            try
            {
                bool certificadoValido = await _validarCertificadoService.Validar(codigo);
                if (certificadoValido)
                {
                    string imagemUrl = Url.Action(nameof(RenderCertificadoImage), new { codigo });
                    return Ok(new
                    {
                        validacao = "Certificado Autêntico!",
                        imagemCertificado = imagemUrl
                    });
                }

                return NotFound(new
                {
                    validacao = "Certificado não encontrado para o código fornecido.",
                    imagemCertificado = (string?)null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro interno em [ValidarCertificado]: {ex.Message}" });
            }
        }

        /// <summary>
        /// Obtém a imagem do certificado correspondente ao código fornecido.
        /// Retorna a imagem em formato binário, caso encontrada.
        /// </summary>
        /// <param name="codigo">Código único do certificado cuja imagem será retornada</param>
        [HttpGet("imagem")]
        public async Task<IActionResult> RenderCertificadoImage([FromQuery] string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return BadRequest(new { mensagem = "Código não informado." });

            try
            {
                var imagemCertificado = await _validarCertificadoService.ObterImagemCertificadoPorCodigo(codigo);
                if (imagemCertificado != null)
                {
                    return File(imagemCertificado.OpenReadStream(), imagemCertificado.ContentType);
                }

                return NotFound(new { mensagem = "Imagem não encontrada para o certificado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao renderizar a imagem do certificado: {ex.Message}" });
            }
        }
    }
}
