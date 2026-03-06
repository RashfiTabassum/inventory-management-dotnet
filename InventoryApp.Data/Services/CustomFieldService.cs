using InventoryApp.Data.Context;
using InventoryApp.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Data.Services
{
    public class CustomFieldService
    {
        private readonly ApplicationDbContext _db;

        public CustomFieldService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<CustomField>> GetByInventoryAsync(int inventoryId)
        {
            return await _db.CustomFields
                .Where(f => f.InventoryId == inventoryId)
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();
        }

        public async Task<CustomField> CreateAsync(CustomField field)
        {
            var maxOrder = await _db.CustomFields
                .Where(f => f.InventoryId == field.InventoryId)
                .MaxAsync(f => (int?)f.DisplayOrder) ?? 0;

            field.DisplayOrder = maxOrder + 1;

            _db.CustomFields.Add(field);
            await _db.SaveChangesAsync();
            return field;
        }

        public async Task<bool> UpdateAsync(CustomField field)
        {
            var existing = await _db.CustomFields
                .FirstOrDefaultAsync(f => f.Id == field.Id);

            if (existing == null) return false;

            existing.Title = field.Title;
            existing.Description = field.Description;
            existing.ShowInTable = field.ShowInTable;
            existing.DisplayOrder = field.DisplayOrder;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int fieldId)
        {
            var field = await _db.CustomFields
                .Include(f => f.Values)
                .FirstOrDefaultAsync(f => f.Id == fieldId);

            if (field == null) return false;

            _db.CustomFieldValues.RemoveRange(field.Values);
            _db.CustomFields.Remove(field);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MoveUpAsync(int fieldId)
        {
            var field = await _db.CustomFields
                .FirstOrDefaultAsync(f => f.Id == fieldId);

            if (field == null || field.DisplayOrder <= 1) return false;

            var above = await _db.CustomFields
                .Where(f => f.InventoryId == field.InventoryId
                    && f.DisplayOrder == field.DisplayOrder - 1)
                .FirstOrDefaultAsync();

            if (above == null) return false;

            above.DisplayOrder++;
            field.DisplayOrder--;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MoveDownAsync(int fieldId)
        {
            var field = await _db.CustomFields
                .FirstOrDefaultAsync(f => f.Id == fieldId);

            if (field == null) return false;

            var below = await _db.CustomFields
                .Where(f => f.InventoryId == field.InventoryId
                    && f.DisplayOrder == field.DisplayOrder + 1)
                .FirstOrDefaultAsync();

            if (below == null) return false;

            below.DisplayOrder--;
            field.DisplayOrder++;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task SaveFieldValueAsync(
            int itemId, int fieldId, string value)
        {
            var existing = await _db.CustomFieldValues
                .FirstOrDefaultAsync(v =>
                    v.ItemId == itemId && v.CustomFieldId == fieldId);

            if (existing == null)
            {
                _db.CustomFieldValues.Add(new CustomFieldValue
                {
                    ItemId = itemId,
                    CustomFieldId = fieldId,
                    Value = value
                });
            }
            else
            {
                existing.Value = value;
            }

            await _db.SaveChangesAsync();
        }

        public async Task<Dictionary<int, string>> GetFieldValuesAsync(
            int itemId)
        {
            return await _db.CustomFieldValues
                .Where(v => v.ItemId == itemId)
                .ToDictionaryAsync(v => v.CustomFieldId, v => v.Value);
        }

        public async Task<int> GetFieldCountByTypeAsync(
            int inventoryId, CustomFieldType type)
        {
            return await _db.CustomFields
                .CountAsync(f => f.InventoryId == inventoryId
                    && f.FieldType == type);
        }
    }
}