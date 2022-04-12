using System.Formats.Asn1;
using System.Text;

namespace ldap
{
    public class LdapEntry
    {
        public Guid Id { get; set; }
        public bool IsGroup { get; set; }
        public string ObjectClass { get; set; }
        public string DistinquishedName { get; set; }
        public string? UserName { get; set; }
        public string? DisplayName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Mail { get; set; }
        public HashSet<string> LinkedEntries { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);
        public LdapEntry(bool isGroup, string name, string? userName, string? first, string? last, string? mail)
        {
            Id = Guid.NewGuid();
            IsGroup = isGroup;
            DisplayName = name;
            if (isGroup)
            {
                DistinquishedName = $"cn={name},dc=authica";
                ObjectClass = "groupOfNames";
            }
            else
            {
                DistinquishedName = $"uid={name.Replace(" ", "")},dc=authica";
                ObjectClass = "inetOrgPerson";
                UserName = userName;
                FirstName = first;
                LastName = last;
                Mail = mail;
            }
        }
        public byte[] GetSearchResultBytes(int messageId, bool typesOnly, HashSet<string> attributes)
        {
            var sb = new StringBuilder("Result: ");
            // TODO implement types only
            // TODO add dc=authica to enable base dn detection
            var resultWriter = new AsnWriter(AsnEncodingRules.BER);
            using (resultWriter.PushSequence())
            {
                resultWriter.WriteInteger(messageId);
                using (resultWriter.PushSequence(LdapTags.SearchResult))
                {
                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(DistinquishedName!)); // CommonName
                    using (resultWriter.PushSequence()) // Attributes
                    {
                        if (attributes.Contains(LdapAtt.Dn))
                            using (resultWriter.PushSequence())
                            {
                                resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(LdapAtt.Dn));
                                using (resultWriter.PushSetOf())
                                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(DistinquishedName));
                                sb.Append($"dn={DistinquishedName},");
                            }
                        if (attributes.Contains(LdapAtt.ObjectClass))
                            using (resultWriter.PushSequence())
                            {
                                resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(LdapAtt.ObjectClass));
                                using (resultWriter.PushSetOf())
                                {
                                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes("top"));
                                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(ObjectClass));
                                }
                            }
                        if (attributes.Contains(LdapAtt.EntryUuid))
                            using (resultWriter.PushSequence())
                            {
                                resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(LdapAtt.EntryUuid));
                                using (resultWriter.PushSetOf())
                                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(Id.ToString()));
                                sb.Append($"entryuuid={Id},");
                            }
                        if (attributes.Contains(LdapAtt.Uid) && !string.IsNullOrWhiteSpace(UserName))
                            using (resultWriter.PushSequence())
                            {
                                resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(LdapAtt.Uid));
                                using (resultWriter.PushSetOf())
                                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(UserName!));
                                sb.Append($"uid={UserName},");
                            }
                        if (attributes.Contains(LdapAtt.Mail) && !string.IsNullOrWhiteSpace(Mail))
                            using (resultWriter.PushSequence())
                            {
                                resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(LdapAtt.Mail));
                                using (resultWriter.PushSetOf())
                                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(Mail));
                                sb.Append($"mail={Mail},");
                            }
                        if (attributes.Contains(LdapAtt.Cn) && !string.IsNullOrWhiteSpace(DisplayName))
                            using (resultWriter.PushSequence())
                            {
                                resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(LdapAtt.Cn));
                                using (resultWriter.PushSetOf())
                                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(DisplayName));
                                sb.Append($"cn={DisplayName},");
                            }
                        if (attributes.Contains(LdapAtt.DisplayName) && !string.IsNullOrWhiteSpace(DisplayName))
                            using (resultWriter.PushSequence())
                            {
                                resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(LdapAtt.DisplayName));
                                using (resultWriter.PushSetOf())
                                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(DisplayName));
                                sb.Append($"displayName={DisplayName},");
                            }
                        if (attributes.Contains(LdapAtt.GivenName) && !string.IsNullOrWhiteSpace(FirstName))
                            using (resultWriter.PushSequence())
                            {
                                resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(LdapAtt.GivenName));
                                using (resultWriter.PushSetOf())
                                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(FirstName));
                                sb.Append($"givenName={FirstName},");
                            }
                        if (attributes.Contains(LdapAtt.Sn) && !string.IsNullOrWhiteSpace(LastName))
                            using (resultWriter.PushSequence())
                            {
                                resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(LdapAtt.Sn));
                                using (resultWriter.PushSetOf())
                                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(LastName));
                                sb.Append($"sn={LastName},");
                            }

                        var linkedAttributeName = IsGroup ? LdapAtt.Member : LdapAtt.MemberOf;
                        if (attributes.Contains(linkedAttributeName) && LinkedEntries.Any())
                        {
                            var linkedAttributeNameBytes = Encoding.UTF8.GetBytes(linkedAttributeName);
                            using (resultWriter.PushSequence())
                            {
                                resultWriter.WriteOctetString(linkedAttributeNameBytes);
                                using (resultWriter.PushSetOf())
                                    foreach (var entry in LinkedEntries)
                                    {
                                        resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(entry));
                                        sb.Append($"{linkedAttributeName}={entry},");
                                    }
                            }
                        }
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine(sb);
            return resultWriter.Encode();
        }
    }
    public static class Db
    {
        public static readonly List<LdapEntry> FlatEntries = new();
        static Db()
        {
            // Groups
            var adminGroup = new LdapEntry(true, "Admins", null, null, null, null);
            var userGroup = new LdapEntry(true, "Users", null, null, null, null);

            // Users
            var tonko = new LdapEntry(false, "Tonko Raf", "trafajac", "Tonko", "Raf", "Tonko.Raf@gmail.com");
            var miro = new LdapEntry(false, "Miroslav Ivancevic", "mivancev", "Miroslav", "Ivancevic", "mivancev@gmail.com");
            var hole = new LdapEntry(false, "Darko Hosko", "dhosko", "Darko", "Hosko", "Darko.Hosko@gmail.com");

            // Joins

            // Admins-Tonko
            adminGroup.LinkedEntries.Add(tonko.DistinquishedName);
            tonko.LinkedEntries.Add(adminGroup.DistinquishedName);
            // Users-Tonko
            userGroup.LinkedEntries.Add(tonko.DistinquishedName);
            tonko.LinkedEntries.Add(userGroup.DistinquishedName);
            // Users-Miro
            userGroup.LinkedEntries.Add(miro.DistinquishedName);
            miro.LinkedEntries.Add(userGroup.DistinquishedName);
            // Users-Hole
            userGroup.LinkedEntries.Add(hole.DistinquishedName);
            hole.LinkedEntries.Add(userGroup.DistinquishedName);

            FlatEntries.Add(adminGroup);
            FlatEntries.Add(userGroup);
            FlatEntries.Add(tonko);
            FlatEntries.Add(miro);
            FlatEntries.Add(hole);
        }
    }
}