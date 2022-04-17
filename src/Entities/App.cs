using System;
using System.Collections.Generic;
using authica.Services;

namespace authica.Entities;

public class App
{
    App()
    {
        Name = null!;
        NameNormalized = null!;
        AuthorityUri = null!;
    }
    internal App(string name, string authorityUri)
    {
        AliasId = Guid.NewGuid();
        Name = name;
        NameNormalized = C.Normalize(name);
        AuthorityUri = authorityUri;
        Created = DateTime.UtcNow;
    }
    public int AppId { get; set; }
    public Guid AliasId { get; set; }
    public string Name { get; set; }
    public string NameNormalized { get; set; }
    public string AuthorityUri { get; set; }
    public string? RedirectUri { get; set; }
    public string? PasswordHash { get; set; }
    public bool AllowAllUsers { get; set; }
    public bool LdapEnabled { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Disabled { get; set; }

    public virtual ICollection<AppRole> AppRoles { get; set; } = new HashSet<AppRole>();

    public App SetPassword(string newPassword, IPasswordHasher hasher)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
            throw new ArgumentNullException(newPassword);

        PasswordHash = hasher.HashPassword(newPassword);
        return this;
    }

    public bool VerifyPassword(string providedPassword, IPasswordHasher hasher)
    {
        if (string.IsNullOrWhiteSpace(providedPassword))
            throw new ArgumentNullException(providedPassword);

        if (PasswordHash == null)
            throw new InvalidOperationException("Cannot verify password as there is no password hashed");

        var result = hasher.VerifyHashedPassword(PasswordHash, providedPassword);
        switch (result)
        {
            case PasswordVerificationResult.Failed:
                return false;
            case PasswordVerificationResult.SuccessRehashNeeded:
                SetPassword(providedPassword, hasher);
                return true;
            default:
                return true;
        }
    }
}