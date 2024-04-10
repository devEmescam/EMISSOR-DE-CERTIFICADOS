using Microsoft.AspNetCore.Mvc;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    public class Home_ParticipanteController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        //// GET: /Home_Organizador/NewEvent
        //public IActionResult NewEvent()
        //{
        //    return View();
        //}

        //// GET: /Home_Organizador/MyEvents
        //public IActionResult MyEvents()
        //{
        //    return View();
        //}

        //// GET: /Home_Organizador/SearchPeople
        //public IActionResult SearchPeople()
        //{
        //    return View();
        //}

        //// GET: /Home_Organizador/AnythingElse
        //public IActionResult AnythingElse()
        //{
        //    return View();
        //}

        // GET: /Home_Organizador/Logout
        public IActionResult Logout()
        {
            // Aqui você pode fazer qualquer lógica necessária para encerrar a sessão do usuário
            return RedirectToAction("Index", "Login"); // Redireciona para a página de login após o logout
        }
    }
}
