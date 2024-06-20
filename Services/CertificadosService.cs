using EMISSOR_DE_CERTIFICADOS.Controllers;
using EMISSOR_DE_CERTIFICADOS.DBConnections;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
//using SixLabors.ImageSharp.Processing;
//using Image = SixLabors.ImageSharp.Image;
using RectangleF = System.Drawing.RectangleF;
using Rectangle = System.Drawing.Rectangle;
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
        //public bool GerarCertificado(int idEvento_Pessoa, int idPessoa, string textoOriginal, IFormFile imagem)
        //{
        //    string textoCertificado = string.Empty;
        //    string textoAutenticidade = string.Empty;
        //    string codigoCertificado = string.Empty;
        //    string cpf = string.Empty;
        //    string caminhoQRCode = string.Empty;
        //    bool retorno = false;

        //    try
        //    {
        //        textoCertificado = ProcessarTextoCertificadoPessoa(idPessoa, textoOriginal);
        //        cpf = RetornarCPF(idPessoa);
        //        codigoCertificado = GerarCodigo(cpf);
        //        textoAutenticidade = RetornarTextoAutenticidade(codigoCertificado);
        //        caminhoQRCode = Path.Combine("wwwroot", "QRCodeEmescam.png");

        //        // Caminho onde o certificado será salvo
        //        string nomeArquivoCertificado = codigoCertificado + ".png";
        //        string caminhoCertificado = Path.Combine("wwwroot", "Certificados", nomeArquivoCertificado);

        //        // Carregar o arquivo IFormFile diretamente
        //        using (var memoryStream = new MemoryStream())
        //        {
        //            imagem.CopyTo(memoryStream);
        //            using (Bitmap certificado = new Bitmap(memoryStream))
        //            {
        //                // Configurar a fonte e o tamanho do texto1
        //                using (Font fonteTextoCertificado = new Font("Arial", 48, FontStyle.Regular, GraphicsUnit.Pixel))
        //                {
        //                    // Criar um objeto de gráficos a partir do certificado
        //                    using (Graphics graphics = Graphics.FromImage(certificado))
        //                    {
        //                        // Configurar o alinhamento central horizontal
        //                        StringFormat formatoCentralizado = new StringFormat
        //                        {
        //                            Alignment = StringAlignment.Center
        //                        };

        //                        // Configurar a região central para o texto1
        //                        RectangleF retanguloTextoCertificado = new RectangleF(certificado.Width / 6, certificado.Height / 2.5f, certificado.Width / 1.5f, certificado.Height / 2); // 2970px X 2100px

        //                        // Desenhar o texto1 na região central
        //                        graphics.DrawString(textoCertificado, fonteTextoCertificado, Brushes.Black, retanguloTextoCertificado, formatoCentralizado);

        //                        // Configurar a fonte e o tamanho do textoAutenticidade
        //                        using (Font fontetextoAutenticidade = new Font("Arial", 24, FontStyle.Bold, GraphicsUnit.Pixel))
        //                        {
        //                            // Configurar a região inferior esquerda para o textotextoAutenticidade
        //                            RectangleF retanguloTextoAutenticidade = new RectangleF(10, certificado.Height - 120, 1500, 50);

        //                            // Desenhar o textoAutenticidade na região inferior esquerda
        //                            graphics.DrawString(textoAutenticidade, fontetextoAutenticidade, Brushes.Black, retanguloTextoAutenticidade);

        //                            // Configurar a região inferior direita para o QRCode
        //                            Bitmap qrCodeBitmap = new Bitmap(caminhoQRCode);
        //                            Rectangle retanguloQRCode = new Rectangle(certificado.Width - 280, certificado.Height - 260, 180, 180);

        //                            // Desenhar o QRCode na região inferior direita
        //                            graphics.DrawImage(qrCodeBitmap, retanguloQRCode);
        //                        }
        //                    }
        //                }

        //                // Convertendo o certificado para um array de bytes
        //                byte[] certificadoBytes;
        //                using (MemoryStream ms = new MemoryStream())
        //                {
        //                    certificado.Save(ms, ImageFormat.Png);
        //                    certificadoBytes = ms.ToArray();
        //                }

        //                // Inserir o certificado no banco de dados
        //                Inserir(idEvento_Pessoa, certificadoBytes, codigoCertificado);

        //                retorno = true;
        //            }
        //        }

        //        return retorno;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Erro em [CertificadosService.GerarCertificado_SEM_NEGRITO]: {ex.Message}");
        //    }
        //}
        public async Task<bool> GerarCertificadoAsync(int idEvento_Pessoa, int idPessoa, string textoOriginal, IFormFile imagem)
        {
            string textoCertificado = string.Empty;
            string textoAutenticidade = string.Empty;
            string codigoCertificado = string.Empty;
            string cpf = string.Empty;
            string caminhoQRCode = string.Empty;
            bool retorno = false;

            try
            {
                textoCertificado = await ProcessarTextoCertificadoPessoaAsync(idPessoa, textoOriginal);
                cpf = await RetornarCPFAsync(idPessoa);
                codigoCertificado = GerarCodigo(cpf);
                textoAutenticidade = RetornarTextoAutenticidade(codigoCertificado);
                caminhoQRCode = Path.Combine("wwwroot", "QRCodeEmescam.png");

                // Carregar o arquivo IFormFile diretamente
                using (var memoryStream = new MemoryStream())
                {
                    await imagem.CopyToAsync(memoryStream);
                    using (Bitmap certificado = new Bitmap(memoryStream))
                    {
                        // Configurar a fonte e o tamanho do texto1
                        using (Font fonteTextoCertificado = new Font("Arial", 48, FontStyle.Regular, GraphicsUnit.Pixel))
                        {
                            using (Graphics graphics = Graphics.FromImage(certificado))
                            {
                                StringFormat formatoCentralizado = new StringFormat { Alignment = StringAlignment.Center };
                                RectangleF retanguloTextoCertificado = new RectangleF(certificado.Width / 6, certificado.Height / 2.5f, certificado.Width / 1.5f, certificado.Height / 2); // 2970px X 2100px
                                graphics.DrawString($@"{textoCertificado}", fonteTextoCertificado, Brushes.Black, retanguloTextoCertificado, formatoCentralizado);

                                using (Font fontetextoAutenticidade = new Font("Arial", 22, FontStyle.Regular, GraphicsUnit.Pixel))
                                {
                                    RectangleF retanguloTextoAutenticidade = new RectangleF(10, certificado.Height - 120, 1500, 50);
                                    graphics.DrawString($@"{textoAutenticidade}", fontetextoAutenticidade, Brushes.Black, retanguloTextoAutenticidade);

                                    Bitmap qrCodeBitmap = new Bitmap(caminhoQRCode);
                                    Rectangle retanguloQRCode = new Rectangle(certificado.Width - 280, certificado.Height - 260, 180, 180);
                                    graphics.DrawImage(qrCodeBitmap, retanguloQRCode);
                                }
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
        public async Task<bool> GerarCertificadoAsync_PDF(int idEvento_Pessoa, int idPessoa, string textoOriginal, IFormFile imagem)
        {
            string textoCertificado = string.Empty;
            string textoAutenticidade = string.Empty;
            string codigoCertificado = string.Empty;
            string cpf = string.Empty;
            string caminhoQRCode = string.Empty;
            bool retorno = false;

            try
            {
                textoCertificado = await ProcessarTextoCertificadoPessoaAsync(idPessoa, textoOriginal);
                cpf = await RetornarCPFAsync(idPessoa);
                codigoCertificado = GerarCodigo(cpf);
                textoAutenticidade = RetornarTextoAutenticidade(codigoCertificado);
                caminhoQRCode = Path.Combine("wwwroot", "QRCodeEmescam.png");

                // Criar o documento PDF usando PdfSharpCore
                var pdfDocument = new PdfDocument();
                var page = pdfDocument.AddPage();
                page.Orientation = PdfSharpCore.PageOrientation.Landscape; // Definir orientação paisagem                
                //page.Width = new XUnit(2970); // Largura em pontos
                //page.Height = new XUnit(2100); // Altura em pontos
                page.Width = 2970; // Largura em pontos
                page.Height = 2100; // Altura em pontos


                using (var gfx = XGraphics.FromPdfPage(page))
                {
                    // Carregar a imagem do certificado do IFormFile
                    using (var memoryStream = new MemoryStream())
                    {
                        await imagem.CopyToAsync(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        using (var certificadoImagem = SixLabors.ImageSharp.Image.Load<Rgba32>(memoryStream))
                        {
                            var imgPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
                            certificadoImagem.Save(imgPath, new SixLabors.ImageSharp.Formats.Png.PngEncoder());

                            using (var xImage = XImage.FromFile(imgPath))
                            {
                                gfx.DrawImage(xImage, 0, 0, page.Width, page.Height);
                            }

                            // Deletar o arquivo temporário da imagem
                            File.Delete(imgPath);
                        }
                    }

                    var fontTextoCertificado = new XFont("Arial", 48, XFontStyle.Regular);
                    var fontTextoAutenticidade = new XFont("Arial", 24, XFontStyle.Bold);

                    // Posicionar texto do certificado
                    XRect retanguloTextoCertificado = new XRect(page.Width / 6, page.Height / 2.5, page.Width / 1.5, page.Height / 2);
                    gfx.DrawString(textoCertificado, fontTextoCertificado, XBrushes.Black, retanguloTextoCertificado, XStringFormats.Center);

                    // Posicionar texto de autenticidade
                    XRect retanguloTextoAutenticidade = new XRect(10, page.Height - 120, 1500, 50);
                    gfx.DrawString(textoAutenticidade, fontTextoAutenticidade, XBrushes.Black, retanguloTextoAutenticidade, XStringFormats.TopLeft);

                    // Posicionar QR Code
                    using (var qrCodeImage = SixLabors.ImageSharp.Image.Load<Rgba32>(caminhoQRCode))
                    {
                        var qrCodePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
                        qrCodeImage.Save(qrCodePath, new SixLabors.ImageSharp.Formats.Png.PngEncoder());

                        using (var xQrCodeImage = XImage.FromFile(qrCodePath))
                        {
                            XRect rectQRCode = new XRect(page.Width - 280, page.Height - 260, 180, 180);
                            gfx.DrawImage(xQrCodeImage, rectQRCode);
                        }

                        // Deletar o arquivo temporário do QR Code
                        File.Delete(qrCodePath);
                    }
                }

                // Salvar o documento PDF em bytes
                byte[] pdfBytes;
                using (var memoryStream = new MemoryStream())
                {
                    pdfDocument.Save(memoryStream);
                    pdfBytes = memoryStream.ToArray();
                }

                //TESTES: Salva o PDF na pasta raiz do projeto
                //File.WriteAllBytes("path_to_save_certificate.pdf", pdfBytes);

                // Salvar o PDF em algum armazenamento ou banco de dados
                await InserirAsync(idEvento_Pessoa, pdfBytes, codigoCertificado);

                retorno = true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [CertificadosService.GerarCertificadoAsync]: {ex.Message}");
            }

            return retorno;
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

                        //texto = $@"{texto}";
                    }
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
