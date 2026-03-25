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
            var usersWithRoles = await _db.Users
                .Select(u => new
                {
                    User = new ApplicationUser
                    {
                        Id = u.Id,
                        Email = u.Email,
                        UserName = u.UserName,
                        IsBlocked = u.IsBlocked,
                        LockoutEnd = u.LockoutEnd
                    },
                    Roles = _db.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name!)
                        .ToList()
                })
                .ToListAsync();

            return usersWithRoles.Select(x => new UserWithRoles
            {
                User = x.User,
                Roles = x.Roles
            }).ToList();
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

            // 1. Delete likes by this user
            var userLikes = await _db.Likes
                .Where(l => l.UserId == userId).ToListAsync();
            _db.Likes.RemoveRange(userLikes);

            // 2. Delete comments by this user
            var userComments = await _db.Comments
                .Where(c => c.AuthorId == userId).ToListAsync();
            _db.Comments.RemoveRange(userComments);

            // 3. Delete access grants for this user
            var userAccess = await _db.InventoryAccesses
                .Where(a => a.UserId == userId).ToListAsync();
            _db.InventoryAccesses.RemoveRange(userAccess);

            // 4. Items created by this user in other people's inventories
            var foreignItems = await _db.Items
                .Where(i => i.CreatedById == userId &&
                            i.Inventory.OwnerId != userId)
                .Include(i => i.CustomFieldValues)
                .Include(i => i.Likes)
                .ToListAsync();
            foreach (var item in foreignItems)
            {
                _db.CustomFieldValues.RemoveRange(item.CustomFieldValues);
                _db.Likes.RemoveRange(item.Likes);
            }
            _db.Items.RemoveRange(foreignItems);

            await _db.SaveChangesAsync();

            // 5. Delete owned inventories with all their contents
            var ownedInventoryIds = await _db.Inventories
                .Where(i => i.OwnerId == userId)
                .Select(i => i.Id)
                .ToListAsync();

            foreach (var invId in ownedInventoryIds)
            {
                var items = await _db.Items
                    .Where(i => i.InventoryId == invId)
                    .Include(i => i.CustomFieldValues)
                    .Include(i => i.Likes)
                    .ToListAsync();
                foreach (var item in items)
                {
                    _db.CustomFieldValues.RemoveRange(item.CustomFieldValues);
                    _db.Likes.RemoveRange(item.Likes);
                }
                _db.Items.RemoveRange(items);

                var fields = await _db.CustomFields
                    .Where(f => f.InventoryId == invId).ToListAsync();
                _db.CustomFields.RemoveRange(fields);

                var comments = await _db.Comments
                    .Where(c => c.InventoryId == invId).ToListAsync();
                _db.Comments.RemoveRange(comments);

                var access = await _db.InventoryAccesses
                    .Where(a => a.InventoryId == invId).ToListAsync();
                _db.InventoryAccesses.RemoveRange(access);
            }

            var ownedInventories = await _db.Inventories
                .Where(i => i.OwnerId == userId).ToListAsync();
            _db.Inventories.RemoveRange(ownedInventories);

            await _db.SaveChangesAsync();

            // 6. Delete the user
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