using EMISSOR_DE_CERTIFICADOS.Helpers;
using EMISSOR_DE_CERTIFICADOS.Interfaces;
using EMISSOR_DE_CERTIFICADOS.Models;
using EMISSOR_DE_CERTIFICADOS.Repositories;
using static EMISSOR_DE_CERTIFICADOS.Repositories.PessoaEventosRepository;

namespace EMISSOR_DE_CERTIFICADOS.Services
{
    internal class PessoaService : IPessoaService
    {
        private readonly IPessoaRepository _pessoaRepository;
        private readonly ISessao _sessao;
        private readonly IPessoaEventosRepository _pessoaEventosRepository;

        public PessoaService(IPessoaRepository pessoaRepository, ISessao sessao, IPessoaEventosRepository pessoaEventosRepository)
        {
            _pessoaRepository = pessoaRepository;
            _sessao = sessao;
            _pessoaEventosRepository = pessoaEventosRepository;
        }
        public async Task<IEnumerable<Pessoa>> BuscarPorNomeCpfEmailAsync(string termo)
        {
            try
            {
                int userId = _sessao.ObterUsuarioId();
                if (userId == 0)
                {
                    throw new Exception("Falha ao identificar usuário logado.");
                }

                return await _pessoaEventosRepository.CarregarDadosAsync(termo, userId, true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaService.BuscarPorNomeCpfEmailAsync]: {ex.Message}");
            }
        }

        public async Task<IEnumerable<EventoPessoa>> BuscarEventosPessoaAsync(int id) 
        {
            try
            {
                int userId = _sessao.ObterUsuarioId();
                if (userId == 0)
                {
                    throw new Exception("Falha ao identificar usuário logado.");
                }

                return await _pessoaEventosRepository.CarregarEventosPessoaAsync(id, userId, true);

            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaService.BuscarEventosPessoa]: {ex.Message}");
            }
        
        }
        public async Task<IEnumerable<PessoaModel>> BuscarTodasPessoasAsync()
        {
            return await _pessoaRepository.BuscarTodasPessoasAsync();
        }
        public async Task<PessoaModel> BuscarPessoaPorIdAsync(int id)
        {
            try
            {
                return await _pessoaRepository.BuscarPessoaPorIdAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaService.BuscarPessoaPorIdAsync]: {ex.Message}");
            }
            
        }
        public async Task<string> InserirPessoaAsync(PessoaModel pessoa)
        {
            int idUsuario = _sessao.ObterUsuarioId();
            try
            {
                if (!await _pessoaRepository.ExistePessoaComCPFAsync(pessoa.CPF, idUsuario))
                {
                    if (Util.ValidaCPF(pessoa.CPF))
                    {
                        if (Util.ValidaEstruturaEmail(pessoa.Email))
                        {
                            
                            await _pessoaRepository.InserirPessoaAsync(pessoa, idUsuario);
                            pessoa.Id = await _pessoaRepository.ObterIdPessoaPorCPFAsync(pessoa.CPF);
                            return "Inserido com Sucesso.";
                        }
                        else
                        {
                            throw new Exception($"O e-mail {pessoa.Email} informado de {pessoa.Nome} não é válido.");
                        }
                    }
                    else
                    {
                        throw new Exception($"O CPF {pessoa.CPF} informado de {pessoa.Nome} não é válido.");
                    }
                }
                else
                {
                    pessoa.Id = await _pessoaRepository.ObterIdPessoaPorCPFAsync(pessoa.CPF);
                    return "CPF informado já existe no banco de dados.";
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaService.InserirPessoaAsync]: {ex.Message}");
            }
        }
        public async Task AtualizarPessoaAsync(PessoaModel pessoa)
        {
            try
            {
                await _pessoaRepository.AtualizarPessoaAsync(pessoa);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaService.AtualizarPessoaAsync]: {ex.Message}");
            }            
        }
        public async Task DeletarPessoaAsync(int id)
        {
            try
            {
                await _pessoaRepository.DeletarPessoaAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaService.DeletarPessoaAsync]: {ex.Message}");
            }            
        }
        public async Task<bool> ExistePessoaComCPFAsync(string cpf, int? userId = null)
        {
            try
            {
                return await _pessoaRepository.ExistePessoaComCPFAsync(cpf, userId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaService.ExistePessoaComCPFAsync]: {ex.Message}");
            }            
        }
        public async Task<int> ObterIdPessoaPorCPFAsync(string cpf)
        {
            try
            {
                return await _pessoaRepository.ObterIdPessoaPorCPFAsync(cpf);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaService.ObterIdPessoaPorCPFAsync]: {ex.Message}");
            }            
        }
        public async Task<string> ObterCPFPorIdPessoaAsync(int id)
        {
            try
            {
                return await _pessoaRepository.ObterCPFPorIdPessoaAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaService.ObterCPFPorIdPessoaAsync]: {ex.Message}");
            }            
        }
        public async Task<string> ObterNomePorIdPessoaAsync(int id)
        {
            try
            {
                return await _pessoaRepository.ObterNomePorIdPessoaAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [PessoaService.ObterNomePorIdPessoaAsync]: {ex.Message}");
            }            
        }
    }
}