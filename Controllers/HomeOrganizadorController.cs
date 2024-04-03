using Microsoft.AspNetCore.Mvc;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    public class Home_OrganizadorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}