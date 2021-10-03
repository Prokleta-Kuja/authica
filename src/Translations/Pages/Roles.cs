namespace authica.Translations
{
    public interface IRoles : IStandard
    {
        string ListTitle { get; }
        string AddTitle { get; }
        string EditTitle { get; }
        string TableId { get; }
        string TableName { get; }
        string TableDisabled { get; }
        string RowDisabled { get; }
        string Name { get; }
        string Disabled { get; }
    }
    public class Roles_en : Standard_en, IRoles
    {
        public string ListTitle => "Roles";
        public string AddTitle => "Add Role";
        public string EditTitle => "Edit Role";
        public string TableId => "Id";
        public string TableName => "Name";
        public string TableDisabled => "Disabled";
        public string RowDisabled => "No";
        public string Name => "Name";
        public string Disabled => "Disabled";
    }
    public class Roles_hr : Standard_hr, IRoles
    {
        public string ListTitle => "Uloge";
        public string AddTitle => "Dodaj ulogu";
        public string EditTitle => "Izmijeni ulogu";
        public string TableId => "Šifra";
        public string TableName => "Naziv";
        public string TableDisabled => "Onemogućeno";
        public string RowDisabled => "Ne";
        public string Name => "Naziv";
        public string Disabled => "Onemogućena";
    }
}