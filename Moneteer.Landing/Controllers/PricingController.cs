﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Moneteer.Landing.V2.Controllers
{
    [AllowAnonymous]
    public class PricingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
