using InventoryApp.Data.Context;
using InventoryApp.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Data.Services
{
    public class SearchService
    {
        private readonly ApplicationDbContext _db;

        public SearchService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<SearchResults> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return new SearchResults();

            var pattern = $"%{query}%";

            var inventories = await _db.Inventories
                .Include(i => i.Owner)
                .Where(i =>
                    EF.Functions.Like(i.Name, pattern) ||
                    EF.Functions.Like(i.Description, pattern) ||
                    i.Tags.Any(t => EF.Functions.Like(t.Name, pattern)))
                .Select(i => new Inventory
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    Category = i.Category,
                    OwnerId = i.OwnerId,
                    Owner = i.Owner,
                    CreatedAt = i.CreatedAt
                })
                .Take(20)
                .ToListAsync();

            var items = await _db.Items
                .Include(i => i.CreatedBy)
                .Where(i =>
                    EF.Functions.Like(i.Name, pattern) ||
                    EF.Functions.Like(i.CustomId, pattern))
                .Select(i => new Item
                {
                    Id = i.Id,
                    Name = i.Name,
                    CustomId = i.CustomId,
                    InventoryId = i.InventoryId,
                    CreatedById = i.CreatedById,
                    CreatedBy = i.CreatedBy,
                    CreatedAt = i.CreatedAt
                })
                .Take(20)
                .ToListAsync();

            return new SearchResults
            {
                Inventories = inventories,
                Items = items,
                Query = query
            };
        }
    }

    public class SearchResults
    {
        public List<Inventory> Inventories { get; set; } = new();
        public List<Item> Items { get; set; } = new();
        public string Query { get; set; } = string.Empty;

        public bool HasResults =>
            Inventories.Count > 0 || Items.Count > 0;
    }
}