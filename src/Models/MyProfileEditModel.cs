using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using authica.Entities;
using authica.Translations;

namespace authica.Models;

public class MyProfileEditModel
{
    public int UserId { get; }
    public Guid AliasId { get; }
    public string? Email { get; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? TimeZone { get; set; }
    public string? Locale { get; set; }
    public string? OldPassword { get; set; }
    public string? NewPassword { get; set; }
    public MyProfileEditModel(User u)
    {
        UserId = u.UserId;
        AliasId = u.AliasId;
        Email = u.Email;
        UserName = u.UserName;
        FirstName = u.FirstName;
        LastName = u.LastName;
        TimeZone = u.TimeZone;
        Locale = u.Locale;
    }
    public Dictionary<string, string>? Validate(IMyProfile translation, HashSet<string> usernames, int minPassLength, int maxPassLength)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(Email))
            errors.Add(nameof(Email), translation.ValidationRequired);

        if (string.IsNullOrWhiteSpace(UserName))
            errors.Add(nameof(UserName), translation.ValidationRequired);
        else if (usernames.Contains(UserName.ToUpper()))
            errors.Add(nameof(UserName), translation.ValidationDuplicate);

        if (!string.IsNullOrWhiteSpace(TimeZone))
            try { TimeZoneInfo.FindSystemTimeZoneById(TimeZone); }
            catch (Exception) { errors.Add(nameof(TimeZone), translation.ValidationInvalid); }

        if (!string.IsNullOrWhiteSpace(Locale))
            try { CultureInfo.GetCultureInfo(Locale); }
            catch (Exception) { errors.Add(nameof(Locale), translation.ValidationInvalid); }

        if (!string.IsNullOrWhiteSpace(NewPassword))
            if (string.IsNullOrWhiteSpace(OldPassword))
                errors.Add(nameof(OldPassword), translation.ValidationRequired);
            else if (NewPassword.Length < minPassLength)
                errors.Add(nameof(NewPassword), translation.ValidationTooShort(minPassLength));
            else if (NewPassword.Length > maxPassLength)
                errors.Add(nameof(NewPassword), translation.ValidationTooLong(maxPassLength));

        return errors.Any() ? errors : null;
    }
}