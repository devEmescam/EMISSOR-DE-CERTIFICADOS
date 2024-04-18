namespace EMISSOR_DE_CERTIFICADOS.Models
{
    public class EventoModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Participantes { get; set; }
        public bool TextoIndividual { get; set; }
        //public byte[] ImagemCertificado { get; set; }
        public IFormFile ImagemCertificado { get; set; }
    }
}
