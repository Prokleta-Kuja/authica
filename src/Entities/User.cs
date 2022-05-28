using System;
using System.Collections.Generic;
using authica.Services;

namespace authica.Entities;

public class User
{
    User()
    {
        Email = null!;
        EmailNormalized = null!;
        UserName = null!;
        UserNameNormalized = null!;
        FirstName = null!;
        LastName = null!;
    }
    internal User(string email, string? userName, string? firstName = null, string? lastName = null)
    {
        var index = email.IndexOf('@');
        var emailValid = index > 0 && index != email.Length - 1 && index == email.LastIndexOf('@');
        if (!emailValid)
            throw new ArgumentException("Invalid", nameof(email));

        AliasId = Guid.NewGuid();
        Email = email.ToLowerInvariant();
        EmailNormalized = C.Normalize(email);
        UserName = userName ?? email[..index];
        UserNameNormalized = C.Normalize(UserName);
        FirstName = firstName;
        LastName = lastName;
        Created = DateTime.UtcNow;
    }

    public int UserId { get; set; }
    public Guid AliasId { get; set; }
    public string Email { get; set; }
    public string EmailNormalized { get; set; }
    public bool EmailVerified { get; set; }
    public string UserName { get; set; }
    public string UserNameNormalized { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? TimeZone { get; set; }
    public string? Locale { get; set; }
    public string? PasswordHash { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Disabled { get; set; }
    public DateTime? LastLogin { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();

    public User SetPassword(string newPassword, IPasswordHasher hasher)
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