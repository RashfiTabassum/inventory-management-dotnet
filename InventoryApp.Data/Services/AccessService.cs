using InventoryApp.Data.Context;
using InventoryApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Data.Services
{
    public class AccessService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccessService(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<List<InventoryAccess>> GetAccessListAsync(int inventoryId)
        {
            return await _db.InventoryAccesses
                .Include(a => a.User)
                .Where(a => a.InventoryId == inventoryId)
                .OrderBy(a => a.User.Email)
                .Select(a => new InventoryAccess
                {
                    Id = a.Id,
                    InventoryId = a.InventoryId,
                    UserId = a.UserId,
                    User = a.User
                })
                .ToListAsync();
        }

        public async Task<List<ApplicationUser>> SearchUsersAsync(
            string query, int inventoryId)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return new List<ApplicationUser>();

            var existingUserIds = await _db.InventoryAccesses
                .Where(a => a.InventoryId == inventoryId)
                .Select(a => a.UserId)
                .ToListAsync();

            return await _db.Users
                .Where(u =>
                    (u.Email!.Contains(query) ||
                     u.UserName!.Contains(query)) &&
                    !existingUserIds.Contains(u.Id))
                .Take(5)
                .Select(u => new ApplicationUser
                {
                    Id = u.Id,
                    Email = u.Email,
                    UserName = u.UserName
                })
                .ToListAsync();
        }

        public async Task<bool> AddAccessAsync(int inventoryId, string userId)
        {
            var exists = await _db.InventoryAccesses
                .AnyAsync(a => a.InventoryId == inventoryId
                    && a.UserId == userId);

            if (exists) return false;

            _db.InventoryAccesses.Add(new InventoryAccess
            {
                InventoryId = inventoryId,
                UserId = userId
            });

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveAccessAsync(int accessId)
        {
            var access = await _db.InventoryAccesses
                .FirstOrDefaultAsync(a => a.Id == accessId);

            if (access == null) return false;

            _db.InventoryAccesses.Remove(access);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePublicWriteAsync(
            int inventoryId, bool isPublic)
        {
            var inventory = await _db.Inventories
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null) return false;

            inventory.IsPublicWrite = isPublic;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}