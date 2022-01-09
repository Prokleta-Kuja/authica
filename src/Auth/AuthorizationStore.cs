using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using authica.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace authica.Auth
{
    public class AuthorizationStore
    {
        static Dictionary<string, HashSet<Guid>> s_appUsers = new();
        readonly ILogger<AuthorizationStore> _logger;
        readonly AppDbContext _ctx;
        public AuthorizationStore(ILogger<AuthorizationStore> logger, AppDbContext ctx)
        {
            _logger = logger;
            _ctx = ctx;
        }
        public void ClearAuthorizations() => s_appUsers.Clear();
        public async Task<bool> HasApp(Uri app)
        {
            if (!s_appUsers.Any())
                await FillStore();

            var authorityUri = app.GetLeftPart(UriPartial.Authority);

            return s_appUsers.ContainsKey(authorityUri);
        }
        public async Task<bool> IsAuthorizedAsync(Uri app, Guid userId)
        {
            if (!s_appUsers.Any())
                await FillStore();

            var authorityUri = app.GetLeftPart(UriPartial.Authority);
            var authorized = s_appUsers.TryGetValue(authorityUri, out var users) && users.Contains(userId);

            return authorized;
        }

        private async Task FillStore()
        {
            var dbApps = await _ctx.Apps
                .AsNoTracking()
                .Where(a => !a.Disabled.HasValue)
                .Include(a => a.AppRoles)
                .ToDictionaryAsync(a => a.AppId);

            var dbUserIds = await _ctx.Users
                .AsNoTracking()
                .Where(u => !u.Disabled.HasValue)
                .Select(u => u.AliasId)
                .ToListAsync();

            var userRoles = await _ctx.UserRoles
                .AsNoTracking()
                .Where(ur => !ur.Role!.Disabled.HasValue && !ur.User!.Disabled.HasValue)
                .Include(ur => ur.User)
                .ToListAsync();

            foreach (var dbApp in dbApps)
            {
                if (dbApp.Value.AllowAllUsers)
                    s_appUsers.TryAdd(dbApp.Value.AuthorityUri, dbUserIds.ToHashSet());
                else
                {
                    var appUsers = new HashSet<Guid>();
                    var appRoles = dbApp.Value.AppRoles.Select(ar => ar.RoleId).ToHashSet();

                    foreach (var userRole in userRoles)
                        if (appRoles.Contains(userRole.RoleId))
                            appUsers.Add(userRole.User!.AliasId);

                    s_appUsers.TryAdd(dbApp.Value.AuthorityUri, appUsers);
                }
            }
        }
    }
}