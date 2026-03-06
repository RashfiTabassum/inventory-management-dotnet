using InventoryApp.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace InventoryApp.Data.CustomId
{
    public class CustomIdGenerator
    {
        private readonly ApplicationDbContext _db;

        public CustomIdGenerator(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<string> GenerateAsync(int inventoryId)
        {
            var inventory = await _db.Inventories
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null ||
                string.IsNullOrEmpty(inventory.CustomIdFormat))
                return string.Empty;

            List<CustomIdPart> parts;
            try
            {
                parts = JsonSerializer
                    .Deserialize<List<CustomIdPart>>(
                        inventory.CustomIdFormat)
                    ?? new List<CustomIdPart>();
            }
            catch
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var rng = new Random();

            foreach (var part in parts)
            {
                sb.Append(await GeneratePartAsync(
                    part, inventoryId, rng));
            }

            return sb.ToString();
        }

        private async Task<string> GeneratePartAsync(
            CustomIdPart part, int inventoryId, Random rng)
        {
            return part.Type switch
            {
                CustomIdPartType.FixedText =>
                    part.FixedText ?? string.Empty,

                CustomIdPartType.Random6Digit =>
                    part.LeadingZeros
                        ? rng.Next(0, 999999).ToString("D6")
                        : rng.Next(0, 999999).ToString(),

                CustomIdPartType.Random9Digit =>
                    part.LeadingZeros
                        ? rng.Next(0, 999999999).ToString("D9")
                        : rng.Next(0, 999999999).ToString(),

                CustomIdPartType.Random20Bit =>
                    rng.Next(0, 1048576).ToString(
                        part.LeadingZeros ? "D7" : ""),

                CustomIdPartType.Random32Bit =>
                    rng.Next(0, int.MaxValue).ToString(
                        part.LeadingZeros ? "D10" : ""),

                CustomIdPartType.Guid =>
                    System.Guid.NewGuid().ToString(),

                CustomIdPartType.DateTime =>
                    System.DateTime.UtcNow.ToString(
                        part.DateFormat ?? "yyyyMMdd"),

                CustomIdPartType.Sequence =>
                    await GetNextSequenceAsync(inventoryId),

                _ => string.Empty
            };
        }

        private async Task<string> GetNextSequenceAsync(int inventoryId)
        {
            var maxSequence = await _db.Items
                .Where(i => i.InventoryId == inventoryId
                    && i.CustomId != null
                    && i.CustomId != string.Empty)
                .CountAsync();

            return (maxSequence + 1).ToString();
        }

        public string GeneratePreview(List<CustomIdPart> parts)
        {
            var sb = new StringBuilder();
            var rng = new Random();

            foreach (var part in parts)
            {
                sb.Append(part.Type switch
                {
                    CustomIdPartType.FixedText =>
                        part.FixedText ?? string.Empty,
                    CustomIdPartType.Random6Digit =>
                        part.LeadingZeros ? "482910" : "48291",
                    CustomIdPartType.Random9Digit =>
                        part.LeadingZeros ? "482910234" : "48291023",
                    CustomIdPartType.Random20Bit =>
                        part.LeadingZeros ? "0524288" : "524288",
                    CustomIdPartType.Random32Bit =>
                        part.LeadingZeros ? "1234567890" : "123456789",
                    CustomIdPartType.Guid =>
                        "a3f2c1d4-...",
                    CustomIdPartType.DateTime =>
                        System.DateTime.UtcNow.ToString(
                            part.DateFormat ?? "yyyyMMdd"),
                    CustomIdPartType.Sequence =>
                        "1",
                    _ => string.Empty
                });
            }

            return sb.ToString();
        }
    }
}