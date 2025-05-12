using EMISSOR_DE_CERTIFICADOS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EMISSOR_DE_CERTIFICADOS.Controllers
{
    [ApiController]
    [Route("api/home-controller")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet("index")]
        public IActionResult Index()
        {
            return Ok(new { Message = "Welcome to the API!" });
        }

        [HttpGet("error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return Problem(detail: "An error occurred.", instance: Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        }
    }
}
