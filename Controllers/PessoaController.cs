using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMISSOR_DE_CERTIFICADOS.Models;
using EMISSOR_DE_CERTIFICADOS.Interfaces; // Adicione o namespace onde o IPessoaService está localizado

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    public class PessoaController : Controller
    {
        private readonly IPessoaService _pessoaService;
        public PessoaController(IPessoaService pessoaService)
        {
            _pessoaService = pessoaService ?? throw new ArgumentNullException(nameof(pessoaService), "O IPessoaService não pode ser nulo.");
        }

        #region *** IActionResults ***        
        public async Task<IActionResult> Index()
        {
            // Recupera todas as pessoas do banco de dados
            var pessoas = await _pessoaService.BuscarTodasPessoasAsync();
            return View(pessoas);
        }
        public async Task<IActionResult> Details(int id)
        {
            // Recupera uma pessoa específica do banco de dados
            var pessoa = await _pessoaService.BuscarPessoaPorIdAsync(id);
            if (pessoa == null)
            {
                return NotFound();
            }
            return View(pessoa);
        }
        [HttpGet]
        public async Task<IActionResult> BuscarPessoas(string termo)
        {
            var pessoas = await _pessoaService.BuscarPorNomeCpfEmailAsync(termo);
            return Json(pessoas);
        }
        [HttpGet]
        public async Task<IActionResult> ObterIdPessoaPorCPF(string cpf)
        {
            int id = 0;
            try
            {
                id = await _pessoaService.ObterIdPessoaPorCPFAsync(cpf);
                return Json(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em [PessoaController.ObterIdPessoaPorCPF]. Erro: {ex.Message}");
            }
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PessoaModel pessoa)
        {
            if (ModelState.IsValid)
            {
                // Insere a pessoa no banco de dados
                await _pessoaService.InserirPessoaAsync(pessoa);
                return RedirectToAction(nameof(Index));
            }
            return View(pessoa);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Recupera a pessoa do banco de dados para edição
            var pessoa = await _pessoaService.BuscarPessoaPorIdAsync(id);
            if (pessoa == null)
            {
                return NotFound();
            }
            return View(pessoa);
        }
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
                await _pessoaService.AtualizarPessoaAsync(pessoa);
                return RedirectToAction(nameof(Index));
            }
            return View(pessoa);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            // Recupera a pessoa do banco de dados para exclusão
            var pessoa = await _pessoaService.BuscarPessoaPorIdAsync(id);
            if (pessoa == null)
            {
                return NotFound();
            }
            return View(pessoa);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Exclui a pessoa do banco de dados
            await _pessoaService.DeletarPessoaAsync(id);
            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
