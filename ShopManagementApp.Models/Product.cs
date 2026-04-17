namespace ShopManagementApp.Models
{
    /// <summary>
    /// Represents a product in the shop inventory.
    /// </summary>
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public decimal PurchasePrice { get; set; }   // Cost price
        public decimal SellingPrice { get; set; }    // Sale price
        public int StockQuantity { get; set; }       // Current stock
        public string Unit { get; set; } = "Pcs";   // Pcs, Meter, Kg, etc.

        // Display name in ComboBox dropdowns
        public override string ToString() => $"{Name} ({Brand}) - ₹{SellingPrice}";
    }
}
