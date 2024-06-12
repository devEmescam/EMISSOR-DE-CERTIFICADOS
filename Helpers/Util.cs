using System.Text.RegularExpressions;

namespace EMISSOR_DE_CERTIFICADOS.Helpers
{
    public class Util
    {
        //GERAL: Esses metodos não realizam operações de I/O não há necessidade de serem assincronos

        public static bool ValidaCPF(string cpf)
        {
            try
            {
                // Verifica se o CPF possui 11 dígitos
                if (cpf.Length != 11)
                    return false;

                // Verifica se o CPF possui todos os dígitos iguais
                bool todosDigitosIguais = true;
                for (int i = 1; i < cpf.Length; i++)
                {
                    if (cpf[i] != cpf[0])
                    {
                        todosDigitosIguais = false;
                        break;
                    }
                }
                if (todosDigitosIguais)
                    return false;

                // Calcula o primeiro dígito verificador
                int soma = 0;
                for (int i = 0; i < 9; i++)
                    soma += (cpf[i] - '0') * (10 - i);
                int resto = soma % 11;
                int digitoVerificador1 = (resto < 2) ? 0 : 11 - resto;

                // Verifica se o primeiro dígito verificador é válido
                if (digitoVerificador1 != cpf[9] - '0')
                    return false;

                // Calcula o segundo dígito verificador
                soma = 0;
                for (int i = 0; i < 10; i++)
                    soma += (cpf[i] - '0') * (11 - i);
                resto = soma % 11;
                int digitoVerificador2 = (resto < 2) ? 0 : 11 - resto;

                // Verifica se o segundo dígito verificador é válido
                if (digitoVerificador2 != cpf[10] - '0')
                    return false;

                // Se chegou até aqui, o CPF é válido
                return true;

            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [Util.ValidaCPF] Erro: {ex.Message}");
            }
            
        }
        public static bool ValidaEstruturaEmail(string email)
        {
            try
            {
                // Verifica presença do "@" e pelo menos um caractere antes do ponto
                if (!Regex.IsMatch(email, @"^.+@.+\..+$"))
                    return false;

                // Verifica formato do nome de usuário: não pode começar ou terminar com um ponto e não pode ter dois pontos, hífens ou underscores consecutivos
                if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                    return false;

                // Verifica comprimento máximo, considerando o tamanho do campo em tabela
                if (email.Length > 150)
                    return false;

                // Verifica validação de caracteres especiais
                if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]+$"))
                    return false;

                // Verifica presença de pelo menos um ponto após o "@"
                if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\..+$"))
                    return false;

                // Por fim: Padroniza para minúsculas antes de inserir no banco de dados
                string emailPadronizado = email.ToLower();

                // Se passou por todas as verificações, o e-mail é considerado válido
                return true;

            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [Util.ValidaEstruturaEmail] Erro: {ex.Message}");
            }
            
        }
        // Cria um objeto IFormFile a partir do array de bytes
        public static IFormFile ConvertToFormFile(byte[] bytes)
        {
            try
            {
                if (bytes == null || bytes.Length == 0)
                {
                    return null;
                }

                return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "ImagemCertificado", "imagem.jpg");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [Util.ConvertToFormFile] Erro: {ex.Message}");
            }                      
        }
    }
}
