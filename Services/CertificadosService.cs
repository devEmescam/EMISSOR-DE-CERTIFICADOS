using DocumentFormat.OpenXml.Drawing;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using QRCoder;
using System.IO;
using EMISSOR_DE_CERTIFICADOS.Controllers;
using EMISSOR_DE_CERTIFICADOS.DBConnections;
using Microsoft.AspNetCore.Hosting;

namespace EMISSOR_DE_CERTIFICADOS.Services
{
    public class CertificadosService
    {        
        private readonly DBHelpers _dbHelper;

        public CertificadosService(DBHelpers dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper), "O DBHelpers não pode ser nulo.");            
        }

        //Essa versão não aplica negrito nas informações necessárias do texto1       
        public bool GerarCertificado_SEM_NEGRITO(int idPessoa, string textoOriginal, IFormFile imagem)
        {
            string textoCertificado = string.Empty;
            string texto2 = "";
            bool retorno = false;

            try
            {
                textoCertificado = ProcessarTextoCertificadoPessoa(idPessoa, textoOriginal);
                texto2 = RetornaTexto2(idPessoa, textoCertificado);
                // Caminho relativo para a imagem do QR Code no diretório wwwroot
                string caminhoQRCode = System.IO.Path.Combine("wwwroot", "QRCodeEmescam.png");

                // Carregar o arquivo IFormFile diretamente
                using (var memoryStream = new MemoryStream())
                {
                    imagem.CopyTo(memoryStream);
                    using (Bitmap certificado = new Bitmap(memoryStream))
                    {
                        // Configurar a fonte e o tamanho do texto1
                        using (System.Drawing.Font fonteTexto1 = new System.Drawing.Font("Arial", 48, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel))
                        {
                            // Criar um objeto de gráficos a partir do certificado
                            using (Graphics graphics = Graphics.FromImage(certificado))
                            {
                                // Configurar o alinhamento central horizontal
                                StringFormat formatoCentralizado = new StringFormat
                                {
                                    Alignment = StringAlignment.Center
                                };

                                // Configurar a região central para o texto1
                                RectangleF retanguloTexto1 = new RectangleF(certificado.Width / 6, certificado.Height / 2.5f, certificado.Width / 1.5f, certificado.Height / 2); // 2970px X 2100px

                                // Desenhar o texto1 na região central
                                graphics.DrawString(textoCertificado, fonteTexto1, Brushes.Black, retanguloTexto1, formatoCentralizado);

                                // Configurar a fonte e o tamanho do texto2
                                using (System.Drawing.Font fonteTexto2 = new System.Drawing.Font("Arial", 24, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel))
                                {
                                    // Configurar a região inferior esquerda para o texto2
                                    RectangleF retanguloTexto2 = new RectangleF(10, certificado.Height - 120, 1500, 50);

                                    // Desenhar o texto2 na região inferior esquerda
                                    graphics.DrawString(texto2, fonteTexto2, Brushes.Black, retanguloTexto2);

                                    // Configurar a região inferior direita para o QRCode
                                    Bitmap qrCodeBitmap = new Bitmap(caminhoQRCode);
                                    System.Drawing.Rectangle retanguloQRCode = new System.Drawing.Rectangle(certificado.Width - 280, certificado.Height - 260, 180, 180);

                                    // Desenhar o QRCode na região inferior direita
                                    graphics.DrawImage(qrCodeBitmap, retanguloQRCode);
                                }
                            }
                        }

                        // Salvar o certificado editado
                        string caminhoCertificadoEditado = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "CertificadoEditado.jpg");
                        certificado.Save(caminhoCertificadoEditado, ImageFormat.Jpeg);

                        retorno = true;

                        // Converter o arquivo JPG para PDF
                        //ConverterParaPdf(caminhoCertificadoEditado);
                    }
                }

                return retorno;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [CertificadosService.GerarCertificado_SEM_NEGRITO]: {ex.Message}");
            }
        }

        ////Essa versão exibe como resultado final cada palavra do texto1 em uma linha, precisa ser melhorado 
        //public static void GerarCertificado_COM_NEGRITO(string caminhoCertificado, string nomePessoa, string emailPessoa, string cpfPessoa)
        //{
        //    string texto1 = "";
        //    string texto2 = "";
        //    string caminhoQRCode = "";

        //    try
        //    {
        //        //texto1 = RetornaTexto1(nomePessoa, cpfPessoa);
        //        texto1 = RetornaTexto1();
        //        texto2 = RetornaTexto2(cpfPessoa);
        //        caminhoQRCode = GerarQRCode();

        //        // Carregar o arquivo JPG
        //        using (Bitmap certificado = new Bitmap(caminhoCertificado))
        //        {
        //            // Configurar a fonte e o tamanho do texto1 com alinhamento central
        //            using (System.Drawing.Font fonteTexto1 = new System.Drawing.Font("Arial", 48, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel))
        //            {
        //                // Criar um objeto de gráficos a partir do certificado
        //                using (Graphics graphics = Graphics.FromImage(certificado))
        //                {
        //                    // Configurar o alinhamento central horizontal
        //                    StringFormat formatoCentralizado = new StringFormat();
        //                    formatoCentralizado.Alignment = StringAlignment.Center;

        //                    // Configurar a região central para o texto1
        //                    RectangleF retanguloTexto1 = new RectangleF(certificado.Width / 6, certificado.Height / 2.5f, certificado.Width / 1.5f, certificado.Height / 2); // 2970px X 2100px

        //                    // Dividir o texto em partes para aplicar estilos diferentes
        //                    string[] partesTexto1 = texto1.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

        //                    // Posição Y inicial para o desenho do texto
        //                    float y = retanguloTexto1.Top;

        //                    // Desenhar cada parte do texto com o estilo apropriado
        //                    foreach (string parte in partesTexto1)
        //                    {
        //                        if (parte.Contains("<pessoa>"))
        //                        {
        //                            string pessoa = parte.Replace("<pessoa>", nomePessoa);

        //                            // Configurar a fonte em negrito
        //                            using (System.Drawing.Font fonteNegrito = new System.Drawing.Font(fonteTexto1, System.Drawing.FontStyle.Bold))
        //                            {
        //                                // Desenhar a parte do texto em negrito
        //                                graphics.DrawString(pessoa, fonteNegrito, Brushes.Black, retanguloTexto1.Left, y, formatoCentralizado);
        //                            }
        //                        }
        //                        else if (parte.Contains("<cpf>"))
        //                        {
        //                            string cpf = parte.Replace("<cpf>", cpfPessoa);

        //                            // Configurar a fonte em negrito
        //                            using (System.Drawing.Font fonteNegrito = new System.Drawing.Font(fonteTexto1, System.Drawing.FontStyle.Bold))
        //                            {
        //                                // Desenhar a parte do texto em negrito
        //                                graphics.DrawString(cpf, fonteNegrito, Brushes.Black, retanguloTexto1.Left, y, formatoCentralizado);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            // Desenhar a parte do texto normal
        //                            graphics.DrawString(parte, fonteTexto1, Brushes.Black, retanguloTexto1.Left, y, formatoCentralizado);
        //                        }

        //                        // Atualizar a posição Y para a próxima linha
        //                        y += fonteTexto1.Height;
        //                    }

        //                    // Configurar a fonte e o tamanho do texto2
        //                    using (System.Drawing.Font fonteTexto2 = new System.Drawing.Font("Arial", 24, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel))
        //                    {
        //                        // Configurar a região inferior esquerda para o texto2
        //                        RectangleF retanguloTexto2 = new RectangleF(10, certificado.Height - 120, 1500, 50);

        //                        // Desenhar o texto2 na região inferior esquerda
        //                        graphics.DrawString(texto2, fonteTexto2, Brushes.Black, retanguloTexto2);

        //                        // Configurar a região inferior direita para o QRCode
        //                        Bitmap qrCodeBitmap = new Bitmap(caminhoQRCode);
        //                        System.Drawing.Rectangle retanguloQRCode = new System.Drawing.Rectangle(certificado.Width - 280, certificado.Height - 260, 180, 180);

        //                        // Desenhar o QRCode na região inferior direita
        //                        graphics.DrawImage(qrCodeBitmap, retanguloQRCode);
        //                    }
        //                }
        //            }

        //            // Salvar o certificado editado
        //            string caminhoCertificadoEditado = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(caminhoCertificado), "CertificadoEditado.jpg");
        //            certificado.Save(caminhoCertificadoEditado, ImageFormat.Jpeg);

        //            // Converter o arquivo JPG para PDF
        //            ConverterParaPdf(caminhoCertificadoEditado);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Erro em CertificadoEditor.EditarCertificado: {ex.Message}");
        //    }
        //}

        // TESTES para resolver conteudo do texto1 com negrito e respeitando a formatação centralizada e espaço do retangulo1 
        //public static void GerarCertificado(string caminhoCertificado, string nomePessoa, string emailPessoa, string cpfPessoa)
        //{
        //    string texto1 = "";
        //    string texto2 = "";
        //    string caminhoQRCode = "";

        //    try
        //    {
        //        texto1 = RetornaTexto1();
        //        texto2 = RetornaTexto2(cpfPessoa);
        //        caminhoQRCode = GerarQRCode();

        //        // Carregar o arquivo JPG
        //        using (Bitmap certificado = new Bitmap(caminhoCertificado))
        //        {
        //            // Configurar a fonte e o tamanho do texto1
        //            using (System.Drawing.Font fonteTexto1 = new System.Drawing.Font("Arial", 48, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel))
        //            {
        //                // Criar um objeto de gráficos a partir do certificado
        //                using (Graphics graphics = Graphics.FromImage(certificado))
        //                {
        //                    // Configurar o alinhamento central horizontal
        //                    StringFormat formatoCentralizado = new StringFormat();
        //                    formatoCentralizado.Alignment = StringAlignment.Center;

        //                    // Configurar a região central para o texto1
        //                    RectangleF retanguloTexto1 = new RectangleF(certificado.Width / 6, certificado.Height / 2.5f, certificado.Width / 1.5f, certificado.Height / 2); // 2970px X 2100px

        //                    // Dividir o texto1 em partes para aplicar estilos diferentes
        //                    string[] partesTexto1 = texto1.Split(new string[] { "<nome>", "<cpf>" }, StringSplitOptions.None);

        //                    // Posição X inicial para o desenho do texto
        //                    float x = retanguloTexto1.Left;

        //                    // Posição Y inicial para o desenho do texto
        //                    float y = retanguloTexto1.Top;

        //                    // Desenhar cada parte do texto com o estilo apropriado
        //                    foreach (string parte in partesTexto1)
        //                    {
        //                        if (parte == "<nome>")
        //                        {

        //                            // Configurar a fonte em negrito
        //                            using (System.Drawing.Font fonteNegrito = new System.Drawing.Font("Arial", 48, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel))
        //                            {
        //                                // Desenhar a parte do texto em negrito
        //                                graphics.DrawString(nomePessoa, fonteNegrito, Brushes.Black, x, y, formatoCentralizado);
        //                            }
        //                        }
        //                        else if (parte == "<cpf>")
        //                        {
        //                            // Configurar a fonte em negrito
        //                            using (System.Drawing.Font fonteNegrito = new System.Drawing.Font("Arial", 48, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel))
        //                            {
        //                                // Desenhar a parte do texto em negrito
        //                                graphics.DrawString(cpfPessoa, fonteNegrito, Brushes.Black, x, y, formatoCentralizado);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            // Desenhar a parte do texto normal
        //                            graphics.DrawString(parte, fonteTexto1, Brushes.Black, x, y, formatoCentralizado);
        //                        }

        //                        // Atualizar a posição X para a próxima parte do texto
        //                        x += graphics.MeasureString(parte, fonteTexto1).Width;
        //                    }

        //                    // Configurar a fonte e o tamanho do texto2
        //                    using (System.Drawing.Font fonteTexto2 = new System.Drawing.Font("Arial", 24, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel))
        //                    {
        //                        // Configurar a região inferior esquerda para o texto2
        //                        RectangleF retanguloTexto2 = new RectangleF(10, certificado.Height - 120, 1500, 50);

        //                        // Desenhar o texto2 na região inferior esquerda
        //                        graphics.DrawString(texto2, fonteTexto2, Brushes.Black, retanguloTexto2);

        //                        // Configurar a região inferior direita para o QRCode
        //                        Bitmap qrCodeBitmap = new Bitmap(caminhoQRCode);
        //                        System.Drawing.Rectangle retanguloQRCode = new System.Drawing.Rectangle(certificado.Width - 280, certificado.Height - 260, 180, 180);

        //                        // Desenhar o QRCode na região inferior direita
        //                        graphics.DrawImage(qrCodeBitmap, retanguloQRCode);
        //                    }
        //                }
        //            }

        //            // Salvar o certificado editado
        //            string caminhoCertificadoEditado = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(caminhoCertificado), "CertificadoEditado.jpg");
        //            certificado.Save(caminhoCertificadoEditado, ImageFormat.Jpeg);

        //            // Converter o arquivo JPG para PDF
        //            ConverterParaPdf(caminhoCertificadoEditado);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Erro em CertificadoEditor.EditarCertificado: {ex.Message}");
        //    }
        //}      
        private string ProcessarTextoCertificadoPessoa(int idPessoa, string texto) 
        {            
            string nomePessoa = string.Empty;
            string cpf = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(texto)) 
                {
                    using (PessoaController pessoaController = new PessoaController(_dbHelper))
                    {
                        nomePessoa = pessoaController.ObterNomePorIdPessoa(idPessoa);
                        if (!string.IsNullOrEmpty(nomePessoa))
                        {
                            texto = texto.Replace("NOME_PESSOA", nomePessoa);
                        }

                        cpf = pessoaController.ObterCPFPorIdPessoa(idPessoa);
                        if (!string.IsNullOrEmpty(cpf))
                        {
                            texto = texto.Replace("CPF_PESSOA", cpf);
                        }
                    }
                }

                return texto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [CertificadoEditor.ProcessarTextoCertificadoPessoa]");
            }        
        }      
        private string RetornaTexto2(int idPessoa, string texto) //string cpfPessoa
        {
            string codigo = "";
            string texto2 = "";
            string cpf = "";

            try
            {
                using (PessoaController pessoaController = new PessoaController(_dbHelper)) 
                {
                    cpf = pessoaController.ObterCPFPorIdPessoa(idPessoa);
                    codigo = GerarCodigo(cpf + texto); // Usa o CPF e conteudo da variavel texto para compor o codigo que será gerado
                    texto2 = $"Código do Certificado: {codigo} - Verifique autenticidade em: definirUrl.com.br";
                }
                    
                return texto2;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em CertificadoEditor.RetornaTexto2: {ex.Message}");
            }
        }
        private static string GerarCodigo(string chave)
        {
            try
            {
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
        
        //public static string GerarQRCode_OLD()
        //{
        //    string caminhoQRCode = "QRCode.jpg";

        //    try
        //    {
        //        // Criar um objeto de configuração para o QRCode
        //        EncodingOptions options = new EncodingOptions
        //        {
        //            Width = 180, // Largura do QRCode em pixels
        //            Height = 180, // Altura do QRCode em pixels
        //            Margin = 0 // Margem ao redor do QRCode
        //        };

        //        BarcodeWriter writer = new BarcodeWriter
        //        {
        //            Format = BarcodeFormat.QR_CODE,
        //            Options = options,
        //            Renderer = new BitmapRenderer()
        //        };

        //        // Gerar o QRCode como um bitmap
        //        Bitmap qrCodeImage = writer.Write("http://emescam.br");

        //        // Salvar a imagem do QRCode
        //        qrCodeImage.Save(caminhoQRCode, ImageFormat.Jpeg);

        //        return caminhoQRCode;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Erro em CertificadoEditor.GerarQRCode: {ex.Message}");
        //    }
        //}
        //public static string GerarQRCode(string qrCodeText)
        //{
        //    string caminhoQRCode = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "QRCode.png");

        //    try
        //    {
        //        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        //        {
        //            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeText, QRCodeGenerator.ECCLevel.Q);
        //            QRCode qrCode = new QRCode(qrCodeData);

        //            // Parâmetros do QR Code
        //            int pixelPerModule = 20; // Tamanho do módulo em pixels
        //            Color darkColor = Color.Black; // Cor dos módulos escuros
        //            Color lightColor = Color.White; // Cor dos módulos claros
        //            bool drawQuietZones = true; // Desenhar margens (quiet zones)
        //            Bitmap qrCodeImage = qrCode.GetGraphic(pixelPerModule, darkColor, lightColor, drawQuietZones);

        //            // Redimensionar a imagem para 180x180 pixels
        //            Bitmap resizedQrCodeImage = new Bitmap(qrCodeImage, new Size(180, 180));

        //            // Salva a imagem do QR Code como um arquivo PNG
        //            resizedQrCodeImage.Save(caminhoQRCode, ImageFormat.Png);
        //        }

        //        return caminhoQRCode;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Erro em CertificadosService.GerarQRCode: {ex.Message}");
        //    }
        //}
        //private static void ConverterParaPdf(string caminhoCertificado)
        //{
        //    try
        //    {
        //        string nomeArquivoPdf = Path.Combine(@"C:\Users\lucas.guimaraes\Documents\LUCAS\PROJETOS\PROJETO EDITOR DE CERTIFICADOS EMESCAM", "CertificadoEditado.pdf");

        //        using (Bitmap bmp = new Bitmap(caminhoCertificado))
        //        {
        //            using (Document pdfDoc = new Document(new iTextSharp.text.Rectangle(0, 0, bmp.Width, bmp.Height)))
        //            {
        //                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, new FileStream(nomeArquivoPdf, FileMode.Create));

        //                // Abre o documento PDF
        //                pdfDoc.Open();

        //                int numberOfPages = bmp.GetFrameCount(FrameDimension.Page);

        //                for (int i = 0; i < numberOfPages; i++)
        //                {
        //                    bmp.SelectActiveFrame(FrameDimension.Page, i);

        //                    iTextSharp.text.Image imagem = iTextSharp.text.Image.GetInstance(bmp, ImageFormat.Jpeg);

        //                    // Adiciona a imagem diretamente ao documento PDF
        //                    pdfDoc.NewPage();
        //                    pdfDoc.Add(imagem);
        //                }
        //            }
        //        }

        //        // Exclui o arquivo JPG após a conversão para PDF
        //        File.Delete(caminhoCertificado);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Erro em CertificadoEditor.ConverterParaPdf: {ex.Message}");
        //    }
        //}

    }
}
