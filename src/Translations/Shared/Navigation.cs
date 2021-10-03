namespace authica.Translations
{
    public interface INavigation : IStandard
    {
        string App { get; }
        string Roles { get; }
        string Users { get; }
        string Apps { get; }
        string SignOut { get; }
    }
    public class Navigation_en : Standard_en, INavigation
    {
        public string App => "authica";
        public string Roles => "Roles";
        public string Users => "Users";
        public string Apps => "Apps";
        public string SignOut => "Sign out";
    }
    public class Navigation_hr : Standard_hr, INavigation
    {
        public string App => "authica";
        public string Roles => "Uloge";
        public string Users => "Users";
        public string Apps => "Appovi";
        public string SignOut => "Odjava";
    }
}