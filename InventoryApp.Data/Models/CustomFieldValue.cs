namespace InventoryApp.Data.Models
{
    public class CustomFieldValue
    {
        public int Id { get; set; }

        public string Value { get; set; } = string.Empty;

        public int ItemId { get; set; }

        public Item Item { get; set; } = null!;

        public int CustomFieldId { get; set; }

        public CustomField CustomField { get; set; } = null!;
    }
}