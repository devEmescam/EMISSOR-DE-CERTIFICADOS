using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using EMISSOR_DE_CERTIFICADOS.Controllers;
using EMISSOR_DE_CERTIFICADOS.DBConnections;

namespace EMISSOR_DE_CERTIFICADOS.Services
{
    public class CertificadosService
    {        
        private readonly DBHelpers _dbHelper;
        public CertificadosService(DBHelpers dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper), "O DBHelpers não pode ser nulo.");            
        }        
        public bool GerarCertificado(int idEvento_Pessoa, int idPessoa, string textoOriginal, IFormFile imagem)
        {
            string textoCertificado = string.Empty;
            string textoAutenticidade = string.Empty;
            string codigoCertificado = string.Empty;
            string cpf = string.Empty;
            string caminhoQRCode = string.Empty;
            bool retorno = false;

            try
            {
                textoCertificado = ProcessarTextoCertificadoPessoa(idPessoa, textoOriginal);
                cpf = RetornarCPF(idPessoa);
                codigoCertificado = GerarCodigo(cpf);
                textoAutenticidade = RetornarTextoAutenticidade(codigoCertificado);                
                caminhoQRCode = Path.Combine("wwwroot", "QRCodeEmescam.png");

                // Caminho onde o certificado será salvo
                string nomeArquivoCertificado = codigoCertificado + ".png";
                string caminhoCertificado = Path.Combine("wwwroot", "Certificados", nomeArquivoCertificado);

                // Carregar o arquivo IFormFile diretamente
                using (var memoryStream = new MemoryStream())
                {
                    imagem.CopyTo(memoryStream);
                    using (Bitmap certificado = new Bitmap(memoryStream))
                    {
                        // Configurar a fonte e o tamanho do texto1
                        using (Font fonteTextoCertificado = new Font("Arial", 48, FontStyle.Regular, GraphicsUnit.Pixel))
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
                                RectangleF retanguloTextoCertificado = new RectangleF(certificado.Width / 6, certificado.Height / 2.5f, certificado.Width / 1.5f, certificado.Height / 2); // 2970px X 2100px

                                // Desenhar o texto1 na região central
                                graphics.DrawString(textoCertificado, fonteTextoCertificado, Brushes.Black, retanguloTextoCertificado, formatoCentralizado);

                                // Configurar a fonte e o tamanho do textoAutenticidade
                                using (Font fontetextoAutenticidade = new Font("Arial", 24, FontStyle.Bold, GraphicsUnit.Pixel))
                                {
                                    // Configurar a região inferior esquerda para o textotextoAutenticidade
                                    RectangleF retanguloTextoAutenticidade = new RectangleF(10, certificado.Height - 120, 1500, 50);

                                    // Desenhar o textoAutenticidade na região inferior esquerda
                                    graphics.DrawString(textoAutenticidade, fontetextoAutenticidade, Brushes.Black, retanguloTextoAutenticidade);

                                    // Configurar a região inferior direita para o QRCode
                                    Bitmap qrCodeBitmap = new Bitmap(caminhoQRCode);
                                    Rectangle retanguloQRCode = new Rectangle(certificado.Width - 280, certificado.Height - 260, 180, 180);

                                    // Desenhar o QRCode na região inferior direita
                                    graphics.DrawImage(qrCodeBitmap, retanguloQRCode);
                                }
                            }
                        }                        

                        // Convertendo o certificado para um array de bytes
                        byte[] certificadoBytes;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            certificado.Save(ms, ImageFormat.Png);
                            certificadoBytes = ms.ToArray();
                        }

                        // Inserir o certificado no banco de dados
                        Inserir(idEvento_Pessoa, certificadoBytes, codigoCertificado);                      

                        retorno = true;                        
                    }
                }

                return retorno;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [CertificadosService.GerarCertificado_SEM_NEGRITO]: {ex.Message}");
            }
        }
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
                                RectangleF retanguloTextoCertificado = new RectangleF(certificado.Width / 6, certificado.Height / 2.5f, certificado.Width / 1.5f, certificado.Height / 2);
                                graphics.DrawString(textoCertificado, fonteTextoCertificado, Brushes.Black, retanguloTextoCertificado, formatoCentralizado);

                                using (Font fontetextoAutenticidade = new Font("Arial", 24, FontStyle.Regular, GraphicsUnit.Pixel))
                                {
                                    RectangleF retanguloTextoAutenticidade = new RectangleF(10, certificado.Height - 120, 1500, 50);
                                    graphics.DrawString(textoAutenticidade, fontetextoAutenticidade, Brushes.Black, retanguloTextoAutenticidade);

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
                throw new Exception($"Erro em [CertificadosService.GerarCertificado_SEM_NEGRITO]: {ex.Message}");
            }
        }
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
                texto = $"Códigosss do Certificado: {codigo} - Verifique autenticidade em: definirUrl.com.br";
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
        private string RetornarCPF(int idPessoa) 
        {
            string cpf = string.Empty;
            try
            {
                using (PessoaController pessoaController = new PessoaController(_dbHelper))
                {                    
                    cpf = pessoaController.ObterCPFPorIdPessoa(idPessoa);                    
                }
                return cpf; 
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [CertificadoEditor.retornarCPF]");
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
        private void Inserir(int idEventoPessoa, byte[] certificadoBytes, string codigoCertificado)
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
                        command.ExecuteNonQuery();
                    }
                }                
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [CertificadosService.Inserir]: {ex.Message}");
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
