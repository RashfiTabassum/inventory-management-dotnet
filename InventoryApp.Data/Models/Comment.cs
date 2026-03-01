using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Data.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int InventoryId { get; set; }

        public Inventory Inventory { get; set; } = null!;

        public string AuthorId { get; set; } = string.Empty;

        public ApplicationUser Author { get; set; } = null!;
    }
}