using BlogProject.Data;
using BlogProject.Models;
using BlogProject.Services;
using BlogProject.ViewModels;
using DeveloperBlog.Models.Settings;
using DeveloperBlog.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// configuration for Db
ConfigurationManager configuration = builder.Configuration;

// Add services to the container.
var connectionString = ConnectionService.GetConnectionString(builder.Configuration);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)); // Beware - somehow a line was added automatically that tried to connect to original sql
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// CF tutorial
builder.Services.AddIdentity<BlogUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddDefaultUI()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// register and inject IOptions of type AppSettings
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
// Register my custom DataService class
builder.Services.AddScoped<DataService>();
// Register SecretsService
builder.Services.AddTransient<SecretsService>();
// Register a preconfigured instance of the MailSettings class
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddScoped<IBlogEmailSender, EmailService>();
// Register Image service
builder.Services.AddScoped<IImageService, BasicImageService>();
// Register Slug service
builder.Services.AddScoped<ISlugService, BasicSlugService>();
// Register Blog Search service
builder.Services.AddScoped<BlogSearchService>();

var app = builder.Build();

// Register class as a service (this is not constructor injection)
var dataService = app.Services.CreateScope().ServiceProvider.GetRequiredService<DataService>();
await dataService.ManageDataAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Custom map controller route
app.MapControllerRoute(
    name: "SlugRoute",
    pattern: "BlogPosts/UrlFriendly/{slug}",
    defaults: new { controller = "Posts", action = "Details" });
// This is configuration of the routing engine
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapRazorPages();

app.Run();
