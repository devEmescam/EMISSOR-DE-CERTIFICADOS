using DocumentFormat.OpenXml.CustomProperties;
using EMISSOR_DE_CERTIFICADOS.DBConnections;
using System.Data;

namespace EMISSOR_DE_CERTIFICADOS.Repositories
{
    public class EmailConfigRepository
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public string ServidorSMTP { get; set; }
        public string Porta { get; set; }
        public string SSL { get; set; }

        private readonly DBHelpers _dbHelper;
        public EmailConfigRepository(DBHelpers dbHelper)
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

        public async Task<List<EmailConfigRepository>> CarregarDadosAsync()
        {
            try
            {
                DataTable oDT = await CarregarDadosDTAsync();
                var emailConfigList = new List<EmailConfigRepository>();

                foreach (DataRow row in oDT.Rows)
                {
                    var emailConfig = new EmailConfigRepository(_dbHelper)
                    {
                        Id = Convert.ToInt32(row["ID"]),
                        Email = Convert.ToString(row["EMAIL"]),
                        Senha = Convert.ToString(row["SENHA"]),
                        ServidorSMTP = Convert.ToString(row["SERVIDOR_SMTP"]),
                        Porta = Convert.ToString(row["PORTA"]),
                        SSL = Convert.ToString(row["SSL"])
                    };

                    emailConfigList.Add(emailConfig);
                }

                return emailConfigList;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EmailConfigRepository.CarregarDados]: {ex.Message}");
            }
        }
    }
}