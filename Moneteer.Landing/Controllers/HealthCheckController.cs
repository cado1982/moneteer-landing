using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Moneteer.Landing.V2.Controllers
{
    [AllowAnonymous]
    public class HealthCheckController : Controller
    {
        [Route("healthcheck")]
        public IActionResult Index()
        {
            return Ok();
        }

    }
}
