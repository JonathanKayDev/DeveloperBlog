namespace DeveloperBlog.Services.Interfaces
{
    public interface ISecretsService
    {
        public string GetMailSettingsMail();
        public string GetMailSettingsPassword();
        public string GetAdminEmail();
        public string GetAdminPassword();
        public string GetModeratorEmail();
        public string GetModeratorPassword();
    }
}
