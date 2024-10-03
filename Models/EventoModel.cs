namespace EMISSOR_DE_CERTIFICADOS.Models
{
    public class EventoModel
    {
        public int Id {get; set;}
        public string Nome {get; set;}                 
        public IFormFile ImagemCertificado {get; set;}
        public string UsuarioCriador { get; set; }
    }
}
