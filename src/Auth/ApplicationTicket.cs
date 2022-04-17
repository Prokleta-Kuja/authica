using System;
using Microsoft.AspNetCore.Authentication;

namespace authica.Auth;

public class ApplicationTicket
{
    public ApplicationTicket(Guid sessionId, Guid userAliasId, DateTime expiresAt, AuthenticationTicket ticket)
    {
        Created = DateTime.UtcNow;
        SessionId = sessionId;
        UserAliasId = userAliasId;
        ExpiresAt = expiresAt;
        Ticket = ticket;
    }

    public Guid SessionId { get; set; }
    public Guid UserAliasId { get; set; }
    public DateTime Created { get; set; }
    public DateTime ExpiresAt { get; set; }
    public AuthenticationTicket Ticket { get; set; }
}