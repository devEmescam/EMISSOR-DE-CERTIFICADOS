using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Interfaces;
using System.Data.SqlClient;

namespace EMISSOR_DE_CERTIFICADOS.Repositories
{
    internal class CertificadosRepository: ICertificadosRepository
    {
        private readonly IDBHelpers _dbHelpers;

        public CertificadosRepository(IDBHelpers dBHelpers) 
        {
            _dbHelpers = dBHelpers;
        }

        public async Task InserirAsync(int idEventoPessoa, byte[] certificadoBytes, string codigoCertificado)
        {
            try
            {
                string sSQL = "UPDATE EVENTO_PESSOA SET IMAGEM_CERTIFICADO = @Certificado, CODIGO_CERTIFICADO = @CodigoCertificado WHERE ID = @IdEventoPessoa";

                using (var connection = _dbHelpers.GetConnection("CertificadoConnection"))
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