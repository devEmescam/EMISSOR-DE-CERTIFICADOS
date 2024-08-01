using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Helpers;
using EMISSOR_DE_CERTIFICADOS.Interfaces;
using EMISSOR_DE_CERTIFICADOS.Models;
using EMISSOR_DE_CERTIFICADOS.Repositories;
using System.Data;

namespace EMISSOR_DE_CERTIFICADOS.Services
{
    internal class OrganizadorService : IOrganizadorService
    {
        private readonly IOrganizadorRepository _organizadorRepository;
        private readonly IPessoaService _pessoaService;        
        private readonly ISessao _sessao;
        private readonly IEventoPessoasRepository _eventoPessoasRepository;

        public OrganizadorService(IOrganizadorRepository organizadorRepository, IPessoaService pessoaService,ISessao sessao, IEventoPessoasRepository eventoPessoasRepository)
        {
            _organizadorRepository = organizadorRepository;
            _pessoaService = pessoaService;
            _eventoPessoasRepository = eventoPessoasRepository;
            _sessao = sessao;
        }

        public async Task<IEnumerable<EventoModel>> BuscarTodosEventosAsync()
        {
            try
            {
                //int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                int userId = _sessao.ObterUsuarioId();
                if (userId == 0)
                {
                    throw new Exception("Falha ao identificar usuário logado.");
                }

                var dataTable = await _organizadorRepository.BuscarTodosEventosAsync(userId);
                var eventos = new List<EventoModel>();

                foreach (DataRow row in dataTable.Rows)
                {
                    byte[] imagemBytes = row["IMAGEM_CERTIFICADO"] as byte[];
                    eventos.Add(new EventoModel
                    {
                        Id = Convert.ToInt32(row["ID"]),
                        Nome = Convert.ToString(row["NOME"]),
                        ImagemCertificado = Util.ConvertToFormFile(imagemBytes)
                    });
                }

                return eventos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [OrganizadorService.BuscarTodosEventosAsync]: {ex.Message}");
            }
        }

        public async Task<EventoModel> BuscarEventoPorIdAsync(int id)
        {
            try
            {
                var oDT = await _organizadorRepository.BuscarEventoPorIdAsync(id);

                if (oDT.Rows.Count > 0)
                {
                    var row = oDT.Rows[0];
                    byte[] imagemBytes = (byte[])row["IMAGEM_CERTIFICADO"];
                    IFormFile imagemCertificado = new FormFile(new MemoryStream(imagemBytes), 0, imagemBytes.Length, "ImagemCertificado", "imagem.jpg");

                    return new EventoModel
                    {
                        Id = Convert.ToInt32(row["ID"]),
                        Nome = Convert.ToString(row["NOME"]),
                        ImagemCertificado = imagemCertificado
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [OrganizadorService.BuscarEventoPorIdAsync]: {ex.Message}");
            }
        }

        public async Task InserirEventoAsync(EventoModel evento, List<TabelaData>? dadosTabela)
        {
            int? idEvento;
            int? idUsuario;

            try
            {
                byte[] imagemBytes = null;

                if (evento.ImagemCertificado != null && evento.ImagemCertificado.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await evento.ImagemCertificado.CopyToAsync(memoryStream);
                        imagemBytes = memoryStream.ToArray();
                    }
                }
                else
                {
                    throw new Exception("Não foi possível identificar o arquivo de imagem do certificado.");
                }

                //idUsuario = HttpContext.Session.GetInt32("UserId");
                idUsuario = _sessao.ObterUsuarioId();

                if (idUsuario == null)
                {
                    throw new Exception("ID do usuário não encontrado na sessão.");
                }

                idEvento = await _organizadorRepository.InserirEventoAsync(evento.Nome, imagemBytes, idUsuario.Value);

                if (idEvento == null)
                {
                    throw new Exception("ID do evento não foi definido.");
                }

                foreach (var registro in dadosTabela)
                {
                    PessoaModel pessoa = new PessoaModel
                    {
                        Nome = registro.Nome.Trim(),
                        CPF = registro.CPF.Trim(),
                        Email = registro.Email.Trim()
                    };

                    await _pessoaService.InserirPessoaAsync(pessoa);

                    await _organizadorRepository.InserirEventoPessoaAsync(idEvento.Value, pessoa.Id, registro.Texto.Trim());
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [OrganizadorService.InserirEventoAsync]: {ex.Message}");
            }
        }

        public async Task AtualizarPessoasEventoAsync(int id, List<TabelaData>? dadosTabela)
        {
            int? idUsuario;

            try
            {
                //idUsuario = HttpContext.Session.GetInt32("UserId");
                idUsuario = _sessao.ObterUsuarioId();
                if (idUsuario == null)
                {
                    throw new Exception("ID do usuário não encontrado na sessão.");
                }

                foreach (var registro in dadosTabela)
                {
                    PessoaModel pessoa = new PessoaModel
                    {
                        Nome = registro.Nome.Trim(),
                        CPF = registro.CPF.Trim(),
                        Email = registro.Email.Trim()
                    };

                    if (!await _pessoaService.ExistePessoaComCPFAsync(pessoa.CPF, idUsuario))
                    {
                        await _pessoaService.InserirPessoaAsync(pessoa);

                        await _organizadorRepository.InserirEventoPessoaAsync(id, pessoa.Id, registro.Texto.Trim());
                    }
                    else
                    {
                        pessoa.Id = await _pessoaService.ObterIdPessoaPorCPFAsync(pessoa.CPF);
                        await _pessoaService.AtualizarPessoaAsync(pessoa);

                        await _organizadorRepository.AtualizarTextoFrenteEventoPessoaAsync(id, pessoa.Id, registro.Texto.Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [OrganizadorService.AtualizarPessoasEventoAsync]: {ex.Message}");
            }
        }

        public async Task<byte[]> BuscarBytesDaImagemNoBDAsync(int id)
        {
            try
            {
                return await _organizadorRepository.BuscarBytesDaImagemNoBDAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [OrganizadorService.BuscarBytesDaImagemNoBDAsync]: {ex.Message}");
            }
        }

        public async Task<Evento> ObterEventoPessoas(int idEvento, bool emitirCertificado)
        {            
            try
            {
                // Validação do idEvento
                if (idEvento <= 0)
                {
                    throw new ArgumentException("ID do evento inválido.");
                }

                // Carregar os dados do evento e das pessoas do evento
                var evento = await _eventoPessoasRepository.CarregarDadosAsync(idEvento, emitirCertificado);

                // Validação do retorno
                if (evento == null)
                {
                    throw new Exception("Evento não encontrado.");
                }

                return evento;

                
            }
            catch (Exception ex)
            {
                throw new Exception("Erro em [Home_OrganizadorController.ObterEventoPessoas]: " + ex.Message);
            }
        }

        public async Task EmitirCertificadoAsync(EventoModel evento, List<int> listaIdPessoas)
        {
            try
            {
                foreach (var idPessoa in listaIdPessoas)
                {
                    await _organizadorRepository.InserirCertificadoAsync(evento.Id, idPessoa);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [OrganizadorService.EmitirCertificadoAsync]: {ex.Message}");
            }
        }

        public async Task<EmailConfigModel> ObterEmailConfigAsync()
        {
            try
            {
                var oDT = await _organizadorRepository.ObterEmailConfigAsync();
                if (oDT.Rows.Count == 0)
                {
                    throw new Exception("Configuração de e-mail não encontrada.");
                }

                var row = oDT.Rows[0];
                return new EmailConfigModel
                {
                    ServidorSMTP = Convert.ToString(row["SMTP"]),
                    Porta = Convert.ToString(row["PORTA"]),
                    Email = Convert.ToString(row["EMAIL"]),
                    Senha = Convert.ToString(row["SENHA"])
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [OrganizadorService.ObterEmailConfigAsync]: {ex.Message}");
            }
        }

    }
}
public class TabelaData
{
    public string Nome { get; set; }
    public string CPF { get; set; }
    public string Email { get; set; }
    public string TipoPessoa { get; set; }
    public string Texto { get; set; }
}