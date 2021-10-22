using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace authica
{
    public static class C
    {
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
        }
        public static class Paths
        {
            public static string AppData => Path.Combine(Environment.CurrentDirectory, "appdata");
            public static string AppDataFor(string file) => Path.Combine(AppData, file);
            public static readonly string AppDbConnectionString = $"Data Source={AppDataFor("app.db")}";
        }
        public static class Configuration
        {
            static FileInfo file = new(Paths.AppDataFor("configuration.json"));
            static JsonSerializerOptions serializerOptions = new()
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
        private RSA _rsa;
        public Settings()
        {
            _rsa = RSA.Create();
            SmtpFromName = Issuer;
            SmtpFromAddress = $"authica@{Domain}";
        }
        public string Issuer { get; set; } = "authica";
        public string Domain { get; set; } = Environment.GetEnvironmentVariable("DOMAIN") ?? "localhost";
        public string HostName { get; set; } = "http://localhost:5000";
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
}