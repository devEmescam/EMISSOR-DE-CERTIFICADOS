using System.Security.Cryptography;
using System.Text;
using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Helpers;
using EMISSOR_DE_CERTIFICADOS.Interfaces;

namespace EMISSOR_DE_CERTIFICADOS.Services
{
    internal class UsuariosService : IUsuarioService
    {
        private readonly IDBHelpers _dbHelper;
        private readonly ISessao _sessao;
        private readonly IPessoaEventosRepository _pessoaEventosRepository;
        private readonly IPessoaService _pessoaService;
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuariosService(IDBHelpers dbHelper, ISessao sessao, IPessoaEventosRepository pessoaEventosRepository, IPessoaService pessoaService, IUsuarioRepository usuarioRepository)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper), "O DBHelper não pode ser nulo.");
            _sessao = sessao ?? throw new ArgumentNullException(nameof(sessao), "O ISessao não pode ser nulo.");
            _pessoaEventosRepository = pessoaEventosRepository ?? throw new ArgumentNullException(nameof(pessoaEventosRepository), "O PessoaEventosRepository não pode ser nulo.");
            _pessoaService = pessoaService ?? throw new ArgumentNullException(nameof(pessoaService), "O IPessoaService não ser nulo.");
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(pessoaService), "O IUsuarioRepository não ser nulo.");
        }        
        public async Task<int> GerarUsuarioAsync(int idPessoa)
        {
            int idUsuario = -1;
            string cpf = string.Empty;            
            string senha = string.Empty;

            try
            {
                cpf = await RetornarCPFAsync(idPessoa);

                if (!await UsuarioExisteAsync(cpf))
                {
                    senha = await CriarSenhaAsync(cpf);
                    idUsuario = await _usuarioRepository.GerarUsuarioAsync(cpf, senha);
                }
                else
                {
                    idUsuario = await RetornarIdAsync(cpf); // Considere tornar este método assíncrono também
                }

                return idUsuario;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [UsuariosService.GerarUsuarioAsync]: {ex.Message}");
            }
        }                
        private async Task<string> CriarSenhaAsync(string cpf)
        {
            return await Task.Run(() =>
            {
                string senha = string.Empty;
                string chave = "EMESCAM";
                try
                {
                    // Concatenar CPF e chave
                    string baseString = cpf + chave;

                    // Gerar hash SHA256 da string base
                    using (SHA256 sha256Hash = SHA256.Create())
                    {
                        // Converter a string base em bytes
                        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(baseString));

                        // Converter os bytes do hash em uma string hexadecimal
                        StringBuilder builder = new StringBuilder();
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            builder.Append(bytes[i].ToString("x2"));
                        }

                        // Pegar os primeiros 8 caracteres da string hexadecimal
                        senha = builder.ToString().Substring(0, 6).ToUpper();
                    }

                    return senha;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro em [UsuariosService.CriarSenhaAsync]: {ex.Message}");
                }
            });
        }                
        private async Task<string> RetornarCPFAsync(int idPessoa)
        {
            string cpf = string.Empty;

            try
            {                
                cpf = await _pessoaService.ObterCPFPorIdPessoaAsync(idPessoa);
                return cpf;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [UsuariosService.RetornarCPFAsync]: {ex.Message}");
            }
        }       
        private async Task<bool> UsuarioExisteAsync(string usuario)
        {
            try
            {
                bool retorno = false;
                if ( await _usuarioRepository.UsuarioExisteAsync(usuario))  
                {
                    retorno = true;
                }
                return retorno; 
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [UsuariosService.UsuarioExisteAsync] Erro: {ex.Message}");
            }
        }                
        private async Task<int> RetornarIdAsync(string usuario)
        {
            try
            {
                int retorno = await _usuarioRepository.RetornarIdAsync(usuario);

                if (retorno != null)
                {
                    return retorno;
                }
                else 
                {
                    throw new InvalidOperationException("Não foi possível encontrar o registo com o Usuario fornecido.");
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [UsuariosService.RetornarIdAsync]: {ex.Message}");
            }
        }
        public async Task<UsuarioSenha> ObterUsuarioESenhaAsync(int idEventoPessoa)
        {
            try
            {
                var usuarioSenha = await _usuarioRepository.ObterUsuarioESenhaAsync(idEventoPessoa);
                return usuarioSenha;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [UsuariosService.ObterUsuarioESenhaAsync]: {ex.Message}");
            }
        }
    }    
}
