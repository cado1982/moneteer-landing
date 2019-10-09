using Microsoft.AspNetCore.Mvc;

namespace Moneteer.Landing.V2.Controllers
{
    public class BlogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
