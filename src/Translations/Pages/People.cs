namespace authica.Translations
{
    public interface IPeople : IStandard
    {
        string PageTitle { get; }
        string AddTitle { get; }
        string EditTitle { get; }
        string Name { get; }
        string FirstName { get; }
        string LastName { get; }
        string DateOfBirth { get; }
        string TableName { get; }
        string TableFirstName { get; }
        string TableLastName { get; }
        string TableDateOfBirth { get; }
        string TableImageCount { get; }
    }
    public class People_en : Standard_en, IPeople
    {
        public string PageTitle => "People";
        public string AddTitle => "Add Person";
        public string EditTitle => "Edit Person";
        public string Name => "Name";
        public string FirstName => "First Name";
        public string LastName => "Last Name";
        public string DateOfBirth => "Date of birth";
        public string TableName => "Name";
        public string TableFirstName => "First";
        public string TableLastName => "Last";
        public string TableDateOfBirth => "DoB";
        public string TableImageCount => "Tag count";
    }
    public class People_hr : Standard_hr, IPeople
    {
        public string PageTitle => "Osobe";
        public string AddTitle => "Dodaj osobu";
        public string EditTitle => "Izmijeni osobu";
        public string Name => "Naziv";
        public string FirstName => "Ime";
        public string LastName => "Prezime";
        public string DateOfBirth => "Datum rođenja";
        public string TableName => "Naziv";
        public string TableFirstName => "Ime";
        public string TableLastName => "Prezime";
        public string TableDateOfBirth => "Rođendan";
        public string TableImageCount => "Broj oznaka";
    }
}