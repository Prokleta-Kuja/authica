namespace authica.Translations;

public interface IRoles : IStandard
{
    string ListTitle { get; }
    string AddTitle { get; }
    string EditTitle { get; }
    string AppsTitle { get; }
    string UsersTitle { get; }
    string TableId { get; }
    string TableName { get; }
    string TableDisabled { get; }
    string RowDisabled { get; }
    string Name { get; }
    string Disabled { get; }
    string ChooseApp { get; }
    string ChooseUser { get; }
}
public class Roles_en : Standard_en, IRoles
{
    public string ListTitle => "Roles";
    public string AddTitle => "Add Role";
    public string EditTitle => "Edit Role";
    public string AppsTitle => "Apps";
    public string UsersTitle => "Users";
    public string TableId => "Id";
    public string TableName => "Name";
    public string TableDisabled => "Disabled";
    public string RowDisabled => "No";
    public string Name => "Name";
    public string Disabled => "Disabled";
    public string ChooseApp => "Choose app";
    public string ChooseUser => "Choose user";
}
public class Roles_hr : Standard_hr, IRoles
{
    public string ListTitle => "Uloge";
    public string AddTitle => "Dodaj ulogu";
    public string EditTitle => "Izmijeni ulogu";
    public string AppsTitle => "Appovi";
    public string UsersTitle => "Korisnici";
    public string TableId => "Šifra";
    public string TableName => "Naziv";
    public string TableDisabled => "Onemogućeno";
    public string RowDisabled => "Ne";
    public string Name => "Naziv";
    public string Disabled => "Onemogućena";
    public string ChooseApp => "Odaberi app";
    public string ChooseUser => "Odaberi korisnika";
}