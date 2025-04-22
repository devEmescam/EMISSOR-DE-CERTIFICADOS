using EMISSOR_DE_CERTIFICADOS.DBConnections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using RectangleF = System.Drawing.RectangleF;
using Rectangle = System.Drawing.Rectangle;
using EMISSOR_DE_CERTIFICADOS.Helpers;
using EMISSOR_DE_CERTIFICADOS.Interfaces;

namespace EMISSOR_DE_CERTIFICADOS.Services
{
    internal class CertificadosService_DRAWING : ICertificadosService
    {
        private readonly IDBHelpers _dbHelper1;
        private readonly ISessao _sessao1;
        private readonly IPessoaEventosRepository _pessoaEventosRepository;
        private readonly IPessoaService _pessoaService1;
        private readonly ICertificadosRepository _certiificadosRepository1;

        public CertificadosService_DRAWING(IDBHelpers dbHelper, ISessao sessao, IPessoaEventosRepository pessoaEventosRepository, IPessoaService pessoaService, ICertificadosRepository certificadosRepository)
        {
            _dbHelper1 = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper), "O DBHelpers não pode ser nulo.");
            _sessao1 = sessao ?? throw new ArgumentNullException(nameof(sessao), "O ISessao não pode ser nulo.");
            _pessoaEventosRepository = pessoaEventosRepository ?? throw new ArgumentNullException(nameof(pessoaEventosRepository), "O PessoaEventosRepository não pode ser nulo.");
            _pessoaService1 = pessoaService ?? throw new ArgumentNullException(nameof(pessoaService), "O IPessoaService não ser nulo.");
            _certiificadosRepository1 = certificadosRepository ?? throw new ArgumentNullException(nameof(certificadosRepository), "O ICertificadosRepository não ser nulo.");
        }              
        public async Task<bool> GerarCertificadoAsync(int idEvento_Pessoa, int idPessoa, string textoOriginal, IFormFile imagem)
        {
            string textoCertificado = string.Empty;
            string textoAutenticidade = string.Empty;
            string codigoCertificado = string.Empty;
            string cpfParticipante = string.Empty;
            string nomeParticipante = string.Empty;
            string caminhoQRCode = string.Empty;
            string textCertLocaleData = string.Empty;            
            string textoAntesNomePessoa = string.Empty;
            bool retorno = false;

            try
            {               
                var resultado = await ProcessarTextoCertificadoAsync1(idPessoa, textoOriginal);
                textoAntesNomePessoa = resultado.Item1;
                nomeParticipante = resultado.Item2;
                textoCertificado = resultado.Item3;
                textCertLocaleData = resultado.Item4;
                //textoCertificado = "Esse ajuste deve alinhar a borda inferior de retTextoFixo com a borda superior de retanguloNomeParticipante, e alinhar a borda esquerda de retTextoFixo com a borda esquerda de retanguloTextoCertificado.Verifique se os tamanhos e posições dos retângulos atendem às suas necessidades. Caso precise ajustar, é possível alterar as variáveis relacionadas às dimensões e posições dos retângulos.";
                //textCertLocaleData = "Vitória, 07 de maio de 2024";                
                //nomeParticipante = "URSULA THAIS MENDES DA COSTA MORAES VARJÃO PARAGUASSÚ DE SÁ";
                cpfParticipante = await RetornarCPFAsync(idPessoa);
                codigoCertificado = GerarCodigo(cpfParticipante);
                textoAutenticidade = RetornarTextoAutenticidade(codigoCertificado);
                caminhoQRCode = Path.Combine("wwwroot", "QRCodeEmescam.png");

                using (var memoryStream = new MemoryStream())
                {
                    await imagem.CopyToAsync(memoryStream);
                    using (Bitmap certificado = new Bitmap(memoryStream))
                    {
                        using (Font fontTextCertLocaleData = new Font("Arial", 48, FontStyle.Regular, GraphicsUnit.Pixel)) // tamanho original: 30
                        using (Font fonteTextoFixo = new Font("Arial", 48, FontStyle.Regular, GraphicsUnit.Pixel)) // tamanho original: 42
                        using (Graphics graphics = Graphics.FromImage(certificado))
                        {
                            // GERAL: Definir algumas caracteristicas dos elementos
                            //Cor do texto
                            Color corTextoCertificado = Color.DarkSlateGray; 
                            Brush pincelTextoCertificado = new SolidBrush(corTextoCertificado);
                            //Bordas dos retangulos
                            float larguraBorda = 2;
                            Pen penBorda = new Pen(Color.Black, larguraBorda);
                            //Margem lateral dos retangulos: usado para definir a largura dos retangulos
                            float margemLateral = certificado.Width / 16;
                            //Alinhamento do texto
                            StringFormat alinhamentoTexto = new StringFormat { Alignment = StringAlignment.Center };

                            //===================================================================================== INICIO
                            // TEXTO CERTIFICADO
                            float posicaoVertical = certificado.Height / 2.0f;
                            float altRetTextCertificado = 200;

                            if (!string.IsNullOrEmpty(textoCertificado))
                            {
                                RectangleF retTextCertificado = new RectangleF(margemLateral, posicaoVertical, certificado.Width - 2 * margemLateral, altRetTextCertificado);

                                // Ajustar tamanho da fonte do texto do certificado
                                float tamanhoFonteTextoCertificado = 48; // tamanho original 42
                                Font fonteTextoCertificado = new Font("Arial", tamanhoFonteTextoCertificado, FontStyle.Regular, GraphicsUnit.Pixel);
                                SizeF tamanhoTextoCertificado = graphics.MeasureString(textoCertificado, fonteTextoCertificado, (int)retTextCertificado.Width);

                                // Ajusta o texto dentro do retângulo diminuindo o tamanho da fonte se necessário
                                while ((tamanhoTextoCertificado.Width > retTextCertificado.Width || tamanhoTextoCertificado.Height > retTextCertificado.Height) && tamanhoFonteTextoCertificado > 10)
                                {
                                    tamanhoFonteTextoCertificado -= 1;
                                    fonteTextoCertificado = new Font("Arial", tamanhoFonteTextoCertificado, FontStyle.Regular, GraphicsUnit.Pixel);
                                    tamanhoTextoCertificado = graphics.MeasureString(textoCertificado, fonteTextoCertificado, (int)retTextCertificado.Width);
                                }

                                // Desenha o texto dentro do retângulo
                                graphics.DrawString(textoCertificado, fonteTextoCertificado, pincelTextoCertificado, retTextCertificado, alinhamentoTexto);
                            }
                            //===================================================================================== FIM

                            //===================================================================================== INICIO
                            // ESPAÇAMENTO ENTRE O TEXTO DO CERTIFICADO E O NOME DO PARTICIPANTE
                            float espacamentoEntreTextoENome = -5;  // Espaçamento reduzido para 0,2 unidades
                            float posicaoVerticalNome = posicaoVertical - altRetTextCertificado - espacamentoEntreTextoENome; // Subir o nome ajustando a posição

                            // NOME PARTICIPANTE
                            float altRetNomeParticipante = 85;
                            RectangleF retNomeParticipante = new RectangleF(margemLateral, posicaoVerticalNome, certificado.Width - 2 * margemLateral, altRetNomeParticipante);

                            if (!string.IsNullOrEmpty(nomeParticipante))
                            {
                                // Ajustar tamanho da fonte do nome do participante
                                float tamanhoFonteNomeParticipante = 80; // tamanho original 60
                                Font fonteNomeParticipante = new Font("Arial", tamanhoFonteNomeParticipante, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel);
                                SizeF tamanhoTextoNome = graphics.MeasureString(nomeParticipante, fonteNomeParticipante);

                                // Ajusta o texto dentro do retângulo diminuindo o tamanho da fonte se necessário
                                while (tamanhoTextoNome.Width > retNomeParticipante.Width && tamanhoFonteNomeParticipante > 10)
                                {
                                    tamanhoFonteNomeParticipante -= 1;
                                    fonteNomeParticipante = new Font("Arial", tamanhoFonteNomeParticipante, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel);
                                    tamanhoTextoNome = graphics.MeasureString(nomeParticipante, fonteNomeParticipante);
                                }

                                // Desenha o texto dentro do retângulo
                                graphics.DrawString(nomeParticipante, fonteNomeParticipante, pincelTextoCertificado, retNomeParticipante, alinhamentoTexto);
                            }
                            //===================================================================================== FIM

                            //===================================================================================== INICIO
                            // TEXTO ANTES DO NOME
                            if (!string.IsNullOrEmpty(textoAntesNomePessoa))
                            {
                                float altRetTextFixo = graphics.MeasureString(textoAntesNomePessoa, fonteTextoFixo).Height;
                                float posVertTextFixo = retNomeParticipante.Y;

                                // Ajustando para mover o texto um pouco mais para cima
                                float deslocamentoParaCima = 40; // Ajuste o valor conforme necessário
                                posVertTextFixo -= deslocamentoParaCima;

                                RectangleF retTextoFixo = new RectangleF(margemLateral, posVertTextFixo - altRetTextFixo, certificado.Width - 2 * margemLateral, altRetTextFixo);

                                // USADO APENAS PARA NECESSIDADE DE AJUSTES: Desenha o retângulo com borda
                                // graphics.DrawRectangle(penBorda, retTextoFixo.X, retTextoFixo.Y, retTextoFixo.Width, retTextoFixo.Height);

                                // Desenha o texto dentro do retângulo
                                graphics.DrawString(textoAntesNomePessoa, fonteTextoFixo, pincelTextoCertificado, retTextoFixo, alinhamentoTexto);
                            }

                            //===================================================================================== FIM

                            //===================================================================================== INICIO
                            // TEXTO LOCAL E DATA
                            if (!string.IsNullOrEmpty(textCertLocaleData)) 
                            {
                                float posVertTextCertLocaleData = posicaoVertical + altRetTextCertificado;
                                float altRetTextCertLocaleData = 50;
                                RectangleF retTextCertLocaleData = new RectangleF(margemLateral, posVertTextCertLocaleData, certificado.Width - 2 * margemLateral, altRetTextCertLocaleData);
                                // USADO APENAS PARA NECESSIDADE DE AJUSTES: Desenha o retângulo com borda
                                //graphics.DrawRectangle(penBorda, retTextCertLocaleData.X, retTextCertLocaleData.Y, retTextCertLocaleData.Width, retTextCertLocaleData.Height);
                                // Desenha o texto dentro do retângulo
                                graphics.DrawString(textCertLocaleData, fontTextCertLocaleData, pincelTextoCertificado, retTextCertLocaleData, alinhamentoTexto);
                            }                            
                            //===================================================================================== FIM

                            //===================================================================================== INICIO
                            // TEXTO DE AUTENTICIDADE e QRCode
                            using (Font fonteTextoAutenticidade = new Font("Arial", 22, FontStyle.Regular, GraphicsUnit.Pixel)) //tamanho original22
                            {                                
                                float posHor = margemLateral - 30; // Use margemLateral para alinhar com retTextCertificado
                                RectangleF retTextAutenticidade = new RectangleF(posHor, certificado.Height - 70, 1500, 50);
                                // USADO APENAS PARA NECESSIDADE DE AJUSTES: Desenha o retângulo com borda
                                //graphics.DrawRectangle(penBorda, retTextAutenticidade.X, retTextAutenticidade.Y, retTextAutenticidade.Width, retTextAutenticidade.Height);
                                graphics.DrawString(textoAutenticidade, fonteTextoAutenticidade, pincelTextoCertificado, retTextAutenticidade);

                                Bitmap qrCodeBitmap = new Bitmap(caminhoQRCode);
                                Rectangle retanguloQRCode = new Rectangle(certificado.Width - 245, certificado.Height - 228, 153, 153);
                                graphics.DrawImage(qrCodeBitmap, retanguloQRCode);
                            }
                            //===================================================================================== FIM
                        }

                        byte[] certificadoBytes;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            certificado.Save(ms, ImageFormat.Png);
                            certificadoBytes = ms.ToArray();
                        }

                        await InserirAsync(idEvento_Pessoa, certificadoBytes, codigoCertificado);
                        retorno = true;
                    }
                }

                return retorno;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [CertificadosService.GerarCertificadoAsync]: {ex.Message}");
            }
        }
        private async Task<(string, string, string, string, string)> ProcessarTextoCertificadoAsync1(int idPessoa, string texto)
        {
            string textoAntesPessoa = string.Empty;
            string textoLocalData = string.Empty;
            string textoCertificado = string.Empty;
            string nomePessoa = string.Empty;
            string cpf = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(texto))
                {
                    // Se o texto não contiver as tags esperadas, atribuir o texto diretamente à variável textoCertificado
                    if (!texto.Contains("NOME_PESSOA") && !texto.Contains("CPF_PESSOA") && !texto.Contains("<br>"))
                    {
                        // Ajuste para garantir que o texto original sem tags seja retornado corretamente
                        textoCertificado = texto;
                    }
                    else
                    {
                        int posNomePessoa = -1;
                        int posBR = -1;

                        if (texto.Contains("NOME_PESSOA"))
                        {
                            nomePessoa = await _pessoaService1.ObterNomePorIdPessoaAsync(idPessoa);
                            posNomePessoa = texto.IndexOf("NOME_PESSOA");
                        }

                        posBR = texto.IndexOf("<br>", StringComparison.OrdinalIgnoreCase);

                        if (posNomePessoa != -1)
                        {
                            textoAntesPessoa = texto.Substring(0, posNomePessoa) + " ";
                        }

                        if (posBR != -1)
                        {
                            textoLocalData = texto.Substring(posBR + "<br>".Length);
                        }

                        if (posNomePessoa != -1 && posBR != -1 && posBR > posNomePessoa)
                        {
                            textoCertificado = texto.Substring(posNomePessoa + "NOME_PESSOA".Length, posBR - (posNomePessoa + "NOME_PESSOA".Length));

                            if (textoCertificado.Contains("CPF_PESSOA"))
                            {
                                cpf = await RetornarCPFAsync(idPessoa);
                                textoCertificado = textoCertificado.Replace("CPF_PESSOA", cpf);
                            }
                        }
                    }
                }

                // Aplicar o estilo de negrito para as tags <negrito>
                textoCertificado = textoCertificado.Replace("<negrito>", "<strong>")
                                                   .Replace("</negrito>", "</strong>");

                // Gerar o HTML do certificado
                string htmlCertificado = $@"
                     <html>
                         <head>
                             <style>
                                 .certificado {{
                                     font-family: Arial, sans-serif;
                                     font-size: 20px;
                                 }}
                                 .certificado strong {{
                                     font-weight: bold;
                                     color: #333; /* Cor mais escura */
                                 }}
                             </style>
                         </head>
                         <body>
                             <div class='certificado'>
                                 {textoAntesPessoa}
                                 <h2>{nomePessoa}</h2>
                                 <p>{textoCertificado}</p>
                                 <p>{textoLocalData}</p>
                             </div>
                         </body>
                     </html>";

                return (textoAntesPessoa, nomePessoa, textoCertificado, textoLocalData, htmlCertificado); // Retorna o HTML completo
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [CertificadoEditor.ProcessarTextoCertificadoAsync]", ex);
            }
        }

        private string RetornarTextoAutenticidade(string codigo)
        {
            string texto = string.Empty;

            try
            {
                texto = $"Código do Certificado: {codigo} - Verifique autenticidade em: https://certificados.emescam.br/Autenticador";
                return texto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em CertificadoEditor.RetornarTextoAutenticidade: {ex.Message}");
            }
        }
        private string GerarCodigo(string cpf)
        {
            string chave = string.Empty;
            try
            {
                //Defini data e hora atual como parte da chave
                chave = DateTime.Now.ToString("g");
                chave = chave.Replace("/", "").Replace(":", "").Replace(" ", "");
                //Concatena com CPF
                chave += cpf;

                // Definindo os caracteres permitidos no código alfanumérico
                const string caracteresPermitidos = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

                // Utilizando o hash do input para garantir a unicidade do código
                int hashCode = chave.GetHashCode();

                // Utilizando um objeto Random para gerar o código
                Random random = new Random(hashCode);

                // StringBuilder para construir o código
                StringBuilder codigo = new StringBuilder();

                // Gerando o código com 22 caracteres
                for (int i = 0; i < 22; i++)
                {
                    // Escolhendo um caractere aleatório do conjunto permitido
                    int indice = random.Next(caracteresPermitidos.Length);
                    codigo.Append(caracteresPermitidos[indice]);
                }
                return codigo.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em CertificadoEditor.GerarCodigo: {ex.Message}");
            }
        }
        private async Task<string> RetornarCPFAsync(int idPessoa)
        {
            try
            {                
                string cpf = await _pessoaService1.ObterCPFPorIdPessoaAsync(idPessoa);
                return cpf;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [CertificadoEditor.RetornarCPFAsync]: {ex.Message}");
            }
        }       
        private async Task InserirAsync(int idEventoPessoa, byte[] certificadoBytes, string codigoCertificado)
        {
            try
            {
                await _certiificadosRepository1.InserirAsync(idEventoPessoa, certificadoBytes, codigoCertificado);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [CertificadosService.Inserir]: {ex.Message}");
            }
        }
    }
}