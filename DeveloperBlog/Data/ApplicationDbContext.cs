using BlogProject.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Data
{
    public class ApplicationDbContext : IdentityDbContext<BlogUser> // Updated to include BlogUser Type
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // To a degree we are changing the name of the Db tables based on the properties below.
        public DbSet<Blog>? Blogs { get; set; }
        public DbSet<Post>? Posts { get; set; }
        public DbSet<Comment>? Comments { get; set; }
        public DbSet<Tag>? Tags { get; set; }
    }
}