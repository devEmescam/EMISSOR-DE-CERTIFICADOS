using ClosedXML.Excel;
using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Models;
using EMISSOR_DE_CERTIFICADOS.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using DocumentFormat.OpenXml.Office2010.Excel;

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
        // POST: /Home_Organizador/NovoEvento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NovoEvento(string TextoFrenteCertificado,EventoModel evento) 
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Insere o evento no banco de dados
                    InserirEvento(TextoFrenteCertificado, evento);
                    return RedirectToAction(nameof(Index));
                }
                return View(evento);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro em Home_OrganizadorController.NovoEvento. Erro: {ex.Message}");
            }           
        }
        // POST: /Home_Organizador/LerPlanilha
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Método para ler e validar a planilha
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
                        Participantes = Convert.ToString(row["PARTICIPANTES"]),                        
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
        private void InserirEvento(string TextoFrenteCertificado,EventoModel evento)
        {
            int idEvento = -1;
            int idPessoa = -1;
            string sSQL = "";

            try
            {               
                byte[] imagemBytes = null; // Inicialize com um valor padrão

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

                // Insira o evento no banco de dados, incluindo a imagem convertida
                // Se o evento for inserido de maneira manual o texto nunca será individual para os participantes envolvidos
                 sSQL = $"INSERT INTO EVENTO (NOME, IMAGEM_CERTIFICADO) " +
                        $"VALUES ('{evento.Nome}', @ImagemCertificado);" +
                        "SELECT SCOPE_IDENTITY();"; // Obtem o ID do evento inserido
                                                    // 
                idEvento = _dbHelper.ExecuteScalar<int>(sSQL, imagemBytes);                

                if (!string.IsNullOrEmpty(evento.Participantes))
                {
                    // Verifica se o conteúdo de "evento.Participantes" tem quebras de linha
                    if (evento.Participantes.Contains("\n"))
                    {
                        // Divide o conteúdo em linhas
                        string[] linhas = evento.Participantes.Split('\n');

                        foreach (var linha in linhas)
                        {
                            // Divide os dados separados por vírgula
                            string[] dados = linha.Split(',');

                            // Cria e insere a pessoa
                            PessoaModel pessoa = new PessoaModel
                            {
                                Nome = dados[0].Trim(),
                                CPF = dados[1].Trim(),
                                Email = dados[2].Trim()
                            };

                            // Chama o método InserirPessoa do controlador PessoaController
                            using (PessoaController pessoaController = new PessoaController(_dbHelper))
                            {
                                pessoaController.InserirPessoa(pessoa);

                                //Nesse momento o evento já foi cadastrado e a pessoa também
                                //Necessário registrar em banco a relação da pessoa com o evento e seus respectivos textos
                                sSQL = "";
                                sSQL = $"INSERT INTO EVENTO_PESSOA (ID_EVENTO, ID_PESSOA, TEXTO_FRENTE) " +
                                       $"VALUES ({idEvento}, {pessoa.Id}, '{TextoFrenteCertificado}')";
                                //"SELECT SCOPE_IDENTITY();"; // Obtem o ID do evento inserido
                                _dbHelper.ExecuteQuery(sSQL);
                            }
                        }
                    }
                    else
                    {
                        // Se não houver quebras de linha, trata como se houvesse apenas um participante
                        string[] dados = evento.Participantes.Split(',');

                        // Cria e insere a pessoa
                        PessoaModel pessoa = new PessoaModel
                        {
                            Nome = dados[0].Trim(),
                            CPF = dados[1].Trim(),
                            Email = dados[2].Trim()
                        };

                        // Chama o método InserirPessoa do controlador PessoaController
                        using (PessoaController pessoaController = new PessoaController(_dbHelper))
                        {
                            pessoaController.InserirPessoa(pessoa);

                            //Nesse momento o evento já foi cadastrado e a pessoa também
                            //Necessário registrar em banco a relação da pessoa com o evento e seus respectivos textos
                            sSQL = "";
                            sSQL = $"INSERT INTO EVENTO_PESSOA (ID_EVENTO, ID_PESSOA, TEXTO_FRENTE) " +
                                   $"VALUES ({idEvento}, {pessoa.Id}, '')";                            
                            _dbHelper.ExecuteQuery(sSQL);
                        }
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
                var query = $"UPDATE EVENTO SET NOME = '{evento.Nome}', PARTICIPANTES = '{evento.Participantes}' WHERE Id = {evento.Id}";
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

                byte[] imagemBytes = _dbHelper.ExecuteQueryArrayBytes(sql,id);

                return imagemBytes; 
            }
            catch (Exception ex)
            {
                // Trate exceções adequadamente
                throw new Exception("Erro ao buscar bytes da imagem no banco de dados: " + ex.Message);
            }
        }
        #endregion
    }
}