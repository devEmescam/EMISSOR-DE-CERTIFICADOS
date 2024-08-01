using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Interfaces;
using EMISSOR_DE_CERTIFICADOS.Models;
using System.Data;

namespace EMISSOR_DE_CERTIFICADOS.Repositories
{
    public class PessoaRepository : IPessoaRepository
    {
        private readonly DBHelpers _dbHelper;
        public PessoaRepository(DBHelpers dbHelper)
        {
            _dbHelper = dbHelper;
        }
        public async Task<IEnumerable<PessoaModel>> BuscarTodasPessoasAsync()
        {
            var query = "SELECT * FROM PESSOA";
            var dataTable = await _dbHelper.ExecuteQueryAsync(query);
            var pessoas = new List<PessoaModel>();

            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    pessoas.Add(new PessoaModel
                    {
                        Id = Convert.ToInt32(row["ID"]),
                        Nome = Convert.ToString(row["NOME"]),
                        CPF = Convert.ToString(row["CPF"]),
                        Email = Convert.ToString(row["EMAIL"])
                    });
                }
            }

            return pessoas;
        }
        public async Task<PessoaModel> BuscarPessoaPorIdAsync(int id)
        {
            var query = $"SELECT * FROM PESSOA WHERE ID = {id}";
            var dataTable = await _dbHelper.ExecuteQueryAsync(query);
            if (dataTable.Rows.Count > 0)
            {
                var row = dataTable.Rows[0];
                return new PessoaModel
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Nome = Convert.ToString(row["Nome"]),
                    CPF = Convert.ToString(row["CPF"]),
                    Email = Convert.ToString(row["Email"])
                };
            }
            return null;
        }
        public async Task InserirPessoaAsync(PessoaModel pessoa, int idUsuarioAdministrativo)
        {
            var query = $"INSERT INTO PESSOA (NOME, CPF, EMAIL, ID_USUARIO_ADMINISTRATIVO, DATA_CADASTRO) VALUES ('{pessoa.Nome}', '{pessoa.CPF}', '{pessoa.Email}', {idUsuarioAdministrativo}, GETDATE())";
            await _dbHelper.ExecuteQueryAsync(query);
        }
        public async Task AtualizarPessoaAsync(PessoaModel pessoa)
        {
            var query = $"UPDATE Pessoa SET Nome = '{pessoa.Nome}', CPF = '{pessoa.CPF}', Email = '{pessoa.Email}' WHERE Id = {pessoa.Id}";
            await _dbHelper.ExecuteQueryAsync(query);
        }
        public async Task DeletarPessoaAsync(int id)
        {
            var query = $"DELETE FROM Pessoa WHERE Id = {id}";
            await _dbHelper.ExecuteQueryAsync(query);
        }
        public async Task<bool> ExistePessoaComCPFAsync(string cpf, int? userId = null)
        {
            var query = $"SELECT COUNT(*) FROM Pessoa WHERE CPF = @CPF and ID_USUARIO_ADMINISTRATIVO = @ID_USUARIO_ADMINISTRATIVO";
            var parameters = new Dictionary<string, object>
        {
            { "@CPF", cpf },
            { "@ID_USUARIO_ADMINISTRATIVO", userId}
        };

            var result = await _dbHelper.ExecuteScalarAsync<int>(query, parameters);
            return result > 0;
        }
        public async Task<int> ObterIdPessoaPorCPFAsync(string cpf)
        {
            var query = $"SELECT Id FROM Pessoa WHERE CPF = '{cpf}'";
            var result = await _dbHelper.ExecuteScalarAsync(query);

            if (result != null)
            {
                return Convert.ToInt32(result);
            }
            else
            {
                return 0;
            }
        }
        public async Task<string> ObterCPFPorIdPessoaAsync(int id)
        {
            var query = $"SELECT CPF FROM Pessoa WHERE ID = {id}";
            var result = await _dbHelper.ExecuteScalarAsync(query);

            if (result != null)
            {
                return Convert.ToString(result);
            }
            else
            {
                throw new InvalidOperationException("Não foi possível encontrar o cpf com o id fornecido.");
            }
        }
        public async Task<string> ObterNomePorIdPessoaAsync(int id)
        {
            var query = $"SELECT NOME FROM Pessoa WHERE ID = {id}";
            var result = await _dbHelper.ExecuteScalarAsync(query);

            if (result != null)
            {
                return Convert.ToString(result);
            }
            else
            {
                throw new InvalidOperationException("Não foi possível encontrar o nome com o id fornecido.");
            }
        }
    }
}