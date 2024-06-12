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
        public int GerarUsuario(int idPessoa) 
        {
            int idUsuario = -1;            
            string cpf = string.Empty;
            string sSQL = string.Empty;
            string senha = string.Empty;            

            try
			{
                cpf = RetornarCPF(idPessoa);

                if (!UsuarioExiste(cpf))
                {
                    senha = CriarSenha(cpf);

                    sSQL = $"INSERT INTO USUARIO (USUARIO, SENHA, ADMINISTRATIVO)" +
                        $" VALUES ('{cpf}', '{senha}', 0);" +                        
                        " SELECT SCOPE_IDENTITY();"; // Obtem o ID do usuario inserido

                    idUsuario = _dbHelper.ExecuteScalar<int>(sSQL);
                }
                else 
                {
                    idUsuario = RetornarId(cpf);
                }
                
                return idUsuario;
            }
			catch (Exception ex)
			{
                throw new Exception($"Erro em [UsuariosService.GerarUsuario]: {ex.Message}");
            }
        }
        public async Task<int> GerarUsuarioAsync(int idPessoa)
        {
            int idUsuario = -1;
            string cpf = string.Empty;
            string sSQL = string.Empty;
            string senha = string.Empty;

            try
            {
                cpf = RetornarCPF(idPessoa);

                if (!UsuarioExiste(cpf))
                {
                    senha = CriarSenha(cpf);

                    sSQL = $"INSERT INTO USUARIO (USUARIO, SENHA, ADMINISTRATIVO)" +
                         $" VALUES ('{cpf}', '{senha}', 0)" +
                         $" WHERE USUARIO <> '{cpf}'; " +
                         "SELECT SCOPE_IDENTITY();"; // Obtem o ID do usuario inserido

                    idUsuario = await _dbHelper.ExecuteScalarAsync<int>(sSQL);
                }
                else
                {
                    idUsuario = RetornarId(cpf); // Considere tornar este método assíncrono também
                }

                return idUsuario;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [UsuariosService.GerarUsuarioAsync]: {ex.Message}");
            }
        }

        private string CriarSenha(string cpf) 
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
                throw new Exception($"Erro em [UsuariosService.CriarSenha]: {ex.Message}");
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

        private string RetornarCPF(int idPessoa) 
        {
            string cpf = string.Empty;           

            try
            {
                using (PessoaController pessoaController = new PessoaController(_dbHelper))
                {
                    cpf = pessoaController.ObterCPFPorIdPessoa(idPessoa);                    
                }
                return cpf;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [UsuariosService.RetornarCPF]: {ex.Message}");
            }
        
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

        private bool UsuarioExiste(string usuario)
        {
            try
            {
                // Consulta o banco de dados para verificar se existe uma pessoa com o mesmo CPF
                var sSQL = $"SELECT COUNT(*) FROM USUARIO WHERE USUARIO = '{usuario}'";
                var result = _dbHelper.ExecuteScalar(sSQL);
                int count = Convert.ToInt32(result);

                return count > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [PessoaController.ExistePessoaComCPF] Erro: {ex.Message}");
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

        private int RetornarId(string usuario) 
        {
            try
            {
                var query = $"SELECT ID FROM USUARIO WHERE USUARIO = '{usuario}'";
                var result = _dbHelper.ExecuteScalar(query);

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
                throw new Exception($"Erro em [UsuariosService.RetornarId]: {ex.Message}");
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
    }
}
