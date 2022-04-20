using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace authica;

public static class C
{
    public static string Normalize(string text) => text.Trim().Replace(' ', '_').ToUpperInvariant();
    public static FileInfo GeoLocationDbFile { get; private set; } = new(C.Paths.AppDataFor("GeoLite2-Country.mmdb"));
    public static class Env
    {
        public static string Locale => Environment.GetEnvironmentVariable("LOCALE") ?? "en-US";
        public static string TimeZone => Environment.GetEnvironmentVariable("TZ") ?? "Europe/Zagreb";
        public static string AdminPassword => Environment.GetEnvironmentVariable("PASSWORD") ?? "P@ssw0rd";
    }
    public static class Routes
    {
        public const string Root = "/";
        public const string SignIn = "/sign-in";
        public const string SignOut = "/sign-out";
        public const string ResetPassword = "/reset-password";
        public const string Forbidden = "/forbidden";
        public const string MyProfile = "/my-profile";
        public const string Apps = "/apps";
        public const string App = "/apps/{AliasId:guid}";
        public static string AppsFor(Guid aliasId) => $"{Apps}/{aliasId}";
        public const string Users = "/users";
        public const string User = "/users/{AliasId:guid}";
        public static string UsersFor(Guid aliasId) => $"{Users}/{aliasId}";
        public const string Roles = "/roles";
        public const string Role = "/roles/{AliasId:guid}";
        public static string RolesFor(Guid aliasId) => $"{Roles}/{aliasId}";
        public const string Configuration = "/configuration";
        public const string Shutdown = "/shutdown";
        public const string Restart = "/shutdown/restart";
        public const string VerifyNginx = "/nginx-verify";
    }
    public static class Headers
    {
        public const string OriginalUrl = "X-Original-URL";
    }
    public static class Paths
    {
        public static string AppData => Path.Combine(Environment.CurrentDirectory, "appdata");
        public static string AppDataFor(string file) => Path.Combine(AppData, file);
        public static readonly string AppDbConnectionString = $"Data Source={AppDataFor("app.db")}";
    }
    public static class Ldap
    {
        public static class Tags
        {
            public static readonly Asn1Tag BindRequest = new(TagClass.Application, 0);
            public static readonly Asn1Tag BindResponse = new(TagClass.Application, 1);
            public static readonly Asn1Tag AuthenticationSimple = new(TagClass.ContextSpecific, 0);
            public static readonly Asn1Tag SearchRequest = new(TagClass.Application, 3);
            public static readonly Asn1Tag SearchResult = new(TagClass.Application, 4);
            public static readonly Asn1Tag SearchDone = new(TagClass.Application, 5);
        }
        public static class Attributes
        {
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
        public enum ResultCode
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
    public static class Configuration
    {
        static readonly FileInfo file = new(Paths.AppDataFor("configuration.json"));
        static readonly JsonSerializerOptions serializerOptions = new()
        {
            WriteIndented = true,
            IgnoreReadOnlyProperties = true,
        };
        public static Settings Current { get; private set; } = new();
        public static async ValueTask LoadAsync()
        {
            if (file.Exists)
            {
                Current = await LoadFromDiskAsync();
                Current.LoadKeys();
            }
            else
            {
                Current.LoadKeys();
                await SaveToDiskAsync(Current);
            }
        }
        public static async Task<Settings> LoadFromDiskAsync()
        {
            var contents = await File.ReadAllTextAsync(file.FullName);
            var settings = JsonSerializer.Deserialize<Settings>(contents) ?? throw new JsonException("Could not load configuration file");
            return settings;
        }
        public static async ValueTask SaveToDiskAsync(Settings settings)
        {
            var contents = JsonSerializer.Serialize(settings, serializerOptions);
            await File.WriteAllTextAsync(file.FullName, contents);
        }
    }
}
public class Settings
{
    private readonly RSA _rsa;
    public Settings()
    {
        _rsa = RSA.Create();
        SmtpFromName = Issuer;
        SmtpFromAddress = $"authica@{Domain}";
    }
    public string Issuer { get; set; } = "authica";
    public string Domain { get; set; } = Environment.GetEnvironmentVariable("DOMAIN") ?? "localhost";
    public string HostName { get; set; } = "http://localhost:5000";
    public bool EnableLdap { get; set; }
    public int MinPasswordLength { get; set; } = 16;
    public int MaxPasswordLength { get; set; } = 64;
    public int MaxInfractions { get; set; } = 5;
    public TimeSpan InfractionExpiration { get; set; } = TimeSpan.FromMinutes(15);
    public TimeSpan BanTime { get; set; } = TimeSpan.FromHours(12);
    public TimeSpan MaxSessionDuration { get; set; } = TimeSpan.FromHours(2);
    public string? MaxMindLicenseKey { get; set; }
    public HashSet<string> AllowedCountryCodes { get; set; } = new();
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public bool SmtpSsl { get; set; }
    public TimeSpan SmtpTimeout { get; set; } = TimeSpan.FromSeconds(10);
    public string? SmtpUser { get; set; }
    public string? SmtpPassword { get; set; }
    public string SmtpFromName { get; set; }
    public string SmtpFromAddress { get; set; }
    public string SmtpSubjectPrefix { get; set; } = "[authica] - ";

    public string? Key { get; set; }
    public RSA SecurityKey => _rsa;
    public void LoadKeys()
    {
        if (string.IsNullOrWhiteSpace(Key))
        {
            Key = Convert.ToBase64String(_rsa.ExportRSAPrivateKey());
        }
        else
        {
            _rsa.ImportRSAPrivateKey(Convert.FromBase64String(Key), out var _);
        }
    }
}