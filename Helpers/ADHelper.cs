using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace EMISSOR_DE_CERTIFICADOS.Helpers
{
    public class ADHelper
    {
        public string strDominio = string.Empty;
        private string strLoginAdmin = string.Empty;
        private string strSenhaAdmin = string.Empty;
        private string strDomainName = string.Empty;
        public DirectoryEntry ADContext;
        public PrincipalContext PrincipalContext;

        public ADHelper()
        {
            this.strDominio = "LDAP://ccsv.br/DC=ccsv,DC=br";
            this.strLoginAdmin = "Moodle";
            this.strSenhaAdmin = "2emesead1";
            this.strDomainName = "ccsv.br";
            this.ADContext = new DirectoryEntry();

        }
        public void ConnectToAD()
        {
            try
            {
                using (ADContext = new DirectoryEntry(this.strDominio, this.strLoginAdmin, this.strSenhaAdmin))
                {
                    // Tenta autenticar
                    object nativeObject = ADContext.NativeObject;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro em [ADHelper.ConnectToAD]: " + ex.Message, ex);
            }
        }
        public void DisconnectFromAD()
        {
            try
            {
                if (ADContext != null)
                {
                    ADContext.Close();
                    ADContext.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro em [ADHelper.DisconnectFromAD]: " + ex.Message, ex);
            }
        }
        public bool VerificaUsuario(string username, string password)
        {
            using (PrincipalContext context = new PrincipalContext(ContextType.Domain, strDomainName, strLoginAdmin, strSenhaAdmin))
            {
                // Tenta encontrar o usuário pelo nome de usuário (sAMAccountName)
                UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);

                try
                {
                    if (user != null)
                    {
                        // Se o usuário for encontrado, tenta validar as credenciais
                        return context.ValidateCredentials(username, password);
                    }

                    // Usuário não encontrado no LDAP
                    return false;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Ops, não foi possível realizar o acesso. Detalhe do erro: {ex.Message}", ex);
                    //TempData["MensagemErro"] = $"Ops, não foi possível realizar o acesso. Detalhe do erro: {erro.Message}";
                    return false;
                }
            }
        }

    }
}
