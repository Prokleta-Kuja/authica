using System;
using System.Collections.Generic;
using System.Linq;
using authica.Translations;

namespace authica.Models
{
    public class AppEditModel
    {
        public int AppId { get; set; }
        public Guid AliasId { get; set; }
        public string? Name { get; set; }
        public string? RedirectUri { get; set; }
        public string? NewSecret { get; set; }
        public bool AllowAllUsers { get; set; } = true;
        public bool Disabled { get; set; }
        public AppEditModel(Entities.App a)
        {
            AppId = a.AppId;
            AliasId = a.AliasId;
            Name = a.Name;
            RedirectUri = a.RedirectUri;
            AllowAllUsers = a.AllowAllUsers;
            Disabled = a.Disabled.HasValue;
        }
        public Dictionary<string, string>? Validate(IApps translation, HashSet<string> names)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add(nameof(Name), translation.ValidationRequired);
            else if (names.Contains(Name.ToUpper()))
                errors.Add(nameof(Name), translation.ValidationDuplicate);

            if (string.IsNullOrWhiteSpace(RedirectUri))
                errors.Add(nameof(RedirectUri), translation.ValidationRequired);

            return errors.Any() ? errors : null;
        }
    }
}