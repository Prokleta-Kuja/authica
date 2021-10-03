using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using authica.Translations;

namespace authica.Models
{
    public class UserCreateModel
    {
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? TimeZone { get; set; } = C.Env.TimeZone;
        public string? Locale { get; set; } = C.Env.Locale;
        public string? Password { get; set; }
        public bool IsAdmin { get; set; }
        public bool Disabled { get; set; }
        public Dictionary<string, string>? Validate(IUsers translation, HashSet<string> emails, HashSet<string> usernames)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(Email))
                errors.Add(nameof(Email), translation.ValidationRequired);
            else if (emails.Contains(Email.ToLower()))
                errors.Add(nameof(Email), translation.ValidationDuplicate);

            if (string.IsNullOrWhiteSpace(UserName))
                errors.Add(nameof(UserName), translation.ValidationRequired);
            else if (usernames.Contains(UserName.ToLower()))
                errors.Add(nameof(UserName), translation.ValidationDuplicate);

            if (!string.IsNullOrWhiteSpace(TimeZone))
                try { TimeZoneInfo.FindSystemTimeZoneById(TimeZone); }
                catch (Exception) { errors.Add(nameof(TimeZone), translation.ValidationInvalid); }

            if (!string.IsNullOrWhiteSpace(Locale))
                try { CultureInfo.GetCultureInfo(Locale); }
                catch (Exception) { errors.Add(nameof(Locale), translation.ValidationInvalid); }

            return errors.Any() ? errors : null;
        }
    }
}