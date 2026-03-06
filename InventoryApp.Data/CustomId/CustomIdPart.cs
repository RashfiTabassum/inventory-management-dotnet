namespace InventoryApp.Data.CustomId
{
    public enum CustomIdPartType
    {
        FixedText,
        Random20Bit,
        Random32Bit,
        Random6Digit,
        Random9Digit,
        Guid,
        DateTime,
        Sequence
    }

    public class CustomIdPart
    {
        public CustomIdPartType Type { get; set; }
        public string? FixedText { get; set; }
        public bool LeadingZeros { get; set; } = true;
        public string? DateFormat { get; set; } = "yyyyMMdd";
    }
}