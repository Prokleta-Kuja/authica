namespace authica.Translations
{
    public interface IConfiguration : IStandard
    {
        string DetailsSection { get; }
        string GeoSection { get; }
        string SmtpSection { get; }

        string Name { get; }
        string HostName { get; }
        string Domain { get; }
        string MaxSessionDuration { get; }
        string MaxInfractions { get; }
        string InfractionExpiration { get; }
        string BanTime { get; }

        string MaxMindLicense { get; }
        string AllowedCountryCodes { get; }

        string SmtpNotSetup { get; }
        string SmtpHost { get; }
        string SmtpPort { get; }
        string SmtpSsl { get; }
        string SmtpTimeout { get; }
        string SmtpUser { get; }
        string SmtpPassword { get; }
        string SmtpFromName { get; }
        string SmtpFromAddress { get; }
        string SmtpSubjectPrefix { get; }
        string SmtpTestEmailAddress { get; }
        string SmtpTestSend { get; }
    }
    public class Configuration_en : Standard_en, IConfiguration
    {
        public string DetailsSection => "Details";
        public string GeoSection => "Geo blocking";
        public string SmtpSection => "SMTP";

        public string Name => "Name";
        public string HostName => "Url";
        public string Domain => "Domain";
        public string MaxSessionDuration => "Session duration";
        public string MaxInfractions => "Max infractions";
        public string InfractionExpiration => "Infraction expiration";
        public string BanTime => "Ban time";

        public string MaxMindLicense => "License key";
        public string AllowedCountryCodes => "Allowed country codes";

        public string SmtpNotSetup => "Not configured. Reset password functionality disabled.";
        public string SmtpHost => "Host";
        public string SmtpPort => "Port";
        public string SmtpSsl => "Use ssl";
        public string SmtpTimeout => "Timeout";
        public string SmtpUser => "User";
        public string SmtpPassword => "Password";
        public string SmtpFromName => "From name";
        public string SmtpFromAddress => "From email";
        public string SmtpSubjectPrefix => "Subject prefix";
        public string SmtpTestEmailAddress => "Test email address";
        public string SmtpTestSend => "Send test email";
    }
    public class Configuration_hr : Standard_hr, IConfiguration
    {
        public string DetailsSection => "Detalji";
        public string GeoSection => "Geo blokiranje";
        public string SmtpSection => "SMTP";

        public string Name => "Naziv";
        public string HostName => "Url";
        public string Domain => "Domena";
        public string MaxSessionDuration => "Duljina sesije";
        public string MaxInfractions => "Maksimalno prekršaja";
        public string InfractionExpiration => "Istek prekršaja nakon";
        public string BanTime => "Duljina zabrane";

        public string MaxMindLicense => "Licenčni ključ";
        public string AllowedCountryCodes => "Kodovi dozvoljenih zemalja";

        public string SmtpNotSetup => "Nije postavljeno. Ponovno postaljanje lozinke neće biti moguće.";
        public string SmtpHost => "Poslužitelj";
        public string SmtpPort => "Port";
        public string SmtpSsl => "Ssl";
        public string SmtpTimeout => "Vremensko ograničenje";
        public string SmtpUser => "Korisnik";
        public string SmtpPassword => "Lozinka";
        public string SmtpFromName => "Od";
        public string SmtpFromAddress => "Od email adrese";
        public string SmtpSubjectPrefix => "Prefiks naslova";
        public string SmtpTestEmailAddress => "Test email adresa";
        public string SmtpTestSend => "Pošalji testni email";
    }
}