namespace authica.Translations
{
    public interface INavigation : IStandard
    {
        string App { get; }
        string Import { get; }
        string People { get; }
        string Locations { get; }
    }
    public class Navigation_en : Standard_en, INavigation
    {
        public string App => "albumica";
        public string Import => "Import";
        public string People => "People";
        public string Locations => "Locations";
    }
    public class Navigation_hr : Standard_hr, INavigation
    {
        public string App => "albumica";
        public string Import => "Uvoz";
        public string People => "Ljudi";
        public string Locations => "Lokacije";
    }
}