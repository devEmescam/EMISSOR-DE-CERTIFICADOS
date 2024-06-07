using ClosedXML.Excel;
using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Models;
using EMISSOR_DE_CERTIFICADOS.Helpers;
using EMISSOR_DE_CERTIFICADOS.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    //CONTROLLER QUE TRATA OS EVENTOS
    public class Home_OrganizadorController : Controller
    {
        private readonly DBHelpers _dbHelper;
        private readonly ISessao _sessao;
        public Home_OrganizadorController(DBHelpers dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper), "O DBHelpers não pode ser nulo.");
        }

        #region *** IActionResults ***
        // GET: EVENTOS
        public IActionResult Index()
        {
            try
            {
                // Recupera todos as eventos do banco de dados
                var eventos = BuscarTodosEventos();

                // Passa o login para a view através do modelo
                ViewBag.Login = HttpContext.Session.GetString("Login"); ;

                return View(eventos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em Home_OrganizadorController.Index. Erro: {ex.Message}");
            }
        }

        // GET: /Home_Organizador/NovoEvento
        public IActionResult NovoEvento()
        {
            return View();
        }       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NovoEvento(string nomeEvento, IFormFile arteCertificadoFile, string tableData)
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
                    InserirEvento(evento, tabelaDataList);
                }
                else 
                {
                    throw new Exception("Não foi possivel identificar os registros de participantes.");
                }

                //// Para este exemplo, apenas retornaremos uma mensagem de confirmação
                //ViewBag.NomeEvento = nomeEvento;
                //return View("Confirmacao");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em Home_OrganizadorController.NovoEvento. Erro: {ex.Message}");
            }           
        }

        // POST: /Home_Organizador/LerPlanilha
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Rotina para ler e validar a planilha
        public IActionResult LerPlanilha(string caminhoArquivo, string evento)
        {
            List<PessoaModel> pessoas = new List<PessoaModel>();

            // Verificar se o arquivo existe
            if (!System.IO.File.Exists(caminhoArquivo))
            {
                //return BadRequest("Arquivo não encontrado.");
                return StatusCode(500, "Arquivo não encontrado.");
            }

            try
            {
                // Abrir o arquivo xlsx
                using (var workbook = new XLWorkbook(caminhoArquivo))
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
                        //return BadRequest("Modelo de planilha inválido.");                        
                        return StatusCode(500, $"Modelo de planilha inválido.");
                    }
                }

                // Agora você tem a lista de pessoas com os dados da planilha
                // Faça o que for necessário com essa lista
                return Ok(pessoas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao ler a planilha: {ex.Message}");
            }
        }

        // Ação para visualizar a imagem do certificado
        public IActionResult VisualizarImagem(int id)
        {
            try
            {
                byte[] imagemBytes = BuscarBytesDaImagemNoBancoDeDados(id);

                // Retorna a imagem como um arquivo para o navegador
                return File(imagemBytes, "image/jpeg");
            }
            catch (Exception ex)
            {
                // Se ocorrer algum erro, retorna um status 500 (Internal Server Error)
                return StatusCode(500, $"Ocorreu um erro ao tentar visualizar a imagem: {ex.Message}");
            }
        }

        // POST:/Home_Organizador
        [HttpPost]
        [ValidateAntiForgeryToken]
        //Rotina de Emissão de certificados:
        //percorre os participantes do evento, cria usuario e senha para cada um, gera o certificado (junta texto e certificado), emite email ao participante com instruções e certificado anexo
        public IActionResult EmitirCeritificado(EventoModel evento) 
        {
            try
            {
                if(ModelState.IsValid) 
                {
                    GerarCertifcado(evento);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em Home_OrganizadorController.EmitirCeritificado. Erro: {ex.Message}");
            }
        }

        // POST:/Home_Organizador/Logout
        // No controlador para logout
        [HttpPost]
        public IActionResult Logout()
        {
            try
            {
                // Limpe os dados da sessão para desconectar o usuário
                HttpContext.Session.Clear();
                _sessao.RemoverSessaoUsuario();
                return Ok(); // Ou qualquer outro código de status apropriado
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao encerrar a sessão: {ex.Message}");
            }
        }

        #endregion

        #region *** METODOS PRIVADOS ***
        // Método para retornar todos os eventos do banco de dados
        private IEnumerable<EventoModel> BuscarTodosEventos()
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
                var dataTable = _dbHelper.ExecuteQuery(query);
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
                throw new Exception($"Ocorreu um erro em [Home_OrganizadorController.BuscarTodosEventos] Erro: {ex.Message}");
            }
        }
        // Método para inserir um novo evento no banco de dados.
        private EventoModel BuscarEventoPorId(int id)
        {
            try
            {
                var query = $"SELECT * FROM EVENTO WHERE ID = {id}";
                var dataTable = _dbHelper.ExecuteQuery(query);
                if (dataTable.Rows.Count > 0)
                {
                    var row = dataTable.Rows[0];


                    // Converte a string base64 para um array de bytes
                    byte[] imagemBytes = Convert.FromBase64String(Convert.ToString(row["IMAGEM_CERTIFICADO"]));

                    // Cria um objeto IFormFile a partir do array de bytes
                    IFormFile imagemCertificado = new FormFile(new MemoryStream(imagemBytes), 0, imagemBytes.Length, "ImagemCertificado", "imagem.jpg");

                    return new EventoModel
                    {
                        Id = Convert.ToInt32(row["ID"]),
                        Nome = Convert.ToString(row["NOME"]),
                        //Participantes = Convert.ToString(row["PARTICIPANTES"]),                        
                        ImagemCertificado = imagemCertificado
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [Home_OrganizadorController.BuscarEventoPorId] Erro: {ex.Message}");
            }
        }        
        private void InserirEvento(EventoModel evento, List<TabelaData>? dadosTabela)
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
                        evento.ImagemCertificado.CopyTo(memoryStream);
                        imagemBytes = memoryStream.ToArray();
                    }
                }
                else 
                {
                    throw new Exception("Não foi possivel identificar o arquivo de imagem do certificado.");
                }
                
                //recuperar o ID do usuario logado 
                idUsuario = HttpContext.Session.GetInt32("UserId");

                if (idUsuario == null)
                {
                    throw new Exception("ID do usuário não encontrado na sessão.");
                }

                // Insira o evento no banco de dados, incluindo a imagem convertida
                sSQL = $"INSERT INTO EVENTO (NOME, IMAGEM_CERTIFICADO, ID_USUARIO_ADMINISTRATIVO) " +
                       $"VALUES ('{evento.Nome}', @ImagemCertificado, {idUsuario.Value});" +
                       "SELECT SCOPE_IDENTITY();"; // Obtem o ID do evento inserido

                var parameters = new Dictionary<string, object>
                {
                    { "@ImagemCertificado", imagemBytes }
                };

                idEvento = _dbHelper.ExecuteScalar<int>(sSQL, parameters);

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
                        pessoaController.InserirPessoa(pessoa, idUsuario);

                        //Nesse momento o evento já foi cadastrado e a pessoa também
                        //Necessário registrar em banco a relação da pessoa com o evento e seus respectivos textos
                        sSQL = "";
                        sSQL = $"INSERT INTO EVENTO_PESSOA (ID_EVENTO, ID_PESSOA, TEXTO_FRENTE) " +
                               $"VALUES ({idEvento}, {pessoa.Id}, '{registro.Texto.Trim()}')";
                        _dbHelper.ExecuteQuery(sSQL);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [Home_OrganizadorController.InserirEvento] Erro: {ex.Message}");
            }
        }

        // Método para atualizar uma pessoa no banco de dados
        private void AtualizarEvento(EventoModel evento)
        {
            try
            {
                //var query = $"UPDATE EVENTO SET NOME = '{evento.Nome}', PARTICIPANTES = '{evento.Participantes}' WHERE Id = {evento.Id}";
                var query = $"UPDATE EVENTO SET NOME = '{evento.Nome}' WHERE Id = {evento.Id}";
                _dbHelper.ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [Home_OrganizadorController.AtualizarEvento] Erro: {ex.Message}");
            }
        }
        private byte[] BuscarBytesDaImagemNoBancoDeDados(int id)
        {
            try
            {
                // Comando SQL para selecionar a imagem do evento com o ID fornecido
                string sql = "SELECT IMAGEM_CERTIFICADO FROM EVENTO WHERE ID = @Id";

                byte[] imagemBytes = _dbHelper.ExecuteQueryArrayBytes(sql, id);

                return imagemBytes;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro em [Home_OrganizadorController.BuscarBytesDaImagemNoBancoDeDados]: " + ex.Message);
            }
        }
        private void GerarCertifcado(EventoModel evento) 
        {
            string sSQL = "";
            DataTable oDT = new DataTable();
            var usuariosService = new UsuariosService(_dbHelper);
            int idUsuario = -1;

            try
            {                
                //Buscar as pessoas do evento
                sSQL = $"SELECT * FROM EVENTO_PESSOA WHERE ID_EVENTO = {evento.Id}";
                oDT = _dbHelper.ExecuteQuery(sSQL);

                if (oDT != null && oDT.Rows.Count > 0) 
                {
                    //Percorre-las
                    foreach (DataRow row in oDT.Rows)
                    {                                                
                        int idPessoa = Convert.ToInt32(row["ID"]);
                        string texto = Convert.ToString(row["TEXTO_FRENTE"]);
                        
                        //Só manda gerar se houver texto e imagem
                        if (!string.IsNullOrEmpty(texto) &&  evento.ImagemCertificado != null) 
                        {
                            //Se gerar certificado
                            if (CertificadosService.GerarCertificado_SEM_NEGRITO(idPessoa, texto, evento.ImagemCertificado)) 
                            {
                                //Criar usuario e senha da pessoa
                                idUsuario = usuariosService.GerarUsuario(idPessoa);
                                if (idUsuario > 0)
                                {
                                    //5- por ultimo enviar email com os dados do usuario e certificado anexo

                                    
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