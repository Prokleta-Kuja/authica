namespace authica.Translations;

public interface ISignOut : IStandard
{
    string Title { get; }
    string SignInAgain { get; }
}
public class SignOut_en : Standard_en, ISignOut
{
    public string Title => "You have been signed out.";
    public string SignInAgain => "Sign in again";
}
public class SignOut_hr : Standard_hr, ISignOut
{
    public string Title => "Odjavljeni ste.";
    public string SignInAgain => "Prijavite se ponovno";
}