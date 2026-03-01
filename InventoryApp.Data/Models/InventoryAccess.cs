namespace InventoryApp.Data.Models
{
    public class InventoryAccess
    {
        public int Id { get; set; }

        public int InventoryId { get; set; }

        public Inventory Inventory { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;
    }
}