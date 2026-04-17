using ShopManagementApp.Data.Repositories;
using ShopManagementApp.Models;
using ShopManagementApp.Utils;

namespace ShopManagementApp.Business.Services
{
    /// <summary>
    /// Business logic for product inventory management.
    /// </summary>
    public class InventoryService
    {
        private readonly ProductRepository _repo = new ProductRepository();

        public List<Product> GetAllProducts() => _repo.GetAll();

        public Product? GetProductById(int id) => _repo.GetById(id);

        /// <summary>Returns products that have stock below the threshold.</summary>
        public List<Product> GetLowStockProducts()
            => _repo.GetAll().Where(p => p.StockQuantity <= Constants.LowStockThreshold).ToList();

        public (bool Success, string Message) AddProduct(Product product)
        {
            if (!ValidationHelper.IsNonEmpty(product.Name))
                return (false, "Product name is required.");
            if (!ValidationHelper.IsPositiveDecimal(product.SellingPrice))
                return (false, "Selling price must be greater than 0.");
            if (product.StockQuantity < 0)
                return (false, "Stock quantity cannot be negative.");

            _repo.Add(product);
            return (true, "Product added successfully.");
        }

        public (bool Success, string Message) UpdateProduct(Product product)
        {
            if (!ValidationHelper.IsNonEmpty(product.Name))
                return (false, "Product name is required.");
            if (!ValidationHelper.IsPositiveDecimal(product.SellingPrice))
                return (false, "Selling price must be greater than 0.");

            bool updated = _repo.Update(product);
            return updated
                ? (true, "Product updated successfully.")
                : (false, "Product not found.");
        }

        public (bool Success, string Message) DeleteProduct(int productId)
        {
            bool deleted = _repo.Delete(productId);
            return deleted
                ? (true, "Product deleted.")
                : (false, "Product not found.");
        }

        /// <summary>Reduce stock after a sale (called by BillingService).</summary>
        public void DeductStock(int productId, int quantity)
            => _repo.AdjustStock(productId, -quantity);
    }
}
