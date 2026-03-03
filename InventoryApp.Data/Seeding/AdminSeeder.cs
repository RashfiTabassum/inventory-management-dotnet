using InventoryApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryApp.Data.Seeding
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(
            IServiceProvider serviceProvider,
            string adminEmail)
        {
            var userManager = serviceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            var user = await userManager.FindByEmailAsync(adminEmail);
            if (user == null) return;

            var isAdmin = await userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }

            var isUser = await userManager.IsInRoleAsync(user, "User");
            if (!isUser)
            {
                await userManager.AddToRoleAsync(user, "User");
            }
        }
    }
}