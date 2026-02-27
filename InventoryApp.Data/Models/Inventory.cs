using System.ComponentModel.DataAnnotations;
using Azure;

namespace InventoryApp.Data.Models
{
    public class Inventory
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsPublicWrite { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string OwnerId { get; set; } = string.Empty;

        public ApplicationUser Owner { get; set; } = null!;

        public ICollection<Item> Items { get; set; } = new List<Item>(); // one inventory can have many items 
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();

        public ICollection<CustomField> CustomFields { get; set; } = new List<CustomField>();

        public ICollection<InventoryAccess> AccessList { get; set; } = new List<InventoryAccess>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}