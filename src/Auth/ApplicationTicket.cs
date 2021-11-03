using System;
using Microsoft.AspNetCore.Authentication;

namespace authica.Auth
{
    public class ApplicationTicket
    {
        public ApplicationTicket(Guid userAliasId, DateTime expiresAt, AuthenticationTicket ticket)
        {
            Created = DateTime.UtcNow;
            UserAliasId = userAliasId;
            ExpiresAt = expiresAt;
            Ticket = ticket;
        }

        public Guid UserAliasId { get; set; }
        public DateTime Created { get; set; }
        public DateTime ExpiresAt { get; set; }
        public AuthenticationTicket Ticket { get; set; }
    }
}