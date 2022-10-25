using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace authica.Auth;

public static class CurrentSessionExtensions
{
    public static void AddCurrentSession(this IServiceCollection svc) => svc.AddScoped<CurrentSession>();
}
public class CurrentSession
{
    readonly ILogger<CurrentSession> _logger;
    public CurrentSession(ILogger<CurrentSession> logger, IHttpContextAccessor accessor)
    {
        _logger = logger;
        IsAuthenticated = accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

        if (IsAuthenticated && accessor.HttpContext!.User.Identity is ClaimsIdentity identity)
        {
            foreach (var claim in identity.Claims)
                if (!IdentityClaims.TryAdd(claim.Type, claim.Value))
                    IdentityClaims[claim.Type] = $"{IdentityClaims[claim.Type]}|{claim.Value}";

            if (TryGetClaimValue(Claims.SessionId, out var session) && Guid.TryParse(session, out var sessionId))
                SessionId = sessionId;

            if (TryGetClaimValue(Claims.Subject, out var sub) && Guid.TryParse(sub, out var aliasId))
                UserAliasId = aliasId;

            TimeZoneId = GetClaimValue(Claims.TimeZone) ?? C.Env.TimeZone;
            LocaleId = GetClaimValue(Claims.Locale) ?? C.Env.Locale;
        }
    }
    public bool IsAuthenticated { get; set; }
    public Guid SessionId { get; set; }
    public Guid UserAliasId { get; set; }
    public string TimeZoneId { get; set; } = C.Env.TimeZone;
    public string LocaleId { get; set; } = C.Env.Locale;
    public Dictionary<string, string> IdentityClaims { get; set; } = new();
    public bool HasClaim(string type) => IdentityClaims.ContainsKey(type);
    public bool HasAllClaims(params string[] types) => types.All(t => IdentityClaims.ContainsKey(t));
    public bool HasAnyClaim(params string[] types) => types.Any(t => IdentityClaims.ContainsKey(t));
    public bool TryGetClaimValue(string type, out string value) => IdentityClaims.TryGetValue(type, out value!);
    public string? GetClaimValue(string type)
    {
        if (IdentityClaims.TryGetValue(type, out var value))
            return value;
        else
            return default;
    }
}