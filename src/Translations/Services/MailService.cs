namespace authica.Translations;

public interface IMailService : IStandard
{
    string TestSubject { get; }
    string TestBody { get; }
    string ResetPasswordSubject { get; }
    string ResetPasswordBody(string resetLink);
}
public class MailService_en : Standard_en, IMailService
{
    public string TestSubject => "Email setting test";
    public string TestBody => "If you received this email, the email configuration seems to be correct.";
    public string ResetPasswordSubject => $"{C.Configuration.Current.SmtpSubjectPrefix}Reset password request";
    public string ResetPasswordBody(string resetLink) => @$"To reset your password please click <a href={resetLink}>here</a>.";
}
public class MailService_hr : Standard_hr, IMailService
{
    public string TestSubject => "Test email postavki";
    public string TestBody => "Ako ste primili ovaj email, email postavke su vjerojatno dobro postavljene.";
    public string ResetPasswordSubject => $"{C.Configuration.Current.SmtpSubjectPrefix}Zahtjev za promjenu lozinke";
    public string ResetPasswordBody(string resetLink) => @$"Za ponovno postavljanje lozinke molimo kliknite <a href={resetLink}>ovdje</a>.";
}