using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Data.Models
{
    public enum CustomFieldType
    {
        SingleLine,
        MultiLine,
        Numeric,
        DocumentLink,
        Boolean
    }

    public class CustomField
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        public CustomFieldType FieldType { get; set; }

        public bool ShowInTable { get; set; } = true;

        public int DisplayOrder { get; set; }

        public int InventoryId { get; set; }

        public Inventory Inventory { get; set; } = null!;

        public ICollection<CustomFieldValue> Values { get; set; } = new List<CustomFieldValue>();
    }
}