using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Helpers;
using EMISSOR_DE_CERTIFICADOS.Interfaces;
using EMISSOR_DE_CERTIFICADOS.Services;
using System.Data;

namespace EMISSOR_DE_CERTIFICADOS.Repositories
{
    internal class UsuarioRepository: IUsuarioRepository
    {        
        private readonly IDBHelpers _dbHelpers;

        public UsuarioRepository( IDBHelpers dBHelpers)
        {            
            _dbHelpers = dBHelpers;
        }
        public async Task<int> GerarUsuarioAsync(string cpf, string senha)
        {
            int idUsuario = -1;            
            string sSQL = string.Empty;
            try
            {
                sSQL = "INSERT INTO USUARIO (USUARIO, SENHA, ADMINISTRATIVO, DATA_CADASTRO) " +
                           "VALUES (@CPF, @Senha, 0, GETDATE()); " +
                           "SELECT SCOPE_IDENTITY();"; // Obtem o ID do usuario inserido

                var parameters = new Dictionary<string, object>
                    {
                        { "@CPF", cpf },
                        { "@Senha", senha }
                    };

                idUsuario = await _dbHelpers.ExecuteScalarAsync<int>(sSQL, parameters);

                return idUsuario;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [UsuarioRepository.GerarUsuarioAsync]: {ex.Message}");
            }
        }
        public async Task<bool> UsuarioExisteAsync(string usuario)
        {
            try
            {
                // Consulta o banco de dados para verificar se existe uma pessoa com o mesmo CPF
                var sSQL = $"SELECT COUNT(*) FROM USUARIO WHERE USUARIO = '{usuario}'";
                var result = await _dbHelpers.ExecuteScalarAsync<int>(sSQL);
                int count = Convert.ToInt32(result);
                return count > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [UsuarioRepository.UsuarioExisteAsync] Erro: {ex.Message}");
            }
        }
        public async Task<int> RetornarIdAsync(string usuario)
        {
            try
            {
                var query = $"SELECT ID FROM USUARIO WHERE USUARIO = '{usuario}'";
                var result = await _dbHelpers.ExecuteScalarAsync<int>(query);

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
                throw new Exception($"Erro em [UsuarioRepository.RetornarIdAsync]: {ex.Message}");
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
                DataTable result = await _dbHelpers.ExecuteQueryAsync(sSQL, parameters);

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
                throw new Exception($"Erro em [UsuarioRepository.ObterUsuarioESenhaAsync]: {ex.Message}");
            }
        }
    }
}
public class UsuarioSenha
{
    public string Usuario { get; set; }
    public string Senha { get; set; }
}