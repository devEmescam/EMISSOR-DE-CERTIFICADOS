using Microsoft.AspNetCore.Mvc;
using System.Data;
using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Models;

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
        // GET: Pessoa
        public IActionResult Index()
        {
            // Recupera todas as pessoas do banco de dados
            var pessoas = GetAllPessoas();
            return View(pessoas);
        }

        // GET: Pessoa/Details/5
        public IActionResult Details(int id)
        {
            // Recupera uma pessoa específica do banco de dados
            var pessoa = GetPessoaById(id);
            if (pessoa == null)
            {
                return NotFound();
            }
            return View(pessoa);
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
                InsertPessoa(pessoa);
                return RedirectToAction(nameof(Index));
            }
            return View(pessoa);
        }

        // GET: Pessoa/Edit/5
        public IActionResult Edit(int id)
        {
            // Recupera a pessoa do banco de dados para edição
            var pessoa = GetPessoaById(id);
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
                UpdatePessoa(pessoa);
                return RedirectToAction(nameof(Index));
            }
            return View(pessoa);
        }

        // GET: Pessoa/Delete/5
        public IActionResult Delete(int id)
        {
            // Recupera a pessoa do banco de dados para exclusão
            var pessoa = GetPessoaById(id);
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
            DeletePessoa(id);
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region *** METODOS PRIVADOS ***
        // Método para retornar todas as pessoas do banco de dados
        private IEnumerable<PessoaModel> GetAllPessoas()
        {
            var query = "SELECT * FROM Pessoa";
            var dataTable = _dbHelper.ExecuteQuery(query);
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

        // Método para retornar uma pessoa específica pelo ID
        private PessoaModel GetPessoaById(int id)
        {
            var query = $"SELECT * FROM Pessoa WHERE Id = {id}";
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

        // Método para inserir uma nova pessoa no banco de dados
        private void InsertPessoa(PessoaModel pessoa)
        {
            var query = $"INSERT INTO Pessoa (Nome, CPF, Email) VALUES ('{pessoa.Nome}', '{pessoa.CPF}', '{pessoa.Email}')";
            _dbHelper.ExecuteQuery(query);
        }

        // Método para atualizar uma pessoa no banco de dados
        private void UpdatePessoa(PessoaModel pessoa)
        {
            var query = $"UPDATE Pessoa SET Nome = '{pessoa.Nome}', CPF = '{pessoa.CPF}', Email = '{pessoa.Email}' WHERE Id = {pessoa.Id}";
            _dbHelper.ExecuteQuery(query);
        }

        // Método para excluir uma pessoa do banco de dados
        private void DeletePessoa(int id)
        {
            var query = $"DELETE FROM Pessoa WHERE Id = {id}";
            _dbHelper.ExecuteQuery(query);
        }
        #endregion
    }
}
