namespace ShopManagementApp.Models
{
    /// <summary>
    /// Represents a customer of the shop.
    /// </summary>
    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public override string ToString() => $"{Name} - {Phone}";
    }
}
