using ShopManagementApp.Data.Repositories;
using ShopManagementApp.Models;
using ShopManagementApp.Utils;

namespace ShopManagementApp.Business.Services
{
    /// <summary>
    /// Business logic for customer management.
    /// </summary>
    public class CustomerService
    {
        private readonly CustomerRepository _repo = new CustomerRepository();

        public List<Customer> GetAllCustomers() => _repo.GetAll();

        public Customer? GetById(int id) => _repo.GetById(id);

        public List<Customer> SearchCustomers(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return _repo.GetAll();
            return _repo.Search(query);
        }

        public (bool Success, string Message, Customer? Customer) AddCustomer(Customer customer)
        {
            if (!ValidationHelper.IsNonEmpty(customer.Name))
                return (false, "Customer name is required.", null);
            if (!ValidationHelper.IsValidPhone(customer.Phone))
                return (false, "Enter a valid 10-digit phone number.", null);

            // Check for duplicate phone number
            var existing = _repo.Search(customer.Phone);
            if (existing.Any(c => c.Phone == customer.Phone))
                return (false, $"Customer with phone {customer.Phone} already exists.", null);

            var added = _repo.Add(customer);
            return (true, "Customer added successfully.", added);
        }

        public (bool Success, string Message) UpdateCustomer(Customer customer)
        {
            if (!ValidationHelper.IsNonEmpty(customer.Name))
                return (false, "Customer name is required.");
            if (!ValidationHelper.IsValidPhone(customer.Phone))
                return (false, "Enter a valid 10-digit phone number.");

            bool updated = _repo.Update(customer);
            return updated ? (true, "Customer updated.") : (false, "Customer not found.");
        }

        public (bool Success, string Message) DeleteCustomer(int customerId)
        {
            bool deleted = _repo.Delete(customerId);
            return deleted ? (true, "Customer deleted.") : (false, "Customer not found.");
        }
    }
}
