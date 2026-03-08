using InventoryApp.Data.Context;
using InventoryApp.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace InventoryApp.Data.Services
{
    public class InventoryWithLikes
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public InventoryCategory Category { get; set; }
        //public int LikeCount { get; set; }
        public int ItemCount { get; set; }
    }
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
                //.Include(i => i.Tags)
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
            //existing.Tags = inventory.Tags;
            existing.CustomIdFormat = inventory.CustomIdFormat;
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
            .Include(i => i.Owner)
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
                    //Tags = i.Tags
                })
                .ToListAsync();
        }

        // Get top 5 most popular inventories
        public async Task<List<InventoryWithLikes>> GetTopInventoriesAsync(int count)
        {
            return await _db.Inventories
                .Select(i => new InventoryWithLikes
                {
                    Id = i.Id,
                    Name = i.Name,
                    Category = i.Category,
                    ItemCount = i.Items.Count() // Correctly maps the count of items to ItemCount
                })
                .OrderByDescending(i => i.ItemCount) // Fix: Use ItemCount instead of i.Items.Count()
                .Take(count)
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
        public async Task<Dictionary<string, int>> GetTagCloudAsync()
        {
            var tags = await _db.Tags
                .GroupBy(t => t.Name.ToLower())
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(t => t.Count)
                .Take(20)
                .ToListAsync();

            return tags.ToDictionary(t => t.Name, t => t.Count);
        }

        public async Task<int> GetTotalItemCountAsync()
        {
            return await _db.Items.CountAsync();
        }

        public async Task<int> GetTotalUserCountAsync()
        {
            return await _db.Users.CountAsync();
        }

        public async Task SaveTagsAsync(int inventoryId, string tagsString)
        {
            var inventory = await _db.Inventories
                .Include(i => i.Tags)
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null) return;

            inventory.Tags.Clear();

            var tagNames = tagsString
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLower())
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct()
                .ToList();

            foreach (var tagName in tagNames)
            {
                var existingTag = await _db.Tags
                    .FirstOrDefaultAsync(t => t.Name == tagName);

                if (existingTag != null)
                {
                    inventory.Tags.Add(existingTag);
                }
                else
                {
                    var newTag = new Tag { Name = tagName };
                    _db.Tags.Add(newTag);
                    inventory.Tags.Add(newTag);
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}