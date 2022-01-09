using System;
using System.Threading.Tasks;
using authica.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace authica.Controllers
{
    public class IntegrationController : ControllerBase
    {
        private readonly CurrentSession _session = null!;
        private readonly AuthorizationStore _authzStore = null!;
        private readonly HttpContext _ctx = null!;

        public IntegrationController(CurrentSession session, AuthorizationStore authzStore, IHttpContextAccessor accessor)
        {
            _session = session;
            _authzStore = authzStore;
            _ctx = accessor.HttpContext!;
        }

        [AllowAnonymous]
        [Route(C.Routes.VerifyNginx)]
        public async Task<IActionResult> Nginx()
        {
            var authenticated = _session.IsAuthenticated;
            if (!authenticated)
                return StatusCode(StatusCodes.Status401Unauthorized);

            if (!_ctx.Request.Headers.TryGetValue(C.Headers.OriginalUrl, out var originalUrl))
                return StatusCode(StatusCodes.Status404NotFound);

            var uri = new Uri(originalUrl);

            var authorized = await _authzStore.IsAuthorizedAsync(uri, _session.UserAliasId);

            if (!authorized)
                return StatusCode(StatusCodes.Status403Forbidden);

            return StatusCode(StatusCodes.Status200OK);
        }
    }
}