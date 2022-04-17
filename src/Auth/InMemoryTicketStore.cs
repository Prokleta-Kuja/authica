using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace authica.Auth;

public class InMemoryTicketStore : ITicketStore
{
    static readonly Dictionary<string, ApplicationTicket> _store = new();
    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = ticket.Principal.FindFirst(Claims.SessionId);
        if (!Guid.TryParse(key?.Value, out _))
            throw new ArgumentException("No session claim in ticket", nameof(ticket));

        await RenewAsync(key.Value, ticket);

        return key.Value;
    }

    public Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        if (!Guid.TryParse(key, out var sessionId))
            throw new ArgumentException("No session key", nameof(key));

        var subject = ticket.Principal.FindFirst(Claims.Subject);
        if (!Guid.TryParse(subject?.Value, out var userAliasId))
            throw new ArgumentException("No subject claim in ticket", nameof(ticket));

        var expiresUtc = ticket.Properties.ExpiresUtc?.UtcDateTime ?? DateTime.UtcNow.Add(C.Configuration.Current.MaxSessionDuration);

        var value = new ApplicationTicket(sessionId, userAliasId, expiresUtc, ticket);
        if (_store.ContainsKey(key))
        {
            var prev = _store[key];
            value.Created = prev.Created;
            _store[key] = value;
        }
        else
            _store.Add(key, value);

        return Task.CompletedTask;
    }

    public Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        AuthenticationTicket? ticket = null;
        if (_store.TryGetValue(key, out var value))
            if (value.ExpiresAt > DateTime.UtcNow)
                ticket = value.Ticket;
            else
                _store.Remove(key);

        return Task.FromResult(ticket);
    }

    public Task RemoveAsync(string key)
    {
        _store.Remove(key);
        return Task.CompletedTask;
    }

    public static bool SessionActive(string? sessionId)
    {
        var active = false;
        if (!string.IsNullOrWhiteSpace(sessionId) && _store.ContainsKey(sessionId))
            if (_store[sessionId].ExpiresAt > DateTime.UtcNow)
                active = true;
            else
                _store.Remove(sessionId);

        return active;
    }

    public static List<ApplicationTicket> GetUsersTickets(Guid userAliasId)
        => _store.Values.Where(at => at.UserAliasId == userAliasId).ToList();

    public static void RemoveUsersTickets(Guid userAliasId)
    {
        var sessionIds = _store.Where(kv => kv.Value.UserAliasId == userAliasId).Select(kv => kv.Key);

        foreach (var sessionId in sessionIds)
            _store.Remove(sessionId);
    }
    public static void RemoveSession(string sessionId) => _store.Remove(sessionId);
}