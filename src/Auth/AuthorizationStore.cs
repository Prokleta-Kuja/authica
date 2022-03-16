using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using authica.Entities;
using authica.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace authica.Auth
{
    public class AuthorizationStore
    {
        static bool s_loaded;
        static Dictionary<Guid, int> s_appAliasIds = new();
        static Dictionary<Guid, int> s_userAliasIds = new();
        static Dictionary<string, int> s_appEndpointIds = new();
        static Dictionary<string, int> s_appNameIds = new();

        static Dictionary<int, HashSet<int>> s_appUsers = new();
        static Dictionary<int, HashSet<LdapEntryModel>> s_appLdapEntires = new();

        readonly ILogger<AuthorizationStore> _logger;
        readonly AppDbContext _ctx;
        public AuthorizationStore(ILogger<AuthorizationStore> logger, AppDbContext ctx)
        {
            _logger = logger;
            _ctx = ctx;
        }
        public void ClearAuthorizations()
        {

            s_appAliasIds.Clear();
            s_userAliasIds.Clear();
            s_appEndpointIds.Clear();
            s_appNameIds.Clear();
            s_appUsers.Clear();
            s_appLdapEntires.Clear();
            s_loaded = false;
        }
        public async Task<bool> HasApp(Uri app)
        {
            if (!s_loaded)
                await FillStore();

            var authorityUri = app.GetLeftPart(UriPartial.Authority);

            return s_appEndpointIds.ContainsKey(authorityUri);
        }
        public async Task<bool> IsAuthorizedAsync(Uri app, Guid userGuid)
        {
            if (!s_loaded)
                await FillStore();

            var authorized = false;
            var authorityUri = app.GetLeftPart(UriPartial.Authority);

            if (s_appEndpointIds.TryGetValue(authorityUri, out var appId) && s_userAliasIds.TryGetValue(userGuid, out var userId))
                authorized = s_appUsers.TryGetValue(appId, out var users) && users.Contains(userId);

            return authorized;
        }

        private async Task FillStore()
        {
            using var throttler = new SemaphoreSlim(1);
            await throttler.WaitAsync();

            if (s_loaded)
                return;

            var dbApps = await _ctx.Apps
                .AsNoTracking()
                .Where(a => !a.Disabled.HasValue)
                .ToListAsync();

            var dbUsers = await _ctx.Users
                .AsNoTracking()
                .Where(u => !u.Disabled.HasValue)
                .ToDictionaryAsync(u => u.UserId);

            var dbRoles = await _ctx.Roles
                .AsNoTracking()
                .Where(r => !r.Disabled.HasValue)
                .ToDictionaryAsync(r => r.RoleId);

            var dbAppRoles = await _ctx.AppRoles
                .AsNoTracking()
                .Where(ar => !ar.Role!.Disabled.HasValue)
                .ToListAsync();

            var dbUserRoles = await _ctx.UserRoles
                .AsNoTracking()
                .Where(ur => !ur.Role!.Disabled.HasValue && !ur.User!.Disabled.HasValue)
                .ToListAsync();


            var appRoleIds = new Dictionary<int, HashSet<int>>();
            foreach (var dbAppRole in dbAppRoles)
                if (appRoleIds.ContainsKey(dbAppRole.AppId))
                    appRoleIds[dbAppRole.AppId].Add(dbAppRole.RoleId);
                else
                    appRoleIds.Add(dbAppRole.AppId, new() { dbAppRole.RoleId });

            var roleUserIds = new Dictionary<int, HashSet<int>>();
            var userRoleIds = new Dictionary<int, HashSet<int>>();
            foreach (var dbUserRole in dbUserRoles)
            {
                if (roleUserIds.ContainsKey(dbUserRole.RoleId))
                    roleUserIds[dbUserRole.RoleId].Add(dbUserRole.UserId);
                else
                    roleUserIds.Add(dbUserRole.RoleId, new() { dbUserRole.UserId });

                if (userRoleIds.ContainsKey(dbUserRole.UserId))
                    userRoleIds[dbUserRole.UserId].Add(dbUserRole.RoleId);
                else
                    userRoleIds.Add(dbUserRole.UserId, new() { dbUserRole.RoleId });
            }

            var allUserIds = new HashSet<int>(dbUsers.Count);
            foreach (var dbUser in dbUsers)
            {
                allUserIds.Add(dbUser.Key);
                s_userAliasIds.TryAdd(dbUser.Value.AliasId, dbUser.Key);
            }

            foreach (var dbApp in dbApps)
            {
                s_appAliasIds.TryAdd(dbApp.AliasId, dbApp.AppId);
                s_appEndpointIds.TryAdd(dbApp.AuthorityUri, dbApp.AppId);
                s_appNameIds.TryAdd(dbApp.NameNormalized, dbApp.AppId);

                if (dbApp.AllowAllUsers)
                    s_appUsers.TryAdd(dbApp.AppId, allUserIds);
                else
                {
                    var appUsers = new HashSet<int>();
                    foreach (var roleId in appRoleIds[dbApp.AppId])
                        appUsers.UnionWith(roleUserIds[roleId]);

                    s_appUsers.Add(dbApp.AppId, appUsers);
                }

                // Ldap
                if (dbApp.LdapEnabled)
                {
                    var userEntries = new Dictionary<int, LdapEntryModel>();
                    var groupEntries = new Dictionary<int, LdapEntryModel>();

                    if (dbApp.AllowAllUsers)
                        foreach (var dbUser in dbUsers)
                            userEntries.Add(dbUser.Key, new(dbUser.Value, dbApp.NameNormalized));
                    else
                        foreach (var userId in s_appUsers[dbApp.AppId])
                            userEntries.Add(userId, new(dbUsers[userId], dbApp.NameNormalized));

                    foreach (var roleId in appRoleIds[dbApp.AppId])
                    {
                        var groupEntry = new LdapEntryModel(dbRoles[roleId], dbApp.NameNormalized);
                        groupEntries.Add(roleId, groupEntry);

                        foreach (var userId in roleUserIds[roleId])
                        {
                            var userEntry = userEntries[userId];
                            userEntry.LinkedEntries.Add(groupEntry.Dn);
                            groupEntry.LinkedEntries.Add(userEntry.Dn);
                        }
                    }

                    s_appLdapEntires.Add(dbApp.AppId, new(groupEntries.Values));
                    s_appLdapEntires[dbApp.AppId].UnionWith(userEntries.Values);
                }
            }

            s_loaded = true;
        }
        public async Task<IEnumerable<LdapEntryModel>> AppUsers(int ldapUserId)
        {
            if (!s_appLdapEntires.Any())
                await FillStore();

            if (s_appLdapEntires.ContainsKey(ldapUserId))
                return s_appLdapEntires[ldapUserId];
            else
                return new List<LdapEntryModel>(0);
        }
    }
}