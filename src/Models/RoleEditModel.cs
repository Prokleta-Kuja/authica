using System;
using System.Collections.Generic;
using System.Linq;
using authica.Entities;
using authica.Translations;

namespace authica.Models;

public class RoleEditModel
{
    public int RoleId { get; set; }
    public Guid AliasId { get; set; }
    public string? Name { get; set; }
    public bool Disabled { get; set; }
    public RoleEditModel(Role r)
    {
        RoleId = r.RoleId;
        AliasId = r.AliasId;
        Name = r.Name;
        Disabled = r.Disabled.HasValue;
    }
    public Dictionary<string, string>? Validate(IRoles translation, HashSet<string> names)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add(nameof(Name), translation.ValidationRequired);
        else if (names.Contains(Name.ToUpper()))
            errors.Add(nameof(Name), translation.ValidationDuplicate);

        return errors.Any() ? errors : null;
    }
}