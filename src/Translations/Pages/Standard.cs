namespace authica.Translations;

public interface IStandard
{
    string Loading { get; }
    string Search { get; }
    string Year { get; }
    string Month { get; }
    string Today { get; }
    string Add { get; }
    string Edit { get; }
    string Remove { get; }
    string Close { get; }
    string SaveChanges { get; }
    string Save { get; }
    string Cancel { get; }
    string Clear { get; }
    string Enable { get; }
    string Disable { get; }
    string NotFound { get; }
    string ValidationRequired { get; }
    string ValidationDuplicate { get; }
    string ValidationInvalid { get; }
    string ValidationTooShort(int minLength);
    string ValidationTooLong(int maxLength);
}
public class Standard_en : IStandard
{
    public string Loading => "Loading...";
    public string Search => "Search";
    public string Year => "Year";
    public string Month => "Month";
    public string Today => "Today";
    public string Add => "Add";
    public string Edit => "Edit";
    public string Remove => "Remove";
    public string Close => "Close";
    public string SaveChanges => "Save changes";
    public string Save => "Save";
    public string Cancel => "Cancel";
    public string Clear => "Clear";
    public string Enable => "Enable";
    public string Disable => "Disable";
    public string NotFound => "Not found.";
    public string ValidationRequired => "Required";
    public string ValidationDuplicate => "Duplicate";
    public string ValidationInvalid => "Invalid";
    public string ValidationTooShort(int minLength) => $"Must be at least {minLength} long";
    public string ValidationTooLong(int maxLength) => $"Can't be longer than {maxLength}";
}
public class Standard_hr : IStandard
{
    public string Loading => "Učitavam...";
    public string Search => "Traži";
    public string Year => "Godina";
    public string Month => "Mjesec";
    public string Today => "Danas";
    public string Add => "Dodaj";
    public string Edit => "Izmijeni";
    public string Remove => "Ukloni";
    public string Close => "Zatvori";
    public string SaveChanges => "Spremi izmjene";
    public string Save => "Spremi";
    public string Cancel => "Odustani";
    public string Clear => "Očisti";
    public string Enable => "Omogući";
    public string Disable => "Onemogući";
    public string NotFound => "Nema.";
    public string ValidationRequired => "Obavezno";
    public string ValidationDuplicate => "Duplikat";
    public string ValidationInvalid => "Neispravno";
    public string ValidationTooShort(int minLength) => $"Mora biti barem {minLength} znakova";
    public string ValidationTooLong(int maxLength) => $"Ne smije biti duže od {maxLength} znakova";
}