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
                    { "@ID_USUARIO_ADMINISTRATIVO", idUsuario}
                };

                var oDT = await _dbHelper.ExecuteQueryAsync(sSQL, parameters);
                if (oDT.Rows.Count == 0)
                {
                    throw new Exception("Pessoa não encontrada.");
                }

                var pessoas = new List<Pessoa>();
                foreach (DataRow row in oDT.Rows)
                {
                    pessoas.Add(new Pessoa
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Nome = Convert.ToString(row["Nome"]),
                        CPF = Convert.ToString(row["CPF"]),
                        Email = Convert.ToString(row["Email"]),
                        //Eventos = await CarregarEventosPessoaAsync(Convert.ToInt32(row["Id"]), idUsuario, visaoOrganizador)
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
                var sSQL = @"SELECT E.NOME, EP.IMAGEM_CERTIFICADO 
                     FROM EVENTO E
                     JOIN EVENTO_PESSOA EP ON (E.ID = EP.ID_EVENTO)
                     WHERE EP.CERTIFICADO_EMITIDO = 1
                     AND EP.ID_PESSOA = @idPessoa";

                // Se for organizador adicionar filtro na consulta
                if (visaoOrganizador)
                {
                    sSQL += " AND E.ID_USUARIO_ADMINISTRATIVO = @idUsuario";
                }

                // Definir os parâmetros
                var parameters = new Dictionary<string, object>
                {
                    { "@idPessoa", idPessoa }
                };

                // Se for organizador adicionar filtro na consulta
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
                    // Recupera os bytes diretamente do banco de dados
                    byte[] imagemBytes = (byte[])row["IMAGEM_CERTIFICADO"];

                    // Cria um objeto IFormFile a partir do array de bytes
                    IFormFile imagemCertificado = new FormFile(new MemoryStream(imagemBytes), 0, imagemBytes.Length, "ImagemCertificado", "imagem.jpg");

                    eventos.Add(new EventoPessoa
                    {
                        Nome = Convert.ToString(row["NOME"]),
                        ImagemCertificado = imagemCertificado
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
    public string Nome { get; set; }
    public IFormFile ImagemCertificado { get; set; }
}