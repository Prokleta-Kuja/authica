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
    string LdapDescription { get; }
    string LdapGeoWarning { get; }
    string LdapTlsWarning { get; }
    string Dns { get; }
    string BaseDN { get; }
    string AppDN { get; }
    string ExampleUserDN { get; }
    string ExampleGroupDN { get; }
    string UserAttributes { get; }
    string UniqueId { get; }
    string Username { get; }
    string Mail { get; }
    string DisplayName { get; }
    string FirstName { get; }
    string LastName { get; }
    string MemberOf { get; }
    string GroupAttributes { get; }
    string Member { get; }
    string ToastAdded { get; }
    string ToastSaved {get;}
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
    public string LdapDescription => "Enable this if the app doesn't support any newer protocols or requires LDAP.";
    public string LdapGeoWarning => "Geo blocking isn't enabled for LDAP access since the app will authenticate on behalf of its users.";
    public string LdapTlsWarning => "TLS isn't enabled so be sure to secure it with TLS in your reverse proxy.";
    public string Dns => "DN's";
    public string BaseDN => "Base DN";
    public string AppDN => "App DN";
    public string ExampleUserDN => "Example user DN";
    public string ExampleGroupDN => "Example group DN";
    public string UserAttributes => "User attributes";
    public string UniqueId => "Unique ID";
    public string Username => "Username";
    public string Mail => "Mail";
    public string DisplayName => "Display name";
    public string FirstName => "First name";
    public string LastName => "Last name";
    public string MemberOf => "List of all group DN it is a member of";
    public string GroupAttributes => "Group attributes";
    public string Member => "List of all user member DN";
    public string ToastAdded => "App has been added.";
    public string ToastSaved => "App saved.";
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
    public string LdapDescription => "Omogućiti samo ako app ne pordržava novije protokole ili zahtjeva LDAP";
    public string LdapGeoWarning => "Geo blokiranje neće biti omogućeno za LDAP pristup obzirom da će app autorizirati svoje korisnike.";
    public string LdapTlsWarning => "TLS nije omogućen stoga se preporućuje osigurati ga preko reverznog posrednika.";
    public string Dns => "DN-ovi";
    public string BaseDN => "Bazni DN";
    public string AppDN => "App DN";
    public string ExampleUserDN => "Primjer korisničkog DN";
    public string ExampleGroupDN => "Primjer grupnog DN";
    public string UserAttributes => "Korisnički atributi";
    public string UniqueId => "Identifikator";
    public string Username => "Korisničko ime";
    public string Mail => "Mail";
    public string DisplayName => "Naziv/ime za prikaz";
    public string FirstName => "Ime";
    public string LastName => "Prezime";
    public string MemberOf => "Popis svih grupnih DN-ova čiji je korisnik član";
    public string GroupAttributes => "Grupni atributi";
    public string Member => "Popis svih korisničkih DN-ova koji su članovi grupe";
    public string ToastAdded => "App je dodana.";
    public string ToastSaved => "App je spremljena.";
}