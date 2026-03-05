using InventoryApp.Data.Context;
using InventoryApp.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Data.Services
{
    public class InventoryService
    {
        private readonly ApplicationDbContext _db;

        public InventoryService(ApplicationDbContext db)
        {
            _db = db;
        }

        // Get all inventories for the main page
        public async Task<List<Inventory>> GetLatestInventoriesAsync(int count = 10)
        {
            return await _db.Inventories
                .Include(i => i.Owner)
                .Include(i => i.Tags)
                .OrderByDescending(i => i.CreatedAt)
                .Take(count)
                .Select(i => new Inventory
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    Category = i.Category,
                    CreatedAt = i.CreatedAt,
                    OwnerId = i.OwnerId,
                    Owner = i.Owner,
                    Tags = i.Tags
                })
                .ToListAsync();
        }

        // Get single inventory by id
        public async Task<Inventory?> GetByIdAsync(int id)
        {
            return await _db.Inventories
                .Include(i => i.Owner)
                .Include(i => i.Tags)
                .Include(i => i.CustomFields)
                .Include(i => i.AccessList)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        // Create new inventory
        public async Task<Inventory> CreateAsync(Inventory inventory)
        {
            _db.Inventories.Add(inventory);
            await _db.SaveChangesAsync();
            return inventory;
        }

        // Update inventory with optimistic locking
        public async Task<bool> UpdateAsync(Inventory inventory)
        {
            var existing = await _db.Inventories
                .FirstOrDefaultAsync(i => i.Id == inventory.Id
                    && i.Version == inventory.Version);

            if (existing == null) return false; // conflict

            existing.Name = inventory.Name;
            existing.Description = inventory.Description;
            existing.Category = inventory.Category;
            existing.ImageUrl = inventory.ImageUrl;
            existing.IsPublicWrite = inventory.IsPublicWrite;
            existing.Version = inventory.Version + 1;

            await _db.SaveChangesAsync();
            return true;
        }

        // Delete inventory
        public async Task<bool> DeleteAsync(int id, string userId, bool isAdmin)
        {
            var inventory = await _db.Inventories
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventory == null) return false;

            if (inventory.OwnerId != userId && !isAdmin) return false;

            _db.Inventories.Remove(inventory);
            await _db.SaveChangesAsync();
            return true;
        }

        // Get inventories owned by a user
        public async Task<List<Inventory>> GetOwnedByUserAsync(string userId)
        {
            return await _db.Inventories
                .Include(i => i.Tags)
                .Where(i => i.OwnerId == userId)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new Inventory
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    Category = i.Category,
                    CreatedAt = i.CreatedAt,
                    OwnerId = i.OwnerId,
                    Tags = i.Tags
                })
                .ToListAsync();
        }

        // Get top 5 most popular inventories
        public async Task<List<Inventory>> GetTopInventoriesAsync()
        {
            return await _db.Inventories
                .Include(i => i.Owner)
                .Include(i => i.Items)
                .OrderByDescending(i => i.Items.Count)
                .Take(5)
                .Select(i => new Inventory
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    Category = i.Category,
                    OwnerId = i.OwnerId,
                    Owner = i.Owner,
                    Items = i.Items
                })
                .ToListAsync();
        }

        public async Task<List<Inventory>> GetInventoriesWithWriteAccessAsync(string userId)
        {
            return await _db.InventoryAccesses
                .Include(a => a.Inventory)
                    .ThenInclude(i => i.Owner)
                .Where(a => a.UserId == userId)
                .Select(a => new Inventory
                {
                    Id = a.Inventory.Id,
                    Name = a.Inventory.Name,
                    Description = a.Inventory.Description,
                    Category = a.Inventory.Category,
                    CreatedAt = a.Inventory.CreatedAt,
                    OwnerId = a.Inventory.OwnerId,
                    Owner = a.Inventory.Owner
                })
                .ToListAsync();
        }
    }
}