using Microsoft.AspNetCore.Mvc;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    public class OrganizadorController : Controller
    {
        public IActionResult Login()
        {
            return View("~/Views/Login_Organizador/Login_organizador.cshtml");
        }
    }
}
