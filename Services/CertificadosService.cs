using EMISSOR_DE_CERTIFICADOS.Controllers;
using EMISSOR_DE_CERTIFICADOS.DBConnections;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
//using PdfSharpCore.Drawing;
//using PdfSharpCore.Pdf;
//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.PixelFormats;
//using SixLabors.ImageSharp.Processing;
//using Image = SixLabors.ImageSharp.Image;
using RectangleF = System.Drawing.RectangleF;
using Rectangle = System.Drawing.Rectangle;
//using SixLabors.Fonts;
//using SixLabors.ImageSharp.Formats.Png;

namespace EMISSOR_DE_CERTIFICADOS.Services
{
    public class CertificadosService
    {
        private readonly DBHelpers _dbHelper;
        public CertificadosService(DBHelpers dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper), "O DBHelpers não pode ser nulo.");
        }      
        public async Task<bool> GerarCertificadoAsync_ok_retangulo_nao_dinamico(int idEvento_Pessoa, int idPessoa, string textoOriginal, IFormFile imagem)
        {
            string textoCertificado = string.Empty;
            string textoAutenticidade = string.Empty;
            string codigoCertificado = string.Empty;
            string cpfParticipante = string.Empty;
            string nomeParticipante = string.Empty;
            string caminhoQRCode = string.Empty;
            string textoCertificadoLocaleData = string.Empty;
            bool retorno = false;
            string textoFixo = "Certificamos que ";

            try
            {
                textoCertificado = await ProcessarTextoCertificadoPessoaAsync(idPessoa, textoOriginal);
                textoCertificadoLocaleData = ProcessarTextoCertificadoLocaleData(idPessoa, textoOriginal);
                cpfParticipante = await RetornarCPFAsync(idPessoa);
                //nomeParticipante = await RetornarNomeAsync(idPessoa);
                nomeParticipante = "URSULA THAIS MENDES DA COSTA MORAES VARJÃO PARAGUASSÚ DE SÁ";
                codigoCertificado = GerarCodigo(cpfParticipante);
                textoAutenticidade = RetornarTextoAutenticidade(codigoCertificado);
                caminhoQRCode = Path.Combine("wwwroot", "QRCodeEmescam.png");

                // Carregar o arquivo IFormFile diretamente
                using (var memoryStream = new MemoryStream())
                {
                    await imagem.CopyToAsync(memoryStream);
                    using (Bitmap certificado = new Bitmap(memoryStream))
                    {
                        // Configurar as fontes e os tamanhos do texto
                        using (Font fonteTextoCertificado = new Font("Arial", 42, FontStyle.Regular, GraphicsUnit.Pixel))
                        using (Font fonteNomeParticipante = new Font("Arial", 60, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel))
                        using (Font fonteTextoCertificadoLocaleData = new Font("Arial", 30, FontStyle.Regular, GraphicsUnit.Pixel))
                        using (Font fonteTextoFixo = new Font("Arial", 42, FontStyle.Regular, GraphicsUnit.Pixel))
                        using (Graphics graphics = Graphics.FromImage(certificado))
                        {
                            // Definindo as propriedades da borda
                            float larguraBorda = 2; // Largura da borda em pixels
                            Pen penBorda = new Pen(System.Drawing.Color.Black, larguraBorda);

                            // Texto do certificado
                            float margemLateral = certificado.Width / 11; // Aumenta a largura reduzindo a margem lateral
                            float posicaoVertical = certificado.Height / 2.1f; // Posição vertical do retângulo do texto do certificado
                            float alturaRetanguloTextoCertificado = 200; // Altura do retângulo do texto do certificado
                            RectangleF retanguloTextoCertificado = new RectangleF(margemLateral, posicaoVertical, certificado.Width - 2 * margemLateral, alturaRetanguloTextoCertificado);
                            // Desenha o retângulo com borda
                            //graphics.DrawRectangle(penBorda, retanguloTextoCertificado.X, retanguloTextoCertificado.Y, retanguloTextoCertificado.Width, retanguloTextoCertificado.Height);
                            // Desenha o texto dentro do retângulo
                            graphics.DrawString(textoCertificado, fonteTextoCertificado, Brushes.Black, retanguloTextoCertificado, StringFormat.GenericTypographic);

                            // Nome do participante
                            // Altura do retângulo do nome do participante
                            StringFormat formatoCentralizado = new StringFormat { Alignment = StringAlignment.Center };
                            float alturaRetanguloNomeParticipante = 65;
                            RectangleF retanguloNomeParticipante = new RectangleF(certificado.Width / 6, posicaoVertical - alturaRetanguloNomeParticipante, certificado.Width / 1.5f, alturaRetanguloNomeParticipante);
                            // Desenha o retângulo com borda
                            //graphics.DrawRectangle(penBorda, retanguloNomeParticipante.X, retanguloNomeParticipante.Y, retanguloNomeParticipante.Width, retanguloNomeParticipante.Height);
                            // Desenha o texto dentro do retângulo
                            graphics.DrawString(nomeParticipante, fonteNomeParticipante, Brushes.Black, retanguloNomeParticipante, formatoCentralizado);

                            // Texto fixo "Certificamos que "
                            float larguraRetanguloTextoFixo = graphics.MeasureString(textoFixo, fonteTextoFixo).Width;
                            float alturaRetanguloTextoFixo = graphics.MeasureString(textoFixo, fonteTextoFixo).Height;
                            float posicaoVerticalTextoFixo = retanguloNomeParticipante.Y;
                            RectangleF retTextoFixo = new RectangleF(margemLateral, posicaoVerticalTextoFixo - alturaRetanguloTextoFixo, larguraRetanguloTextoFixo, alturaRetanguloTextoFixo);
                            // Desenha o retângulo com borda
                            //graphics.DrawRectangle(penBorda, retTextoFixo.X, retTextoFixo.Y, retTextoFixo.Width, retTextoFixo.Height);
                            // Desenha o texto dentro do retângulo
                            graphics.DrawString(textoFixo, fonteTextoFixo, Brushes.Black, retTextoFixo);

                            // Texto do certificadoLocaleData
                            // Largura do retângulo para o texto do certificadoLocaleData
                            float larguraRetanguloTextoCertificadoLocaleData = 375;
                            // Posição horizontal do retângulo do texto do certificadoLocaleData
                            float posicaoHorizontalTextoCertificadoLocaleData = certificado.Width - larguraRetanguloTextoCertificadoLocaleData - margemLateral;
                            // Posição vertical do retângulo do texto do certificadoLocaleData
                            float posicaoVerticalTextoCertificadoLocaleData = posicaoVertical + alturaRetanguloTextoCertificado;
                            RectangleF retanguloTextoCertificadoLocaleData = new RectangleF(posicaoHorizontalTextoCertificadoLocaleData, posicaoVerticalTextoCertificadoLocaleData, larguraRetanguloTextoCertificadoLocaleData, 50);
                            // Desenha o retângulo com borda
                            //graphics.DrawRectangle(penBorda, retanguloTextoCertificadoLocaleData.X, retanguloTextoCertificadoLocaleData.Y, retanguloTextoCertificadoLocaleData.Width, retanguloTextoCertificadoLocaleData.Height);
                            // Desenha o texto dentro do retângulo
                            graphics.DrawString(textoCertificadoLocaleData, fonteTextoCertificadoLocaleData, Brushes.Black, retanguloTextoCertificadoLocaleData, StringFormat.GenericTypographic);

                            // Texto de autenticidade
                            using (Font fonteTextoAutenticidade = new Font("Arial", 22, FontStyle.Regular, GraphicsUnit.Pixel))
                            {
                                float posicaoHorizontal = certificado.Width - 1925; // Ajuste conforme necessário
                                RectangleF retanguloTextoAutenticidade = new RectangleF(posicaoHorizontal, certificado.Height - 70, 1500, 50);
                                graphics.DrawString(textoAutenticidade, fonteTextoAutenticidade, Brushes.Black, retanguloTextoAutenticidade);

                                Bitmap qrCodeBitmap = new Bitmap(caminhoQRCode);
                                Rectangle retanguloQRCode = new Rectangle(certificado.Width - 245, certificado.Height - 225, 180, 180);
                                graphics.DrawImage(qrCodeBitmap, retanguloQRCode);
                            }
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
        public async Task<bool> GerarCertificadoAsync(int idEvento_Pessoa, int idPessoa, string textoOriginal, IFormFile imagem)
        {
            string textoCertificado = string.Empty;
            string textoAutenticidade = string.Empty;
            string codigoCertificado = string.Empty;
            string cpfParticipante = string.Empty;
            string nomeParticipante = string.Empty;
            string caminhoQRCode = string.Empty;
            string textCertLocaleData = string.Empty;
            bool retorno = false;
            string textoFixo = "Certificamos que ";

            try
            {
                textoCertificado = await ProcessarTextoCertificadoPessoaAsync(idPessoa, textoOriginal);
                //textoCertificado = "Esse ajuste deve alinhar a borda inferior de retTextoFixo com a borda superior de retanguloNomeParticipante, e alinhar a borda esquerda de retTextoFixo com a borda esquerda de retanguloTextoCertificado.Verifique se os tamanhos e posições dos retângulos atendem às suas necessidades. Caso precise ajustar, é possível alterar as variáveis relacionadas às dimensões e posições dos retângulos.";
                //textCertLocaleData = "Vitória, 07 de maio de 2024";
                textCertLocaleData = ProcessarTextoCertificadoLocaleData(idPessoa, textoOriginal);
                cpfParticipante = await RetornarCPFAsync(idPessoa);
                //nomeParticipante = "URSULA THAIS MENDES DA COSTA MORAES VARJÃO PARAGUASSÚ DE SÁ";
                nomeParticipante = await RetornarNomeAsync(idPessoa);
                codigoCertificado = GerarCodigo(cpfParticipante);
                textoAutenticidade = RetornarTextoAutenticidade(codigoCertificado);
                caminhoQRCode = Path.Combine("wwwroot", "QRCodeEmescam.png");

                using (var memoryStream = new MemoryStream())
                {
                    await imagem.CopyToAsync(memoryStream);
                    using (Bitmap certificado = new Bitmap(memoryStream))
                    {
                        using (Font fontTextCertLocaleData = new Font("Arial", 30, FontStyle.Regular, GraphicsUnit.Pixel))
                        using (Font fonteTextoFixo = new Font("Arial", 42, FontStyle.Regular, GraphicsUnit.Pixel))
                        using (Graphics graphics = Graphics.FromImage(certificado))
                        {
                            float larguraBorda = 2;
                            Pen penBorda = new Pen(System.Drawing.Color.Black, larguraBorda);
                            float margemLateral = certificado.Width / 11;
                            float posicaoVertical = certificado.Height / 2.1f;
                            float altRetTextCertificado = 200;
                            RectangleF retTextCertificado = new RectangleF(margemLateral, posicaoVertical, certificado.Width - 2 * margemLateral, altRetTextCertificado);

                            // Ajustar tamanho da fonte do texto do certificado
                            float tamanhoFonteTextoCertificado = 42;
                            Font fonteTextoCertificado = new Font("Arial", tamanhoFonteTextoCertificado, FontStyle.Regular, GraphicsUnit.Pixel);
                            SizeF tamanhoTextoCertificado = graphics.MeasureString(textoCertificado, fonteTextoCertificado, (int)retTextCertificado.Width);

                            while ((tamanhoTextoCertificado.Width > retTextCertificado.Width || tamanhoTextoCertificado.Height > retTextCertificado.Height) && tamanhoFonteTextoCertificado > 10)
                            {
                                tamanhoFonteTextoCertificado -= 1;
                                fonteTextoCertificado = new Font("Arial", tamanhoFonteTextoCertificado, FontStyle.Regular, GraphicsUnit.Pixel);
                                tamanhoTextoCertificado = graphics.MeasureString(textoCertificado, fonteTextoCertificado, (int)retTextCertificado.Width);
                            }

                            // Desenha o retângulo com borda
                            //graphics.DrawRectangle(penBorda, retTextCertificado.X, retTextCertificado.Y, retTextCertificado.Width, retTextCertificado.Height);
                            // Desenha o texto dentro do retângulo
                            graphics.DrawString(textoCertificado, fonteTextoCertificado, Brushes.Black, retTextCertificado, new StringFormat(StringFormatFlags.LineLimit) { Alignment = StringAlignment.Center });

                            // Nome do participante
                            float altRetNomeParticipante = 65;
                            RectangleF retNomeParticipante = new RectangleF(certificado.Width / 6, posicaoVertical - altRetNomeParticipante, certificado.Width / 1.5f, altRetNomeParticipante);
                            StringFormat formatoCentralizado = new StringFormat { Alignment = StringAlignment.Center };

                            // Ajustar tamanho da fonte do nome do participante
                            float tamanhoFonteNomeParticipante = 60;
                            Font fonteNomeParticipante = new Font("Arial", tamanhoFonteNomeParticipante, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel);
                            SizeF tamanhoTextoNome = graphics.MeasureString(nomeParticipante, fonteNomeParticipante);
                            while (tamanhoTextoNome.Width > retNomeParticipante.Width && tamanhoFonteNomeParticipante > 10)
                            {
                                tamanhoFonteNomeParticipante -= 1;
                                fonteNomeParticipante = new Font("Arial", tamanhoFonteNomeParticipante, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel);
                                tamanhoTextoNome = graphics.MeasureString(nomeParticipante, fonteNomeParticipante);
                            }

                            // Desenha o texto dentro do retângulo
                            graphics.DrawString(nomeParticipante, fonteNomeParticipante, Brushes.Black, retNomeParticipante, formatoCentralizado);

                            // Texto fixo "Certificamos que "
                            float largRetTextoFixo = graphics.MeasureString(textoFixo, fonteTextoFixo).Width;
                            float altRetTextFixo = graphics.MeasureString(textoFixo, fonteTextoFixo).Height;
                            float posVertTextFixo = retNomeParticipante.Y;
                            RectangleF retTextoFixo = new RectangleF(margemLateral, posVertTextFixo - altRetTextFixo, largRetTextoFixo, altRetTextFixo);
                            // Desenha o texto dentro do retângulo
                            graphics.DrawString(textoFixo, fonteTextoFixo, Brushes.Black, retTextoFixo);

                            // Texto do certificadoLocaleData
                            float largRetTextCertLocaleData = 375;
                            float posHorTextCertLocaleData = certificado.Width - largRetTextCertLocaleData - margemLateral;
                            float posVertTextCertLocaleData = posicaoVertical + altRetTextCertificado;
                            RectangleF retTextCertLocaleData = new RectangleF(posHorTextCertLocaleData, posVertTextCertLocaleData, largRetTextCertLocaleData, 50);
                            // Desenha o texto dentro do retângulo
                            graphics.DrawString(textCertLocaleData, fontTextCertLocaleData, Brushes.Black, retTextCertLocaleData, StringFormat.GenericTypographic);

                            // Texto de autenticidade
                            using (Font fonteTextoAutenticidade = new Font("Arial", 22, FontStyle.Regular, GraphicsUnit.Pixel))
                            {
                                float posHor = certificado.Width - 1925;
                                RectangleF retTextAutenticidade = new RectangleF(posHor, certificado.Height - 70, 1500, 50);
                                graphics.DrawString(textoAutenticidade, fonteTextoAutenticidade, Brushes.Black, retTextAutenticidade);

                                Bitmap qrCodeBitmap = new Bitmap(caminhoQRCode);
                                Rectangle retanguloQRCode = new Rectangle(certificado.Width - 245, certificado.Height - 225, 180, 180);
                                graphics.DrawImage(qrCodeBitmap, retanguloQRCode);
                            }
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
        private async Task<string> ProcessarTextoCertificadoPessoaAsync(int idPessoa, string texto)
        {
            string nomePessoa = string.Empty;
            string cpf = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(texto))
                {
                    using (PessoaController pessoaController = new PessoaController(_dbHelper))
                    {
                        nomePessoa = await pessoaController.ObterNomePorIdPessoaAsync(idPessoa);
                        if (!string.IsNullOrEmpty(nomePessoa))
                        {
                            texto = texto.Replace("NOME_PESSOA", nomePessoa);
                        }

                        cpf = await pessoaController.ObterCPFPorIdPessoaAsync(idPessoa);
                        if (!string.IsNullOrEmpty(cpf))
                        {
                            texto = texto.Replace("CPF_PESSOA", cpf);
                        }                        
                    }

                    // Remover todas as tags <br> e o texto à direita de cada uma
                    string[] parts = texto.Split(new string[] { "<br>" }, StringSplitOptions.None);
                    texto = parts[0];
                }

                return texto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [CertificadoEditor.ProcessarTextoCertificadoPessoa]", ex);
            }
        }
        private string ProcessarTextoCertificadoLocaleData(int idPessoa, string texto)
        {
            try
            {
                if (!string.IsNullOrEmpty(texto))
                {                   
                    // Remover todas as tags <br> e o texto à direita de cada uma
                    string[] parts = texto.Split(new string[] { "<br>" }, StringSplitOptions.None);
                    texto = parts[parts.Length -1];
                }

                return texto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [CertificadoEditor.ProcessarTextoCertificadoPessoa]", ex);
            }
        }
        private string RetornarTextoAutenticidade(string codigo)
        {
            string texto = string.Empty;

            try
            {
                texto = $"Código do Certificado: {codigo} - Verifique autenticidade em: definirUrl.com.br";
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
                using (PessoaController pessoaController = new PessoaController(_dbHelper))
                {
                    string cpf = await pessoaController.ObterCPFPorIdPessoaAsync(idPessoa);
                    return cpf;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [CertificadoEditor.RetornarCPFAsync]: {ex.Message}");
            }
        }
        private async Task<string> RetornarNomeAsync(int idPessoa)
        {
            try
            {
                using (PessoaController pessoaController = new PessoaController(_dbHelper))
                {
                    string nome = await pessoaController.ObterNomePorIdPessoaAsync(idPessoa);
                    return nome;
                }
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
                string sSQL = "UPDATE EVENTO_PESSOA SET IMAGEM_CERTIFICADO = @Certificado, CODIGO_CERTIFICADO = @CodigoCertificado WHERE ID = @IdEventoPessoa";

                using (var connection = _dbHelper.GetConnection("CertificadoConnection"))
                {
                    using (var command = new SqlCommand(sSQL, (SqlConnection)connection))
                    {
                        command.Parameters.AddWithValue("@Certificado", certificadoBytes);
                        command.Parameters.AddWithValue("@CodigoCertificado", codigoCertificado);
                        command.Parameters.AddWithValue("@IdEventoPessoa", idEventoPessoa);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [CertificadosService.Inserir]: {ex.Message}");
            }
        }
    }
}