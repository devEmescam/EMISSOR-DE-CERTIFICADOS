using Microsoft.AspNetCore.Mvc;

namespace Autenticador.Controllers
{
    public class AutenticadorController : Controller
    {
        // Ação Index que renderiza a view Autenticador/Index.cshtml
        public IActionResult Index()
        {
            return View();
        }
    }
}
