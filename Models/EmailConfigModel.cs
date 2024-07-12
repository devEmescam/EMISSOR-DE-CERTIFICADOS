namespace EMISSOR_DE_CERTIFICADOS.Models
{
    public class EmailConfigModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public string ServidorSMTP { get; set; }
        public string Porta { get; set; }
        public string SSL { get; set; }
        public IFormFile ImagemAssinatura { get; set; }
        public byte[] ImagemAssinaturaEmail { get; set; }    
    }
}
