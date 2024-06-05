using System.Data;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using EMISSOR_DE_CERTIFICADOS.DBConnections;

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
                senha = CriarSenha(cpf);

                sSQL = $"INSERT INTO USUARIO (USUARIO, SENHA)" +
                    $" VALUES ('{cpf}', '{senha}');" +
                    "SELECT SCOPE_IDENTITY();"; // Obtem o ID do usuario inserido

                idUsuario = _dbHelper.ExecuteScalar<int>(sSQL);
                return idUsuario;

            }
			catch (Exception ex)
			{
                throw new Exception($"Erro em [UsuariosService.GerarUsuario]: {ex.Message}");
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
                    senha = builder.ToString().Substring(0, 8);
                }

                return senha;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [UsuariosService.CriarSenha]: {ex.Message}");
            }
        
        }

        private string RetornarCPF(int idPessoa) 
        {
            string cpf = string.Empty;
            string sSQL = string.Empty;
            DataTable oDT = new DataTable();

            try
            {
                sSQL = $"SELECT CPF FROM PESSOA WHERE ID = {idPessoa}";

                oDT = _dbHelper.ExecuteQuery(sSQL);
                if (oDT != null && oDT.Rows.Count > 0)
                {
                    var row = oDT.Rows[0];
                    cpf = Convert.ToString(row["CPF"]);
                }
                return cpf;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [UsuariosService.RetornarCPF]: {ex.Message}");
            }
        
        }
    }
}
