using System.ComponentModel.DataAnnotations;

namespace EMISSOR_DE_CERTIFICADOS.Models
{
    public class LoginModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o Usuário.")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Informe a Senha.")]
        public string Senha { get; set; }

        public bool Administrativo { get; set; }
    }
}
