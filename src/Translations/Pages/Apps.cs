namespace authica.Translations;

public interface IApps : IStandard
{
    string ListTitle { get; }
    string AddTitle { get; }
    string EditTitle { get; }
    string RestrictionsTitle { get; }
    string TableId { get; }
    string TableName { get; }
    string TableCreated { get; }
    string TableDisabled { get; }
    string RowDisabled { get; }
    string Name { get; }
    string AuthorityUri { get; }
    string RedirectUri { get; }
    string Secret { get; }
    string NewSecret { get; }
    string AllowAllUsers { get; }
    string DisallowAllUsers { get; }
    string LdapEnabled { get; }
    string Disabled { get; }
    string ChooseRole { get; }
}
public class Apps_en : Standard_en, IApps
{
    public string ListTitle => "Apps";
    public string AddTitle => "Add App";
    public string EditTitle => "Edit App";
    public string RestrictionsTitle => "Restrictions";
    public string TableId => "Id";
    public string TableName => "Name";
    public string TableCreated => "Created";
    public string TableDisabled => "Disabled";
    public string RowDisabled => "No";
    public string Name => "Name";
    public string AuthorityUri => "Host Uri";
    public string RedirectUri => "Redirect Uri";
    public string Secret => "Secret";
    public string NewSecret => "New Secret";
    public string AllowAllUsers => "Allow all users";
    public string DisallowAllUsers => "Restrict to this roles";
    public string LdapEnabled => "Enable LDAP";
    public string Disabled => "Disabled";
    public string ChooseRole => "Choose role";
}
public class Apps_hr : Standard_hr, IApps
{
    public string ListTitle => "Appovi";
    public string AddTitle => "Dodaj app";
    public string EditTitle => "Izmijeni app";
    public string RestrictionsTitle => "Ograničenja";
    public string TableId => "Šifra";
    public string TableName => "Naziv";
    public string TableCreated => "Dodano";
    public string TableDisabled => "Onemogućeno";
    public string RowDisabled => "Ne";
    public string Name => "Naziv";
    public string AuthorityUri => "Uri";
    public string RedirectUri => "Preusmjeri na uri";
    public string Secret => "Tajna";
    public string NewSecret => "Nova tajna";
    public string AllowAllUsers => "Dozvoli svim korisnicima";
    public string DisallowAllUsers => "Ograniči na uloge";
    public string LdapEnabled => "Omogući LDAP";
    public string Disabled => "Onemogućeno";
    public string ChooseRole => "Odaberi ulogu";
}