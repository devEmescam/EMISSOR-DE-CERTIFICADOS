using System;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
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
                // Buscar as informações do email
                var emailConfigList = await emailConfigRepo.CarregarDadosAsync();

                if (emailConfigList == null || emailConfigList.Count == 0) 
                {
                    throw new Exception("Nenhuma configuração de e-mail encontrada.");
                }
                
                var emailConfig = emailConfigList.First();               

                // Buscar os dados do usuario
                var usuarioService = new UsuariosService(_dbHelper);
                var dadosUsuario = await usuarioService.ObterUsuarioESenhaAsync(idEventoPessoa);

                await ObterNomeEventoAsync(idEventoPessoa);
                if (string.IsNullOrEmpty(_evento))
                {
                    throw new Exception("Nenhum evento encontrado para o idEventoPessoa fornecido.");
                }

                // Buscar textoAssunto do email
                assunto = await BuscarTextoAssuntoAsync();
                // Buscar textoCorpo do email

                // Buscar o certificado
                corpo = await BuscarTextoCorpoAsync(idEventoPessoa, dadosUsuario.Usuario, dadosUsuario.Senha);

                // Obter o token de autenticação
                await ObterToken(login, senha);
                if (string.IsNullOrEmpty(_token))
                {
                    throw new Exception("Falha ao obter token de autenticação.");
                }

                // Chamar método independente para enviar o email
                return await EnviarEmail(emailConfig, destinatario, assunto, corpo, cc, anexoImagem, assinaturaImagem);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EmailService.EnviarEmailAsync]: {ex.Message}");
            }
        }

        private async Task<string> BuscarTextoAssuntoAsync()
        {
            string retorno = string.Empty;  
            string evento  = string.Empty;
            string sSQL = string.Empty; 

            try
            {              

                return $"Emissão do Certificado de Participação - {_evento}";                 
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EmailService.BuscarTextoAssuntoAsync]: {ex.Message}");
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

        private async Task<string> BuscarTextoCorpoAsync(int idEventoPessoa, string usuario, string senha)
        {            
            string nome = string.Empty;
            string sSQL = string.Empty;
            string textoCorpo = string.Empty;

            try
            {
               nome = await ObterNomePessoaAsync(idEventoPessoa);

                textoCorpo = $"Prezado(a) {nome}, " + Environment.NewLine + Environment.NewLine +
                             $" Esperamos que esta mensagem o (a) encontre bem." + Environment.NewLine + Environment.NewLine +
                             $" Temos o prazer de informar que o certificado de participação no evento {_evento} foi " + Environment.NewLine +
                             $" emitido com sucesso. Para sua conveniência, criamos um USUÁRIO e SENHA para que você possa" + Environment.NewLine +
                             $" acessar nosso site e visualizar ou baixar o seu certificado emitido." + Environment.NewLine + Environment.NewLine +
                             $" Para acessar o seu certificado, siga os passos abaixo:" + Environment.NewLine + Environment.NewLine +
                             $" 1. Acesse o site através do seguinte endereço: https://emescam.certificados.edu/participante" + Environment.NewLine +
                             $" 2. Utilize as seguintes credenciais para fazer o login:" + Environment.NewLine +
                             $" Usuário: {usuario}" + Environment.NewLine +
                             $" Senha: {senha}" + Environment.NewLine + Environment.NewLine +
                             $" Caso encontre qualquer dificuldade ou tenha dúvidas, por favor, não hesite em entrar em contato " + Environment.NewLine +
                             $" conosco através deste e-mail ou pelo telefone" + Environment.NewLine + Environment.NewLine +
                             $" Agradecemos pela sua participação e esperamos vê-lo (a) em nossos próximos eventos.";

                return textoCorpo;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EmailService.BuscarTextoCorpoAsync]: {ex.Message}");
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

        private async Task ObterToken(string login, string senha)
        {          
            try
            {
                var url = "https://apps.emescam.br/site/comunicador/api/auth/login";
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

        private async Task<(bool success, string retorno)> EnviarEmail(EmailConfigRepository emailConfig, string destinatario, string assunto, string corpo,  string[] cc, byte[] anexo, byte[] assinatura)
        {
            try
            {
                var url = "https://apps.emescam.br/site/comunicador/api/email/enviar";
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                var form = new MultipartFormDataContent();

                // Adicionar campos obrigatórios ao formulário
                form.Add(new StringContent(destinatario), "Destinatario");
                form.Add(new StringContent(assunto), "Assunto");
                form.Add(new StringContent(corpo), "Corpo");
                //form.Add(new StringContent(remetente), "Remetente");
                //form.Add(new StringContent(senhaEmail), "Senha");

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
