using System;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ldap // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static TcpListener s_server = null!;
        static void Main(string[] args)
        {
            var address = new IPAddress(new byte[] { 0, 0, 0, 0 });
            var endpoint = new IPEndPoint(address, 389);
            s_server = new TcpListener(endpoint);
            s_server.Start();

            while (s_server.Server.IsBound)
            {
                try
                {
                    var client = s_server.AcceptTcpClient();
                    //HandleClient(client);
                    var task = Task.Factory.StartNew(() => HandleClient(client), TaskCreationOptions.LongRunning);
                }
                catch (ObjectDisposedException) { } // Thrown when server is stopped while still receiving. This can be safely ignored
            }
        }
        static async Task HandleClient(TcpClient client)
        {
            Console.WriteLine($"Connection from {client.Client.RemoteEndPoint} open");
            // TODO: handle TLS
            // TODO: handle is bound
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

                // Determine what kind of message
                switch (tag.TagValue)
                {
                    case 0: // Bind
                        // Request
                        var bindReader = messageReader.ReadSequence(LdapTags.BindRequest);

                        var version = bindReader.ReadInteger();
                        var user = Encoding.UTF8.GetString(bindReader.ReadOctetString());

                        var authentication = bindReader.PeekTag();

                        if (authentication.TagValue != LdapTags.AuthenticationSimple.TagValue)
                        {
                            var unsupportedWriter = new AsnWriter(AsnEncodingRules.BER);
                            using (unsupportedWriter.PushSequence())
                            {
                                unsupportedWriter.WriteInteger(messageId);
                                using var kita = unsupportedWriter.PushSequence(LdapTags.BindResponse);
                                unsupportedWriter.WriteEnumeratedValue(LdapResultCode.AuthMethodNotSupported); // Result code
                                unsupportedWriter.WriteOctetString(System.Text.Encoding.ASCII.GetBytes("")); // Matched DN
                                unsupportedWriter.WriteOctetString(System.Text.Encoding.ASCII.GetBytes("")); // Diagnostic message
                                var unsupportedData = unsupportedWriter.Encode();
                                await stream.WriteAsync(unsupportedData, 0, unsupportedData.Length);
                            }
                        }

                        var pass = Encoding.UTF8.GetString(bindReader.ReadOctetString(LdapTags.AuthenticationSimple));
                        Debug.WriteLine($"BIND: {user}:{pass}");

                        if (pass == "invalid")
                        {
                            var invalidWriter = new AsnWriter(AsnEncodingRules.BER);
                            using (invalidWriter.PushSequence())
                            {
                                invalidWriter.WriteInteger(messageId);
                                using var kita = invalidWriter.PushSequence(LdapTags.BindResponse);
                                invalidWriter.WriteEnumeratedValue(LdapResultCode.AuthMethodNotSupported); // Result code
                                invalidWriter.WriteOctetString(System.Text.Encoding.ASCII.GetBytes("")); // Matched DN
                                invalidWriter.WriteOctetString(System.Text.Encoding.ASCII.GetBytes("")); // Diagnostic message
                            }
                            var invalidData = invalidWriter.Encode();
                            await stream.WriteAsync(invalidData, 0, invalidData.Length);
                        }

                        bindReader.ThrowIfNotEmpty();
                        messageReader.ThrowIfNotEmpty();

                        // Success Response
                        var successWriter = new AsnWriter(AsnEncodingRules.BER);
                        using (successWriter.PushSequence())
                        {
                            successWriter.WriteInteger(messageId);
                            using var kita = successWriter.PushSequence(LdapTags.BindResponse);
                            successWriter.WriteEnumeratedValue(LdapResultCode.Success); // Result code
                            successWriter.WriteOctetString(System.Text.Encoding.ASCII.GetBytes("")); // Matched DN
                            successWriter.WriteOctetString(System.Text.Encoding.ASCII.GetBytes("")); // Diagnostic message
                        }
                        var successData = successWriter.Encode();
                        await stream.WriteAsync(successData, 0, successData.Length);
                        break;
                    case 2: // Unbind
                        connectionOpen = false;
                        Console.WriteLine($"Connection from {client.Client.RemoteEndPoint} closed");
                        client.Close();
                        break;
                    case 3: // Search
                        var searchReader = messageReader.ReadSequence(LdapTags.SearchRequest);
                        var baseObject = Encoding.UTF8.GetString(searchReader.ReadOctetString());
                        var scope = searchReader.ReadEnumeratedBytes().ToArray().First(); //0-baseobject, 1-singlelevel, 2-wholesubtree
                        var derefAliases = searchReader.ReadEnumeratedBytes(); //0-neverDerefAliases,...
                        var sizeLimit = searchReader.ReadInteger();
                        var timeLimit = searchReader.ReadInteger();
                        var typesOnly = searchReader.ReadBoolean();

                        var parser = new FilterParser();
                        var filter = parser.GetFilterPredicate(searchReader);

                        Debug.WriteLine(parser.Parsed);

                        var attributes = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                        var attReader = searchReader.ReadSequence();
                        while (attReader.HasData)
                            attributes.Add(Encoding.UTF8.GetString(attReader.ReadOctetString()));

                        if (!attributes.Any())
                            attributes = LdapAtt.All;

                        Debug.WriteLine(string.Join(", ", attributes));

                        // Success Response
                        var takeSize = sizeLimit == 0 ? int.MaxValue : (int)sizeLimit;

                        var query = scope == 0
                            ? Db.FlatEntries.Where(e => e.DistinquishedName.Equals(baseObject, StringComparison.InvariantCultureIgnoreCase))
                            : Db.FlatEntries.Where(filter);

                        var results = query.Take(takeSize);
                        foreach (var result in results)
                        {
                            var data = result.GetSearchResultBytes((int)messageId, typesOnly, attributes);
                            await stream.WriteAsync(data, 0, data.Length);
                        }

                        // Done
                        var doneWriter = new AsnWriter(AsnEncodingRules.BER);
                        using (doneWriter.PushSequence())
                        {
                            var resultCode = results.Any() ? LdapResultCode.Success : LdapResultCode.NoSuchObject;
                            doneWriter.WriteInteger(messageId);
                            using var kita = doneWriter.PushSequence(LdapTags.SearchDone);
                            doneWriter.WriteEnumeratedValue(resultCode); // Result code
                            doneWriter.WriteOctetString(System.Text.Encoding.ASCII.GetBytes("")); // Matched DN
                            doneWriter.WriteOctetString(System.Text.Encoding.ASCII.GetBytes("")); // Diagnostic message
                        }
                        var doneData = doneWriter.Encode();
                        await stream.WriteAsync(doneData, 0, doneData.Length);
                        break;
                    default: break;
                }
            }
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
    class FilterParser
    {
        private const StringComparison IGNORE_CASE = StringComparison.InvariantCultureIgnoreCase;

        public StringBuilder Parsed { get; set; } = new();
        public Func<LdapEntry, bool> GetFilterPredicate(AsnReader reader)
        {
            Func<LdapEntry, bool> predicate = entry => false;
            var filterTag = reader.PeekTag();
            switch (filterTag.TagValue)
            {
                case 0: // An and filter encapsulates some number of other filters and will only match an entry if all of the encapsulated filters match that entry.
                    var andReader = reader.ReadSequence(filterTag);
                    var andFilters = new List<Func<LdapEntry, bool>>();

                    while (andReader.HasData)
                    {
                        Parsed.Append("(&");
                        andFilters.Add(GetFilterPredicate(andReader));
                        Parsed.Append(")");
                    }

                    predicate = entry => andFilters.All(filter => filter(entry));
                    break;
                case 1: // An or filter encapsulates some number of other filters and will only match an entry if at least one of the encapsulated filters matches that entry.
                    var orReader = reader.ReadSequence(filterTag);
                    var orFilters = new List<Func<LdapEntry, bool>>();

                    while (orReader.HasData)
                    {
                        Parsed.Append("(|");
                        orFilters.Add(GetFilterPredicate(orReader));
                        Parsed.Append(")");
                    }

                    predicate = entry => orFilters.Any(filter => filter(entry));
                    break;
                case 2: // A not filter encapsulates exactly one filter (which may be any kind of filter, including an and or or filter that combines multiple other filters) and inverts the result obtained from evaluating the encapsulated filter against an entry. So a not filter will only match an entry if the encapsulated filter does not match that entry.
                    var notReader = reader.ReadSequence(filterTag);
                    var notFilter = GetFilterPredicate(notReader);

                    Parsed.Append("!");
                    predicate = entry => !notFilter(entry);
                    break;
                case 3: // An equalityMatch filter (also known as an equality filter) will match any entry that contains a given value for an attribute with a specified attribute description.
                    var equalityReader = reader.ReadSequence(filterTag);
                    var att = Encoding.UTF8.GetString(equalityReader.ReadOctetString());
                    var val = Encoding.UTF8.GetString(equalityReader.ReadOctetString());

                    Parsed.Append($"({att}={val})");

                    if (att.Equals(LdapAtt.Dn, IGNORE_CASE))
                        predicate = entry => entry.DistinquishedName.Equals(val, IGNORE_CASE);
                    else if (att.Equals(LdapAtt.ObjectClass, IGNORE_CASE))
                        predicate = entry => entry.ObjectClass.Equals(val, IGNORE_CASE);
                    else if (att.Equals(LdapAtt.EntryUuid, IGNORE_CASE))
                        predicate = entry => entry.Id.ToString().Equals(val, IGNORE_CASE);
                    else if (att.Equals(LdapAtt.Uid, IGNORE_CASE))
                        predicate = entry => entry.UserName?.Equals(val, IGNORE_CASE) ?? false;
                    else if (att.Equals(LdapAtt.Mail, IGNORE_CASE))
                        predicate = entry => entry.Mail?.Equals(val, IGNORE_CASE) ?? false;
                    else if (att.Equals(LdapAtt.Cn, IGNORE_CASE))
                        predicate = entry => entry.DisplayName?.Equals(val, IGNORE_CASE) ?? false;
                    else if (att.Equals(LdapAtt.DisplayName, IGNORE_CASE))
                        predicate = entry => entry.DisplayName?.Equals(val, IGNORE_CASE) ?? false;
                    else if (att.Equals(LdapAtt.GivenName, IGNORE_CASE))
                        predicate = entry => entry.FirstName?.Equals(val, IGNORE_CASE) ?? false;
                    else if (att.Equals(LdapAtt.Sn, IGNORE_CASE))
                        predicate = entry => entry.LastName?.Equals(val, IGNORE_CASE) ?? false;
                    else if (att.Equals(LdapAtt.Member, IGNORE_CASE))
                        predicate = entry => entry.IsGroup && entry.LinkedEntries.Contains(val);
                    else if (att.Equals(LdapAtt.MemberOf, IGNORE_CASE))
                        predicate = entry => !entry.IsGroup && entry.LinkedEntries.Contains(val);
                    break;
                case 7: // A present filter (also known as a presence filter) will match any entry that contains at least one value for a specified attribute.
                    var attribute = Encoding.UTF8.GetString(reader.ReadOctetString(filterTag));

                    Parsed.Append($"({attribute}=*)");

                    if (attribute.Equals(LdapAtt.Dn, IGNORE_CASE))
                        predicate = entry => !string.IsNullOrWhiteSpace(entry.DistinquishedName);
                    else if (attribute.Equals(LdapAtt.ObjectClass, IGNORE_CASE))
                        predicate = entry => !string.IsNullOrWhiteSpace(entry.ObjectClass);
                    else if (attribute.Equals(LdapAtt.EntryUuid, IGNORE_CASE))
                        predicate = entry => true;
                    else if (attribute.Equals(LdapAtt.Uid, IGNORE_CASE))
                        predicate = entry => !string.IsNullOrWhiteSpace(entry.UserName);
                    else if (attribute.Equals(LdapAtt.Mail, IGNORE_CASE))
                        predicate = entry => !string.IsNullOrWhiteSpace(entry.Mail);
                    else if (attribute.Equals(LdapAtt.Cn, IGNORE_CASE))
                        predicate = entry => !string.IsNullOrWhiteSpace(entry.DisplayName);
                    else if (attribute.Equals(LdapAtt.DisplayName, IGNORE_CASE))
                        predicate = entry => !string.IsNullOrWhiteSpace(entry.DisplayName);
                    else if (attribute.Equals(LdapAtt.GivenName, IGNORE_CASE))
                        predicate = entry => !string.IsNullOrWhiteSpace(entry.FirstName);
                    else if (attribute.Equals(LdapAtt.Sn, IGNORE_CASE))
                        predicate = entry => !string.IsNullOrWhiteSpace(entry.LastName);
                    else if (attribute.Equals(LdapAtt.Member, IGNORE_CASE))
                        predicate = entry => entry.IsGroup && entry.LinkedEntries.Any();
                    else if (attribute.Equals(LdapAtt.MemberOf, IGNORE_CASE))
                        predicate = entry => !entry.IsGroup && entry.LinkedEntries.Any();
                    break;
                case 4: // substrings
                case 5: // greaterorEqual
                case 6: // lessorEqual
                default:
                    break;
            }

            return predicate;
        }
    }
    static class LdapTags
    {
        internal static readonly Asn1Tag BindRequest = new Asn1Tag(TagClass.Application, 0);
        internal static readonly Asn1Tag BindResponse = new Asn1Tag(TagClass.Application, 1);
        internal static readonly Asn1Tag AuthenticationSimple = new Asn1Tag(TagClass.ContextSpecific, 0);
        internal static readonly Asn1Tag SearchRequest = new Asn1Tag(TagClass.Application, 3);
        internal static readonly Asn1Tag SearchResult = new Asn1Tag(TagClass.Application, 4);
        internal static readonly Asn1Tag SearchDone = new Asn1Tag(TagClass.Application, 5);
    }
    static class LdapAtt
    {
        // Don't forget to add everywhere
        public const string Dn = "dn";
        public const string ObjectClass = "objectClass";
        public const string EntryUuid = "entryuuid";
        public const string Uid = "uid";
        public const string Mail = "mail";
        public const string Cn = "cn";
        public const string DisplayName = "displayName";
        public const string GivenName = "givenName";
        public const string Sn = "sn";
        public const string Member = "member";
        public const string MemberOf = "memberOf";
        public static readonly HashSet<string> All = new()
        {
            Dn,
            ObjectClass,
            EntryUuid,
            Uid,
            Mail,
            Cn,
            DisplayName,
            GivenName,
            Sn,
            Member,
            MemberOf,
        };
    }
    internal enum LdapResultCode
    {
        Success = 0,
        OperationsError = 1,
        ProtocolError = 2,
        TimeLimitExceeded = 3,
        SizeLimitExceeded = 4,
        AuthMethodNotSupported = 7,
        NoSuchObject = 32,
        InappropriateAuthentication = 48,
        InvalidCredentials = 49,
        InsufficientAccessRights = 50,
    }
}