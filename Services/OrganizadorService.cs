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
        private readonly IPessoaRepository _pessoaRepository;        
        private readonly ISessao _sessao;
        private readonly IEventoPessoasRepository _eventoPessoasRepository;
        private readonly IUsuarioService _usuarioService;
        private readonly IEmailService _emailService;
        private readonly ICertificadosService _certificadosService;
        public OrganizadorService(IOrganizadorRepository organizadorRepository, IPessoaRepository pessoaRepository, ISessao sessao, IEventoPessoasRepository eventoPessoasRepository, 
                                  IUsuarioService usuarioService, IEmailService emailService, ICertificadosService certificadosService)
        {
            _organizadorRepository = organizadorRepository;
            _pessoaRepository = pessoaRepository;
            _eventoPessoasRepository = eventoPessoasRepository;
            _sessao = sessao;
            _usuarioService = usuarioService;
            _emailService = emailService;
            _certificadosService = certificadosService;
        }
        public async Task<IEnumerable<EventoModel>> BuscarTodosEventosAsync()
        {
            try
            {                
                int userId = _sessao.ObterUsuarioId();
                if (userId == null || userId == 0)
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
            int idUsuario = 0;

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
                
                idUsuario = _sessao.ObterUsuarioId();
                if (idUsuario == null || idUsuario == 0)
                {
                    throw new Exception("ID do usuário não encontrado na sessão.");
                }

                idEvento = await _organizadorRepository.InserirEventoAsync(evento.Nome, imagemBytes, idUsuario);

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

                    await _pessoaRepository.InserirPessoaAsync(pessoa, idUsuario);
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
            int idUsuario = 0;

            try
            {                
                idUsuario = _sessao.ObterUsuarioId();
                if (idUsuario == null || idUsuario == 0)
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

                    if (!await _pessoaRepository.ExistePessoaComCPFAsync(pessoa.CPF, idUsuario))
                    {
                        await _pessoaRepository.InserirPessoaAsync(pessoa, idUsuario);
                        await _organizadorRepository.InserirEventoPessoaAsync(id, pessoa.Id, registro.Texto.Trim());
                    }
                    else
                    {
                        pessoa.Id = await _pessoaRepository.ObterIdPessoaPorCPFAsync(pessoa.CPF);
                        await _pessoaRepository.AtualizarPessoaAsync(pessoa);
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
                throw new Exception("Erro em [OrganizadorService.ObterEventoPessoas]: " + ex.Message);
            }
        }
        public async Task EmitirCertificadoAsync(EventoModel evento, List<int> listaIdPessoas)
        {            
            string loginUsuarioADM = string.Empty;
            string senhaUsuarioADM = string.Empty;
            string idPessoas = string.Empty;

            try
            {
                loginUsuarioADM = _sessao.ObterUsuarioLogin();
                senhaUsuarioADM = _sessao.ObterUsuarioPassword();

                idPessoas = string.Join(", ", listaIdPessoas);

                if (string.IsNullOrEmpty(idPessoas))
                {
                    throw new Exception("Nenhuma pessoa selecionada para envio de certificado.");
                }

                var oDT = await _organizadorRepository.BuscarPessoasEventoAsync(evento.Id, idPessoas);

                if (oDT != null && oDT.Rows.Count > 0)
                {
                    foreach (DataRow row in oDT.Rows)
                    {
                        int idEventoPessoa = Convert.ToInt32(row["ID"]);
                        int idPessoa = Convert.ToInt32(row["ID_PESSOA"]);
                        string texto = Convert.ToString(row["TEXTO_FRENTE"]);

                        if (!string.IsNullOrEmpty(texto) && evento.ImagemCertificado != null)
                        {
                            if (await _certificadosService.GerarCertificadoAsync(idEventoPessoa, idPessoa, texto, evento.ImagemCertificado))
                            {
                                int idUsuario = await _usuarioService.GerarUsuarioAsync(idPessoa);
                                if (idUsuario > 0)
                                {
                                    var (success, retorno) = await _emailService.EnviarEmailAsync(loginUsuarioADM, senhaUsuarioADM, idEventoPessoa);

                                    if (success)
                                    {
                                        await _organizadorRepository.AtualizarCertificadoEmitidoAsync(idEventoPessoa, true, "");
                                    }
                                    else
                                    {
                                        if (retorno.Length > 500)
                                        {
                                            retorno = retorno.Substring(0, 500);
                                        }

                                        await _organizadorRepository.AtualizarCertificadoEmitidoAsync(idEventoPessoa, false, retorno);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro em [Home_OrganizadorController.EmitirCertificadoAsync]: " + ex.Message);
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