using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using authica.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace authica.Controllers;

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

    [AllowAnonymous]
    [Route(C.Routes.VerifyCaddy)]
    public async Task<IActionResult> Caddy()
    {
        if (!_ctx.Request.Headers.TryGetValue(C.Headers.OriginalHost, out var host)
            || !_ctx.Request.Headers.TryGetValue(C.Headers.ForwardedUri, out var path))
            return StatusCode(StatusCodes.Status400BadRequest, GetHeadersMessage());

        var url = $"https://{host}{path}";
        var uri = new Uri(url);

        if (!_session.IsAuthenticated)
            return Redirect($"{C.Configuration.Current.HostName.TrimEnd('/')}{C.Routes.SignIn}?{nameof(Pages.Auth.SignInModel.Rd)}={HttpUtility.UrlEncode(url)}");

        var authorized = await _authzStore.IsAuthorizedAsync(uri, _session.UserAliasId);
        if (!authorized)
            return StatusCode(StatusCodes.Status403Forbidden, "Unauthorized");

        if (_session.IdentityClaims.TryGetValue(Claims.UserName, out var username))
            _ctx.Response.Headers.Add(C.Headers.RemoteUser, username);

        if (_session.IdentityClaims.TryGetValue(Claims.Email, out var email))
            _ctx.Response.Headers.Add(C.Headers.RemoteEmail, email);

        if (_session.IdentityClaims.TryGetValue(Claims.DisplayName, out var displayName))
            _ctx.Response.Headers.Add(C.Headers.RemoteName, displayName);

        if (_session.IdentityClaims.TryGetValue(ClaimTypes.Role, out var roles))
            _ctx.Response.Headers.Add(C.Headers.RemoteGroups, roles);

        return StatusCode(StatusCodes.Status200OK);
    }
    string GetHeadersMessage()
    {
        var headers = _ctx.Request.Headers.Select(h => $"{h.Key}: {h.Value}");
        return $"Expected headers not found.\n\nHeadears received: \n{string.Join('\n', headers)}";
    }
}