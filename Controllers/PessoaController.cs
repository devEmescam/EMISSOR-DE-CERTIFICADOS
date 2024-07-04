using Microsoft.AspNetCore.Mvc;
using System.Data;
using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Models;
using EMISSOR_DE_CERTIFICADOS.Helpers;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    public class PessoaController : Controller
    {
        private readonly DBHelpers _dbHelper;

        public PessoaController(DBHelpers dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper), "O DBHelpers não pode ser nulo.");
        }

        #region *** IActionResults ***        
        public async Task<IActionResult> Index()
        {
            // Recupera todas as pessoas do banco de dados
            var pessoas = await BuscarTodasPessoasAsync();
            return View(pessoas);
        }

        // GET: Pessoa/Details/5        
        public async Task<IActionResult> Details(int id)
        {
            // Recupera uma pessoa específica do banco de dados
            var pessoa = await BuscarPessoaPorIdAsync(id);
            if (pessoa == null)
            {
                return NotFound();
            }
            return View(pessoa);
        }
        
        [HttpGet]
        public async Task<IActionResult> BuscarPessoas(string termo)
        {
            var pessoas = (await BuscarPorNomeCpfEmailAsync(termo)).Select(p => new {p.Id, p.Nome, p.CPF, p.Email}).ToList();
            return Json(pessoas);
        }

        // GET: Pessoa/Create        
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: Pessoa/Create        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PessoaModel pessoa)
        {
            if (ModelState.IsValid)
            {
                // Insere a pessoa no banco de dados
                await InserirPessoaAsync(pessoa);
                return RedirectToAction(nameof(Index));
            }
            return View(pessoa);
        }

        // GET: Pessoa/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Recupera a pessoa do banco de dados para edição
            var pessoa = await BuscarPessoaPorIdAsync(id);
            if (pessoa == null)
            {
                return NotFound();
            }
            return View(pessoa);
        }

        // POST: Pessoa/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PessoaModel pessoa)
        {
            if (id != pessoa.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                // Atualiza a pessoa no banco de dados
                await AtualizarPessoaAsync(pessoa);
                return RedirectToAction(nameof(Index));
            }
            return View(pessoa);
        }

        // GET: Pessoa/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            // Recupera a pessoa do banco de dados para exclusão
            var pessoa = await BuscarPessoaPorIdAsync(id);
            if (pessoa == null)
            {
                return NotFound();
            }
            return View(pessoa);
        }

        // POST: Pessoa/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Exclui a pessoa do banco de dados
            await DeletarPessoaAsync(id);
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region *** METODOS ***      
        // VERSÃO ASYNC: Método para buscar pessoas pelo termo informado
        private async Task<IEnumerable<PessoaModel>> BuscarPorNomeCpfEmailAsync(string termo)
        {
            try
            {
                var query = @"SELECT * FROM PESSOA 
                              WHERE Nome LIKE @Termo 
                                    OR CPF LIKE @Termo 
                                    OR Email LIKE @Termo";

                var parameters = new Dictionary<string, object>
                {
                    { "@Termo", "%" + termo + "%" }
                };

                var dataTable = await _dbHelper.ExecuteQueryAsync(query, parameters);
                var pessoas = new List<PessoaModel>();

                foreach (DataRow row in dataTable.Rows)
                {
                    pessoas.Add(new PessoaModel
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Nome = Convert.ToString(row["Nome"]),
                        CPF = Convert.ToString(row["CPF"]),
                        Email = Convert.ToString(row["Email"])
                    });
                }

                return pessoas;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [PessoaController.BuscarPorNomeCpfEmailAsync] Erro: {ex.Message}");
            }
        }
                
        // VERSÃO ASYNC: Método assíncrono para retornar todas as pessoas do banco de dados
        private async Task<IEnumerable<PessoaModel>> BuscarTodasPessoasAsync()
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
                throw new Exception($"Ocorreu um erro em [PessoaController.BuscarTodasPessoasAsync] Erro: {ex.Message}");
            }
        }
                
        // VERSÃO ASYNC: Método assíncrono para retornar uma pessoa específica pelo ID
        private async Task<PessoaModel> BuscarPessoaPorIdAsync(int id)
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
                throw new Exception($"Ocorreu um erro em [PessoaController.BuscarPessoaPorIdAsync] Erro: {ex.Message}");
            }
        }
                
        // VERSÃO ASYNC: Método assíncrono para inserir uma nova pessoa no banco de dados. Publico porque é chamado no momento de inserir novos EVENTOS
        public async Task<string> InserirPessoaAsync(PessoaModel pessoa, int? userId = null)
        {
            int? idUsuario = userId;
            try
            {               

                // Valida se a pessoa não existe no banco de dados
                if (!await ExistePessoaComCPFAsync(pessoa.CPF))
                {
                    if (Util.ValidaCPF(pessoa.CPF))
                    {
                        if (Util.ValidaEstruturaEmail(pessoa.Email))
                        {
                            if (idUsuario == null)
                            {
                                //recuperar o ID do usuario logado 
                                idUsuario = HttpContext.Session.GetInt32("UserId");
                                if (idUsuario == null)
                                {
                                    throw new Exception("ID do usuário não encontrado na sessão.");
                                }
                            }

                            // Se não existir, insere a nova pessoa
                            var query = $"INSERT INTO PESSOA (NOME, CPF, EMAIL, ID_USUARIO_ADMINISTRATIVO, DATA_CADASTRO) VALUES ('{pessoa.Nome}', '{pessoa.CPF}', '{pessoa.Email}', {idUsuario}, GETDATE())";

                            await _dbHelper.ExecuteQueryAsync(query);

                            pessoa.Id = await ObterIdPessoaPorCPFAsync(pessoa.CPF);
                            return "Inserido com Sucesso.";
                        }
                        else
                        {
                            throw new Exception($"Ocorreu um erro em [PessoaController.InserirPessoa] Erro: O e-mail {pessoa.Email} informado de {pessoa.Nome} não é válido.");
                        }
                    }
                    else
                    {
                        throw new Exception($"Ocorreu um erro em [PessoaController.InserirPessoa] Erro: O CPF {pessoa.CPF} informado de {pessoa.Nome} não é válido.");
                    }
                }
                else
                {
                    pessoa.Id = await ObterIdPessoaPorCPFAsync(pessoa.CPF);
                    return "CPF informado já existe em banco de dados.";
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [PessoaController.InserirPessoaAsync] Erro: {ex.Message}");
            }
        }
                
        // VERSÃO ASYNC: Método assíncrono para atualizar uma pessoa no banco de dados
        private async Task AtualizarPessoaAsync(PessoaModel pessoa)
        {
            try
            {
                var query = $"UPDATE Pessoa SET Nome = '{pessoa.Nome}', CPF = '{pessoa.CPF}', Email = '{pessoa.Email}' WHERE Id = {pessoa.Id}";
                await _dbHelper.ExecuteQueryAsync(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [PessoaController.AtualizarPessoaAsync] Erro: {ex.Message}");
            }
        }

        // VERSÃO ASYNC: Método assíncrono para excluir uma pessoa do banco de dados
        private async Task DeletarPessoaAsync(int id)
        {
            try
            {
                var query = $"DELETE FROM Pessoa WHERE Id = {id}";
                await _dbHelper.ExecuteQueryAsync(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [PessoaController.DeletarPessoaAsync] Erro: {ex.Message}");
            }
        }
                
        // VERSÃO ASYNC: Método assíncrono que valida se a pessoa existe através do CPF
        private async Task<bool> ExistePessoaComCPFAsync(string cpf)
        {
            try
            {
                // Consulta o banco de dados para verificar se existe uma pessoa com o mesmo CPF
                var query = $"SELECT COUNT(*) FROM Pessoa WHERE CPF = @CPF";
                var parameters = new Dictionary<string, object>
                {
                    { "@CPF", cpf }
                };

                var result = await _dbHelper.ExecuteScalarAsync<int>(query, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [PessoaController.ExistePessoaComCPFAsync] Erro: {ex.Message}");
            }
        }

        //VERSÃO ASYNC: Método assíncrono que retorna o ID da pessoa através do CPF
        private async Task<int> ObterIdPessoaPorCPFAsync(string cpf)
        {
            try
            {
                var query = $"SELECT Id FROM Pessoa WHERE CPF = '{cpf}'";
                var result = await _dbHelper.ExecuteScalarAsync(query);

                // Se o resultado não for nulo, converte para inteiro e retorna o ID
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
                else
                {
                    throw new InvalidOperationException("Não foi possível encontrar o id com o CPF fornecido.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [PessoaController.ObterIdPessoaPorCPFAsync] Erro: {ex.Message}");
            }
        }

        //VERSÃO ASYNC: Método assíncrono que retorna o CPF da pessoa através do ID
        public async Task<string> ObterCPFPorIdPessoaAsync(int id)
        {
            try
            {
                var query = $"SELECT CPF FROM Pessoa WHERE ID = {id}";
                var result = await _dbHelper.ExecuteScalarAsync(query);

                // Se o resultado não for nulo, converte para string e retorna o conteúdo
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
                throw new Exception($"Erro em PessoaController.ObterCPFPorIdPessoaAsync: {ex.Message}");
            }
        }        

        //Método assíncrono que retorna o Nome da pessoa através do ID
        public async Task<string> ObterNomePorIdPessoaAsync(int id)
        {
            try
            {
                var query = $"SELECT NOME FROM Pessoa WHERE ID = {id}";
                var result = await _dbHelper.ExecuteScalarAsync(query);

                // Se o resultado não for nulo, converte para string e retorna o conteúdo
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
                throw new Exception($"Erro em PessoaController.ObterNomePorIdPessoa: {ex.Message}");
            }
        }
        #endregion
    }
}
