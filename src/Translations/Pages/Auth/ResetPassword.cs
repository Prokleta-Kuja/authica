namespace authica.Translations
{
    public interface IResetPassword : IStandard
    {
        string EmailSentTitle { get; }
        string EmailSentDescription { get; }
        string EmailSetupTitle { get; }
        string EmailSetupDescription { get; }
        string ResetPassword { get; }
        string Email { get; }
        string NewPassword { get; }
        string Reset { get; }
        string SendConfirmation { get; }
        string IpBlocked { get; }
    }
    public class ResetPassword_en : Standard_en, IResetPassword
    {
        public string EmailSentTitle => "Email has been sent.";
        public string EmailSentDescription => "Follow instructions in email to reset your password.";
        public string EmailSetupTitle => "Reset password not possible.";
        public string EmailSetupDescription => "Email service has not been configured.";
        public string ResetPassword => "Reset password";
        public string Email => "Email";
        public string NewPassword => "New password";
        public string Reset => "Reset";
        public string SendConfirmation => "Send confirmation";
        public string IpBlocked => "Your IP address has been blocked.";
    }
    public class ResetPassword_hr : Standard_hr, IResetPassword
    {
        public string EmailSentTitle => "Email poslan.";
        public string EmailSentDescription => "Pratite upute u mailu kako bi resetirali lozinku.";
        public string EmailSetupTitle => "Resetiranje lozinke nije moguće.";
        public string EmailSetupDescription => "Email servis nije podešen.";
        public string ResetPassword => "Resetiranje lozinke";
        public string Email => "Email";
        public string NewPassword => "Nova lozinka";
        public string Reset => "Postavi";
        public string SendConfirmation => "Pošalji potvrdu";
        public string IpBlocked => "Vaša IP adresa je blokirana.";
    }
}