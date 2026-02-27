using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Data.Models
{
    public class Item
    {
        public int Id { get; set; } 

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string CustomId { get; set; } = string.Empty; // seperate from the auto-generated Id, allows users to assign their own identifier to the item. it's unique only within the context of the inventory it belongs to, so different inventories can have items with the same CustomId without conflict.

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string CreatedById { get; set; } = string.Empty; // foreign key to the ApplicationUser who created this item, allows us to track which user created each item and can be used for auditing or displaying creator information in the UI.

        public ApplicationUser CreatedBy { get; set; } = null!;

        public int InventoryId { get; set; } // foreign key to the Inventory that this item belongs to.
        public Inventory Inventory { get; set; } = null!;

        public ICollection<CustomFieldValue> CustomFieldValues { get; set; } = new List<CustomFieldValue>();

        public ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}