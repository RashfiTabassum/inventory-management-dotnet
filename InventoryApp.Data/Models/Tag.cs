using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Data.Models
{
    public class Tag // has many to many relationship with Inventory, an inventory can have multiple tags and a tag can be associated with multiple inventories. this allows us to categorize and filter inventories based on tags, making it easier for users to find related inventories or group them by common characteristics.
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    }
}