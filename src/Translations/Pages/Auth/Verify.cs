namespace authica.Translations
{
    public interface IVerify : IStandard
    {
        string Invalid { get; }
        string UnknownApp { get; }
        string Unauthorized { get; }
    }
    public class Verify_en : Standard_en, IVerify
    {
        public string Invalid => "Redirect uri missing or invalid.";
        public string UnknownApp => "Unknown app";
        public string Unauthorized => "You are not authorized for this uri.";
    }
    public class Verify_hr : Standard_hr, IVerify
    {
        public string Invalid => "Uri za preusmjeravanje nedostaje ili je neispravan.";

        public string UnknownApp => "Nepoznati app";
        public string Unauthorized => "Niste autorizirani za ovaj uri.";
    }
}