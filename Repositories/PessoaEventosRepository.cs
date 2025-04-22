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
                     AND (Nome LIKE @Termo OR CPF LIKE @Termo OR Email LIKE @Termo)";


                if (!visaoOrganizador) 
                {
                sSQL += "AND ID_USUARIO_ADMINISTRATIVO = @ID_USUARIO_ADMINISTRATIVO";
                }                     

                var parameters = new Dictionary<string, object>
                {
                    { "@Termo", "%" + termo.Trim() + "%" }                                        
                };

                if (!visaoOrganizador)
                {
                    parameters.Add("@ID_USUARIO_ADMINISTRATIVO", idUsuario);
                }

                var oDT = await _dbHelper.ExecuteQueryAsync(sSQL, parameters);                

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
                var sSQL = @"SELECT EP.ID ID_EVENTO_PESSOA, E.ID ID_EVENTO, E.NOME, EP.IMAGEM_CERTIFICADO 
                    FROM EVENTO E
                    JOIN EVENTO_PESSOA EP ON (E.ID = EP.ID_EVENTO)
                    WHERE EP.CERTIFICADO_EMITIDO = 1
                    AND EP.ID_PESSOA = @idPessoa";

                if (!visaoOrganizador)
                {
                    sSQL += " AND E.ID_USUARIO_ADMINISTRATIVO = @idUsuario";
                }

                var parameters = new Dictionary<string, object>
                {
                    { "@idPessoa", idPessoa }
                };

                if (!visaoOrganizador)
                {
                    parameters.Add("@idUsuario", idUsuario);
                }

                var oDT = await _dbHelper.ExecuteQueryAsync(sSQL, parameters);
                if (oDT.Rows.Count == 0)
                {
                    return null;
                }

                var eventos = new List<EventoPessoa>();
                foreach (DataRow row in oDT.Rows)
                {
                    byte[] imagemBytes = row["IMAGEM_CERTIFICADO"] as byte[];
                    //string imagemCertificadoBase64 = imagemBytes != null ? Convert.ToBase64String(imagemBytes) : null;

                    // Adicionar o prefixo data:image/...;base64
                    string imagemCertificadoBase64 = null;
                    if (imagemBytes != null)
                    {
                        string tipoImagem = IdentificarTipoImagem(imagemBytes); // Identifica o tipo de imagem
                        if (!string.IsNullOrEmpty(tipoImagem))
                        {
                            imagemCertificadoBase64 = $"data:image/{tipoImagem};base64,{Convert.ToBase64String(imagemBytes)}";
                        }
                    }

                    eventos.Add(new EventoPessoa
                    {
                        IdEventoPessoa = Convert.ToInt32(row["ID_EVENTO_PESSOA"]),
                        IdEvento = Convert.ToInt32(row["ID_EVENTO"]),
                        Nome = Convert.ToString(row["NOME"]),
                        ImagemCertificadoBase64 = imagemCertificadoBase64
                    });
                }

                return eventos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaEventosRepository.CarregarEventosPessoaAsync]: {ex.Message}");
            }
        }

        private string IdentificarTipoImagem(byte[] imagemBytes)
        {

            try
            {
                if (imagemBytes.Length >= 4)
                {
                    // Verifica os cabeçalhos conhecidos de arquivos de imagem
                    if (imagemBytes[0] == 0x89 && imagemBytes[1] == 0x50 && imagemBytes[2] == 0x4E && imagemBytes[3] == 0x47)
                    {
                        return "png"; // PNG
                    }
                    if (imagemBytes[0] == 0xFF && imagemBytes[1] == 0xD8 && imagemBytes[2] == 0xFF)
                    {
                        return "jpeg"; // JPEG
                    }
                    if (imagemBytes[0] == 0x47 && imagemBytes[1] == 0x49 && imagemBytes[2] == 0x46)
                    {
                        return "gif"; // GIF
                    }
                }
                return null; // Tipo desconhecido
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaEventosRepository.IdentificarTipoImagem]: {ex.Message}");
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
    public int IdEvento { get; set; }
    public string Nome { get; set; }
    public string ImagemCertificadoBase64 { get; set; } // Adiciona a propriedade base64

    //public IFormFile ImagemCertificado { get; set; }
}