using ClosedXML.Excel;
using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Models;
using EMISSOR_DE_CERTIFICADOS.Helpers;
using EMISSOR_DE_CERTIFICADOS.Services;
using EMISSOR_DE_CERTIFICADOS.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Newtonsoft.Json;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    //CONTROLLER QUE TRATA OS EVENTOS
    public class Home_OrganizadorController : Controller
    {
        private readonly DBHelpers _dbHelper;
        private readonly ISessao _sessao;
        
        public Home_OrganizadorController(DBHelpers dbHelper, ISessao sessao)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper), "O DBHelpers não pode ser nulo.");
            _sessao = sessao ?? throw new ArgumentNullException(nameof(sessao), "O ISessao não pode ser nulo.");
        }

        #region *** IActionResults ***        
        [HttpGet] // GET: EVENTOS
        public async Task<IActionResult> Index()
        {
            try
            {
                // Recupera todos os eventos do banco de dados
                var eventos = await BuscarTodosEventosAsync();

                // Passa o login para a view através do modelo
                ViewBag.Login = HttpContext.Session.GetString("Login");

                return View(eventos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em [Home_OrganizadorController.Index] Erro: {ex.Message}");
            }
        }        
        [HttpPost] // POST: /Home_Organizador/NovoEvento
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NovoEvento(string nomeEvento, IFormFile arteCertificadoFile, string tableData)
        {
            EventoModel evento = new EventoModel();

            try
            {
                evento = new EventoModel
                {
                    Nome = nomeEvento,
                    ImagemCertificado = arteCertificadoFile
                };

                if (!string.IsNullOrEmpty(tableData))
                {
                    var tabelaDataList = JsonConvert.DeserializeObject<List<TabelaData>>(tableData);

                    // Registra o evento no banco de dados
                    await InserirEventoAsync(evento, tabelaDataList);
                }
                else
                {
                    throw new Exception("Não foi possível identificar os registros de participantes.");
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em [Home_OrganizadorController.NovoEvento]. Erro: {ex.Message}");
            }
        }        
        [HttpGet] //GET: /Home_Organizador/ObterPessoasEvento: Usado para carregar dados no card que adicionará novas pessoas ao evento registrado em banco de dados
        public async Task<IActionResult> ObterPessoasEvento(int id) 
        {
            try
            {
                if (id <= 0) 
                {
                    return StatusCode(500, new { success = false, message = "Não foi possível identificar o evento." });
                }
                var eventoPessoas = await ObterEventoPessoas(id, false);
                return Json(eventoPessoas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em [Home_OrganizadorController.ObterPessoasEvento]");
            }
        }        
        [HttpPost] //POST: /Home_Organizador/AdicionarPessoas: adiciona novas pessoas ao evento registrado em banco de dados
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarPessoasEvento(int id, string tableData)
        {
            try
            {
                if (id <= 0)
                {
                    throw new Exception("Não foi possível identificar o evento.");
                }

                if (string.IsNullOrEmpty(tableData)) 
                {
                    throw new Exception("Não foi possível identificar os registros de participantes.");
                }               

                var tabelaDataList = JsonConvert.DeserializeObject<List<TabelaData>>(tableData);
                // Adiciona novas pessoas no evento de id informado
                await AtualizarPessoasEventoAsync(id, tabelaDataList);            
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em [Home_OrganizadorController.AdicionarPessoas]. Erro: {ex.Message}");
            }
        }         
        [HttpPost] // POST: /Home_Organizador/LerPlanilha
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LerPlanilha(string caminhoArquivo, string evento)
        {
            List<PessoaModel> pessoas = new List<PessoaModel>();

            // Verificar se o arquivo existe
            if (!System.IO.File.Exists(caminhoArquivo))
            {
                return StatusCode(500, "Arquivo não encontrado.");
            }

            try
            {
                // Abrir o arquivo xlsx
                using (var workbook = await Task.Run(() => new XLWorkbook(caminhoArquivo)))
                {
                    var worksheet = workbook.Worksheet(1); // Selecionar a primeira planilha

                    // Verificar o modelo da planilha (Modelo 1: Pessoas_Outros_Tipos)
                    if (worksheet.Cell("E1").Value.ToString().Equals("TIPO"))
                    {
                        // Modelo 1: Pessoas_Outros_Tipos
                        for (int row = 2; row <= worksheet.LastRowUsed().RowNumber(); row++)
                        {
                            var cpf = worksheet.Cell(row, 1).Value.ToString();
                            var nome = worksheet.Cell(row, 2).Value.ToString();
                            var email = worksheet.Cell(row, 3).Value.ToString();
                            var tipo = worksheet.Cell(row, 4).Value.ToString();
                            var texto = worksheet.Cell(row, 5).Value.ToString();

                            // Adicionar a pessoa à lista
                            pessoas.Add(new PessoaModel { CPF = cpf, Nome = nome, Email = email });
                        }
                    }
                    // Verificar o modelo da planilha (Modelo 2: Pessoas_Participantes_Texto_Unico)
                    else if (worksheet.Cell("D1").Value.ToString().Equals("TEXTO"))
                    {
                        // Modelo 2: Pessoas_Participantes_Texto_Unico
                        for (int row = 2; row <= worksheet.LastRowUsed().RowNumber(); row++)
                        {
                            var cpf = worksheet.Cell(row, 1).Value.ToString();
                            var nome = worksheet.Cell(row, 2).Value.ToString();
                            var email = worksheet.Cell(row, 3).Value.ToString();
                            var texto = worksheet.Cell(row, 4).Value.ToString();

                            // Adicionar a pessoa à lista
                            pessoas.Add(new PessoaModel { CPF = cpf, Nome = nome, Email = email });
                        }
                    }
                    else
                    {
                        return StatusCode(500, $"Modelo de planilha inválido.");
                    }
                }

                // Faça o que for necessário com essa lista
                return Ok(pessoas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro em [Home_OrganizadorController.LerPlanilha]. Erro: {ex.Message}");
            }
        }
        public async Task<IActionResult> VisualizarImagem(int id)
        {
            try
            {
                byte[] imagemBytes = await BuscarBytesDaImagemNoBDAsync(id);

                // Retorna a imagem como um arquivo para o navegador
                return File(imagemBytes, "image/jpeg");
            }
            catch (Exception ex)
            {
                // Se ocorrer algum erro, retorna um status 500 (Internal Server Error)                
                return StatusCode(500, $"Erro em [Home_OrganizadorController.VisualizarImagem]. Erro: {ex.Message}");
            }
        }
        //Home_Organizador/DetalhesEventoPessoas: Chamado pela ação de tela referente a emissão dos certificados das pessoas do evento        
        public async Task<IActionResult> DetalhesEventoPessoas(int idEvento)
        {
            try
            {
                // Obter os detalhes do evento e das pessoas
                var evento = await ObterEventoPessoas(idEvento, true);

                // Validação do retorno
                if (evento == null)
                {
                    return NotFound(new { message = "Evento não encontrado." });
                }

                // Retornar os detalhes como JSON
                return Json(evento);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Ocorreu um erro em [Home_OrganizadorController.DetalhesEventoPessoas]. Erro: {ex.Message}" });
            }
        }        
        [HttpPost] // POST:/Home_Organizador/EmitirCertificado
        [ValidateAntiForgeryToken]
        //Rotina de Emissão de certificados                
        public async Task<IActionResult> EmitirCertificado(int id, List<int> idPessoas)
        {
            try
            {
                if (idPessoas.Count == 0)
                {
                    return StatusCode(500, new { success = false, message = "Nenhuma pessoa selecionada para emissão de certificado." });
                }

                EventoModel evento = await BuscarEventoPorIdAsync(id);

                if (ModelState.IsValid)
                {
                    await EmitirCertificadoAsync(evento, idPessoas);
                }
                else
                {
                    return NotFound(new { success = false, message = "Evento não encontrado." });
                }

                // Obter um objeto atualizado com os dados do processo de emissão que foi realizado
                var eventoPessoas = await ObterEventoPessoas(id, true);

                // Return JSON objeto com status de sucesso
                return Json(new { success = true, data = eventoPessoas });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Ocorreu um erro em Home_OrganizadorController.EmitirCertificado. Erro: {ex.Message}" });
            }
        }
        public async Task<IActionResult> ObterEmailConfig() 
        {
            try
            {
                var emailConfig = await ObterEmailConfigAsync();
                return View(emailConfig);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Ocorreu um erro em Home_OrganizadorController.ObterEmailConfig. Erro: {ex.Message}" });
            }        
        }                
        [HttpPost] // POST:/Home_Organizador/Logout
        public IActionResult Logout()
        {
            try
            {
                // Limpe os dados da sessão para desconectar o usuário
                HttpContext.Session.Clear();
                _sessao.RemoverSessaoUsuario();
                //return Ok(); // Precisa direcionar para esse local: https://certificados.emescam.br/organizador/login
                // Redirecionar para a página de login do organizador
                //return RedirectToAction("Index", "Login_organizador");
                return View("~/Views/Login_Organizador/Login_organizador.cshtml");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao encerrar a sessão: {ex.Message}");
            }
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

        #region *** METODOS PRIVADOS ***        
        // VERSÃO ASYNC: Método para retornar todos os eventos do banco de dados
        private async Task<IEnumerable<EventoModel>> BuscarTodosEventosAsync()
        {
            try
            {
                // Recupera o Id do usuário da sessão
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

                // Se o Id do usuário for zero, significa que não está logado ou o Id não foi encontrado
                if (userId == 0)
                {
                    throw new Exception($"Falha ao identificar usuário logado.");
                }

                var query = $"SELECT ID, NOME, IMAGEM_CERTIFICADO FROM EVENTO WHERE ID_USUARIO_ADMINISTRATIVO = {userId}";
                var dataTable = await _dbHelper.ExecuteQueryAsync(query);
                var eventos = new List<EventoModel>();

                foreach (DataRow row in dataTable.Rows)
                {
                    // Converte a string base64 para um array de bytes                    
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
                throw new Exception($"Ocorreu um erro em [Home_OrganizadorController.BuscarTodosEventosAsync] Erro: {ex.Message}");
            }
        }
        // VERSÃO ASYNC: Método assíncrono para buscar evento no banco de dados.
        private async Task<EventoModel> BuscarEventoPorIdAsync(int id)
        {
            try
            {
                var sSQL = $"SELECT * FROM EVENTO WHERE ID = {id}";
                var oDT = await _dbHelper.ExecuteQueryAsync(sSQL);

                if (oDT.Rows.Count > 0)
                {
                    var row = oDT.Rows[0];

                    // Recupera os bytes diretamente do banco de dados
                    byte[] imagemBytes = (byte[])row["IMAGEM_CERTIFICADO"];

                    // Cria um objeto IFormFile a partir do array de bytes
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
                throw new Exception($"Ocorreu um erro em [Home_OrganizadorController.BuscarEventoPorIdAsync] Erro: {ex.Message}");
            }
        }
        // VERSÃO ASYNC: Método para inserir evento no banco de dados.
        private async Task InserirEventoAsync(EventoModel evento, List<TabelaData>? dadosTabela)
        {
            int? idEvento;
            int? idUsuario;
            string sSQL = string.Empty;

            try
            {
                // Inicializa com um valor padrão
                byte[] imagemBytes = null;

                // Verifica se o arquivo de imagem foi fornecido
                if (evento.ImagemCertificado != null && evento.ImagemCertificado.Length > 0)
                {
                    // Converte o arquivo de imagem para um array de bytes                    
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

                // Recupera o ID do usuário logado 
                idUsuario = HttpContext.Session.GetInt32("UserId");

                if (idUsuario == null)
                {
                    throw new Exception("ID do usuário não encontrado na sessão.");
                }

                // Insere o evento no banco de dados, incluindo a imagem convertida
                sSQL = "INSERT INTO EVENTO (NOME, IMAGEM_CERTIFICADO, ID_USUARIO_ADMINISTRATIVO, DATA_CADASTRO) " +
                       "VALUES (@Nome, @ImagemCertificado, @IdUsuarioAdministrativo, GETDATE()); " +
                       "SELECT SCOPE_IDENTITY();"; // Obtem o ID do evento inserido

                var parameters = new Dictionary<string, object>
                {
                    { "@Nome", evento.Nome },
                    { "@ImagemCertificado", imagemBytes },
                    { "@IdUsuarioAdministrativo", idUsuario.Value }
                };

                idEvento = await _dbHelper.ExecuteScalarAsync<int>(sSQL, parameters);

                if (idEvento == null)
                {
                    throw new Exception("ID do evento não foi definido.");
                }

                // Processar os dados da tabela
                foreach (var registro in dadosTabela)
                {
                    // Criar objeto e inserir a pessoa
                    PessoaModel pessoa = new PessoaModel
                    {
                        Nome = registro.Nome.Trim(),
                        CPF = registro.CPF.Trim(),
                        Email = registro.Email.Trim()
                    };

                    // Chama o método InserirPessoa do controlador PessoaController
                    using (PessoaController pessoaController = new PessoaController(_dbHelper))
                    {
                        await pessoaController.InserirPessoaAsync(pessoa, idUsuario);

                        // Nesse momento o evento já foi cadastrado e a pessoa também
                        // Necessário registrar em banco a relação da pessoa com o evento e seus respectivos textos
                        sSQL = "";
                        sSQL = "INSERT INTO EVENTO_PESSOA (ID_EVENTO, ID_PESSOA, TEXTO_FRENTE) " +
                               "VALUES (@idEvento, @idPessoa, @texto)";

                        var parametersEP = new Dictionary<string, object>
                        {
                            {"@idEvento",idEvento },
                            {"@idPessoa", pessoa.Id},
                            {"@texto", registro.Texto.Trim()}
                        };

                        await _dbHelper.ExecuteQueryAsync(sSQL, parametersEP);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [Home_OrganizadorController.InserirEventoAsync]. Erro: {ex.Message}");
            }
        }
        // VERSÃO ASYNC: Método assíncrono para atualizar um evento no banco de dados
        private async Task AtualizarPessoasEventoAsync(int id, List<TabelaData>? dadosTabela)
        {
            int? idUsuario;
            string sSQL = string.Empty;

            try
            {
                // Recupera o ID do usuário logado 
                idUsuario = HttpContext.Session.GetInt32("UserId");

                if (idUsuario == null)
                {
                    throw new Exception("ID do usuário não encontrado na sessão.");
                }

                // Processar os dados da tabela
                foreach (var registro in dadosTabela)
                {
                    // Criar um objeto da pessoa
                    PessoaModel pessoa = new PessoaModel
                    {
                        Nome = registro.Nome.Trim(),
                        CPF = registro.CPF.Trim(),
                        Email = registro.Email.Trim()
                    };

                    //Cria uma instancia de pessoaController para chamar os metodos contidos nessa classe
                    using (PessoaController pessoaController = new PessoaController(_dbHelper))
                    {
                        //Necessário identificar se as pessoas percorridas em dadosTabela já existem no banco de dados para decidir sobre INSERIR ou ATUALIZAR                        
                        if (!await pessoaController.ExistePessoaComCPFAsync(pessoa.CPF, idUsuario))
                        {
                            // Chama o método InserirPessoa do controlador PessoaController
                            await pessoaController.InserirPessoaAsync(pessoa, idUsuario);

                            // Necessário registrar em banco a relação da pessoa com o evento e seus respectivos textos
                            sSQL = "";
                            sSQL = "INSERT INTO EVENTO_PESSOA (ID_EVENTO, ID_PESSOA, TEXTO_FRENTE) " +
                                   "VALUES (@idEvento, @idPessoa, @texto)";
                            var parametersEP = new Dictionary<string, object>
                            {
                                {"@idEvento",id },
                                {"@idPessoa", pessoa.Id},
                                {"@texto", registro.Texto.Trim()}
                            };

                            await _dbHelper.ExecuteQueryAsync(sSQL, parametersEP);
                        }
                        else 
                        {
                            //Nesse caso a pessoa existe e terá suas informações atualizadas
                            //Decido atualizar todas que chegarem nesse fluxo para garantir que algum dado modificado em tela seja armazenado em banco de dados

                            //Busco o idPessoa para poder atualizar os registros corretos nas tabelas envolvidas
                            pessoa.Id = await pessoaController.ObterIdPessoaPorCPFAsync(pessoa.CPF);

                            //Atualizo os dados da pessoa
                            await pessoaController.AtualizarPessoaAsync(pessoa);

                            //Necessário atualizar o texto do certificado da pessoa que foi atualizada
                            sSQL = "";
                            sSQL = $"UPDATE EVENTO_PESSOA SET TEXTO_FRENTE = '{registro.Texto}' WHERE ID_EVENTO = {id} AND ID_PESSOA = {pessoa.Id}";
                            await _dbHelper.ExecuteQueryAsync(sSQL);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [Home_OrganizadorController.AtualizarPessoasEventoAsync] Erro: {ex.Message}");
            }
        }
        // VERSÃO ASYNC: Método que retorna os bytes da imagem
        private async Task<byte[]> BuscarBytesDaImagemNoBDAsync(int id)
        {
            try
            {
                // Comando SQL para selecionar a imagem do evento com o ID fornecido
                string sql = "SELECT IMAGEM_CERTIFICADO FROM EVENTO WHERE ID = @Id";

                byte[] imagemBytes = await _dbHelper.ExecuteQueryArrayBytesAsync(sql, id);

                return imagemBytes;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro em [Home_OrganizadorController.BuscarBytesDaImagemNoBDAsync]: " + ex.Message);
            }
        }
        private async Task<Evento> ObterEventoPessoas(int idEvento, bool emitirCertificado)
        {
            var eventoRepository = new EventoPessoasRepository(_dbHelper);
            try
            {
                // Validação do idEvento
                if (idEvento <= 0)
                {
                    throw new ArgumentException("ID do evento inválido.");
                }

                // Carregar os dados do evento e das pessoas do evento
                var evento = await eventoRepository.CarregarDadosAsync(idEvento, emitirCertificado);

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
        // VERSÃO ASYNC: Metodo que gera certificado, cria usuario e emite email a pessoa do evento        
        private async Task EmitirCertificadoAsync(EventoModel evento, List<int> listaIdPessoas)
        {
            DataTable oDT = new DataTable();
            var usuariosService = new UsuariosService(_dbHelper);
            var certificadoService = new CertificadosService(_dbHelper);
            var emailService = new EmailService(_dbHelper);
            string sSQL = "";
            int idUsuario = -1;
            string loginUsuarioADM = string.Empty;
            string senhaUsuarioADM = string.Empty;
            string idPessoas = string.Empty;

            try
            {
                loginUsuarioADM = HttpContext.Session.GetString("Login");
                senhaUsuarioADM = HttpContext.Session.GetString("Senha");

                idPessoas = string.Join(", ", listaIdPessoas);

                if (!string.IsNullOrEmpty(idPessoas))
                {
                    // Buscar as pessoas do evento com base nos IDs fornecidos
                    sSQL = $"SELECT * FROM EVENTO_PESSOA WHERE (CERTIFICADO_EMITIDO IS NULL OR CERTIFICADO_EMITIDO = 0) " +
                           $"AND ID_EVENTO = {evento.Id} AND ID_PESSOA IN ({idPessoas})";
                }
                else
                {
                    throw new Exception("Nenhuma pessoa selecionada para envio de certificado.");
                }

                oDT = await _dbHelper.ExecuteQueryAsync(sSQL);

                if (oDT != null && oDT.Rows.Count > 0)
                {
                    //Percorrer os registros/pessoas do evento
                    foreach (DataRow row in oDT.Rows)
                    {
                        int idEventoPessoa = Convert.ToInt32(row["ID"]);
                        int idPessoa = Convert.ToInt32(row["ID_PESSOA"]);
                        string texto = Convert.ToString(row["TEXTO_FRENTE"]);

                        //Só manda gerar se houver texto e imagem
                        if (!string.IsNullOrEmpty(texto) && evento.ImagemCertificado != null)
                        {
                            //Se gerar certificado
                            if (await certificadoService.GerarCertificadoAsync(idEventoPessoa, idPessoa, texto, evento.ImagemCertificado))
                            {
                                //Criar usuario e senha da pessoa
                                idUsuario = await usuariosService.GerarUsuarioAsync(idPessoa);
                                if (idUsuario > 0)
                                {
                                    //Enviar email com os dados do usuario e certificado anexo
                                    var (success, retorno) = await emailService.EnviarEmailAsync(loginUsuarioADM, senhaUsuarioADM, idEventoPessoa);

                                    // Validar se o email foi de fato enviado para poder atualizar campo CERTIFICADO_EMITIDO da tabela EVENTO_PESSOA
                                    if (success)
                                    {
                                        // Atualizar EVENTO_PESSOA registrando resultado do processo e data 
                                        sSQL = $"UPDATE EVENTO_PESSOA SET CERTIFICADO_EMITIDO = 1, DATA_EMISSAO = GETDATE()  WHERE ID = {idEventoPessoa}";
                                        await _dbHelper.ExecuteQueryAsync(sSQL);
                                    }
                                    else
                                    {
                                        // Truncar a mensagem de retorno se ela for maior que 500 caracteres
                                        if (retorno.Length > 500)
                                        {
                                            retorno = retorno.Substring(0, 500);
                                        }

                                        // Atualizar EVENTO_PESSOA registrando resultado do processo e data e mensagem retornada 
                                        sSQL = $"UPDATE EVENTO_PESSOA SET CERTIFICADO_EMITIDO = 0, DATA_EMISSAO = '', MENSAGEM_RETORNO_EMAIL = '{retorno}' WHERE ID = {idEventoPessoa}";
                                        await _dbHelper.ExecuteQueryAsync(sSQL);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro em [Home_OrganizadorController.GerarCertifcado]: " + ex.Message);
            }
        }
        private async Task<EmailConfigModel> ObterEmailConfigAsync() 
        {
            var emailConfigRepo = new EmailConfigRepository(_dbHelper);
            var emailConfig = new EmailConfigModel();

            try
            {
                emailConfig = await emailConfigRepo.CarregarDadosAsync();
                return emailConfig;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro em [Home_OrganizadorController.ObterEmailConfigAsync]: " + ex.Message);
            }
        }
        #endregion
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