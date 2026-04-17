namespace ShopManagementApp.Models
{
    /// <summary>
    /// Represents a repair job (motor repair, fan repair, welding, etc.)
    /// </summary>
    public class Repair
    {
        public int RepairId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;

        // What device / item needs repair
        public string DeviceType { get; set; } = string.Empty;   // e.g. "Water Motor", "Fan"
        public string FaultDescription { get; set; } = string.Empty; // What is wrong

        public string Technician { get; set; } = string.Empty;   // Who is repairing
        public decimal EstimatedCost { get; set; }
        public decimal FinalCost { get; set; }

        // Status: Pending → In Progress → Completed → Delivered
        public string Status { get; set; } = "Pending";

        public DateTime ReceivedDate { get; set; } = DateTime.Now;
        public DateTime? DeliveryDate { get; set; }  // null until delivered
        public string Notes { get; set; } = string.Empty;
    }
}
