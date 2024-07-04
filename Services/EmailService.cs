using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.Excel;
using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Repositories;
using Newtonsoft.Json;

namespace EMISSOR_DE_CERTIFICADOS.Services
{
    public class EmailService
    {
        private string _token;
        private string _evento;
        private readonly DBHelpers _dbHelper;

        public EmailService(DBHelpers dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper), "O DBHelper não pode ser nulo.");
        }
        public async Task<(bool success, string retorno)> EnviarEmailAsync(string login, string senha, int idEventoPessoa)
        {
            string remetente = string.Empty;
            string senhaEmail = string.Empty;
            byte[] assinaturaImagem = null;
            byte[] anexoImagem = null;
            string corpo = string.Empty;
            string destinatario = string.Empty;
            string[] cc = null;
            string assunto = string.Empty;
            var emailConfigRepo = new EmailConfigRepository(_dbHelper);
            string loginUsuario = string.Empty;
            string senhaUsuario = string.Empty;

            try
            {
                // Buscar as configurações do email
                var emailConfigList = await emailConfigRepo.CarregarDadosAsync();
                if (emailConfigList == null || emailConfigList.Count == 0)
                {
                    throw new Exception("Nenhuma configuração de e-mail encontrada.");
                }
                var emailConfig = emailConfigList.First();

                //Buscar destinatario
                destinatario = await ObterEmailPessoaAsync(idEventoPessoa);
                if (string.IsNullOrEmpty(destinatario))
                {
                    throw new Exception("Nenhum email de destinatário encontrado para o idEventoPessoa fornecido.");
                }

                //Buscar CC
                //TO DO: PROVAVELMENTE ISSO SERÁ TRATADO TAMBÉM PELA CONFIGURAÇÃO DE EMAIL


                // Buscar os dados do usuario
                var usuarioService = new UsuariosService(_dbHelper);
                var dadosUsuario = await usuarioService.ObterUsuarioESenhaAsync(idEventoPessoa);

                await ObterNomeEventoAsync(idEventoPessoa);
                if (string.IsNullOrEmpty(_evento))
                {
                    throw new Exception("Nenhum evento encontrado para o idEventoPessoa fornecido.");
                }

                // Buscar textoAssunto do email
                //assunto = await ObterTextoAssuntoAsync();
                assunto = $"Emissão do Certificado de Participação - {_evento}";

                // Buscar textoCorpo do email
                corpo = await ObterTextoCorpoAsync(idEventoPessoa, dadosUsuario.Usuario, dadosUsuario.Senha);

                // Buscar o certificado
                anexoImagem = await ObterCertificadoAsync(idEventoPessoa);
                if (anexoImagem == null)
                {
                    throw new Exception("Nenhum certificado encontrado para o idEventoPessoa fornecido.");
                }

                // Obter o token de autenticação
                await ObterToken(login, senha);
                if (string.IsNullOrEmpty(_token))
                {
                    throw new Exception("Falha ao obter token de autenticação.");
                }

                // Chamar método para enviar o email
                //return await EnviarEmail(emailConfig, destinatario, assunto, corpo, cc, anexoImagem, assinaturaImagem);
                var resultadoEnvio = await EnviarEmail(emailConfig, destinatario, assunto, corpo, cc, anexoImagem, assinaturaImagem);
                return resultadoEnvio;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EmailService.EnviarEmailAsync]: {ex.Message}");
            }
        }       
        private async Task ObterNomeEventoAsync(int idEventoPessoa)
        {
            string sSQL = string.Empty;

            try
            {
                sSQL = "SELECT E.NOME FROM EVENTO_PESSOA EP" +
                      " JOIN EVENTO E ON (EP.ID_EVENTO = E.ID)" +
                      " WHERE EP.ID = @IdEventoPessoa";

                var parameters = new Dictionary<string, object>
                {
                    { "@IdEventoPessoa", idEventoPessoa }
                };

                _evento = await _dbHelper.ExecuteScalarAsync<string>(sSQL, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EmailService.RetornarNomeEventoAsync]: {ex.Message}");
            }
        }
        private async Task<string> ObterTextoCorpoAsync(int idEventoPessoa, string usuario, string senha)
        {
            string nome = string.Empty;
            string textoCorpo = string.Empty;

            try
            {
                nome = await ObterNomePessoaAsync(idEventoPessoa);

                textoCorpo = $@"
                                <p>Prezado(a) {nome},</p>
                                <p>Esperamos que esta mensagem o (a) encontre bem.</p>
                                <p>Temos o prazer de informar que o certificado de participação no evento {_evento} foi emitido com sucesso. Para sua conveniência, criamos um USUÁRIO e SENHA para que você possa acessar nosso site e visualizar ou baixar o seu certificado emitido.</p>
                                <p>Para acessar o seu certificado, siga os passos abaixo:</p>
                                <ol>
                                    <li>Acesse o site através do seguinte endereço: <a href='https://emescam.certificados.edu/participante'>https://emescam.certificados.edu/participante</a></li>
                                    <li>Utilize as seguintes credenciais para fazer o login:</li>
                                    <ul style='list-style-type: none;'>
                                        <li><b>Usuário:</b> {usuario}</li>
                                        <li><b>Senha:</b> {senha}</li>
                                    </ul>
                                </ol>
                                <p>Caso encontre qualquer dificuldade ou tenha dúvidas, por favor, não hesite em entrar em contato conosco através deste e-mail ou pelo telefone.</p>
                                <p>Agradecemos pela sua participação e esperamos vê-lo (a) em nossos próximos eventos.</p>
                            ";

                return textoCorpo;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EmailService.ObterTextoCorpoAsync]: {ex.Message}");
            }
        }
        private async Task<string> ObterNomePessoaAsync(int idEventoPessoa)
        {
            string sSQL = string.Empty;
            string retorno = string.Empty;

            try
            {
                sSQL = "SELECT P.NOME FROM EVENTO_PESSOA EP" +
                     " JOIN PESSOA P ON (EP.ID_PESSOA = P.ID)" +
                     " WHERE EP.ID = @IdEventoPessoa";

                var parameters = new Dictionary<string, object>
                {
                    { "@IdEventoPessoa", idEventoPessoa }
                };

                retorno = await _dbHelper.ExecuteScalarAsync<string>(sSQL, parameters);

                if (string.IsNullOrEmpty(retorno))
                {
                    throw new Exception("Nenhum nomePessoa encontrado para o idEventoPessoa fornecido.");
                }

                return retorno;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EmailService.ObterNomePessoaAsync]: {ex.Message}");
            }
        }
        private async Task<string> ObterEmailPessoaAsync(int idEventoPessoa)
        {
            string sSQL = string.Empty;
            string retorno = string.Empty;

            try
            {

                sSQL = "SELECT P.EMAIL FROM EVENTO_PESSOA EP" +
                    " JOIN PESSOA P ON (EP.ID_PESSOA = P.ID)" +
                    " WHERE EP.ID = @IdEventoPessoa";

                var parameters = new Dictionary<string, object>
                {
                    { "@IdEventoPessoa", idEventoPessoa }
                };

                retorno = await _dbHelper.ExecuteScalarAsync<string>(sSQL, parameters);

                if (string.IsNullOrEmpty(retorno))
                {
                    throw new Exception("Nenhum nomePessoa encontrado para o idEventoPessoa fornecido.");
                }

                return retorno;

            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EmailService.ObterEmailPessoaAsync]: {ex.Message}");
            }
        }
        private async Task<byte[]> ObterCertificadoAsync(int idEventoPessoa)
        {
            try
            {
                // Comando SQL para selecionar a imagem do evento com o ID fornecido
                string sql = "SELECT IMAGEM_CERTIFICADO FROM EVENTO_PESSOA WHERE ID = @Id";

                byte[] imagemBytes = await _dbHelper.ExecuteQueryArrayBytesAsync(sql, idEventoPessoa);

                return imagemBytes;

            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EmailService.ObterCertificadoAsync]: {ex.Message}");
            }
        }
        private async Task ObterToken(string login, string senha)
        {
            try
            {
                var url = "https://apps.emescam.br/site/comunicador/api/auth/login";
                //var url = "https://localhost:7056/api/auth/login";
                var httpClient = new HttpClient();

                var payload = new
                {
                    Username = login,
                    Password = senha
                };

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<TokenResponse>(responseString);

                    // Recebe o token gerado pelo end point
                    _token = responseObject.Token;
                }
                else
                {
                    throw new Exception("Falha na autenticação: " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EmailService.ObterToken]: {ex.Message}");
            }
        }
        private async Task<(bool success, string retorno)> EnviarEmail(EmailConfigRepository emailConfig, string destinatario, string assunto, string corpo, string[] cc, byte[] anexo, byte[] assinatura)
        {
            try
            {
                var url = "https://apps.emescam.br/site/comunicador/api/email/enviar";
                //var url = "https://localhost:7056/api/email/enviar";
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                var form = new MultipartFormDataContent
                {
                    // Adicionar campos obrigatórios ao formulário
                    { new StringContent(destinatario), "Destinatario" },
                    { new StringContent(assunto), "Assunto" },
                    { new StringContent(corpo), "Corpo" },
                    { new StringContent(emailConfig.Email), "Remetente" },
                    { new StringContent(emailConfig.Senha), "Senha" },
                    { new StringContent(emailConfig.ServidorSMTP), "ServidorSMTP" },
                    { new StringContent(emailConfig.Porta), "Porta" },
                    { new StringContent(emailConfig.SSL), "SSL" }
                };


                // Adicionar campos opcionais
                if (cc != null && cc.Length > 0)
                {
                    foreach (var ccAddress in cc)
                    {
                        form.Add(new StringContent(ccAddress), "Cc");
                    }
                }

                if (anexo != null && anexo.Length > 0)
                {
                    var anexoContent = new ByteArrayContent(anexo);
                    anexoContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    form.Add(anexoContent, "Anexo", "certificado.jpg");
                }

                if (assinatura != null && assinatura.Length > 0)
                {
                    var assinaturaContent = new ByteArrayContent(assinatura);
                    assinaturaContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    form.Add(assinaturaContent, "Assinatura", "assinatura.jpg");
                }

                // Enviar a solicitação
                var response = await httpClient.PostAsync(url, form);

                // Processar a resposta
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }
                else
                {
                    var retorno = await response.Content.ReadAsStringAsync();
                    return (false, retorno);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EmailService.EnviarEmail]: {ex.Message}");
            }

        }
        private class TokenResponse
        {
            [JsonProperty("token")]
            public string Token { get; set; }
        }
    }
}
