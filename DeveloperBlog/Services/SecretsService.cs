using BlogProject.ViewModels;
using DeveloperBlog.Models.Settings;
using DeveloperBlog.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DeveloperBlog.Services
{
    public class SecretsService : ISecretsService
    {
        private readonly AppSettings _appSettings;
        private readonly MailSettings _mailSettings;

        public SecretsService(IOptions<AppSettings> appSettings, IOptions<MailSettings> mailSettings)
        {
            _appSettings = appSettings.Value;
            _mailSettings = mailSettings.Value;
        }

        public string GetAdminEmail()
        {
            var ev = Environment.GetEnvironmentVariable("AdminEmail");

            return string.IsNullOrEmpty(ev) ? _appSettings.SiteAdminCredentials.Email : ev;
        }

        public string GetAdminPassword()
        {
            var ev = Environment.GetEnvironmentVariable("AdminPassword");

            return string.IsNullOrEmpty(ev) ? _appSettings.SiteAdminCredentials.Password : ev;
        }

        public string GetMailSettingsMail()
        {
            var ev = Environment.GetEnvironmentVariable("MailSettingsMail");

            return string.IsNullOrEmpty(ev) ? _mailSettings.Mail : ev;
        }

        public string GetMailSettingsPassword()
        {
            var ev = Environment.GetEnvironmentVariable("MailSettingsPassword");

            return string.IsNullOrEmpty(ev) ? _mailSettings.Password : ev;
        }

        public string GetModeratorEmail()
        {
            var ev = Environment.GetEnvironmentVariable("ModeratorEmail");

            return string.IsNullOrEmpty(ev) ? _appSettings.SiteModeratorCredentials.Email : ev;
        }

        public string GetModeratorPassword()
        {
            var ev = Environment.GetEnvironmentVariable("ModeratorPassword");

            return string.IsNullOrEmpty(ev) ? _appSettings.SiteModeratorCredentials.Password : ev;
        }
    }
}
