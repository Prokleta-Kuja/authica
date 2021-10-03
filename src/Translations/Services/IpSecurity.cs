namespace authica.Translations
{
    public interface IIpSecurity : IStandard
    {
        string IpBlocked { get; }
    }
    public class IpSecurity_en : Standard_en, IIpSecurity
    {
        public string IpBlocked => "Your IP address has been blocked.";
    }
    public class IpSecurity_hr : Standard_hr, IIpSecurity
    {
        public string IpBlocked => "Vaša IP adresa je blokirana.";
    }
}