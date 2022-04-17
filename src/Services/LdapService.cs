using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using authica.Auth;
using authica.Entities;
using authica.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace authica.Services;

public class LdapService : BackgroundService
{
    const StringComparison IGNORE_CASE = StringComparison.InvariantCultureIgnoreCase;
    static TcpListener s_server = null!;
    static readonly HashSet<TcpClient> s_clients = new();

    readonly ILogger<LdapService> _logger;
    readonly IPasswordHasher _hasher;
    readonly IServiceProvider _scvProvider;

    public LdapService(ILogger<LdapService> logger, IPasswordHasher hasher, IServiceProvider svcProvider)
    {
        _logger = logger;
        _scvProvider = svcProvider;
        _hasher = hasher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //TODO: add config check for ldap enable
        // if (false)
        //     return;

        var address = new IPAddress(new byte[] { 0, 0, 0, 0 });
        var endpoint = new IPEndPoint(address, 389);

        s_server = new TcpListener(endpoint);
        s_server.Start();

        while (s_server.Server.IsBound)
            try
            {
                var client = await s_server.AcceptTcpClientAsync(stoppingToken);
                s_clients.Add(client);
                var task = Task.Factory.StartNew(() => HandleClient(client), TaskCreationOptions.LongRunning);
            }
            catch (ObjectDisposedException) { } // Thrown when server is stopped while still receiving. This can be safely ignored
            catch (Exception)
            {
                // TODO: log
            }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var client in s_clients)
        {
            client?.Close();
            if (client != null)
                s_clients.Remove(client);
        }

        return base.StopAsync(cancellationToken);
    }

    async Task HandleClient(TcpClient client)
    {
        using var svcScope = _scvProvider.CreateScope();
        var authenticated = false;
        var appId = 0;
        var connectionOpen = true;

        using var stream = client.GetStream();
        while (connectionOpen && TryParsePacket(stream, out var messageBytes))
        {
            var reader = new AsnReader(messageBytes, AsnEncodingRules.BER);
            var messageReader = reader.ReadSequence();
            var messageId = messageReader.ReadInteger();

            var tag = messageReader.PeekTag();

            if (tag.TagClass != TagClass.Application)
                throw new ArgumentException("Input type is expected to be " + TagClass.Application + " but was " + tag.TagClass);

            switch (tag.TagValue)
            {
                case 0: // Bind
                    var bindReader = messageReader.ReadSequence(C.Ldap.Tags.BindRequest);

                    var version = bindReader.ReadInteger();
                    var userDn = Encoding.UTF8.GetString(bindReader.ReadOctetString());

                    var authentication = bindReader.PeekTag();

                    if (authentication.TagValue != C.Ldap.Tags.AuthenticationSimple.TagValue)
                    {
                        var unsupportedWriter = new AsnWriter(AsnEncodingRules.BER);
                        using (unsupportedWriter.PushSequence())
                        {
                            unsupportedWriter.WriteInteger(messageId);
                            using var kita = unsupportedWriter.PushSequence(C.Ldap.Tags.BindResponse);
                            unsupportedWriter.WriteEnumeratedValue(C.Ldap.ResultCode.AuthMethodNotSupported); // Result code
                            unsupportedWriter.WriteOctetString(Encoding.UTF8.GetBytes("")); // Matched DN
                            unsupportedWriter.WriteOctetString(Encoding.UTF8.GetBytes("")); // Diagnostic message
                            var unsupportedData = unsupportedWriter.Encode();
                            await stream.WriteAsync(unsupportedData);
                        }
                        break;
                    }

                    var password = Encoding.UTF8.GetString(bindReader.ReadOctetString(C.Ldap.Tags.AuthenticationSimple));

                    var db = svcScope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var bind = LdapEntryModel.ParseDn(userDn);
                    if (!string.IsNullOrWhiteSpace(bind.App))
                    {
                        var normalizedApp = C.Normalize(bind.App);
                        var app = await db.Apps
                            .Where(a => !a.Disabled.HasValue)
                            .SingleOrDefaultAsync(u => u.NameNormalized == normalizedApp);

                        if (app != null)
                        {
                            appId = app.AppId;
                            if (string.IsNullOrWhiteSpace(bind.User))
                                authenticated = app.VerifyPassword(password, _hasher);
                            else
                            {
                                var normalizedUser = C.Normalize(bind.User);
                                var user = await db.Users
                                    .Where(u => !u.Disabled.HasValue && (u.UserNameNormalized == normalizedUser || u.EmailNormalized == normalizedUser))
                                    .SingleOrDefaultAsync();

                                if (user != null)
                                {
                                    if (!user.Disabled.HasValue)
                                        authenticated = user.VerifyPassword(password, _hasher);

                                    if (authenticated)
                                    {
                                        user.LastLogin = DateTime.UtcNow;
                                        await db.SaveChangesAsync();
                                    }
                                }
                            }
                        }
                    }

                    if (!authenticated)
                    {
                        var invalidWriter = new AsnWriter(AsnEncodingRules.BER);
                        using (invalidWriter.PushSequence())
                        {
                            invalidWriter.WriteInteger(messageId);
                            using var kita = invalidWriter.PushSequence(C.Ldap.Tags.BindResponse);
                            invalidWriter.WriteEnumeratedValue(C.Ldap.ResultCode.InvalidCredentials); // Result code
                            invalidWriter.WriteOctetString(System.Text.Encoding.UTF8.GetBytes("")); // Matched DN
                            invalidWriter.WriteOctetString(System.Text.Encoding.UTF8.GetBytes("")); // Diagnostic message
                        }
                        var invalidData = invalidWriter.Encode();
                        await stream.WriteAsync(invalidData);
                        break;
                    }

                    bindReader.ThrowIfNotEmpty();
                    messageReader.ThrowIfNotEmpty();

                    // Success Response
                    var successWriter = new AsnWriter(AsnEncodingRules.BER);
                    using (successWriter.PushSequence())
                    {
                        successWriter.WriteInteger(messageId);
                        using var kita = successWriter.PushSequence(C.Ldap.Tags.BindResponse);
                        successWriter.WriteEnumeratedValue(C.Ldap.ResultCode.Success); // Result code
                        successWriter.WriteOctetString(System.Text.Encoding.UTF8.GetBytes("")); // Matched DN
                        successWriter.WriteOctetString(System.Text.Encoding.UTF8.GetBytes("")); // Diagnostic message
                    }
                    var successData = successWriter.Encode();
                    await stream.WriteAsync(successData);
                    break;
                case 2: // Unbind
                    connectionOpen = false;
                    client.Close();
                    break;
                case 3: // Search
                    if (!authenticated || appId == 0)
                    {
                        connectionOpen = false;
                        client.Close();
                        break;
                    }

                    var searchReader = messageReader.ReadSequence(C.Ldap.Tags.SearchRequest);
                    var baseObject = Encoding.UTF8.GetString(searchReader.ReadOctetString());
                    var scope = searchReader.ReadEnumeratedBytes().ToArray().First(); //0-baseobject, 1-singlelevel, 2-wholesubtree
                    var derefAliases = searchReader.ReadEnumeratedBytes(); //0-neverDerefAliases, ...
                    var sizeLimit = searchReader.ReadInteger();
                    var timeLimit = searchReader.ReadInteger();
                    var typesOnly = searchReader.ReadBoolean();

                    var filter = GetFilterPredicate(searchReader, scope, baseObject);

                    var attributes = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                    var attReader = searchReader.ReadSequence();
                    while (attReader.HasData)
                        attributes.Add(Encoding.UTF8.GetString(attReader.ReadOctetString()));

                    if (!attributes.Any())
                        attributes = C.Ldap.Attributes.All;

                    // Success Response
                    var takeSize = sizeLimit == 0 ? int.MaxValue : (int)sizeLimit;

                    var authzStore = svcScope.ServiceProvider.GetRequiredService<AuthorizationStore>();
                    var allAppUsers = await authzStore.AppUsers(appId);

                    var results = allAppUsers.Where(filter).Take(takeSize);
                    foreach (var result in results)
                    {
                        var data = result.GetSearchResultBytes((int)messageId, typesOnly, attributes);
                        await stream.WriteAsync(data);
                    }

                    // Done
                    var doneWriter = new AsnWriter(AsnEncodingRules.BER);
                    using (doneWriter.PushSequence())
                    {
                        var resultCode = results.Any() ? C.Ldap.ResultCode.Success : C.Ldap.ResultCode.NoSuchObject;
                        doneWriter.WriteInteger(messageId);
                        using var kita = doneWriter.PushSequence(C.Ldap.Tags.SearchDone);
                        doneWriter.WriteEnumeratedValue(resultCode); // Result code
                        doneWriter.WriteOctetString(System.Text.Encoding.UTF8.GetBytes("")); // Matched DN
                        doneWriter.WriteOctetString(System.Text.Encoding.UTF8.GetBytes("")); // Diagnostic message
                    }
                    var doneData = doneWriter.Encode();
                    await stream.WriteAsync(doneData);
                    break;
                default: break;
            }
        }
        static Func<LdapEntryModel, bool> GetFilterPredicate(AsnReader reader, byte? scope = null, string? baseObject = null)
        {
            Func<LdapEntryModel, bool> predicate = entry => false;

            var filterTag = reader.PeekTag();
            switch (filterTag.TagValue)
            {
                case 0: // An and filter encapsulates some number of other filters and will only match an entry if all of the encapsulated filters match that entry.
                    var andReader = reader.ReadSequence(filterTag);
                    var andFilters = new List<Func<LdapEntryModel, bool>>();

                    while (andReader.HasData)
                        andFilters.Add(GetFilterPredicate(andReader));

                    predicate = entry => andFilters.All(filter => filter(entry));
                    break;
                case 1: // An or filter encapsulates some number of other filters and will only match an entry if at least one of the encapsulated filters matches that entry.
                    var orReader = reader.ReadSequence(filterTag);
                    var orFilters = new List<Func<LdapEntryModel, bool>>();

                    while (orReader.HasData)
                        orFilters.Add(GetFilterPredicate(orReader));

                    predicate = entry => orFilters.Any(filter => filter(entry));
                    break;
                case 2: // A not filter encapsulates exactly one filter (which may be any kind of filter, including an and or or filter that combines multiple other filters) and inverts the result obtained from evaluating the encapsulated filter against an entry. So a not filter will only match an entry if the encapsulated filter does not match that entry.
                    var notReader = reader.ReadSequence(filterTag);
                    var notFilter = GetFilterPredicate(notReader);

                    predicate = entry => !notFilter(entry);
                    break;
                case 3: // An equalityMatch filter (also known as an equality filter) will match any entry that contains a given value for an attribute with a specified attribute description.
                    var equalityReader = reader.ReadSequence(filterTag);
                    var att = Encoding.UTF8.GetString(equalityReader.ReadOctetString());
                    var val = Encoding.UTF8.GetString(equalityReader.ReadOctetString());

                    if (att.Equals(C.Ldap.Attributes.Dn, IGNORE_CASE))
                        predicate = entry => entry.Dn.Equals(val, IGNORE_CASE);
                    else if (att.Equals(C.Ldap.Attributes.ObjectClass, IGNORE_CASE))
                        predicate = entry => entry.ObjectClass.Equals(val, IGNORE_CASE);
                    else if (att.Equals(C.Ldap.Attributes.EntryUuid, IGNORE_CASE))
                        predicate = entry => entry.EntryUUID.ToString().Equals(val, IGNORE_CASE);
                    else if (att.Equals(C.Ldap.Attributes.Uid, IGNORE_CASE))
                        predicate = entry => entry.Uid?.Equals(val, IGNORE_CASE) ?? false;
                    else if (att.Equals(C.Ldap.Attributes.Mail, IGNORE_CASE))
                        predicate = entry => entry.Mail?.Equals(val, IGNORE_CASE) ?? false;
                    else if (att.Equals(C.Ldap.Attributes.Cn, IGNORE_CASE))
                        predicate = entry => entry.Cn?.Equals(val, IGNORE_CASE) ?? false;
                    else if (att.Equals(C.Ldap.Attributes.DisplayName, IGNORE_CASE))
                        predicate = entry => entry.DisplayName?.Equals(val, IGNORE_CASE) ?? false;
                    else if (att.Equals(C.Ldap.Attributes.GivenName, IGNORE_CASE))
                        predicate = entry => entry.GivenName?.Equals(val, IGNORE_CASE) ?? false;
                    else if (att.Equals(C.Ldap.Attributes.Sn, IGNORE_CASE))
                        predicate = entry => entry.Sn?.Equals(val, IGNORE_CASE) ?? false;
                    else if (att.Equals(C.Ldap.Attributes.Member, IGNORE_CASE))
                        predicate = entry => entry.IsGroup && entry.LinkedEntries.Contains(val);
                    else if (att.Equals(C.Ldap.Attributes.MemberOf, IGNORE_CASE))
                        predicate = entry => !entry.IsGroup && entry.LinkedEntries.Contains(val);
                    break;
                case 7: // A present filter (also known as a presence filter) will match any entry that contains at least one value for a specified attribute.
                    var attribute = Encoding.UTF8.GetString(reader.ReadOctetString(filterTag));

                    if (attribute.Equals(C.Ldap.Attributes.Dn, IGNORE_CASE))
                        predicate = entry => !string.IsNullOrWhiteSpace(entry.Dn);
                    else if (attribute.Equals(C.Ldap.Attributes.ObjectClass, IGNORE_CASE))
                        predicate = entry => !string.IsNullOrWhiteSpace(entry.ObjectClass);
                    else if (attribute.Equals(C.Ldap.Attributes.EntryUuid, IGNORE_CASE))
                        predicate = entry => true;
                    else if (attribute.Equals(C.Ldap.Attributes.Uid, IGNORE_CASE))
                        predicate = entry => !string.IsNullOrWhiteSpace(entry.Uid);
                    else if (attribute.Equals(C.Ldap.Attributes.Mail, IGNORE_CASE))
                        predicate = entry => !string.IsNullOrWhiteSpace(entry.Mail);
                    else if (attribute.Equals(C.Ldap.Attributes.Cn, IGNORE_CASE))
                        predicate = entry => !string.IsNullOrWhiteSpace(entry.Cn);
                    else if (attribute.Equals(C.Ldap.Attributes.DisplayName, IGNORE_CASE))
                        predicate = entry => !string.IsNullOrWhiteSpace(entry.DisplayName);
                    else if (attribute.Equals(C.Ldap.Attributes.GivenName, IGNORE_CASE))
                        predicate = entry => !string.IsNullOrWhiteSpace(entry.GivenName);
                    else if (attribute.Equals(C.Ldap.Attributes.Sn, IGNORE_CASE))
                        predicate = entry => !string.IsNullOrWhiteSpace(entry.Sn);
                    else if (attribute.Equals(C.Ldap.Attributes.Member, IGNORE_CASE))
                        predicate = entry => entry.IsGroup && entry.LinkedEntries.Any();
                    else if (attribute.Equals(C.Ldap.Attributes.MemberOf, IGNORE_CASE))
                        predicate = entry => !entry.IsGroup && entry.LinkedEntries.Any();
                    break;
                case 4: // substrings
                case 5: // greaterorEqual
                case 6: // lessorEqual
                default:
                    break;
            }

            if (scope.HasValue && !string.IsNullOrWhiteSpace(baseObject))
                switch (scope.Value)
                {
                    case 0: // Indicates that only the entry whose DN is provided in the baseObject element of the request will be considered. None of its subordinates will be examined.
                        predicate += entry => entry.Dn.Equals(baseObject, IGNORE_CASE);
                        break;
                    case 1: // Indicates that only the entries that are immediate subordinates to the baseObject entry will be considered. The base entry itself will not be considered, nor will entries more than one level beneath the base entry.
                    case 2: // Indicates that the baseObject entry will be considered, as well as all of its subordinates to any depth.
                    default:
                        break;
                }

            return predicate;
        }
        static bool TryParsePacket(Stream stream, out byte[] messageBytes)
        {
            using var ms = new MemoryStream();
            var packetLength = new List<Byte>();
            var streamPosition = 0;
            int? packetSize = null;
            var isMultiByteSize = false;
            int? multiByteSize = null;

            while (true)
            {
                var buffer = new byte[1];
                var read = stream.Read(buffer, 0, buffer.Length);

                if (streamPosition != 1)
                {
                    if (isMultiByteSize && (streamPosition - 2) < multiByteSize)
                        packetLength.Add(buffer[0]);
                    else if (isMultiByteSize && (streamPosition - 2) == multiByteSize)
                    {
                        var hexValue = BitConverter.ToString(packetLength.ToArray()).Replace("-", "");
                        packetSize = Convert.ToInt32(hexValue, 16) + 2 + packetLength.Count;
                    }
                }
                else
                {
                    var number = Convert.ToInt32(buffer[0]);
                    if (number <= 127)
                        packetSize = number + 2;
                    else
                    {
                        isMultiByteSize = true;
                        multiByteSize = (buffer[0] >> 0) & 127;
                    }
                }

                ms.Write(buffer, 0, read);
                streamPosition++;

                if (read <= 0 || streamPosition == packetSize)
                {
                    messageBytes = ms.ToArray();
                    return read > 0;
                }
            }
        }
    }
}