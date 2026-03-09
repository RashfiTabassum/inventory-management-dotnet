using InventoryApp.Web.Components;
using AspNet.Security.OAuth.GitHub;
using InventoryApp.Data.Context;
using InventoryApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: false);

// Add DbContext — auto-detect SQL Server vs PostgreSQL from connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
var isPostgres = connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase)
              || connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
              || connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase);

// Convert PostgreSQL URI format to key-value format (Npgsql requires it)
if (isPostgres && Uri.TryCreate(connectionString, UriKind.Absolute, out var uri))
{
    var userInfo = uri.UserInfo.Split(':');
    var port = uri.Port > 0 ? uri.Port : 5432;
    connectionString = $"Host={uri.Host};Port={port};Database={uri.AbsolutePath.TrimStart('/')}" +
                       $";Username={userInfo[0]};Password={userInfo.ElementAtOrDefault(1)}";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (isPostgres)
        options.UseNpgsql(connectionString);   // PostgreSQL (production on Render)
    else
        options.UseSqlServer(connectionString); // SQL Server (local development)
});

//builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<InventoryApp.Data.Services.InventoryService>();
builder.Services.AddScoped<InventoryApp.Data.Services.ItemService>();
builder.Services.AddScoped<InventoryApp.Data.Services.AdminService>();
builder.Services.AddScoped<InventoryApp.Data.Services.AccessService>();
builder.Services.AddScoped<InventoryApp.Data.Services.CustomFieldService>();
builder.Services.AddScoped<InventoryApp.Data.CustomId.CustomIdGenerator>();
builder.Services.AddScoped<InventoryApp.Data.Services.CommentService>();
builder.Services.AddScoped<InventoryApp.Data.Services.SearchService>();
builder.Services.AddSingleton<InventoryApp.Web.Services.LocalizationService>();
builder.Services.AddSingleton<InventoryApp.Web.Services.CloudinaryService>();


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        options.CallbackPath = "/signin-google";//new
    })
    .AddGitHub(options =>
    {
        options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"]!;
        options.Scope.Add("user:email");
        options.CallbackPath = "/signin-github";//new
    });
// Add Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddRazorPages();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Only redirect to HTTPS in development (Render handles SSL at the proxy level)
app.UseForwardedHeaders(new Microsoft.AspNetCore.HttpOverrides.ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});
if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (isPostgres)
    {
        // Drop leftover migrations table from previous failed MigrateAsync attempt
        // so EnsureCreated sees an empty DB and creates the full schema.
        await db.Database.ExecuteSqlRawAsync(
            "DROP TABLE IF EXISTS \"__EFMigrationsHistory\"");
        await db.Database.EnsureCreatedAsync();
    }
    else
    {
        await db.Database.MigrateAsync();
    }

    await InventoryApp.Data.Seeding.RoleSeeder
        .SeedRolesAsync(scope.ServiceProvider);

    await InventoryApp.Data.Seeding.AdminSeeder
        .SeedAdminAsync(scope.ServiceProvider, "rashfi2004@gmail.com");
}

app.Use(async (context, next) =>
{
    var loc = context.RequestServices
        .GetRequiredService<InventoryApp.Web.Services.LocalizationService>();
    if (context.Request.Cookies.TryGetValue("lang", out var lang))
    {
        loc.SetLanguage(lang);
    }
    await next();
});
app.Run();