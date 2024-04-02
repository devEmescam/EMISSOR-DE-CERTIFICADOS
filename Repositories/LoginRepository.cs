using EMISSOR_DE_CERTIFICADOS.DBConnections;
using System.Data;

namespace EMISSOR_DE_CERTIFICADOS.Repositories
{
    public class LoginRepository
    {
        private readonly DBHelpers _dbHelper;

        public int RetornarIdUsuario(string usuario, string senha, bool administrativo) 
        {
            string sSQL = "";
            Int32 Id = 0;
            DataTable oDT = new DataTable();

            try
            {
                if (administrativo) 
                {
                    sSQL = $"SELECT ID FROM USUARIO WHERE USUARIO = '{usuario}' WHERE ADMINISTRATIVO = {administrativo}";
                }
                else
                {
                    sSQL = $"SELECT ID FROM USUARIO WHERE USUARIO = '{usuario}' AND SENHA = '{senha}' AND ADMINISTRATIVO = {administrativo}";
                }                

                oDT = _dbHelper.ExecuteQuery(sSQL, "CertificadoConnection");

                if (oDT != null && oDT.Rows.Count > 0) 
                {
                Id =  oDT.Rows[0].Field<int>("ID");
                }

                return Id;
            }
			catch (Exception ex)
			{
                throw new Exception($"Ocorreu um erro em [LoginRepository.RetornarIdUsuario] Erro: {ex.Message}");
            }
        }
    }
}
