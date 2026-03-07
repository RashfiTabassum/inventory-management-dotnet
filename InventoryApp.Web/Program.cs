using InventoryApp.Web.Components;
using AspNet.Security.OAuth.GitHub;
using InventoryApp.Data.Context;
using InventoryApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

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
    })
    .AddGitHub(options =>
    {
        options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"]!;
        options.Scope.Add("user:email");
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
    await InventoryApp.Data.Seeding.RoleSeeder
        .SeedRolesAsync(scope.ServiceProvider);

    await InventoryApp.Data.Seeding.AdminSeeder
        .SeedAdminAsync(scope.ServiceProvider, "rashfi2004@gmail.com");
}
app.Run();