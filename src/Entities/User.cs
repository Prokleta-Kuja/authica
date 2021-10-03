using System;
using System.Collections.Generic;
using authica.Services;

namespace authica.Entities
{
    public class User
    {
        User()
        {
            Email = null!;
            UserName = null!;
            FirstName = null!;
            LastName = null!;
        }
        internal User(string email, string? userName, string? firstName, string? lastName)
        {
            var index = email.IndexOf('@');
            var emailValid = index > 0 && index != email.Length - 1 && index == email.LastIndexOf('@');
            if (!emailValid)
                throw new ArgumentException("Invalid", nameof(email));

            AliasId = Guid.NewGuid();
            Email = email.ToLowerInvariant();
            UserName = userName ?? email.Substring(0, index);
            FirstName = firstName;
            LastName = lastName;
            Created = DateTime.UtcNow;
        }

        public int UserId { get; set; }
        public Guid AliasId { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? TimeZone { get; set; }
        public string? Locale { get; set; }
        public string? PasswordHash { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Disabled { get; set; }
        public DateTime? LastLogin { get; set; }

        public ICollection<UserRole>? UserRoles { get; set; }

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
}