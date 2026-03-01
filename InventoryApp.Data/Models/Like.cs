namespace InventoryApp.Data.Models
{
    public class Like
    {
        public int Id { get; set; }

        public int ItemId { get; set; }

        public Item Item { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;
    }
}