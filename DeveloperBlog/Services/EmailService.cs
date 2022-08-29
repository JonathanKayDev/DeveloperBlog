using BlogProject.ViewModels;
using DeveloperBlog.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BlogProject.Services
{
    public class EmailService : IBlogEmailSender
    {
        #region Properties
        private readonly MailSettings _mailSettings;
        private readonly SecretsService _secretsService;
        #endregion

        #region Constructor
        public EmailService(IOptions<MailSettings> mailSettings, SecretsService secretsService)
        {
            _mailSettings = mailSettings.Value;
            _secretsService = secretsService;
        }
        #endregion

        #region Send Contact Email Async
        public async Task SendContactEmailAsync(string emailFrom, string name, string subject, string htmlMessage)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_secretsService.GetMailSettingsMail());
            email.To.Add(MailboxAddress.Parse(_secretsService.GetMailSettingsMail()));
            email.Subject = subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = $"<b>{name}</b> has sent you an email and can be reached at: <b>{emailFrom}</b><br/><br/>{htmlMessage}";

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_secretsService.GetMailSettingsMail(), _secretsService.GetMailSettingsPassword());

            await smtp.SendAsync(email);

            smtp.Disconnect(true);
        }
        #endregion

        #region Send Email Async
        public async Task SendEmailAsync(string emailTo, string subject, string htmlMessage)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_secretsService.GetMailSettingsMail());
            email.To.Add(MailboxAddress.Parse(emailTo));
            email.Subject = subject;

            var builder = new BodyBuilder()
            {
                HtmlBody = htmlMessage
            };

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_secretsService.GetMailSettingsMail(), _secretsService.GetMailSettingsPassword());

            await smtp.SendAsync(email);

            smtp.Disconnect(true);
        } 
        #endregion
    }
}
