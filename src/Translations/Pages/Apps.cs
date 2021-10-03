namespace authica.Translations
{
    public interface IApps : IStandard
    {
        string ListTitle { get; }
        string AddTitle { get; }
        string EditTitle { get; }
        string TableId { get; }
        string TableName { get; }
        string TableCreated { get; }
        string TableDisabled { get; }
        string RowDisabled { get; }
        string Name { get; }
        string RedirectUri { get; }
        string Secret { get; }
        string NewSecret { get; }
        string AllowAllUsers { get; }
        string Disabled { get; }
    }
    public class Apps_en : Standard_en, IApps
    {
        public string ListTitle => "Apps";
        public string AddTitle => "Add App";
        public string EditTitle => "Edit App";
        public string TableId => "Id";
        public string TableName => "Name";
        public string TableCreated => "Created";
        public string TableDisabled => "Disabled";
        public string RowDisabled => "No";
        public string Name => "Name";
        public string RedirectUri => "Redirect Uri";
        public string Secret => "Secret";
        public string NewSecret => "New Secret";
        public string AllowAllUsers => "Allow All Users";
        public string Disabled => "Disabled";
    }
    public class Apps_hr : Standard_hr, IApps
    {
        public string ListTitle => "Appovi";
        public string AddTitle => "Dodaj app";
        public string EditTitle => "Izmijeni app";
        public string TableId => "Šifra";
        public string TableName => "Naziv";
        public string TableCreated => "Dodano";
        public string TableDisabled => "Onemogućeno";
        public string RowDisabled => "Ne";
        public string Name => "Naziv";
        public string RedirectUri => "Preusmjeri na uri";
        public string Secret => "Tajna";
        public string NewSecret => "Nova tajna";
        public string AllowAllUsers => "Dozvoli sve korisnike";
        public string Disabled => "Onemogućeno";
    }
}