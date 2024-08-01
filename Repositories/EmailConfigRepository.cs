using DocumentFormat.OpenXml.CustomProperties;
using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Models;
using System.Data;

namespace EMISSOR_DE_CERTIFICADOS.Repositories
{
    public class EmailConfigRepository
    {
        private readonly IDBHelpers _dbHelper;
        public EmailConfigRepository(IDBHelpers dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper), "O DBHelper não pode ser nulo.");
        }
        private async Task<DataTable> CarregarDadosDTAsync()
        {
            try
            {
                var sSQL = "SELECT * FROM EMAIL_CONFIG";
                return await _dbHelper.ExecuteQueryAsync(sSQL);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EmailConfigRepository.CarregarDadosDT]: {ex.Message}");
            }
        }

        public async Task<EmailConfigModel> CarregarDadosAsync()
        {
            try
            {
                DataTable oDT = await CarregarDadosDTAsync();
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
