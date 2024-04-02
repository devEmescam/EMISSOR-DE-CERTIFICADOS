using Microsoft.AspNetCore.Mvc;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    public class ParticipanteController : Controller
    {
        public IActionResult Login()
        {
            return View("~/Views/Login_Participante/Login_participante.cshtml");
        }

    }
}
