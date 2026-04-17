using ShopManagementApp.Data.Repositories;
using ShopManagementApp.Models;
using ShopManagementApp.Utils;

namespace ShopManagementApp.Business.Services
{
    /// <summary>
    /// Business logic for repair job management.
    /// </summary>
    public class RepairService
    {
        private readonly RepairRepository _repo = new RepairRepository();

        public List<Repair> GetAllRepairs()     => _repo.GetAll();
        public List<Repair> GetActiveRepairs()  => _repo.GetActiveRepairs();
        public Repair? GetById(int id)          => _repo.GetById(id);

        public List<Repair> GetByStatus(string status) => _repo.GetByStatus(status);
        public List<Repair> GetByCustomer(int customerId) => _repo.GetByCustomer(customerId);

        /// <summary>Count of repairs not yet delivered (for dashboard).</summary>
        public int GetActiveRepairCount() => _repo.GetActiveRepairs().Count;

        public (bool Success, string Message, Repair? Repair) CreateRepair(Repair repair)
        {
            if (!ValidationHelper.IsNonEmpty(repair.CustomerName))
                return (false, "Customer name is required.", null);
            if (!ValidationHelper.IsNonEmpty(repair.DeviceType))
                return (false, "Device type is required.", null);
            if (!ValidationHelper.IsNonEmpty(repair.FaultDescription))
                return (false, "Fault description is required.", null);

            var saved = _repo.Add(repair);
            return (true, $"Repair job R-{saved.RepairId} created.", saved);
        }

        public (bool Success, string Message) UpdateStatus(int repairId, string newStatus, decimal finalCost = 0)
        {
            var repair = _repo.GetById(repairId);
            if (repair == null) return (false, "Repair job not found.");

            repair.Status = newStatus;
            if (newStatus == "Delivered")
            {
                repair.DeliveryDate = DateTime.Now;
                if (finalCost > 0) repair.FinalCost = finalCost;
            }

            _repo.Update(repair);
            return (true, $"Status updated to '{newStatus}'.");
        }

        public (bool Success, string Message) UpdateRepair(Repair repair)
        {
            if (!ValidationHelper.IsNonEmpty(repair.CustomerName))
                return (false, "Customer name is required.");

            bool updated = _repo.Update(repair);
            return updated ? (true, "Repair updated.") : (false, "Repair not found.");
        }

        public (bool Success, string Message) DeleteRepair(int repairId)
        {
            bool deleted = _repo.Delete(repairId);
            return deleted ? (true, "Repair deleted.") : (false, "Repair not found.");
        }
    }
}
