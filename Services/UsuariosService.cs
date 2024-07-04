using System.Data;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Controllers;

namespace EMISSOR_DE_CERTIFICADOS.Services
{
    public class UsuariosService
    {
        private readonly DBHelpers _dbHelper;
        public UsuariosService(DBHelpers dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper), "O DBHelper não pode ser nulo.");
        }        
        public async Task<int> GerarUsuarioAsync(int idPessoa)
        {
            int idUsuario = -1;
            string cpf = string.Empty;
            string sSQL = string.Empty;
            string senha = string.Empty;

            try
            {
                cpf =  await RetornarCPFAsync(idPessoa);

                if (!await UsuarioExisteAsync(cpf))
                {
                    senha = await CriarSenhaAsync(cpf);

                    sSQL = "INSERT INTO USUARIO (USUARIO, SENHA, ADMINISTRATIVO, DATA_CADASTRO) " +
                           "VALUES (@CPF, @Senha, 0, GETDATE()); " +
                           "SELECT SCOPE_IDENTITY();"; // Obtem o ID do usuario inserido

                    var parameters = new Dictionary<string, object>
                    {
                        { "@CPF", cpf },
                        { "@Senha", senha }
                    };

                    idUsuario = await _dbHelper.ExecuteScalarAsync<int>(sSQL, parameters);
                }
                else
                {
                    idUsuario = await RetornarIdAsync(cpf); // Considere tornar este método assíncrono também
                }

                return idUsuario;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [UsuariosService.GerarUsuarioAsync]: {ex.Message}");
            }
        }                
        private async Task<string> CriarSenhaAsync(string cpf)
        {
            return await Task.Run(() =>
            {
                string senha = string.Empty;
                string chave = "EMESCAM";
                try
                {
                    // Concatenar CPF e chave
                    string baseString = cpf + chave;

                    // Gerar hash SHA256 da string base
                    using (SHA256 sha256Hash = SHA256.Create())
                    {
                        // Converter a string base em bytes
                        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(baseString));

                        // Converter os bytes do hash em uma string hexadecimal
                        StringBuilder builder = new StringBuilder();
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            builder.Append(bytes[i].ToString("x2"));
                        }

                        // Pegar os primeiros 8 caracteres da string hexadecimal
                        senha = builder.ToString().Substring(0, 6).ToUpper();
                    }

                    return senha;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro em [UsuariosService.CriarSenhaAsync]: {ex.Message}");
                }
            });
        }                
        private async Task<string> RetornarCPFAsync(int idPessoa)
        {
            string cpf = string.Empty;

            try
            {
                using (PessoaController pessoaController = new PessoaController(_dbHelper))
                {
                    cpf = await pessoaController.ObterCPFPorIdPessoaAsync(idPessoa);
                }
                return cpf;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [UsuariosService.RetornarCPFAsync]: {ex.Message}");
            }
        }       
        private async Task<bool> UsuarioExisteAsync(string usuario)
        {
            try
            {
                // Consulta o banco de dados para verificar se existe uma pessoa com o mesmo CPF
                var sSQL = $"SELECT COUNT(*) FROM USUARIO WHERE USUARIO = '{usuario}'";
                var result = await _dbHelper.ExecuteScalarAsync<int>(sSQL);
                int count = Convert.ToInt32(result);

                return count > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [PessoaController.UsuarioExisteAsync] Erro: {ex.Message}");
            }
        }                
        private async Task<int> RetornarIdAsync(string usuario)
        {
            try
            {
                var query = $"SELECT ID FROM USUARIO WHERE USUARIO = '{usuario}'";
                var result = await _dbHelper.ExecuteScalarAsync<int>(query);

                // Se o resultado não for nulo, converte para inteiro e retorna o ID
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
                else
                {
                    throw new InvalidOperationException("Não foi possível encontrar o registo com o Usuario fornecido.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [UsuariosService.RetornarIdAsync]: {ex.Message}");
            }
        }
        public async Task<UsuarioSenha> ObterUsuarioESenhaAsync(int idEventoPessoa)
        {
            try
            {
                var sSQL = "SELECT U.USUARIO, U.SENHA" +
                           " FROM EVENTO_PESSOA EP" +
                           " JOIN PESSOA P ON (EP.ID_PESSOA = P.ID)" +
                           " JOIN USUARIO U ON (P.CPF = U.USUARIO)" +
                           " WHERE EP.ID = @IdEventoPessoa";

                var parameters = new Dictionary<string, object>
                {
                    { "@IdEventoPessoa", idEventoPessoa }
                };

                // Executar a consulta
                DataTable result = await _dbHelper.ExecuteQueryAsync(sSQL, parameters);

                if (result.Rows.Count == 0)
                {
                    throw new Exception("Nenhum usuário encontrado para o idEventoPessoa fornecido.");
                }

                // Extrair os dados da primeira linha retornada
                DataRow row = result.Rows[0];
                var usuarioSenha = new UsuarioSenha
                {
                    Usuario = Convert.ToString(row["USUARIO"]),
                    Senha = Convert.ToString(row["SENHA"])
                };

                return usuarioSenha;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [UsuariosService.ObterUsuarioESenhaAsync]: {ex.Message}");
            }
        }
    }
    public class UsuarioSenha
    {
        public string Usuario { get; set; }
        public string Senha { get; set; }
    }
}
