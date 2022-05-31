namespace authica.Translations;

public interface IMyProfile : IStandard
{
    string PageTitle { get; }
    string Email { get; }
    string UserName { get; }
    string FirstName { get; }
    string LastName { get; }
    string TimeZone { get; }
    string Locale { get; }
    string ChangePassword { get; }
    string OldPassword { get; }
    string NewPassword { get; }
    string Sessions { get; }
    string Mfa { get; }
    string OtpChange { get; }
    string OtpAdd { get; }
    string OtpOldCode { get; }
    string OtpOldHelp { get; }
    string OtpNewCode { get; }
    string OtpClipboard { get; }
    string OtpClipboardCopied { get; }
    string OtpSecretCode { get; }
    string ToastSessionRemoved { get; }
    string ToastSaved { get; }
}
public class MyProfile_en : Standard_en, IMyProfile
{
    public string PageTitle => "My profile";
    public string Email => "Email";
    public string UserName => "Username";
    public string FirstName => "First Name";
    public string LastName => "Last Name";
    public string TimeZone => "Time Zone";
    public string Locale => "Locale";
    public string ChangePassword => "Change Password";
    public string OldPassword => "Current Password";
    public string NewPassword => "New Password";
    public string Sessions => "Sessions";
    public string Mfa => "Multi factor";
    public string OtpChange => "Change OTP device";
    public string OtpAdd => "Add OTP device";
    public string OtpOldCode => "Old code";
    public string OtpOldHelp => "In case you don't have previous device, please ask Admin for help";
    public string OtpNewCode => "New code";
    public string OtpClipboard => "Copy to Clipboard";
    public string OtpClipboardCopied => "Copied!";
    public string OtpSecretCode => "Secret code";
    public string ToastSessionRemoved => "Session removed.";
    public string ToastSaved => "Changes saved.";
}
public class MyProfile_hr : Standard_hr, IMyProfile
{
    public string PageTitle => "Moj profil";
    public string Email => "Email";
    public string UserName => "Korisničko ime";
    public string FirstName => "Ime";
    public string LastName => "Prezime";
    public string TimeZone => "Vremenska zona";
    public string Locale => "Lokalitet";
    public string ChangePassword => "Izmjena lozinke";
    public string OldPassword => "Trenutna lozinka";
    public string NewPassword => "Nova lozinka";
    public string Sessions => "Sesije";
    public string Mfa => "Više faktora";
    public string OtpChange => "Promijeni OTP uređaj";
    public string OtpAdd => "Dodaj OTP uređaj";
    public string OtpOldCode => "Stari kod";
    public string OtpOldHelp => "Ako nemate prethodni uređaj, obratite se administratoru za pomoć";
    public string OtpNewCode => "Novi kod";
    public string OtpClipboard => "Kopiraj";
    public string OtpClipboardCopied => "Kopirano!";
    public string OtpSecretCode => "Tajni kod";
    public string ToastSessionRemoved => "Sesija poništena.";
    public string ToastSaved => "Izmjene spremljene.";
}