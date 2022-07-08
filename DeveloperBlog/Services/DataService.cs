using BlogProject.Data;
using BlogProject.Enums;
using BlogProject.Models;
using DeveloperBlog.Models.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlogProject.Services
{
    public class DataService
    {
        #region Properties
        // Add ability to talk to the Db with constructor injection
        private readonly ApplicationDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BlogUser> _userManager;
        private readonly AppSettings _appSettings;
        #endregion

        #region Constructor
        // Constructor
        public DataService(ApplicationDbContext dbContext,
                           RoleManager<IdentityRole> roleManager,
                           UserManager<BlogUser> userManager,
                           IOptions<AppSettings> appSettings)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _userManager = userManager;
            _appSettings = appSettings.Value;
        }
        #endregion


        // Public Methods

        #region Manage Data Async
        public async Task ManageDataAsync()
        {
            // Create the Db from the Migrations
            await _dbContext.Database.MigrateAsync();

            // 1: Seed a few roles into the system
            await SeedRolesAsync();

            // 2: Seed a few users into the system
            await SeedUsersAsync();
        }
        #endregion

        // Private Methods

        #region Seed Roles Async
        private async Task SeedRolesAsync()
        {
            // If there are already Roles in the system, do nothing.
            if (_dbContext.Roles.Any())
            {
                return;
            }
            else // Otherwise create a few roles
            {
                foreach (var role in Enum.GetNames(typeof(BlogRole)))
                {
                    // Use Role Manager to create roles
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
        #endregion

        #region Seed Users Async
        private async Task SeedUsersAsync()
        {
            // If there are already Users in the system, do nothing.
            if (_dbContext.Users.Any())
            {
                return;
            }

            // Step 1: Creates a new instance of BlogUser
            // Administrator
            var adminUser = new BlogUser()
            {
                Email = _appSettings.SiteAdminCredentials.Email,
                UserName = _appSettings.SiteAdminCredentials.Email,
                FirstName = _appSettings.SiteAdminCredentials.FirstName,
                LastName = _appSettings.SiteAdminCredentials.LastName,
                DisplayName = _appSettings.SiteAdminCredentials.DisplayName,
                EmailConfirmed = true,
            };

            // Step 2: Use the UserManager to create a new user that is defined by the adminUser
            await _userManager.CreateAsync(adminUser, _appSettings.SiteAdminCredentials.Password);

            // Step 3: Add this new user to the Administrator role
            await _userManager.AddToRoleAsync(adminUser, BlogRole.Administrator.ToString());

            // Moderator
            var modUser = new BlogUser()
            {
                Email = _appSettings.SiteModeratorCredentials.Email,
                UserName = _appSettings.SiteModeratorCredentials.Email,
                FirstName = _appSettings.SiteModeratorCredentials.FirstName,
                LastName = _appSettings.SiteModeratorCredentials.LastName,
                DisplayName = _appSettings.SiteModeratorCredentials.DisplayName,
                EmailConfirmed = true,
            };

            await _userManager.CreateAsync(modUser, _appSettings.SiteModeratorCredentials.Password);
            await _userManager.AddToRoleAsync(modUser, BlogRole.Moderator.ToString());

        } 
        #endregion


    }
}
