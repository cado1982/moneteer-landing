using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Moneteer.Landing.V2.Controllers
{
    [AllowAnonymous]
    public class HealthCheckController : Controller
    {
        private readonly ILogger<HealthCheckController> logger;

        public HealthCheckController(ILogger<HealthCheckController> logger)
        {
            this.logger = logger;
        }

        [Route("healthcheck")]
        public IActionResult Index()
        {
            this.logger.LogInformation(Request.Headers.ToString());
            return Ok();
        }

    }
}
