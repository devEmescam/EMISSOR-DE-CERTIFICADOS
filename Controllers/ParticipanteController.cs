using Microsoft.AspNetCore.Mvc;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    public class ParticipanteController : Controller
    {
        public IActionResult Login()
        {
            return View("~/Views/Login_participante/Login_participante.cshtml");
        }

    }
}
