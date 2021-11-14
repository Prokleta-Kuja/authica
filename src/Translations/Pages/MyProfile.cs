namespace authica.Translations
{
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
    }
}