using Microsoft.AspNetCore.Mvc;

namespace Moneteer.Landing.V2.Controllers
{
    public class HowItWorksController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
