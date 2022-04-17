namespace authica.Translations;

public interface IShutdown : IStandard
{
    string ShutdownTitle { get; }
    string RestartTitle { get; }
    string ShutdownDescription { get; }
    string RestartDescription { get; }
}
public class Shutdown_en : Standard_en, IShutdown
{
    public string ShutdownTitle => "App will shutdown";
    public string RestartTitle => "App will restart";
    public string ShutdownDescription => "If there is a restart policy, app will start again.";
    public string RestartDescription => "Please wait...";
}
public class Shutdown_hr : Standard_hr, IShutdown
{
    public string ShutdownTitle => "App će se isključiti";
    public string RestartTitle => "App će se ponovno pokrenuti";
    public string ShutdownDescription => "Ako postoje pravila za ponovno pokretanje, app će se ponovno pokrenuti.";
    public string RestartDescription => "Pričekajte...";
}