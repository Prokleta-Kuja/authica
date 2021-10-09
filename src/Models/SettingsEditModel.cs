using System;
using System.Collections.Generic;
using System.Linq;
using authica.Translations;

namespace authica.Models
{
    public class SettingsEditModel
    {
        public string? Name { get; set; }
        public string? HostName { get; set; }
        public string? Domain { get; set; }
        public int? MaxInfractions { get; set; }
        public TimeSpan? InfractionExpiration { get; set; }
        public TimeSpan? BanTime { get; set; }
        public TimeSpan? MaxSessionDuration { get; set; }
        public string? MaxMindLicenseKey { get; set; }
        public HashSet<string> AllowedCountryCodes { get; set; }
        public string? SmtpHost { get; set; }
        public int? SmtpPort { get; set; }
        public bool SmtpSsl { get; set; }
        public TimeSpan? SmtpTimeout { get; set; }
        public string? SmtpUser { get; set; }
        public string? SmtpPassword { get; set; }
        public string? SmtpFromName { get; set; }
        public string? SmtpFromAddress { get; set; }
        public string? SmtpSubjectPrefix { get; set; }
        public string? Key { get; set; }

        public bool IsMailSetup => SmtpPort.HasValue
            && !string.IsNullOrWhiteSpace(SmtpHost)
            && !string.IsNullOrWhiteSpace(SmtpPassword)
            && !string.IsNullOrWhiteSpace(SmtpFromName)
            && !string.IsNullOrWhiteSpace(SmtpFromAddress);

        public SettingsEditModel(Settings s)
        {
            Name = s.Name;
            HostName = s.HostName;
            Domain = s.Domain;
            MaxInfractions = s.MaxInfractions;
            InfractionExpiration = s.InfractionExpiration;
            BanTime = s.BanTime;
            MaxSessionDuration = s.MaxSessionDuration;
            MaxMindLicenseKey = s.MaxMindLicenseKey;
            AllowedCountryCodes = s.AllowedCountryCodes;
            SmtpHost = s.SmtpHost;
            SmtpPort = s.SmtpPort;
            SmtpSsl = s.SmtpSsl;
            SmtpTimeout = s.SmtpTimeout;
            SmtpUser = s.SmtpUser;
            SmtpPassword = s.SmtpPassword;
            SmtpFromName = s.SmtpFromName;
            SmtpFromAddress = s.SmtpFromAddress;
            SmtpSubjectPrefix = s.SmtpSubjectPrefix;
        }

        public Settings Convert()
        {
            var s = new Settings();
            s.Name = Name!;
            s.HostName = HostName!;
            s.Domain = Domain!;
            s.MaxInfractions = MaxInfractions!.Value;
            s.InfractionExpiration = InfractionExpiration!.Value;
            s.BanTime = BanTime!.Value;
            s.MaxSessionDuration = MaxSessionDuration!.Value;
            s.MaxMindLicenseKey = MaxMindLicenseKey;
            s.AllowedCountryCodes = AllowedCountryCodes;
            s.SmtpHost = SmtpHost;
            s.SmtpPort = SmtpPort;
            s.SmtpSsl = SmtpSsl;
            s.SmtpTimeout = SmtpTimeout!.Value;
            s.SmtpUser = SmtpUser;
            s.SmtpPassword = SmtpPassword;
            s.SmtpFromName = SmtpFromName!;
            s.SmtpFromAddress = SmtpFromAddress!;
            s.SmtpSubjectPrefix = SmtpSubjectPrefix!;

            return s;
        }
        public Dictionary<string, string>? Validate(IConfiguration translation)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add(nameof(Name), translation.ValidationRequired);

            if (string.IsNullOrWhiteSpace(HostName))
                errors.Add(nameof(HostName), translation.ValidationRequired);

            if (string.IsNullOrWhiteSpace(Domain))
                errors.Add(nameof(Domain), translation.ValidationRequired);

            if (!MaxInfractions.HasValue)
                errors.Add(nameof(MaxInfractions), translation.ValidationRequired);
            else if (MaxInfractions.Value < 0)
                errors.Add(nameof(MaxInfractions), translation.ValidationInvalid);

            if (!InfractionExpiration.HasValue)
                errors.Add(nameof(InfractionExpiration), translation.ValidationRequired);

            if (!BanTime.HasValue)
                errors.Add(nameof(BanTime), translation.ValidationRequired);

            if (!MaxSessionDuration.HasValue)
                errors.Add(nameof(MaxSessionDuration), translation.ValidationRequired);

            // TODO:
            //    s.AllowedCountryCodes = AllowedCountryCodes;

            if (SmtpPort.HasValue && (SmtpPort.Value <= 0 || SmtpPort.Value > UInt16.MaxValue))
                errors.Add(nameof(SmtpPort), translation.ValidationInvalid);

            if (!SmtpTimeout.HasValue)
                errors.Add(nameof(SmtpTimeout), translation.ValidationRequired);

            if (!string.IsNullOrWhiteSpace(SmtpUser) && string.IsNullOrWhiteSpace(SmtpPassword))
                errors.Add(nameof(SmtpPassword), translation.ValidationRequired);
            else if (string.IsNullOrWhiteSpace(SmtpUser) && !string.IsNullOrWhiteSpace(SmtpPassword))
                errors.Add(nameof(SmtpUser), translation.ValidationRequired);

            if (string.IsNullOrWhiteSpace(SmtpFromName))
                errors.Add(nameof(SmtpFromName), translation.ValidationRequired);

            if (string.IsNullOrWhiteSpace(SmtpFromAddress))
                errors.Add(nameof(SmtpFromAddress), translation.ValidationRequired);
            else
            {
                var index = SmtpFromAddress.IndexOf('@');
                var emailValid = index > 0 && index != SmtpFromAddress.Length - 1 && index == SmtpFromAddress.LastIndexOf('@');
                if (!emailValid)
                    errors.Add(nameof(SmtpFromAddress), translation.ValidationInvalid);
            }

            if (string.IsNullOrWhiteSpace(SmtpSubjectPrefix))
                errors.Add(nameof(SmtpSubjectPrefix), translation.ValidationRequired);

            return errors.Any() ? errors : null;
        }
    }
}