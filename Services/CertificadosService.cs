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
    internal class CertificadosService : ICertificadosService
    {
        private readonly IDBHelpers _dbHelper;
        private readonly ISessao _sessao;
        private readonly IPessoaEventosRepository _pessoaEventosRepository;
        private readonly IPessoaService _pessoaService;
        private readonly ICertificadosRepository _certiificadosRepository;

        public CertificadosService(IDBHelpers dbHelper, ISessao sessao, IPessoaEventosRepository pessoaEventosRepository, IPessoaService pessoaService, ICertificadosRepository certificadosRepository)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper), "O DBHelpers não pode ser nulo.");
            _sessao = sessao ?? throw new ArgumentNullException(nameof(sessao), "O ISessao não pode ser nulo.");
            _pessoaEventosRepository = pessoaEventosRepository ?? throw new ArgumentNullException(nameof(pessoaEventosRepository), "O PessoaEventosRepository não pode ser nulo.");
            _pessoaService = pessoaService ?? throw new ArgumentNullException(nameof(pessoaService), "O IPessoaService não ser nulo.");
            _certiificadosRepository = certificadosRepository ?? throw new ArgumentNullException(nameof(certificadosRepository), "O ICertificadosRepository não ser nulo.");
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
                var resultado = await ProcessarTextoCertificadoAsync(idPessoa, textoOriginal);
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
                        using (Font fontTextCertLocaleData = new Font("Arial", 36, FontStyle.Regular, GraphicsUnit.Pixel)) // tamanho original: 30
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
                            //TEXTO CERTIFICADO
                            float posicaoVertical = certificado.Height / 2.1f;
                            float altRetTextCertificado = 200;
                            RectangleF retTextCertificado = new RectangleF(margemLateral, posicaoVertical, certificado.Width - 2 * margemLateral, altRetTextCertificado);
                            // Ajustar tamanho da fonte do texto do certificado
                            float tamanhoFonteTextoCertificado = 48; //tamanho original 42
                            Font fonteTextoCertificado = new Font("Arial", tamanhoFonteTextoCertificado, FontStyle.Regular, GraphicsUnit.Pixel);
                            SizeF tamanhoTextoCertificado = graphics.MeasureString(textoCertificado, fonteTextoCertificado, (int)retTextCertificado.Width);
                            //Ajusta o texto dentro do retangulo diminuindo o tamanho da fonte se necessário
                            while ((tamanhoTextoCertificado.Width > retTextCertificado.Width || tamanhoTextoCertificado.Height > retTextCertificado.Height) && tamanhoFonteTextoCertificado > 10)
                            {
                                tamanhoFonteTextoCertificado -= 1;
                                fonteTextoCertificado = new Font("Arial", tamanhoFonteTextoCertificado, FontStyle.Regular, GraphicsUnit.Pixel);
                                tamanhoTextoCertificado = graphics.MeasureString(textoCertificado, fonteTextoCertificado, (int)retTextCertificado.Width);
                            }
                            // USADO APENAS PARA NECESSIDADE DE AJUSTES: Desenha o retângulo com borda
                            //graphics.DrawRectangle(penBorda, retTextCertificado.X, retTextCertificado.Y, retTextCertificado.Width, retTextCertificado.Height);
                            // Desenha o texto dentro do retângulo
                            graphics.DrawString(textoCertificado, fonteTextoCertificado, pincelTextoCertificado, retTextCertificado, alinhamentoTexto);
                            //===================================================================================== FIM

                            //===================================================================================== INICIO
                            //NOME PARTICIPANTE                                                        
                            float altRetNomeParticipante = 85;
                            RectangleF retNomeParticipante = new RectangleF(margemLateral, posicaoVertical - altRetNomeParticipante, certificado.Width - 2 * margemLateral, altRetNomeParticipante);
                            // Ajustar tamanho da fonte do nome do participante
                            float tamanhoFonteNomeParticipante = 66; // tamanho original 60
                            Font fonteNomeParticipante = new Font("Arial", tamanhoFonteNomeParticipante, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel);
                            SizeF tamanhoTextoNome = graphics.MeasureString(nomeParticipante, fonteNomeParticipante);
                            //Ajusta o texto dentro do retangulo diminuindo o tamanho da fonte se necessário
                            while (tamanhoTextoNome.Width > retNomeParticipante.Width && tamanhoFonteNomeParticipante > 10)
                            {
                                tamanhoFonteNomeParticipante -= 1;
                                fonteNomeParticipante = new Font("Arial", tamanhoFonteNomeParticipante, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel);
                                tamanhoTextoNome = graphics.MeasureString(nomeParticipante, fonteNomeParticipante);
                            }
                            // USADO APENAS PARA NECESSIDADE DE AJUSTES: Desenha o retângulo com borda
                            //graphics.DrawRectangle(penBorda, retNomeParticipante.X, retNomeParticipante.Y, retNomeParticipante.Width, retNomeParticipante.Height);
                            // Desenha o texto dentro do retângulo
                            graphics.DrawString(nomeParticipante, fonteNomeParticipante, pincelTextoCertificado, retNomeParticipante, alinhamentoTexto);
                            //===================================================================================== FIM

                            //===================================================================================== INICIO
                            // TEXTO ANTES DO NOME
                            float altRetTextFixo = graphics.MeasureString(textoAntesNomePessoa, fonteTextoFixo).Height;
                            float posVertTextFixo = retNomeParticipante.Y;
                            RectangleF retTextoFixo = new RectangleF(margemLateral, posVertTextFixo - altRetTextFixo, certificado.Width - 2 * margemLateral, altRetTextFixo);
                            // USADO APENAS PARA NECESSIDADE DE AJUSTES: Desenha o retângulo com borda
                            //graphics.DrawRectangle(penBorda, retTextoFixo.X, retTextoFixo.Y, retTextoFixo.Width, retTextoFixo.Height);
                            // Desenha o texto dentro do retângulo
                            graphics.DrawString(textoAntesNomePessoa, fonteTextoFixo, pincelTextoCertificado, retTextoFixo, alinhamentoTexto);
                            //===================================================================================== FIM

                            //===================================================================================== INICIO
                            // TEXTO LOCAL E DATA                                                        
                            float posVertTextCertLocaleData = posicaoVertical + altRetTextCertificado;
                            float altRetTextCertLocaleData = 50;                            
                            RectangleF retTextCertLocaleData = new RectangleF(margemLateral, posVertTextCertLocaleData, certificado.Width - 2 * margemLateral, altRetTextCertLocaleData);
                            // USADO APENAS PARA NECESSIDADE DE AJUSTES: Desenha o retângulo com borda
                            //graphics.DrawRectangle(penBorda, retTextCertLocaleData.X, retTextCertLocaleData.Y, retTextCertLocaleData.Width, retTextCertLocaleData.Height);
                            // Desenha o texto dentro do retângulo
                            graphics.DrawString(textCertLocaleData, fontTextCertLocaleData, pincelTextoCertificado, retTextCertLocaleData, alinhamentoTexto);
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
        private async Task<(string, string, string, string)> ProcessarTextoCertificadoAsync(int idPessoa, string texto)
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
                    //using (PessoaController pessoaController = new PessoaController(_dbHelper,_sessao, _pessoaEventosRepository))
                    //{
                    //    nomePessoa = await pessoaController.ObterNomePorIdPessoaAsync(idPessoa);
                    //}

                    nomePessoa = await _pessoaService.ObterNomePorIdPessoaAsync(idPessoa);

                    int posNomePessoa = texto.IndexOf("NOME_PESSOA");
                    int posBR = texto.IndexOf("<br>", StringComparison.OrdinalIgnoreCase);

                    if (posNomePessoa != -1)
                    {
                        textoAntesPessoa = texto.Substring(0, posNomePessoa) + " ";
                    }

                    if (posBR != -1)
                    {
                        textoLocalData = texto.Substring(posBR + "<br>".Length);
                    }

                    //Valida se foi encontrado um indice para "NOME_PESSOA" e "<br>"
                    // e (&&) garante que a posição de "<br>" e posterior a "NOME_PESSOA"
                    if (posNomePessoa != -1 && posBR != -1 && posBR > posNomePessoa)
                    {
                        //Atribui a variavel o que estiver entre "NOME_PESSOA" e "<br>" usando como referencia o indices das posições e comprimento das chaves "NOME_PESSOA" e "<br>"
                        textoCertificado = texto.Substring(posNomePessoa + "NOME_PESSOA".Length, posBR - (posNomePessoa + "NOME_PESSOA".Length));

                        if (textoCertificado.Contains("CPF_PESSOA")) 
                        {
                            cpf = await RetornarCPFAsync(idPessoa);
                            textoCertificado = textoCertificado.Replace("CPF_PESSOA", cpf);
                        }
                    }
                }
                return (textoAntesPessoa, nomePessoa, textoCertificado, textoLocalData);
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
                string cpf = await _pessoaService.ObterCPFPorIdPessoaAsync(idPessoa);
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
                //string sSQL = "UPDATE EVENTO_PESSOA SET IMAGEM_CERTIFICADO = @Certificado, CODIGO_CERTIFICADO = @CodigoCertificado WHERE ID = @IdEventoPessoa";

                //using (var connection = _dbHelper.GetConnection("CertificadoConnection"))
                //{
                //    using (var command = new SqlCommand(sSQL, (SqlConnection)connection))
                //    {
                //        command.Parameters.AddWithValue("@Certificado", certificadoBytes);
                //        command.Parameters.AddWithValue("@CodigoCertificado", codigoCertificado);
                //        command.Parameters.AddWithValue("@IdEventoPessoa", idEventoPessoa);
                //        await command.ExecuteNonQueryAsync();
                //    }
                //}

                await _certiificadosRepository.InserirAsync(idEventoPessoa, certificadoBytes, codigoCertificado);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [CertificadosService.Inserir]: {ex.Message}");
            }
        }
    }
}