using EMISSOR_DE_CERTIFICADOS.DBConnections;
using System.Data;

namespace EMISSOR_DE_CERTIFICADOS.Repositories
{
    public class EventoPessoasRepository
    {
        private readonly DBHelpers _dbHelper;

        public EventoPessoasRepository(DBHelpers dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper), "O DBHelper não pode ser nulo.");
        }

        public async Task<Evento> CarregarDadosAsync(int idEvento)
        {
            try
            {
                var sSQL = $"SELECT ID, NOME, IMAGEM_CERTIFICADO, DATA_CADASTRO FROM EVENTO WHERE ID = {idEvento}";
                var oDT = await _dbHelper.ExecuteQueryAsync(sSQL);

                if (oDT.Rows.Count == 0)
                {
                    throw new Exception("Evento não encontrado.");
                }

                var row = oDT.Rows[0];

                var evento = new Evento
                {
                    Id = Convert.ToInt32(row["ID"]),
                    Nome = Convert.ToString(row["NOME"]),
                    ImagemCertificado = ConvertByteArrayToFormFile((byte[])row["IMAGEM_CERTIFICADO"], "certificado.png"),
                    DataCadastro = Convert.ToString(row["DATA_CADASTRO"]),
                    PessoasEventos = await CarregarPessoasEventoAsync(idEvento)
                };

                return evento;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EventoPessoasRepository.CarregarDadosAsync]: {ex.Message}");
            }
        }

        private async Task<List<PessoaEvento>> CarregarPessoasEventoAsync(int idEvento)
        {
            try
            {                
                var sSQL = $"SELECT EP.ID, P.NOME, EP.CERTIFICADO_EMITIDO, FORMAT(EP.DATA_EMISSAO,'dd/MM/yyyy' ) DATA_EMISSAO, EP.MENSAGEM_RETORNO_EMAIL " + 
                           $"FROM EVENTO_PESSOA EP " +
                           $"JOIN PESSOA P ON (EP.ID_PESSOA = P.ID) " +
                           $"WHERE ID_EVENTO = {idEvento}";

                var oDT = await _dbHelper.ExecuteQueryAsync(sSQL);
                var pessoasEventoList = new List<PessoaEvento>();

                foreach (DataRow row in oDT.Rows)
                {
                    var pessoaEvento = new PessoaEvento
                    {
                        Id = Convert.ToInt32(row["ID"]),
                        Nome = Convert.ToString(row["NOME"]),
                        CertificadoEmitido = Convert.ToBoolean(row["CERTIFICADO_EMITIDO"]),
                        DataEmissao = row["DATA_EMISSAO"] == DBNull.Value ? null : Convert.ToString(row["DATA_EMISSAO"]),
                        MensagemRetornoEmail = Convert.ToString(row["MENSAGEM_RETORNO_EMAIL"])
                    };

                    pessoasEventoList.Add(pessoaEvento);
                }

                return pessoasEventoList;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EventoPessoasRepository.CarregarPessoasEventoAsync]: {ex.Message}");
            }
        }

        private IFormFile ConvertByteArrayToFormFile(byte[] byteArray, string fileName)
        {
            var stream = new MemoryStream(byteArray);
            return new FormFile(stream, 0, byteArray.Length, fileName, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/octet-stream"
            };
        }
    }

    public class Evento
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public IFormFile ImagemCertificado { get; set; }
        public string DataCadastro { get; set; }
        public List<PessoaEvento> PessoasEventos { get; set; }
    }

    public class PessoaEvento
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public bool CertificadoEmitido { get; set; }
        public string DataEmissao { get; set; }
        public string MensagemRetornoEmail { get; set; }
    }
}
