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

        // GET: PESSOAS
        public IActionResult Index()
        {
            // Recupera todas as pessoas do banco de dados
            var pessoas = BuscarTodasPessoas();
            return View(pessoas);
        }

        // GET: Pessoa/Details/5
        public IActionResult Details(int id)
        {
            // Recupera uma pessoa específica do banco de dados
            var pessoa = BuscarPessoaPorId(id);
            if (pessoa == null)
            {
                return NotFound();
            }
            return View(pessoa);
        }

        [HttpGet]
        public IActionResult BuscarPessoas(string termo)
        {          

            var pessoas = BuscarPessoasPorNomeOuCpfOuEmail(termo).Select(p => new {p.Id, p.Nome, p.CPF, p.Email}).ToList();
            return Json(pessoas);
        }

        // GET: Pessoa/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pessoa/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PessoaModel pessoa)
        {
            if (ModelState.IsValid)
            {
                // Insere a pessoa no banco de dados
                InserirPessoa(pessoa);
                return RedirectToAction(nameof(Index));
            }
            return View(pessoa);
        }

        // GET: Pessoa/Edit/5
        public IActionResult Edit(int id)
        {
            // Recupera a pessoa do banco de dados para edição
            var pessoa = BuscarPessoaPorId(id);
            if (pessoa == null)
            {
                return NotFound();
            }
            return View(pessoa);
        }

        // POST: Pessoa/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, PessoaModel pessoa)
        {
            if (id != pessoa.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                // Atualiza a pessoa no banco de dados
                AtualizarPessoa(pessoa);
                return RedirectToAction(nameof(Index));
            }
            return View(pessoa);
        }

        // GET: Pessoa/Delete/5
        public IActionResult Delete(int id)
        {
            // Recupera a pessoa do banco de dados para exclusão
            var pessoa = BuscarPessoaPorId(id);
            if (pessoa == null)
            {
                return NotFound();
            }
            return View(pessoa);
        }

        // POST: Pessoa/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            // Exclui a pessoa do banco de dados
            DeletarPessoa(id);
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region *** METODOS PRIVADOS ***      

        // Método para buscar pessoas pelo termo informado
        private IEnumerable<PessoaModel> BuscarPessoasPorNomeOuCpfOuEmail(string termo)
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

                var dataTable = _dbHelper.ExecuteQuery(query, parameters);
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
                throw new Exception($"Ocorreu um erro em [PessoaController.BuscarPessoasPorNomeOuCpfOuEmail] Erro: {ex.Message}");
            }
        }

        // Método para retornar todas as pessoas do banco de dados
        private IEnumerable<PessoaModel> BuscarTodasPessoas()
        {
            try
            {
                var query = "SELECT * FROM PESSOA";
                var dataTable = _dbHelper.ExecuteQuery(query);
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
                throw new Exception($"Ocorreu um erro em [PessoaController.BuscarTodasPessoas] Erro: {ex.Message}");
            }
        }

        // Método para retornar uma pessoa específica pelo ID
        private PessoaModel BuscarPessoaPorId(int id)
        {
            try
            {
                var query = $"SELECT * FROM PESSOA WHERE ID = {id}";
                var dataTable = _dbHelper.ExecuteQuery(query);
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
                throw new Exception($"Ocorreu um erro em [PessoaController.BuscarPessoaPorId] Erro: {ex.Message}");
            }
        }

        // Método para inserir uma nova pessoa no banco de dados. Publico porque é chamado no momento de inserir novos EVENTOS
        public string InserirPessoa(PessoaModel pessoa, int? userId = null)
        {
            int? idUsuario = userId;
            try
            {
                if (pessoa.Nome == "Maria Cecilia Amarante") 
                {
                    string aux = pessoa.Nome;
                }

                // Valida se a pessoa não existe no banco de dados
                if (!ExistePessoaComCPF(pessoa.CPF))
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
                            var query = $"INSERT INTO Pessoa (Nome, CPF, Email, ID_USUARIO_ADMINISTRATIVO) VALUES ('{pessoa.Nome}', '{pessoa.CPF}', '{pessoa.Email}', {idUsuario})";

                            _dbHelper.ExecuteQuery(query);

                            pessoa.Id = ObterIdPessoaPorCPF(pessoa.CPF);
                            return "Inserido com Sucesso.";
                        }
                        else
                        {
                            //return "O e-mail informado não é válido.";
                            throw new Exception($"Ocorreu um erro em [PessoaController.InserirPessoa] Erro: O e-mail {pessoa.Email} informado de {pessoa.Nome} não é válido.");
                        }
                    }
                    else
                    {
                        //return "O CPF informado não é válido.";
                        throw new Exception($"Ocorreu um erro em [PessoaController.InserirPessoa] Erro: O CPF {pessoa.CPF} informado de {pessoa.Nome} não é válido.");
                    }
                }
                else
                {
                    pessoa.Id = ObterIdPessoaPorCPF(pessoa.CPF);
                    return "CPF informado já existe em banco de dados.";
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [PessoaController.InserirPessoa] Erro: {ex.Message}");
            }
        }

        // Método para atualizar uma pessoa no banco de dados
        private void AtualizarPessoa(PessoaModel pessoa)
        {
            try
            {
                var query = $"UPDATE Pessoa SET Nome = '{pessoa.Nome}', CPF = '{pessoa.CPF}', Email = '{pessoa.Email}' WHERE Id = {pessoa.Id}";
                _dbHelper.ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [PessoaController.AtualizarPessoa] Erro: {ex.Message}");
            }
        }

        // Método para excluir uma pessoa do banco de dados
        private void DeletarPessoa(int id)
        {
            try
            {
                var query = $"DELETE FROM Pessoa WHERE Id = {id}";
                _dbHelper.ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [PessoaController.DeletePessoa] Erro: {ex.Message}");
            }
        }

        private bool ExistePessoaComCPF(string cpf)
        {
            try
            {
                // Consulta o banco de dados para verificar se existe uma pessoa com o mesmo CPF
                var query = $"SELECT COUNT(*) FROM Pessoa WHERE CPF = '{cpf}'";
                var result = _dbHelper.ExecuteScalar(query);
                int count = Convert.ToInt32(result);

                return count > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [PessoaController.ExistePessoaComCPF] Erro: {ex.Message}");
            }
        }

        public int ObterIdPessoaPorCPF(string cpf)
        {
            try
            {
                var query = $"SELECT Id FROM Pessoa WHERE CPF = '{cpf}'";
                var result = _dbHelper.ExecuteScalar(query);

                // Se o resultado não for nulo, converte para inteiro e retorna o ID
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
                else
                {
                    throw new InvalidOperationException("Não foi possível encontrar uma pessoa com o CPF fornecido.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [PessoaController.ObterIdPessoaPorCPF] Erro: {ex.Message}");
            }
        }

        #endregion
    }
}
