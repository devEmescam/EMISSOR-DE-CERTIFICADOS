namespace EMISSOR_DE_CERTIFICADOS.Helpers
{
    public class SessaoTimeoutHelper
    {
        private readonly RequestDelegate _next;
        public SessaoTimeoutHelper(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Verifica se a rota atual é para a página de login ou para a página inicial
                var path = context.Request.Path.ToString().ToLower();
                if (path == "/" || path == "/home/index" || path.Contains("login"))
                {
                    await _next(context);
                    return;
                }

                // Verifica se a sessão está configurada
                if (context.Session.GetString("sessaoUsuarioLogado") == null)
                {
                    if (context.Request.IsAjaxRequest())
                    {
                        // Retorna um status 401 Unauthorized para requisições AJAX
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Unauthorized");
                    }
                    else
                    {
                        // Se não houver sessão, redireciona para a página inicial
                        context.Response.Redirect("/home/index");
                    }
                    return;
                }
                await _next(context);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [SessaoTimeoutHelper.InvokeAsync] Erro: {ex.Message}");
            }            
        }
    }
    public static class HttpRequestExtensions
    {
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }
}