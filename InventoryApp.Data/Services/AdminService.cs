using InventoryApp.Data.Context;
using InventoryApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Data.Services
{
    public class AdminService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminService(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<List<UserWithRoles>> GetAllUsersAsync()
        {
            var users = await _db.Users
                .Select(u => new ApplicationUser
                {
                    Id = u.Id,
                    Email = u.Email,
                    UserName = u.UserName,
                    IsBlocked = u.IsBlocked,
                    LockoutEnd = u.LockoutEnd
                })
                .ToListAsync();

            var result = new List<UserWithRoles>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new UserWithRoles
                {
                    User = user,
                    Roles = roles.ToList()
                });
            }

            return result;
        }

        public async Task<bool> BlockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.IsBlocked = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;
            user.LockoutEnabled = true;

            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task<bool> UnblockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.IsBlocked = false;
            user.LockoutEnd = null;

            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var ownedInventories = await _db.Inventories
                .Where(i => i.OwnerId == userId)
                .ToListAsync();

            _db.Inventories.RemoveRange(ownedInventories);
            await _db.SaveChangesAsync();

            await _userManager.DeleteAsync(user);
            return true;
        }

        public async Task<bool> AddAdminRoleAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin) return true;

            await _userManager.AddToRoleAsync(user, "Admin");
            return true;
        }

        public async Task<bool> RemoveAdminRoleAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            await _userManager.RemoveFromRoleAsync(user, "Admin");
            return true;
        }
    }

    public class UserWithRoles
    {
        public ApplicationUser User { get; set; } = null!;
        public List<string> Roles { get; set; } = new();
    }
}