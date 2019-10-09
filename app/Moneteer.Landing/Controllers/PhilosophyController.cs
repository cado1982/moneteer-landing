using Microsoft.AspNetCore.Mvc;

namespace Moneteer.Landing.V2.Controllers
{
    public class PhilosophyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
