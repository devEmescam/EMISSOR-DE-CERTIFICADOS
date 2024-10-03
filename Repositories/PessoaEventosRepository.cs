using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Interfaces;
using System.Data;

namespace EMISSOR_DE_CERTIFICADOS.Repositories
{
    internal class PessoaEventosRepository: IPessoaEventosRepository
    {
        private readonly IDBHelpers _dbHelper;
        public PessoaEventosRepository(IDBHelpers dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper), "O DBHelper não pode ser nulo.");
        }
        public async Task<List<Pessoa>> CarregarDadosAsync(string termo, int idUsuario, bool visaoOrganizador)
        {
            try
            {
                var sSQL = @"SELECT * FROM PESSOA 
                     WHERE 1=1
                     AND (Nome LIKE @Termo OR CPF LIKE @Termo OR Email LIKE @Termo) 
                     AND ID_USUARIO_ADMINISTRATIVO = @ID_USUARIO_ADMINISTRATIVO";

                var parameters = new Dictionary<string, object>
        {
            { "@Termo", "%" + termo + "%" },
            { "@ID_USUARIO_ADMINISTRATIVO", idUsuario }
        };

                var oDT = await _dbHelper.ExecuteQueryAsync(sSQL, parameters);
                if (oDT.Rows.Count == 0)
                {
                    throw new Exception("Pessoa não encontrada.");
                }

                var pessoas = new List<Pessoa>();
                foreach (DataRow row in oDT.Rows)
                {
                    var pessoaId = Convert.ToInt32(row["Id"]);

                    pessoas.Add(new Pessoa
                    {
                        Id = pessoaId,
                        Nome = Convert.ToString(row["Nome"]),
                        CPF = Convert.ToString(row["CPF"]),
                        Email = Convert.ToString(row["Email"]),
                        // Carregar os eventos da pessoa
                        Eventos = await CarregarEventosPessoaAsync(pessoaId, idUsuario, visaoOrganizador)
                    });
                }

                return pessoas;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaEventosRepository.CarregarDadosAsync]: {ex.Message}");
            }
        }

        public async Task<List<EventoPessoa>> CarregarEventosPessoaAsync(int idPessoa, int idUsuario, bool visaoOrganizador)
        {
            try
            {
                // Inicializar a string SQL básica
                var sSQL = @"SELECT E.ID, E.NOME, EP.IMAGEM_CERTIFICADO 
             FROM EVENTO E
             JOIN EVENTO_PESSOA EP ON (E.ID = EP.ID_EVENTO)
             WHERE EP.CERTIFICADO_EMITIDO = 1
             AND EP.ID_PESSOA = @idPessoa";

                // Se for organizador, adicionar filtro na consulta
                if (visaoOrganizador)
                {
                    sSQL += " AND E.ID_USUARIO_ADMINISTRATIVO = @idUsuario";
                }

                // Definir os parâmetros
                var parameters = new Dictionary<string, object>
{
    { "@idPessoa", idPessoa }
};

                // Se for organizador, adicionar o parâmetro idUsuario
                if (visaoOrganizador)
                {
                    parameters.Add("@idUsuario", idUsuario);
                }

                // Executar a consulta
                var oDT = await _dbHelper.ExecuteQueryAsync(sSQL, parameters);
                if (oDT.Rows.Count == 0)
                {
                    return null;
                }

                // Processar os resultados
                var eventos = new List<EventoPessoa>();
                foreach (DataRow row in oDT.Rows)
                {
                    // Recupera os bytes da imagem do certificado diretamente do banco de dados
                    byte[] imagemBytes = row["IMAGEM_CERTIFICADO"] as byte[];

                    // Converte os bytes para uma string base64, se houver imagem
                    string imagemCertificadoBase64 = imagemBytes != null ? Convert.ToBase64String(imagemBytes) : null;

                    eventos.Add(new EventoPessoa
                    {
                        IdEventoPessoa = Convert.ToInt32(row["ID"]),
                        Nome = Convert.ToString(row["NOME"]),
                        ImagemCertificadoBase64 = imagemCertificadoBase64 // Armazena a string base64
                    });
                }

                return eventos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaEventosRepository.CarregarEventosPessoaAsync]: {ex.Message}");
            }
        }




    }
}
public class Pessoa
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string CPF { get; set; }
    public string Email { get; set; }
    public List<EventoPessoa> Eventos { get; set; }
}
public class EventoPessoa
{
    public int IdEventoPessoa { get; set; }
    public string Nome { get; set; }
    public string ImagemCertificadoBase64 { get; set; } // Adiciona a propriedade base64

    //public IFormFile ImagemCertificado { get; set; }
}