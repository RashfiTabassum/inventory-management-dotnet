using InventoryApp.Data.Context;
using InventoryApp.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Data.Services
{
    public class ItemService
    {
        private readonly ApplicationDbContext _db;

        public ItemService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Item>> GetByInventoryAsync(int inventoryId)
        {
            return await _db.Items
                .Include(i => i.CreatedBy)
                .Include(i => i.CustomFieldValues)
                    .ThenInclude(v => v.CustomField)
                .Include(i => i.Likes)
                .Where(i => i.InventoryId == inventoryId)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new Item
                {
                    Id = i.Id,
                    Name = i.Name,
                    CustomId = i.CustomId,
                    CreatedAt = i.CreatedAt,
                    CreatedById = i.CreatedById,
                    CreatedBy = i.CreatedBy,
                    InventoryId = i.InventoryId,
                    CustomFieldValues = i.CustomFieldValues,
                    Likes = i.Likes
                })
                .ToListAsync();
        }

        public async Task<Item?> GetByIdAsync(int id)
        {
            return await _db.Items
                .Include(i => i.CreatedBy)
                .Include(i => i.CustomFieldValues)
                    .ThenInclude(v => v.CustomField)
                .Include(i => i.Likes)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Item> CreateAsync(Item item)
        {
            _db.Items.Add(item);
            await _db.SaveChangesAsync();
            return item;
        }

        public async Task<bool> UpdateAsync(Item item)
        {
            var existing = await _db.Items
                .Include(i => i.CustomFieldValues)
                .FirstOrDefaultAsync(i => i.Id == item.Id);

            if (existing == null) return false;

            existing.Name = item.Name;
            existing.CustomId = item.CustomId;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId, bool isAdmin)
        {
            var item = await _db.Items
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null) return false;

            if (item.CreatedById != userId && !isAdmin) return false;

            _db.Items.Remove(item);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasWriteAccessAsync(
            int inventoryId, string userId)
        {
            var inventory = await _db.Inventories
                .Include(i => i.AccessList)
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null) return false;
            if (inventory.OwnerId == userId) return true;
            if (inventory.IsPublicWrite) return true;

            return inventory.AccessList
                .Any(a => a.UserId == userId);
        }
    }
}