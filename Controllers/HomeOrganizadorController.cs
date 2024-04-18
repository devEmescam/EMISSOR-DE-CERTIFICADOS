using ClosedXML.Excel;
using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    //CONTROLLER QUE TRATA OS EVENTOS
    public class Home_OrganizadorController : Controller
    {

        private readonly DBHelpers _dbHelper;

        public Home_OrganizadorController(DBHelpers dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper), "O DBHelpers não pode ser nulo.");
        }

        #region *** IActionResults ***
        // GET: EVENTOS
        public IActionResult Index()
        {
            // Recupera todos as eventos do banco de dados
            var eventos  = BuscarTodosEventos();    
            return View(eventos);
        }

        // GET: /Home_Organizador/NovoEvento
        public IActionResult NovoEvento()
        {
            return View();
        }

        // POST: /Home_Organizador/NovoEvento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NovoEvento(EventoModel evento) 
        {
            if (ModelState.IsValid) 
            {
                // Insere o evento no banco de dados
                InserirEvento(evento);
                return RedirectToAction(nameof(Index));
            }
            return View(evento);
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
                return BadRequest("Arquivo não encontrado.");
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
                        return BadRequest("Modelo de planilha inválido.");
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


        //// GET: /Home_Organizador/MyEvents
        //public IActionResult MyEvents()
        //{
        //    return View();
        //}

        //// GET: /Home_Organizador/SearchPeople
        //public IActionResult SearchPeople()
        //{
        //    return View();
        //}

        //// GET: /Home_Organizador/AnythingElse
        //public IActionResult AnythingElse()
        //{
        //    return View();
        //}

        // GET: /Home_Organizador/Logout
        public IActionResult Logout()
        {
            // Aqui você pode fazer qualquer lógica necessária para encerrar a sessão do usuário
            return RedirectToAction("Index", "Login"); // Redireciona para a página de login após o logout
        }
        #endregion

        #region *** METODOS PRIVADOS ***

        // Método para retornar todos os eventos do banco de dados
        private IEnumerable<EventoModel> BuscarTodosEventos() 
        {
            try
            {
                var query = "SELECT * FROM EVENTO";
                var dataTable = _dbHelper.ExecuteQuery(query);
                var eventos = new List<EventoModel>();

                foreach (DataRow row in dataTable.Rows) 
                {
                    // Converte a string base64 para um array de bytes
                    byte[] imagemBytes = Convert.FromBase64String(Convert.ToString(row["IMAGEM_CERTIFICADO"]));

                    // Cria um objeto IFormFile a partir do array de bytes
                    IFormFile imagemCertificado = new FormFile(new MemoryStream(imagemBytes), 0, imagemBytes.Length, "ImagemCertificado", "imagem.jpg");

                    eventos.Add(new EventoModel
                    {
                        Id = Convert.ToInt32(row["ID"]),
                        Nome = Convert.ToString(row["NOME"]),
                        Participantes = Convert.ToString(row["PARTICIPANTES"]),
                        TextoIndividual = Convert.ToBoolean(row["TEXTO_INDIVIDUAL"]),
                        ImagemCertificado = imagemCertificado
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
                        TextoIndividual = Convert.ToBoolean(row["TEXTO_INDIVIDUAL"]),
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

        private void InserirEvento(EventoModel evento)
        {
            try
            {
                //var query = $"INSERT INTO EVENTO (NOME, TEXTO_INDIVIDUAL, PARTICIPANTES) VALUES ('{evento.Nome}', '{evento.TextoIndividual}', '{evento.Participantes}')";
                //_dbHelper.ExecuteQuery(query);

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
                var query = $"INSERT INTO EVENTO (NOME, TEXTO_INDIVIDUAL, PARTICIPANTES, IMAGEM_CERTIFICADO) " +
                            $"VALUES ('{evento.Nome}', '0', '{evento.Participantes}', @ImagemCertificado)";
                _dbHelper.ExecuteQuery(query, imagemBytes);

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
                var query = $"UPDATE EVENTO SET NOME = '{evento.Nome}', TEXTO_INDIVIDUAL = '{evento.TextoIndividual}', PARTICIPANTES = '{evento.Participantes}' WHERE Id = {evento.Id}";
                _dbHelper.ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [Home_OrganizadorController.AtualizarEvento] Erro: {ex.Message}");
            }
        }
        #endregion
    }
}