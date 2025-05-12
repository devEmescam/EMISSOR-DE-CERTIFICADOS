using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMISSOR_DE_CERTIFICADOS.Models;
using EMISSOR_DE_CERTIFICADOS.Interfaces;
using EMISSOR_DE_CERTIFICADOS.Helpers;
using System;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    [ApiController]
    [Route("api/pessoa-controller")]
    public class PessoaController : ControllerBase
    {
        private readonly ISessao _sessao;
        private readonly IPessoaService _pessoaService;

        public PessoaController(IPessoaService pessoaService, ISessao sessao)
        {
            _pessoaService = pessoaService ?? throw new ArgumentNullException(nameof(pessoaService), "O IPessoaService não pode ser nulo.");
            _sessao = sessao ?? throw new ArgumentNullException(nameof(sessao), "O ISessao não pode ser nulo.");
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var pessoas = await _pessoaService.BuscarTodasPessoasAsync();
                return Ok(pessoas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro: {ex.Message}");
            }
        }

        [HttpGet("BuscarPessoas")]
        public async Task<IActionResult> BuscarPessoas(string termo)
        {
            try
            {
                var pessoas = await _pessoaService.BuscarPorNomeCpfEmailAsync(termo);
                return Ok(pessoas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro: {ex.Message}");
            }
        }

        [HttpGet("BuscarEventosPessoa/{id}")]
        public async Task<IActionResult> BuscarEventosPessoa(int id)
        {
            try
            {
                var eventos = await _pessoaService.BuscarEventosPessoaAsync(id);
                return Ok(eventos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro: {ex.Message}");
            }
        }

        [HttpPost("ReenviarInstrucoes")]
        public async Task<IActionResult> ReenviarInstrucoes([FromBody] int idEventoPessoa)
        {
            try
            {
                bool retorno = await _pessoaService.ReenviarInstrucoesAsync(idEventoPessoa);
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro: {ex.Message}");
            }
        }

        [HttpGet("ObterIdPessoaPorCPF")]
        public async Task<IActionResult> ObterIdPessoaPorCPF(string cpf)
        {
            try
            {
                int id = await _pessoaService.ObterIdPessoaPorCPFAsync(cpf);
                return Ok(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro: {ex.Message}");
            }
        }

        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] PessoaModel pessoa)
        {
            if (id != pessoa.Id)
            {
                return BadRequest("O ID fornecido não corresponde ao ID da pessoa.");
            }

            try
            {
                await _pessoaService.AtualizarPessoaAsync(pessoa);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro: {ex.Message}");
            }
        }

        [HttpPut("AtualizarEmail")]
        public async Task<IActionResult> AtualizarEmail(int idPessoa, string email)
        {
            try
            {
                await _pessoaService.AtualizarEmailAsync(idPessoa, email);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro: {ex.Message}");
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var pessoa = await _pessoaService.BuscarPessoaPorIdAsync(id);
                if (pessoa == null)
                {
                    return NotFound("Pessoa não encontrada.");
                }

                await _pessoaService.DeletarPessoaAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro: {ex.Message}");
            }
        }

        [HttpGet("CheckSession")]
        public IActionResult CheckSession()
        {
            var user = _sessao.BuscarSessaodoUsuario();
            if (user == null)
            {
                return Unauthorized();
            }
            return Ok();
        }
    }
}