using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using authica.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace authica.Auth;

public static class CookieAuth
{
    public const string Scheme = CookieAuthenticationDefaults.AuthenticationScheme;
    public static Action<CookieAuthenticationOptions> Configure = o =>
    {
        o.Cookie.Name = $".{C.Configuration.Current.Issuer}";
        o.Cookie.IsEssential = true;
        o.Cookie.HttpOnly = true;
        o.Cookie.Path = C.Routes.Root;
        o.Cookie.Domain = $".{C.Configuration.Current.Domain}";
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.Cookie.MaxAge = C.Configuration.Current.MaxSessionDuration;

        o.SessionStore = new InMemoryTicketStore();
        o.ClaimsIssuer = C.Configuration.Current.Issuer;
        o.LoginPath = C.Routes.SignIn;
        o.LogoutPath = C.Routes.SignOut;
        o.AccessDeniedPath = C.Routes.Forbidden;
        o.ExpireTimeSpan = C.Configuration.Current.MaxSessionDuration;
    };
    public static AuthenticationProperties CreateAuthProps() =>
        new()
        {
            AllowRefresh = true,
            IssuedUtc = DateTime.UtcNow
        };
    public static ClaimsPrincipal CreatePrincipal(User user) => new(CreateIdentity(user));
    public static ClaimsIdentity CreateIdentity(User user)
    {
        var claims = new List<Claim>
            {
                new(Claims.SessionId, Guid.NewGuid().ToString()),
                new(Claims.Subject, user.AliasId.ToString()),
                new(Claims.UserName, user.UserName),
                new(Claims.Email, user.Email),
                new(Claims.EmailVerified, user.EmailVerified.ToString(), ClaimValueTypes.Boolean),
            };

        var hasFirstName = !string.IsNullOrWhiteSpace(user.FirstName);
        var hasLastName = !string.IsNullOrWhiteSpace(user.LastName);

        if (hasFirstName)
            claims.Add(new(Claims.FirstName, user.FirstName!));

        if (hasLastName)
            claims.Add(new(Claims.LastName, user.LastName!));

        if (hasFirstName && hasLastName)
            claims.Add(new(Claims.DisplayName, $"{user.FirstName} {user.LastName}"));

        if (!string.IsNullOrWhiteSpace(user.TimeZone))
            claims.Add(new(Claims.TimeZone, user.TimeZone));

        if (!string.IsNullOrWhiteSpace(user.Locale))
            claims.Add(new(Claims.Locale, user.Locale));

        if (user.UserRoles!.Any())
            claims.AddRange(user.UserRoles!.Select(ur => new Claim(ClaimTypes.Role, ur.Role!.Name)));

        if (user.IsAdmin)
            claims.Add(new(Claims.IsAdmin, user.IsAdmin.ToString()));

        var identity = new ClaimsIdentity(claims, "ApplicationCookie");
        return identity;
    }
}