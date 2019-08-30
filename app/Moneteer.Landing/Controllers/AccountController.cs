using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using IdentityModel;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;

namespace Moneteer.Landing.V2.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public Task Login()
        {
            return HttpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
            {
                RedirectUri = "/"
            });
        }

        [HttpGet]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" }).ConfigureAwait(false);
        }

        // public async Task<IActionResult> FrontChannelLogout(string sid)
        // {
        //     if (User.Identity.IsAuthenticated)
        //     {
        //         var currentSid = User.FindFirst("sid")?.Value ?? "";
        //         if (string.Equals(currentSid, sid, StringComparison.Ordinal))
        //         {
        //             await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //         }
        //     }

        //     return NoContent();
        // }

        // [HttpPost]
        // [AllowAnonymous]
        // public async Task<IActionResult> BackChannelLogout(string logout_token)
        // {
        //     Response.Headers.Add("Cache-Control", "no-cache, no-store");
        //     Response.Headers.Add("Pragma", "no-cache");

        //     try
        //     {
        //         var user = await ValidateLogoutToken(logout_token);

        //         // these are the sub & sid to signout
        //         var sub = user.FindFirst("sub")?.Value;
        //         var sid = user.FindFirst("sid")?.Value;

        //         return Ok();
        //     }
        //     catch { }

        //     return BadRequest();
        // }

        // private async Task<ClaimsPrincipal> ValidateLogoutToken(string logoutToken)
        // {
        //     var claims = await ValidateJwt(logoutToken);

        //     if (claims.FindFirst("sub") == null && claims.FindFirst("sid") == null) throw new Exception("Invalid logout token");

        //     var nonce = claims.FindFirstValue("nonce");
        //     if (!String.IsNullOrWhiteSpace(nonce)) throw new Exception("Invalid logout token");

        //     var eventsJson = claims.FindFirst("events")?.Value;
        //     if (String.IsNullOrWhiteSpace(eventsJson)) throw new Exception("Invalid logout token");

        //     var events = JObject.Parse(eventsJson);
        //     var logoutEvent = events.TryGetValue("http://schemas.openid.net/event/backchannel-logout");
        //     if (logoutEvent == null) throw new Exception("Invalid logout token");

        //     return claims;
        // }

        // private static async Task<ClaimsPrincipal> ValidateJwt(string jwt)
        // {
        //     // read discovery document to find issuer and key material
        //     var client = new HttpClient();
        //     var disco = await client.GetDiscoveryDocumentAsync(Constants.Authority);

        //     var keys = new List<SecurityKey>();
        //     foreach (var webKey in disco.KeySet.Keys)
        //     {
        //         var e = Base64Url.Decode(webKey.E);
        //         var n = Base64Url.Decode(webKey.N);

        //         var key = new RsaSecurityKey(new RSAParameters { Exponent = e, Modulus = n })
        //         {
        //             KeyId = webKey.Kid
        //         };

        //         keys.Add(key);
        //     }

        //     var parameters = new TokenValidationParameters
        //     {
        //         ValidIssuer = disco.Issuer,
        //         ValidAudience = "mvc.manual",
        //         IssuerSigningKeys = keys,

        //         NameClaimType = JwtClaimTypes.Name,
        //         RoleClaimType = JwtClaimTypes.Role,

        //         RequireSignedTokens = true
        //     };

        //     var handler = new JwtSecurityTokenHandler();
        //     handler.InboundClaimTypeMap.Clear();

        //     var user = handler.ValidateToken(jwt, parameters, out var _);
        //     return user;
        // }
    }
}
