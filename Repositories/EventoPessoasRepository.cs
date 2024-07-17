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

        public async Task<Evento> CarregarDadosAsync(int idEvento, bool emitirCertificado)
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
                    Id = row["ID"] != DBNull.Value ? Convert.ToInt32(row["ID"]) : 0,
                    Nome = row["NOME"] != DBNull.Value ? Convert.ToString(row["NOME"]) : string.Empty,
                    ImagemCertificado = row["IMAGEM_CERTIFICADO"] != DBNull.Value ? ConvertByteArrayToFormFile((byte[])row["IMAGEM_CERTIFICADO"], "certificado.png") : null,
                    DataCadastro = row["DATA_CADASTRO"] != DBNull.Value ? Convert.ToString(row["DATA_CADASTRO"]) : null,
                    PessoasEventos = await CarregarPessoasEventoAsync(idEvento, emitirCertificado)
                };

                return evento;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EventoPessoasRepository.CarregarDadosAsync]: {ex.Message}");
            }
        }

        private async Task<List<PessoaEvento>> CarregarPessoasEventoAsync(int idEvento, bool emitirCertificado)
        {
            string sSQL = string.Empty;

            try
            {               

                sSQL = "SELECT EP.ID_PESSOA, P.NOME, EP.CERTIFICADO_EMITIDO, FORMAT(EP.DATA_EMISSAO,'dd/MM/yyyy') DATA_EMISSAO, EP.MENSAGEM_RETORNO_EMAIL, EP.TEXTO_FRENTE, P.CPF, P.EMAIL ";
                sSQL += "FROM EVENTO_PESSOA EP ";
                sSQL += "JOIN PESSOA P ON (EP.ID_PESSOA = P.ID) ";
                sSQL += "WHERE ID_EVENTO = " + idEvento;               

                if (!emitirCertificado)
                {
                    // As duas linhas abaixo garantem que sejam retornados somente registros que não tiveram certificados emitidos
                    sSQL += "AND CERTIFICADO_EMITIDO <> 1 ";
                    sSQL += "AND (DATA_EMISSAO IS NULL OR DATA_EMISSAO = '' OR DATA_EMISSAO = '1900-01-01')";                    
                }



                var oDT = await _dbHelper.ExecuteQueryAsync(sSQL);
                var pessoasEventoList = new List<PessoaEvento>();

                foreach (DataRow row in oDT.Rows)
                {
                    var pessoaEvento = new PessoaEvento
                    {
                        Id = row["ID_PESSOA"] != DBNull.Value ? Convert.ToInt32(row["ID_PESSOA"]) : 0,
                        Nome = row["NOME"] != DBNull.Value ? Convert.ToString(row["NOME"]) : string.Empty,
                        CertificadoEmitido = row["CERTIFICADO_EMITIDO"] != DBNull.Value && Convert.ToBoolean(row["CERTIFICADO_EMITIDO"]),
                        DataEmissao = row["DATA_EMISSAO"] != DBNull.Value ? Convert.ToString(row["DATA_EMISSAO"]) : null,
                        MensagemRetornoEmail = row["MENSAGEM_RETORNO_EMAIL"] != DBNull.Value ? Convert.ToString(row["MENSAGEM_RETORNO_EMAIL"]) : string.Empty,
                        Texto = row["TEXTO_FRENTE"] != DBNull.Value ? Convert.ToString(row["TEXTO_FRENTE"]) : string.Empty,
                        Cpf = row["CPF"] != DBNull.Value ? Convert.ToString(row["CPF"]) : string.Empty,
                        Email = row["Email"] != DBNull.Value ? Convert.ToString(row["Email"]) : string.Empty
                    };

                    //Se DataEmissao tiver valor  = 01/01/1900 tratar igual null ou em branco
                    if (pessoaEvento.DataEmissao == "01/01/1900") 
                    {
                        pessoaEvento.DataEmissao = null;
                    }

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
        public string Texto { get; set; }
        public string Cpf { get; set; }
        public string Email { get; set; }
    }
}
