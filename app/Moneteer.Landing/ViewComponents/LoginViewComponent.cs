using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moneteer.Landing.ViewComponents
{
    public class LoginViewComponent : ViewComponent
    {

        private bool _persistentLoginAttempted = false;
        private const string _persistentLoginFlag = "persistent_login_attempt";


        public LoginViewComponent()
        {
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {

            // Always clean up an existing flag.
            bool FlagFound = false;
            if (!String.IsNullOrEmpty(TempData[_persistentLoginFlag] as string))
            {
                FlagFound = true;
                TempData.Remove(_persistentLoginFlag);
            }

            // Try to refresh a persistent login the first time an anonymous user hits the index page in this session
            if (!User.Identity.IsAuthenticated && !_persistentLoginAttempted)
            {
                _persistentLoginAttempted = true;
                // If there was a flag, this is the return-trip from a failed persistent login attempt.
                if (!FlagFound)
                {
                    // No flag was found. Create it, then begin the OIDC challenge flow.
                    TempData[_persistentLoginFlag] = _persistentLoginFlag;
                    await HttpContext.ChallengeAsync("persistent");
                }
            }

            return View();

        }
    }
}
