namespace authica.Translations;

public interface INavigation : IStandard
{
    string App { get; }
    string Roles { get; }
    string Users { get; }
    string Apps { get; }
    string Configuration { get; }
    string MyProfile { get; }
    string Restart { get; }
    string Shutdown { get; }
    string SignOut { get; }
}
public class Navigation_en : Standard_en, INavigation
{
    public string App => "authica";
    public string Roles => "Roles";
    public string Users => "Users";
    public string Apps => "Apps";
    public string Configuration => "Configuration";
    public string MyProfile => "My profile";
    public string Restart => "Restart";
    public string Shutdown => "Shutdown";
    public string SignOut => "Sign out";
}
public class Navigation_hr : Standard_hr, INavigation
{
    public string App => "authica";
    public string Roles => "Uloge";
    public string Users => "Korisnici";
    public string Apps => "Appovi";
    public string Configuration => "Postavke";
    public string MyProfile => "Moj profil";
    public string Restart => "Ponovno pokreni";
    public string Shutdown => "Ugasi";
    public string SignOut => "Odjava";
}