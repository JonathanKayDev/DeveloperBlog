namespace DeveloperBlog.Models.Settings
{
    public class AppSettings
    {
        public SiteAdminCredentials SiteAdminCredentials { get; set; }
        public SiteModeratorCredentials SiteModeratorCredentials { get; set; }
    }

    public class SiteAdminCredentials
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
    }

    public class SiteModeratorCredentials
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
    }
}
