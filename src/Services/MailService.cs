using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using authica.Entities;
using authica.Translations;
using Microsoft.Extensions.Logging;

namespace authica.Services
{
    public class MailService
    {
        readonly ILogger<MailService> _logger;
        readonly IMailService _t = LocalizationFactory.MailService();

        public MailService(ILogger<MailService> logger)
        {
            _logger = logger;
        }

        public bool IsSetup => C.Configuration.Current.SmtpPort.HasValue
            && !string.IsNullOrWhiteSpace(C.Configuration.Current.SmtpHost)
            && !string.IsNullOrWhiteSpace(C.Configuration.Current.SmtpPassword)
            && !string.IsNullOrWhiteSpace(C.Configuration.Current.SmtpFromName)
            && !string.IsNullOrWhiteSpace(C.Configuration.Current.SmtpFromAddress);

        public async Task SendAsync(MailMessage message, CancellationToken cancellationToken = default)
        {
            if (!IsSetup)
                throw new System.AggregateException("Mail service not setup");

            using var client = new SmtpClient(C.Configuration.Current.SmtpHost, C.Configuration.Current.SmtpPort!.Value);
            client.Timeout = (int)C.Configuration.Current.SmtpTimeout.TotalMilliseconds;
            client.EnableSsl = C.Configuration.Current.SmtpSsl;

            if (!string.IsNullOrWhiteSpace(C.Configuration.Current.SmtpUser) && !string.IsNullOrWhiteSpace(C.Configuration.Current.SmtpPassword))
                client.Credentials = new NetworkCredential(C.Configuration.Current.SmtpUser, C.Configuration.Current.SmtpPassword);

            await client.SendMailAsync(message, cancellationToken);

            _logger.LogDebug("Mail {Subject} sent successfully to {To}", message.Subject, message.To[0].Address);
        }
        public MailMessage GetStandardMessage(MailAddress from, MailAddress to, string subject, string body)
        {
            var message = new MailMessage(from, to) { IsBodyHtml = true };
            message.BodyEncoding = message.HeadersEncoding = message.SubjectEncoding = Encoding.UTF8;
            message.Subject = subject;
            message.Body = body;

            // disable autoresponders like out-of-office
            message.Headers.Add("Auto-Submitted", "auto-generated");
            message.Headers.Add("Precedence", "bulk");
            // https://blog.mailtrap.io/list-unsubscribe-header/
            // https://certified-senders.org/wp-content/uploads/2017/07/CSA_one-click_list-unsubscribe.pdf
            //message.Headers.Add("List-Unsubscribe", "<http://www.example.com/unsubscribe.html>");

            return message;
        }
        public async Task SendPasswordResetAsync(User user, Guid token, CancellationToken cancellationToken = default)
        {
            var resetLink = $"{C.Configuration.Current.HostName}{C.Routes.ResetPassword}/{token}";
            var from = new MailAddress(C.Configuration.Current.SmtpFromAddress, C.Configuration.Current.SmtpFromName);
            var to = new MailAddress(user.Email, $"{user.FirstName} {user.LastName}");
            // TODO: localize
            var subject = _t.ResetPasswordSubject;
            var body = _t.ResetPasswordBody(resetLink);
            var message = GetStandardMessage(from, to, subject, body);

            await SendAsync(message, cancellationToken);
        }
    }
}