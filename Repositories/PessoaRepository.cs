using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Interfaces;
using EMISSOR_DE_CERTIFICADOS.Models;
using System.Data;

namespace EMISSOR_DE_CERTIFICADOS.Repositories
{
    internal class PessoaRepository : IPessoaRepository
    {
        private readonly IDBHelpers _dbHelper;
        public PessoaRepository(IDBHelpers dbHelper)
        {
            _dbHelper = dbHelper;
        }
        public async Task<IEnumerable<PessoaModel>> BuscarTodasPessoasAsync()
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaRepository.BuscarTodasPessoasAsync]: {ex.Message}");
            }
        }
        public async Task<PessoaModel> BuscarPessoaPorIdAsync(int id)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaRepository.BuscarPessoaPorIdAsync]: {ex.Message}");
            }           
        }
        public async Task InserirPessoaAsync(PessoaModel pessoa, int idUsuarioAdministrativo)
        {
            try
            {
                var query = $"INSERT INTO PESSOA (NOME, CPF, EMAIL, ID_USUARIO_ADMINISTRATIVO, DATA_CADASTRO) VALUES ('{pessoa.Nome}', '{pessoa.CPF}', '{pessoa.Email}', {idUsuarioAdministrativo}, GETDATE())" +
                    "SELECT SCOPE_IDENTITY();"; 
                //await _dbHelper.ExecuteQueryAsync(query);

                var result = await _dbHelper.ExecuteScalarAsync<int>(query);
                pessoa.Id =  result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaRepository.InserirPessoaAsync]: {ex.Message}");
            }           
        }
        public async Task AtualizarPessoaAsync(PessoaModel pessoa)
        {
            try
            {
                var query = $"UPDATE Pessoa SET Nome = '{pessoa.Nome}', CPF = '{pessoa.CPF}', Email = '{pessoa.Email}' WHERE Id = {pessoa.Id}";
                await _dbHelper.ExecuteQueryAsync(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaRepository.AtualizarPessoaAsync]: {ex.Message}");
            }            
        }
        public async Task DeletarPessoaAsync(int id)
        {
            try
            {
                var query = $"DELETE FROM Pessoa WHERE Id = {id}";
                await _dbHelper.ExecuteQueryAsync(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaRepository.DeletarPessoaAsync]: {ex.Message}");
            }
        }
        public async Task<bool> ExistePessoaComCPFAsync(string cpf, int? userId = null)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaRepository.ExistePessoaComCPFAsync]: {ex.Message}");
            }
        }
        public async Task<int> ObterIdPessoaPorCPFAsync(string cpf)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaRepository.ObterIdPessoaPorCPFAsync]: {ex.Message}");
            }           
        }
        public async Task<string> ObterCPFPorIdPessoaAsync(int id)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaRepository.ObterCPFPorIdPessoaAsync]: {ex.Message}");
            }            
        }
        public async Task<string> ObterNomePorIdPessoaAsync(int id)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaRepository.ObterNomePorIdPessoaAsync]: {ex.Message}");
            }
        }
        public async Task<string> ObterNomePorCPFAsync(string cpf) 
        {
            try
            {
                var query = $"SELECT NOME FROM PESSOA WHERE CPF = '{cpf}'";
                var result = await _dbHelper.ExecuteScalarAsync(query);
                if (result != null)
                {
                    return Convert.ToString(result);
                }
                else 
                {
                    throw new InvalidOperationException("Não foi possível encontrar o nome com o cpf fornecido.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaRepository.ObterNomePorCPFAsync]: {ex.Message}");
            }
        }
    }
}