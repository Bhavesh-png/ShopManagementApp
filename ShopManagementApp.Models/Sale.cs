namespace ShopManagementApp.Models
{
    /// <summary>
    /// Represents a complete sale / bill transaction.
    /// Customer fields removed — walk-in sales without customer tracking.
    /// Added Notes for repair description or sale remarks.
    /// </summary>
    public class Sale
    {
        public int      SaleId      { get; set; }
        public DateTime SaleDate    { get; set; } = DateTime.Now;
        public decimal  SubTotal    { get; set; }
        public decimal  Discount    { get; set; }
        public decimal  FinalAmount { get; set; }
        public string   PaymentMode { get; set; } = "Cash";
        public string   Notes       { get; set; } = "";   // optional notes / repair description

        public List<SaleItem> Items { get; set; } = new List<SaleItem>();
    }
}
