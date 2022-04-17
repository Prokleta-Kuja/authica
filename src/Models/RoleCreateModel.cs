using System.Collections.Generic;
using System.Linq;
using authica.Translations;

namespace authica.Models;

public class RoleCreateModel
{
    public string? Name { get; set; }
    public bool Disabled { get; set; }
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