using System;
using System.Collections.Generic;
using System.Linq;
using authica.Translations;

namespace authica.Models;

public class AppEditModel
{
    public int AppId { get; set; }
    public Guid AliasId { get; set; }
    public string? Name { get; set; }
    public string? AuthorityUri { get; set; }
    public string? RedirectUri { get; set; }
    public string? NewSecret { get; set; }
    public bool AllowAllUsers { get; set; } = true;
    public bool LdapEnabled { get; set; }
    public bool Disabled { get; set; }
    public AppEditModel(Entities.App a)
    {
        AppId = a.AppId;
        AliasId = a.AliasId;
        Name = a.Name;
        AuthorityUri = a.AuthorityUri;
        RedirectUri = a.RedirectUri;
        AllowAllUsers = a.AllowAllUsers;
        LdapEnabled = a.LdapEnabled;
        Disabled = a.Disabled.HasValue;
    }
    public Dictionary<string, string>? Validate(IApps translation, HashSet<string> names, int minPassLength, int maxPassLength)
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

        if (!string.IsNullOrWhiteSpace(NewSecret))
            if (NewSecret.Length < minPassLength)
                errors.Add(nameof(NewSecret), translation.ValidationTooShort(minPassLength));
            else if (NewSecret.Length > maxPassLength)
                errors.Add(nameof(NewSecret), translation.ValidationTooLong(maxPassLength));

        if (!string.IsNullOrWhiteSpace(RedirectUri) && !Uri.TryCreate(RedirectUri, UriKind.Relative, out var _))
            errors.Add(nameof(RedirectUri), translation.ValidationInvalid);

        return errors.Any() ? errors : null;
    }
}