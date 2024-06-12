using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EMISSOR_DE_CERTIFICADOS.Services
{
    public class EmailService
    {
        private string _token;

        public async Task<bool> EnviarEmailAsync(string login, string senha, int idEventoPessoa)
        {
            try
            {
                // Obter o token de autenticação
                await ObterTokenAsync(login, senha);

                if (string.IsNullOrEmpty(_token))
                {
                    throw new Exception("Falha ao obter token de autenticação.");
                }

                // Aqui você pode adicionar o código para enviar o e-mail usando o token de autenticação obtido.
                // Exemplo: chamar um endpoint para enviar o e-mail, usando o token armazenado.

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EmailService.EnviarEmailAsync]: {ex.Message}");
            }
        }

        private async Task ObterTokenAsync(string login, string senha)
        {
            var url = "https://apps.emescam.br/site/comunicador/api/auth/login";
            var httpClient = new HttpClient();

            var payload = new
            {
                Username = login,
                Password = senha
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<TokenResponse>(responseString);
                    _token = responseObject.Token; // Supondo que a resposta tenha um campo "Token"
                }
                else
                {
                    throw new Exception("Falha na autenticação: " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter token de autenticação: {ex.Message}");
            }
        }

        private class TokenResponse
        {
            [JsonProperty("token")]
            public string Token { get; set; }
        }
    }
}
