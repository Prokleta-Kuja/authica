namespace authica.Translations;

public interface IUsers : IStandard
{
    string ListTitle { get; }
    string AddTitle { get; }
    string EditTitle { get; }
    string RolesTitle { get; }
    string TableId { get; }
    string TableEmail { get; }
    string TableUserName { get; }
    string TableCreated { get; }
    string TableDisabled { get; }
    string TableLastLogin { get; }
    string RowDisabled { get; }
    string RowLastLogin { get; }
    string Email { get; }
    string UserName { get; }
    string FirstName { get; }
    string LastName { get; }
    string TimeZone { get; }
    string Locale { get; }
    string Password { get; }
    string NewPassword { get; }
    string IsAdmin { get; }
    string Disabled { get; }
    string ChooseRole { get; }
}
public class Users_en : Standard_en, IUsers
{
    public string ListTitle => "Users";
    public string AddTitle => "Add User";
    public string EditTitle => "Edit User";
    public string RolesTitle => "Roles";
    public string TableId => "Id";
    public string TableEmail => "Email";
    public string TableUserName => "Username";
    public string TableCreated => "Created";
    public string TableDisabled => "Disabled";
    public string TableLastLogin => "Last login";
    public string RowDisabled => "No";
    public string RowLastLogin => "Never";
    public string Email => "Email";
    public string UserName => "Username";
    public string FirstName => "First Name";
    public string LastName => "Last Name";
    public string TimeZone => "Time Zone";
    public string Locale => "Locale";
    public string Password => "Password";
    public string NewPassword => "New Password";
    public string IsAdmin => "Is Admin";
    public string Disabled => "Disabled";
    public string ChooseRole => "Choose role";
}
public class Users_hr : Standard_hr, IUsers
{
    public string ListTitle => "Korisnici";
    public string AddTitle => "Dodaj korisnika";
    public string EditTitle => "Izmijeni korisnika";
    public string RolesTitle => "Uloge";
    public string TableId => "Šifra";
    public string TableEmail => "Email";
    public string TableUserName => "Korisničko ime";
    public string TableCreated => "Dodano";
    public string TableDisabled => "Onemogućeno";
    public string TableLastLogin => "Zadnja prijava";
    public string RowDisabled => "Ne";
    public string RowLastLogin => "Nikad";
    public string Email => "Email";
    public string UserName => "Korisničko ime";
    public string FirstName => "Ime";
    public string LastName => "Prezime";
    public string TimeZone => "Vremenska zona";
    public string Locale => "Lokalitet";
    public string Password => "Lozinka";
    public string NewPassword => "Nova lozinka";
    public string IsAdmin => "Je administrator";
    public string Disabled => "Onemogućen";
    public string ChooseRole => "Odaberi ulogu";
}