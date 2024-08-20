using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMISSOR_DE_CERTIFICADOS.Models;
using EMISSOR_DE_CERTIFICADOS.Interfaces;
using EMISSOR_DE_CERTIFICADOS.Helpers; // Adicione o namespace onde o IPessoaService está localizado

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    public class PessoaController : Controller
    {
        private readonly ISessao _sessao;
        private readonly IPessoaService _pessoaService;
        public PessoaController(IPessoaService pessoaService, ISessao sessao)
        {
            _pessoaService = pessoaService ?? throw new ArgumentNullException(nameof(pessoaService), "O IPessoaService não pode ser nulo.");
            _sessao = sessao ?? throw new ArgumentNullException(nameof(sessao), "O ISessao não pode ser nulo.");
        }

        #region *** IActionResults ***        
        public async Task<IActionResult> Index()
        {
            try
            {
                // Recupera todas as pessoas do banco de dados
                var pessoas = await _pessoaService.BuscarTodasPessoasAsync();
                return View(pessoas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em [PessoaController.Index] Erro: {ex.Message}");
            }            
        }        
        [HttpGet]
        public async Task<IActionResult> BuscarPessoas(string termo)
        {
            try
            {
                var pessoas = await _pessoaService.BuscarPorNomeCpfEmailAsync(termo);
                return Json(pessoas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em [PessoaController.BuscarPessoas] Erro: {ex.Message}");
            }            
        }
        [HttpGet]
        public async Task<IActionResult> BuscarEventosPessoa(int id)
        {
            try
            {
                var eventos = await _pessoaService.BuscarEventosPessoaAsync(id);
                return Json(eventos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em [PessoaController.BuscarEventosPessoa] Erro: {ex.Message}");
            }           
        }
        [HttpPost]
        public async Task<IActionResult> ReenviarInstrucoes(int idEventoPessoa) 
        {
            try
            {                
                bool retorno = await _pessoaService.ReenviarInstrucoesAsync(idEventoPessoa);
                return View();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em [PessoaController.ReenviarInstrucoes] Erro: {ex.Message}");
            }
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarEmail(int idPessoa, string email)
        {
            try
            {
                await _pessoaService.AtualizarEmailAsync(idPessoa, email);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em [PessoaController.AtualizarEmail] Erro: {ex.Message}");
            }            
        }
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
        [HttpGet]
        public IActionResult CheckSession()
        {
            var user = _sessao.BuscarSessaodoUsuario();
            if (user == null)
            {
                return Unauthorized();
            }
            return Ok();
        }
        #endregion
    }
}