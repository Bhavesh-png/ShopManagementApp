namespace ShopManagementApp.Models
{
    /// <summary>
    /// Represents one line item in a sale (one product + quantity).
    /// </summary>
    public class SaleItem
    {
        public int SaleItemId { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Calculated automatically
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
