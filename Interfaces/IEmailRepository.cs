namespace EMISSOR_DE_CERTIFICADOS.Interfaces
{
    public interface IEmailRepository
    {
        Task<string> RetornarNomeEventoAsync(int idEventoPessoa);
        Task<string> RetornarNomePessoaAsync(int idEventoPessoa);
        Task<string> RetornarEmailPessoaAsync(int idEventoPessoa);
        Task<byte[]> RetornarCertificadoAsync(int idEventoPessoa);
    }
}
