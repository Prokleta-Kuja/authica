namespace authica.Translations;

public interface ISignIn : IStandard
{
    string Title { get; }
    string UsernameEmail { get; }
    string Password { get; }
    string ForgotPassword { get; }
    string SignIn { get; }
    string External { get; }
    string IpBlocked { get; }
    string ValidationCredentials { get; }
}
public class SignIn_en : Standard_en, ISignIn
{
    public string Title => "Sign in";
    public string UsernameEmail => "Username or email";
    public string Password => "Password";
    public string ForgotPassword => "Forgot password?";
    public string SignIn => "Sign in";
    public string External => "External";
    public string IpBlocked => "Your IP address has been blocked.";
    public string ValidationCredentials => "Invalid credentials";
}
public class SignIn_hr : Standard_hr, ISignIn
{
    public string Title => "Prijava";
    public string UsernameEmail => "Korisničko ime ili email";
    public string Password => "Lozinka";
    public string ForgotPassword => "Zaboravljena lozinka?";
    public string SignIn => "Prijava";
    public string External => "Vanjski";
    public string IpBlocked => "Vaša IP adresa je blokirana.";
    public string ValidationCredentials => "Neispravne vjerodajnice";
}