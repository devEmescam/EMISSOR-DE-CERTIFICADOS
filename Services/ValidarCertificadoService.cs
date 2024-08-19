using EMISSOR_DE_CERTIFICADOS.Helpers;
using EMISSOR_DE_CERTIFICADOS.Interfaces;

namespace EMISSOR_DE_CERTIFICADOS.Services
{
    internal class ValidarCertificadoService: IValidarCertificadoService
    {
		private readonly IValidarCertificadoRepository _validarCertificadoRepository;

		public ValidarCertificadoService(IValidarCertificadoRepository validarCertificadoRepository) 
		{
			_validarCertificadoRepository = validarCertificadoRepository;	
		}

        public async Task<bool> Validar(string codigo) 
        {
			try
			{
				return await _validarCertificadoRepository.ValidarCodigoCertificado(codigo);
			}
			catch (Exception ex)
			{
				throw new Exception($"Ocorreu erro em [ValidarCertificadoService.Validar]. Erro: {ex.Message}");
			}
        }

        public async Task<IFormFile> ObterImagemCertificadoPorCodigo(string codigo)
        {
            try
            {
                var imagemCertificado = await _validarCertificadoRepository.ObterImagemCertificadoPorCodigo(codigo);

                if (imagemCertificado != null && imagemCertificado.Rows.Count > 0)
                {
                    // Supondo que a coluna "IMAGEM_CERTIFICADO" contenha os bytes da imagem
                    byte[] imagemBytes = imagemCertificado.Rows[0]["IMAGEM_CERTIFICADO"] as byte[];

                    if (imagemBytes != null)
                    {
                        return Util.ConvertToFormFile(imagemBytes);
                    }
                }

                return null; // Retorna null se a imagem não for encontrada
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu erro em [ValidarCertificadoService.ObterImagemCertificadoPorCodigo]. Erro: {ex.Message}");
            }
        }

    }
}
