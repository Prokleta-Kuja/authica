namespace authica.Translations
{
    public interface IMailService : IStandard
    {
        string ResetPasswordSubject { get; }
        string ResetPasswordBody(string resetLink);
    }
    public class MailService_en : Standard_en, IMailService
    {
        public string ResetPasswordSubject => $"{C.Configuration.Current.SmtpSubjectPrefix}Reset password request";
        public string ResetPasswordBody(string resetLink) => @$"To reset your password please click <a href={resetLink}>here</a>.";
    }
    public class MailService_hr : Standard_hr, IMailService
    {
        public string ResetPasswordSubject => $"{C.Configuration.Current.SmtpSubjectPrefix}Zahtjev za promjenu lozinke";
        public string ResetPasswordBody(string resetLink) => @$"Za ponovno postavljanje lozinke molimo kliknite <a href={resetLink}>ovdje</a>.";
    }
}