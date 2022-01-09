using System;
using System.Collections.Generic;
using System.Linq;
using authica.Translations;

namespace authica.Models
{
    public class AppCreateModel
    {
        public string? Name { get; set; }
        public string? AuthorityUri { get; set; }
        public string? RedirectUri { get; set; }
        public string? Secret { get; set; }
        public bool AllowAllUsers { get; set; } = true;
        public bool Disabled { get; set; }
        public Dictionary<string, string>? Validate(IApps translation, HashSet<string> names)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add(nameof(Name), translation.ValidationRequired);
            else if (names.Contains(Name.ToUpper()))
                errors.Add(nameof(Name), translation.ValidationDuplicate);

            if (string.IsNullOrWhiteSpace(AuthorityUri))
                errors.Add(nameof(AuthorityUri), translation.ValidationRequired);
            else if (!Uri.TryCreate(AuthorityUri, UriKind.Absolute, out var full) || !string.IsNullOrWhiteSpace(full.PathAndQuery.Trim('/')))
                errors.Add(nameof(AuthorityUri), translation.ValidationInvalid);

            if (!string.IsNullOrWhiteSpace(RedirectUri) && !Uri.TryCreate(RedirectUri, UriKind.Relative, out var _))
                errors.Add(nameof(RedirectUri), translation.ValidationInvalid);

            return errors.Any() ? errors : null;
        }
    }
}