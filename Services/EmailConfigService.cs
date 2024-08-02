using EMISSOR_DE_CERTIFICADOS.Interfaces;
using EMISSOR_DE_CERTIFICADOS.Models;
using System.Data;

namespace EMISSOR_DE_CERTIFICADOS.Services
{
    internal class EmailConfigService : IEmailConfigService
    {
        private readonly IEmailConfigRepository _emailConfigRepository;
        public EmailConfigService(IEmailConfigRepository emailConfigRepository) 
        {
            _emailConfigRepository = emailConfigRepository;
        }
        public async Task<EmailConfigModel> CarregarDadosAsync()
        {
            try
            {
                DataTable oDT = await _emailConfigRepository.CarregarDadosDTAsync();
                var emailConfigList = new List<EmailConfigModel>();

                foreach (DataRow row in oDT.Rows)
                {
                    var emailConfig = new EmailConfigModel()
                    {
                        Id = row["ID"] != DBNull.Value ? Convert.ToInt32(row["ID"]) : 0,
                        Email = row["EMAIL"] != DBNull.Value ? Convert.ToString(row["EMAIL"]) : string.Empty,
                        Senha = row["SENHA"] != DBNull.Value ? Convert.ToString(row["SENHA"]) : string.Empty,
                        ServidorSMTP = row["SERVIDOR_SMTP"] != DBNull.Value ? Convert.ToString(row["SERVIDOR_SMTP"]) : string.Empty,
                        Porta = row["PORTA"] != DBNull.Value ? Convert.ToString(row["PORTA"]) : string.Empty,
                        SSL = row["SSL"] != DBNull.Value ? Convert.ToString(row["SSL"]) : "0",
                        ImagemAssinaturaEmail = row["IMAGEM_ASSINATURA"] != DBNull.Value ? (byte[])row["IMAGEM_ASSINATURA"] : null
                    };

                    emailConfigList.Add(emailConfig);
                }

                return emailConfigList.First();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EmailConfigRepository.CarregarDados]: {ex.Message}");
            }
        }
        private IFormFile ConvertByteArrayToFormFile(byte[] byteArray, string fileName)
        {
            try
            {
                if (byteArray == null || byteArray.Length == 0)
                {
                    return null;
                }

                var stream = new MemoryStream(byteArray);
                return new FormFile(stream, 0, byteArray.Length, "file", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/octet-stream"
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EventoPessoasRepository.ConvertByteArrayToFormFile]: {ex.Message}");
            }
        }
    }
}