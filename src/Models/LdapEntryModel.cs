using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using authica.Entities;

namespace authica.Models
{
    public class LdapEntryModel
    {
        const string ObjectClassUser = "inetOrgPerson";
        const string ObjectClassGroup = "groupOfNames";

        public bool IsGroup { get; set; }
        public string ObjectClass { get; set; }
        public string EntryUUID { get; set; }
        public string Cn { get; set; }
        public string Dn { get; set; }
        public string DisplayName { get; set; }
        public string? Uid { get; set; }
        public string? Mail { get; set; }
        public string? GivenName { get; set; }
        public string? Sn { get; set; }
        public HashSet<string> LinkedEntries { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);

        public LdapEntryModel(User u, string appName)
        {
            if (u.Disabled.HasValue)
                throw new InvalidOperationException("User is disabled");

            ObjectClass = "inetOrgPerson";
            EntryUUID = u.AliasId.ToString();
            Cn = $"{u.FirstName} {u.LastName}";
            Uid = u.UserName;
            Dn = $"uid={Uid},dc={appName},dc=authica";
            DisplayName = Cn;
            Mail = u.Email;
            GivenName = u.FirstName;
            Sn = u.LastName;
        }
        public LdapEntryModel(Role r, string appName)
        {
            if (r.Disabled.HasValue)
                throw new InvalidOperationException("Role is disabled");

            IsGroup = true;
            ObjectClass = "groupOfNames";
            EntryUUID = r.AliasId.ToString();
            Cn = r.Name;
            Dn = $"cn={Cn},dc={appName},dc=authica";
            DisplayName = r.Name;
        }
        public static (string App, string User) ParseDn(string dn)
        {
            var app = string.Empty;
            var user = string.Empty;
            var dnParts = dn.Split(',').ToList();

            switch (dnParts.Count)
            {
                case 3: // User - uid=admin,dc=INTERNAL_APP,dc=authica
                    user = dnParts[0].Substring(dnParts[0].IndexOf('=') + 1);
                    app = dnParts[1].Substring(dnParts[1].IndexOf('=') + 1);
                    break;
                case 2: // App - dc=INTERNAL_APP,dc=authica
                    app = dnParts[0].Substring(dnParts[0].IndexOf('=') + 1);
                    break;
                default:
                    break;
            }

            return (app, user);
        }
        public byte[] GetSearchResultBytes(int messageId, bool typesOnly, HashSet<string> attributes)
        {
            var resultWriter = new AsnWriter(AsnEncodingRules.BER);
            using (resultWriter.PushSequence())
            {
                resultWriter.WriteInteger(messageId);
                using (resultWriter.PushSequence(C.Ldap.Tags.SearchResult))
                {
                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(Dn));
                    using (resultWriter.PushSequence()) // Attributes
                    {
                        if (attributes.Contains(C.Ldap.Attributes.ObjectClass))
                            using (resultWriter.PushSequence())
                            {
                                resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(C.Ldap.Attributes.ObjectClass));
                                using (resultWriter.PushSetOf())
                                {
                                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes("top"));
                                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(ObjectClass));
                                }
                            }
                        if (attributes.Contains(C.Ldap.Attributes.EntryUuid))
                            using (resultWriter.PushSequence())
                            {
                                resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(C.Ldap.Attributes.EntryUuid));
                                using (resultWriter.PushSetOf())
                                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(EntryUUID));
                            }
                        if (attributes.Contains(C.Ldap.Attributes.Cn) && !string.IsNullOrWhiteSpace(Cn))
                            using (resultWriter.PushSequence())
                            {
                                resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(C.Ldap.Attributes.Cn));
                                using (resultWriter.PushSetOf())
                                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(Cn));
                            }
                        if (attributes.Contains(C.Ldap.Attributes.Dn))
                            using (resultWriter.PushSequence())
                            {
                                resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(C.Ldap.Attributes.Dn));
                                using (resultWriter.PushSetOf())
                                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(Dn));
                            }
                        if (attributes.Contains(C.Ldap.Attributes.DisplayName) && !string.IsNullOrWhiteSpace(DisplayName))
                            using (resultWriter.PushSequence())
                            {
                                resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(C.Ldap.Attributes.DisplayName));
                                using (resultWriter.PushSetOf())
                                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(DisplayName));
                            }
                        if (attributes.Contains(C.Ldap.Attributes.Uid) && !string.IsNullOrWhiteSpace(Uid))
                            using (resultWriter.PushSequence())
                            {
                                resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(C.Ldap.Attributes.Uid));
                                using (resultWriter.PushSetOf())
                                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(Uid));
                            }
                        if (attributes.Contains(C.Ldap.Attributes.Mail) && !string.IsNullOrWhiteSpace(Mail))
                            using (resultWriter.PushSequence())
                            {
                                resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(C.Ldap.Attributes.Mail));
                                using (resultWriter.PushSetOf())
                                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(Mail));
                            }
                        if (attributes.Contains(C.Ldap.Attributes.GivenName) && !string.IsNullOrWhiteSpace(GivenName))
                            using (resultWriter.PushSequence())
                            {
                                resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(C.Ldap.Attributes.GivenName));
                                using (resultWriter.PushSetOf())
                                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(GivenName));
                            }
                        if (attributes.Contains(C.Ldap.Attributes.Sn) && !string.IsNullOrWhiteSpace(Sn))
                            using (resultWriter.PushSequence())
                            {
                                resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(C.Ldap.Attributes.Sn));
                                using (resultWriter.PushSetOf())
                                    resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(Sn));
                            }

                        var linkedAttributeName = IsGroup ? C.Ldap.Attributes.Member : C.Ldap.Attributes.MemberOf;
                        if (attributes.Contains(linkedAttributeName) && LinkedEntries.Any())
                        {
                            var linkedAttributeNameBytes = Encoding.UTF8.GetBytes(linkedAttributeName);
                            using (resultWriter.PushSequence())
                            {
                                resultWriter.WriteOctetString(linkedAttributeNameBytes);
                                using (resultWriter.PushSetOf())
                                    foreach (var entry in LinkedEntries)
                                        resultWriter.WriteOctetString(Encoding.UTF8.GetBytes(entry));
                            }
                        }
                    }
                }
            }

            return resultWriter.Encode();
        }

        public override bool Equals(object? obj)
        {
            return obj is LdapEntryModel entry && Equals(entry);
        }

        public bool Equals(LdapEntryModel entry)
        {
            return EntryUUID == entry.EntryUUID;
        }

        public override int GetHashCode()
        {
            return EntryUUID.GetHashCode();
        }

        public override string? ToString()
        {
            return Dn;
        }
    }
}