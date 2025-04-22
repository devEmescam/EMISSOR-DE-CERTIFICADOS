
using EMISSOR_DE_CERTIFICADOS.Interfaces;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas;
using iText.Layout.Properties;
using System.Text;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using System.Text.RegularExpressions;

namespace EMISSOR_DE_CERTIFICADOS.Services
{
    internal class CertificadosService : ICertificadosService
    {        
        private readonly IPessoaService _pessoaService;
        private readonly ICertificadosRepository _certiificadosRepository;

        public CertificadosService(IPessoaService pessoaService, ICertificadosRepository certificadosRepository)
        {            
            _pessoaService = pessoaService ?? throw new ArgumentNullException(nameof(pessoaService), "O IPessoaService não ser nulo.");
            _certiificadosRepository = certificadosRepository ?? throw new ArgumentNullException(nameof(certificadosRepository), "O ICertificadosRepository não ser nulo.");
        }       
        public async Task<bool> GerarCertificadoAsync_NEGRITO_TudoEmLinhaUnica(int idEvento_Pessoa, int idPessoa, string textoOriginal, IFormFile imagem)
        {
            string textoAutenticidade = string.Empty;
            string codigoCertificado = string.Empty;
            string caminhoQRCode = string.Empty;
            bool retorno = false;

            try
            {
                // Gerar código do certificado e texto de autenticidade
                codigoCertificado = await GerarCodigo(idPessoa);
                textoAutenticidade = RetornarTextoAutenticidade(codigoCertificado);
                caminhoQRCode = System.IO.Path.Combine("wwwroot", "QRCodeEmescam.png");

                // Criar stream separado para o PDF
                using (var pdfStream = new MemoryStream())
                {
                    // Inicializar o PDF em memória
                    using (var imageStream = new MemoryStream())
                    {
                        // Copiar a arte do certificado
                        await imagem.CopyToAsync(imageStream);
                        imageStream.Position = 0;

                        // Criar o documento PDF
                        using (var pdfWriter = new PdfWriter(pdfStream))
                        using (var pdfDocument = new PdfDocument(pdfWriter))
                        {
                            // Definir tamanho da página como A4 e orientação Paisagem
                            pdfDocument.SetDefaultPageSize(PageSize.A4.Rotate());                            

                            // Criar um documento sem margens padrão
                            var document = new Document(pdfDocument, PageSize.A4.Rotate(), false);
                            document.SetMargins(0, 0, 0, 0); // Remove qualquer margem extra

                            // Criar imagem corretamente a partir do novo stream, ocupando toda a página
                            var imageData = ImageDataFactory.Create(imageStream.ToArray());
                            var image = new iText.Layout.Element.Image(imageData)
                                .ScaleToFit(pdfDocument.GetDefaultPageSize().GetWidth(), pdfDocument.GetDefaultPageSize().GetHeight()) // Ajusta para ocupar todo o espaço
                                .SetFixedPosition(0, 0); // Posiciona no canto inferior esquerdo (0,0)
                            document.Add(image);                                                                                    

                            // Processar e adicionar o texto principal
                            var partesTexto = await ProcessarTextoEstilizadoAsync(textoOriginal, idPessoa);
                            //Definir o tipo de fonte
                            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                            //Definir o tamanho da fonte
                            var paragraph = new Paragraph().SetFontSize(14);
                            // Alinhamento padrão do texto
                            TextAlignment ultimoAlinhamento = TextAlignment.CENTER;

                            // Adicionar o texto no PDF
                            foreach (var (textoParte, estilo, alinhamento) in partesTexto)
                            {
                                // Criar um objeto 'Text' para cada textoParte
                                var text = new Text(textoParte).SetFont(estilo).SetFontColor(new DeviceRgb(50, 50, 50)); 

                                //Validar se precisa aplicar o negrito
                                if (estilo.ToString().Contains("Bold"))
                                {
                                    text.SetBold(); 
                                }

                                // Adicionamos ao mesmo parágrafo
                                paragraph.Add(text);

                                // Atualiza a variável com o último alinhamento encontrado
                                ultimoAlinhamento = alinhamento; 
                            }

                            // Após o loop, adicionamos o parágrafo ao documento
                            float posicaoX = 100;  // Ajuste conforme necessário
                            // Define a distancia entre margem superior e inicio do texto
                            float posicaoY = 265;
                            // Define a largura maxima da linha do texto
                            float largura = 650;

                            //Seta os posicionamentos definidos no paragrafo
                            paragraph.SetFixedPosition(posicaoX, posicaoY, largura);
                            // Seta o alinhamento definido
                            paragraph.SetTextAlignment(ultimoAlinhamento); 
                            //Adiciona o paragrafo no documento
                            document.Add(paragraph);

                            //Definir tamanho do QrCode
                            float tamanhoQrCode = 65; 
                            // Definir margens para posicionamento
                            float margemDireita = 38;
                            float margemInferior = 34;

                            // Adicionar o QR Code: POSICIONADO NA PARTE INFERIOR DIREITA 'DENTRO' DA ARTE PROXIMOS AS BORDAS
                            var qrCodeImage = ImageDataFactory.Create(caminhoQRCode);
                            var qrCode = new Image(qrCodeImage)
                                .ScaleToFit(tamanhoQrCode, tamanhoQrCode) // Define o tamanho do QR Code
                                .SetFixedPosition(
                                    pdfDocument.GetDefaultPageSize().GetWidth() - tamanhoQrCode - margemDireita, // Alinha à direita
                                    margemInferior // Posiciona no rodapé
                                );

                            document.Add(qrCode);

                            // Adicionar texto de autenticidade: POSICIONADO NA PARTE INFERIOR ESQUERDA ABAIXO DA BORDA DA ARTE
                            var autenticidadeParagraph = new Paragraph(textoAutenticidade)
                                .SetFont(font)
                                .SetFontSize(8)
                                .SetFixedPosition(35, 15, 600); // Ajuste a posição conforme necessário
                            document.Add(autenticidadeParagraph);

                            // Fechar o documento PDF
                            document.Close();
                        }

                        // Salvar o arquivo PDF gerado
                        byte[] certificadoBytes = pdfStream.ToArray();
                        await InserirAsync(idEvento_Pessoa, certificadoBytes, codigoCertificado);

                        // Salvar o arquivo localmente em C:\Temp
                        string caminhoDiretorio = @"C:\Temp";
                        if (!Directory.Exists(caminhoDiretorio))
                        {
                            Directory.CreateDirectory(caminhoDiretorio);
                        }

                        string caminhoArquivo = System.IO.Path.Combine(caminhoDiretorio, $"Certificado_{codigoCertificado}.pdf");
                        await File.WriteAllBytesAsync(caminhoArquivo, certificadoBytes);

                        retorno = true;
                    }
                }                
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [CertificadosService.GerarCertificadoAsync]: {ex.Message}");
            }

            return retorno;
        }      
        public async Task<bool> GerarCertificadoAsync(int idEvento_Pessoa, int idPessoa, string textoOriginal, IFormFile imagem)
        {
            string textoAutenticidade = string.Empty;
            string codigoCertificado = string.Empty;
            string caminhoQRCode = string.Empty;
            bool retorno = false;

            try
            {
                // Gerar código do certificado e texto de autenticidade
                codigoCertificado = await GerarCodigo(idPessoa);
                textoAutenticidade = RetornarTextoAutenticidade(codigoCertificado);
                caminhoQRCode = System.IO.Path.Combine("wwwroot", "QRCodeEmescam.png");

                // Buscar informações do participante
                string nomePessoa = await _pessoaService.ObterNomePorIdPessoaAsync(idPessoa);
                string cpfPessoa = await _pessoaService.ObterCPFPorIdPessoaAsync(idPessoa);

                // Substituir marcadores por valores reais
                textoOriginal = textoOriginal.Replace("NOME_PESSOA", nomePessoa).Replace("CPF_PESSOA", cpfPessoa);

                // Criar stream separado para o PDF
                using (var pdfStream = new MemoryStream())
                {
                    // Inicializar o PDF em memória
                    using (var imageStream = new MemoryStream())
                    {
                        // Copiar a arte do certificado
                        await imagem.CopyToAsync(imageStream);
                        imageStream.Position = 0;

                        // Criar o documento PDF
                        using (var pdfWriter = new PdfWriter(pdfStream))
                        using (var pdfDocument = new PdfDocument(pdfWriter))
                        {
                            // Definir tamanho da página como A4 e orientação Paisagem
                            pdfDocument.SetDefaultPageSize(PageSize.A4.Rotate());

                            // Criar um documento sem margens padrão
                            var document = new Document(pdfDocument, PageSize.A4.Rotate(), false);
                            document.SetMargins(0, 0, 0, 0); // Remove qualquer margem extra

                            // Criar imagem corretamente a partir do novo stream, ocupando toda a página
                            var imageData = ImageDataFactory.Create(imageStream.ToArray());
                            var image = new iText.Layout.Element.Image(imageData)
                                .ScaleToFit(pdfDocument.GetDefaultPageSize().GetWidth(), pdfDocument.GetDefaultPageSize().GetHeight()) // Ajusta para ocupar todo o espaço
                                .SetFixedPosition(0, 0); // Posiciona no canto inferior esquerdo (0,0)
                            document.Add(image);

                            // *** TEXTO CERTIFICADO -- INICIO ***
                            //// Posição e largura do texto
                            //float posicaoX = 100; // distância a partir da margem esquerda
                            //float posicaoY = 165; // distância a partir da margem inferior
                            //float largura = 650; // largura máxima do parágrafo

                            //// Criar o parágrafo e definir tamanho da fonte
                            //var paragraph = new Paragraph().SetFontSize(14);

                            //// Fonte normal e fonte em negrito
                            //var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                            //var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                            //// Alinhamento padrão do texto
                            //TextAlignment ultimoAlinhamento = TextAlignment.CENTER;

                            //// Processar o texto e aplicar formatação
                            //var regex = new Regex(@"<b>(.*?)<\/b>", RegexOptions.Singleline);
                            //var equivalencias = regex.Matches(textoOriginal);
                            //int ultimoIndex = 0;

                            //foreach (Match equivalencia in equivalencias)
                            //{
                            //    // Adicionar o texto antes da marcação <b>, se houver
                            //    if (equivalencia.Index > ultimoIndex)
                            //    {
                            //        var textoAntes = textoOriginal.Substring(ultimoIndex, equivalencia.Index - ultimoIndex);
                            //        paragraph.Add(new Text(textoAntes).SetFont(font).SetFontColor(new DeviceRgb(50, 50, 50)));
                            //    }

                            //    // Adicionar o texto dentro da marcação <b> com negrito
                            //    var textoNegrito = equivalencia.Groups[1].Value;
                            //    paragraph.Add(new Text(textoNegrito).SetFont(fontBold).SetFontColor(new DeviceRgb(50, 50, 50)));
                            //    ultimoIndex = equivalencia.Index + equivalencia.Length;
                            //}

                            //// Adicionar qualquer texto restante após a última marcação <b>
                            //if (ultimoIndex < textoOriginal.Length)
                            //{
                            //    var textoRestante = textoOriginal.Substring(ultimoIndex);
                            //    paragraph.Add(new Text(textoRestante).SetFont(font).SetFontColor(new DeviceRgb(50, 50, 50)));
                            //}

                            //// Definir posição e alinhamento do parágrafo
                            //paragraph.SetFixedPosition(posicaoX, posicaoY, largura);
                            //paragraph.SetTextAlignment(ultimoAlinhamento);
                            //document.Add(paragraph);
                            float posicaoX = 100; // distância a partir da margem esquerda
                            float posicaoY = 165; // distância a partir da margem inferior
                            float largura = 650; // largura máxima do parágrafo

                            // Criar a fonte normal e fonte em negrito
                            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                            var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                            // Alinhamento padrão do texto
                            TextAlignment alinhamentoAtual = TextAlignment.CENTER;

                            // Expressões regulares para detectar marcações de alinhamento e negrito
                            var regexAlinhamento = new Regex(@"<(center|left|right|justify)>(.*?)<\/\1>", RegexOptions.Singleline);
                            var regexNegrito = new Regex(@"<b>(.*?)<\/b>", RegexOptions.Singleline);

                            int ultimoIndex = 0;
                            var paragrafoAtual = new Paragraph().SetFontSize(14);

                            // Processar o texto identificando trechos alinhados e sem marcação
                            var matches = regexAlinhamento.Matches(textoOriginal);
                            List<(string texto, TextAlignment alinhamento)> blocos = new();

                            foreach (Match match in matches)
                            {
                                // Adicionar o trecho sem marcação de alinhamento antes do atual
                                if (match.Index > ultimoIndex)
                                {
                                    string textoSemAlinhamento = textoOriginal.Substring(ultimoIndex, match.Index - ultimoIndex);
                                    blocos.Add((textoSemAlinhamento, TextAlignment.CENTER)); // Padrão centralizado
                                }

                                // Determinar o alinhamento do bloco atual
                                TextAlignment alinhamento = match.Groups[1].Value switch
                                {
                                    "left" => TextAlignment.LEFT,
                                    "right" => TextAlignment.RIGHT,
                                    "justify" => TextAlignment.JUSTIFIED,
                                    _ => TextAlignment.CENTER
                                };

                                string textoAlinhado = match.Groups[2].Value;
                                blocos.Add((textoAlinhado, alinhamento));
                                ultimoIndex = match.Index + match.Length;
                            }

                            // Adicionar qualquer texto restante após o último bloco identificado
                            if (ultimoIndex < textoOriginal.Length)
                            {
                                string textoRestante = textoOriginal.Substring(ultimoIndex);
                                blocos.Add((textoRestante, TextAlignment.CENTER)); // Padrão centralizado
                            }

                            // Processar cada bloco identificado
                            foreach (var (texto, alinhamento) in blocos)
                            {
                                var paragrafo = new Paragraph().SetFontSize(14).SetTextAlignment(alinhamento);

                                int indexNegrito = 0;
                                var negritos = regexNegrito.Matches(texto);

                                foreach (Match negrito in negritos)
                                {
                                    // Adicionar texto antes da marcação <b>, se houver
                                    if (negrito.Index > indexNegrito)
                                    {
                                        string textoAntes = texto.Substring(indexNegrito, negrito.Index - indexNegrito);
                                        paragrafo.Add(new Text(textoAntes).SetFont(font).SetFontColor(new DeviceRgb(50, 50, 50)));
                                    }

                                    // Adicionar texto em negrito
                                    string textoNegrito = negrito.Groups[1].Value;
                                    paragrafo.Add(new Text(textoNegrito).SetFont(fontBold).SetFontColor(new DeviceRgb(50, 50, 50)));

                                    indexNegrito = negrito.Index + negrito.Length;
                                }

                                // Adicionar qualquer texto restante após a última marcação <b>
                                if (indexNegrito < texto.Length)
                                {
                                    string textoRestante = texto.Substring(indexNegrito);
                                    paragrafo.Add(new Text(textoRestante).SetFont(font).SetFontColor(new DeviceRgb(50, 50, 50)));
                                }

                                // Adicionar o parágrafo ao documento
                                paragrafo.SetFixedPosition(posicaoX, posicaoY, largura);
                                document.Add(paragrafo);

                                // Ajustar a posição para o próximo bloco (exemplo: movendo para cima)
                                posicaoY -= 20;
                            }


                            // *** TEXTO CERTIFICADO -- FIM ***

                            // *** QRCODE -- INICIO ***
                            //Definir tamanho do QrCode
                            float tamanhoQrCode = 65;
                            // Definir margens para posicionamento
                            float margemDireita = 38;
                            float margemInferior = 34;

                            // Adicionar o QR Code: POSICIONADO NA PARTE INFERIOR DIREITA 'DENTRO' DA ARTE PROXIMOS AS BORDAS
                            var qrCodeImage = ImageDataFactory.Create(caminhoQRCode);
                            var qrCode = new Image(qrCodeImage)
                                .ScaleToFit(tamanhoQrCode, tamanhoQrCode) // Define o tamanho do QR Code
                                .SetFixedPosition(
                                    pdfDocument.GetDefaultPageSize().GetWidth() - tamanhoQrCode - margemDireita, // Alinha à direita
                                    margemInferior // Posiciona no rodapé
                                );

                            document.Add(qrCode);
                            // *** QRCODE -- FIM ***

                            // *** TEXTO AUTENTICIDADE RODAPÉ -- INICIO ***
                            // Adicionar texto de autenticidade: POSICIONADO NA PARTE INFERIOR ESQUERDA ABAIXO DA BORDA DA ARTE
                            var autenticidadeParagraph = new Paragraph(textoAutenticidade)
                                .SetFont(font)
                                .SetFontSize(8)
                                .SetFixedPosition(35, 15, 600); // Ajuste a posição conforme necessário
                            document.Add(autenticidadeParagraph);
                            // *** TEXTO AUTENTICIDADE RODAPÉ -- FIM ***

                            // Fechar o documento PDF
                            document.Close();
                        }

                        // Salvar o arquivo PDF gerado
                        byte[] certificadoBytes = pdfStream.ToArray();
                        await InserirAsync(idEvento_Pessoa, certificadoBytes, codigoCertificado);

                        // Salvar o arquivo localmente
                        string caminhoDiretorio = @"C:\Temp";
                        if (!Directory.Exists(caminhoDiretorio))
                        {
                            Directory.CreateDirectory(caminhoDiretorio);
                        }

                        string caminhoArquivo = System.IO.Path.Combine(caminhoDiretorio, $"Certificado_{codigoCertificado}.pdf");
                        await File.WriteAllBytesAsync(caminhoArquivo, certificadoBytes);

                        retorno = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [CertificadosService.GerarCertificadoAsync]: {ex.Message}");
            }

            return retorno;
        }
        private async Task<List<(string texto, PdfFont estilo, iText.Layout.Properties.TextAlignment alinhamento)>> ProcessarTextoEstilizadoAsync(string texto, int idPessoa)
        {

            try
            {
                var partes = new List<(string texto, PdfFont estilo, iText.Layout.Properties.TextAlignment alinhamento)>();
                var builder = new StringBuilder();
                PdfFont estiloAtual = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                iText.Layout.Properties.TextAlignment alinhamentoAtual = iText.Layout.Properties.TextAlignment.CENTER;

                // Buscar informações do participante
                string nomePessoa = await _pessoaService.ObterNomePorIdPessoaAsync(idPessoa);
                string cpfPessoa = await _pessoaService.ObterCPFPorIdPessoaAsync(idPessoa);

                // Substituir marcadores por valores reais
                texto = texto.Replace("NOME_PESSOA", nomePessoa);
                texto = texto.Replace("CPF_PESSOA", cpfPessoa);

                // Dividir o texto em parágrafos usando a marcação <p>
                var paragrafos = texto.Split(new[] { "<p>", "</p>" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var paragrafo in paragrafos)
                {
                    if (string.IsNullOrWhiteSpace(paragrafo))
                    {
                        // Adiciona um espaço extra para separar parágrafos
                        partes.Add(("\n", estiloAtual, alinhamentoAtual));
                        continue;
                    }

                    for (int i = 0; i < paragrafo.Length; i++)
                    {
                        if (paragrafo.Substring(i).StartsWith("<b>"))
                        {
                            if (builder.Length > 0)
                            {
                                partes.Add((builder.ToString(), estiloAtual, alinhamentoAtual));
                                builder.Clear();
                            }
                            estiloAtual = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                            i += 2; // Avança o índice para ignorar "<b>"
                        }
                        else if (paragrafo.Substring(i).StartsWith("</b>"))
                        {
                            if (builder.Length > 0)
                            {
                                partes.Add((builder.ToString(), estiloAtual, alinhamentoAtual));
                                builder.Clear();
                            }
                            estiloAtual = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                            i += 3; // Avança o índice para ignorar "</b>"
                        }
                        else if (paragrafo.Substring(i).StartsWith("<i>"))
                        {
                            if (builder.Length > 0)
                            {
                                partes.Add((builder.ToString(), estiloAtual, alinhamentoAtual));
                                builder.Clear();
                            }
                            estiloAtual = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);
                            i += 2; // Avança o índice para ignorar "<i>"
                        }
                        else if (paragrafo.Substring(i).StartsWith("</i>"))
                        {
                            if (builder.Length > 0)
                            {
                                partes.Add((builder.ToString(), estiloAtual, alinhamentoAtual));
                                builder.Clear();
                            }
                            estiloAtual = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                            i += 3; // Avança o índice para ignorar "</i>"
                        }
                        else
                        {
                            builder.Append(paragrafo[i]);
                        }
                    }

                    if (builder.Length > 0)
                    {
                        partes.Add((builder.ToString(), estiloAtual, alinhamentoAtual));
                        builder.Clear();
                    }

                    // Não adicionar quebras de linha extras entre segmentos de um mesmo parágrafo
                }

                // Unificar texto dentro de um parágrafo
                var paragrafoUnificado = new List<(string texto, PdfFont estilo, iText.Layout.Properties.TextAlignment alinhamento)>();
                foreach (var parte in partes)
                {
                    if (paragrafoUnificado.Count > 0 &&
                        paragrafoUnificado[^1].alinhamento == parte.alinhamento &&
                        paragrafoUnificado[^1].estilo == parte.estilo)
                    {
                        // Concatenar texto com o mesmo estilo e alinhamento
                        var ultimaParte = paragrafoUnificado[^1];
                        paragrafoUnificado[^1] = (ultimaParte.texto + parte.texto, ultimaParte.estilo, ultimaParte.alinhamento);
                    }
                    else
                    {
                        paragrafoUnificado.Add(parte);
                    }
                }

                // Retornar o resultado unificado
                return paragrafoUnificado;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em CertificadosService.ProcessarTextoEstilizadoAsync: {ex.Message}");
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
                throw new Exception($"Erro em CertificadosService.RetornarTextoAutenticidade: {ex.Message}");
            }
        }
        private async Task<string> GerarCodigo(int idPessoa)
        {
            string chave = string.Empty;
            string cpf = string.Empty;
            try
            {
                cpf = await _pessoaService.ObterCPFPorIdPessoaAsync(idPessoa);

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
        private async Task InserirAsync(int idEventoPessoa, byte[] certificadoBytes, string codigoCertificado)
        {
            try
            {
                await _certiificadosRepository.InserirAsync(idEventoPessoa, certificadoBytes, codigoCertificado);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [CertificadosService.Inserir]: {ex.Message}");
            }
        }
    }
}