using ShopManagementApp.Business.Helpers;
using ShopManagementApp.Data.Repositories;
using ShopManagementApp.Models;
using ShopManagementApp.Utils;

namespace ShopManagementApp.Business.Services
{
    /// <summary>
    /// Business logic for creating bills / sales.
    /// Customer fields removed — walk-in sales only.
    /// Added Notes parameter for optional remarks.
    /// </summary>
    public class BillingService
    {
        private readonly SalesRepository   _salesRepo   = new SalesRepository();
        private readonly ProductRepository _productRepo = new ProductRepository();
        private readonly InventoryService  _inventory   = new InventoryService();

        public List<Sale> GetAllSales() => _salesRepo.GetAll();

        public List<Sale> GetSalesByDateRange(DateTime from, DateTime to)
            => _salesRepo.GetByDateRange(from, to);

        public decimal GetTodayRevenue()
            => _salesRepo.GetByDateRange(DateTime.Today, DateTime.Today).Sum(s => s.FinalAmount);

        /// <summary>
        /// Creates and saves a sale (no customer required).
        /// Validates items, checks stock, saves to Excel, deducts stock.
        /// </summary>
        public (bool Success, string Message, Sale? Sale) CreateSale(
            List<SaleItem> items,
            decimal discount,
            string paymentMode,
            string notes = "")
        {
            if (!items.Any())
                return (false, "Please add at least one product to the bill.", null);

            decimal subTotal = CalculationHelper.CalculateSubTotal(items);

            if (!CalculationHelper.IsDiscountValid(discount, subTotal))
                return (false, $"Discount (₹{discount}) cannot exceed sub-total (₹{subTotal}).", null);

            // Stock validation — skip ProductId==0 (custom / non-inventory items)
            foreach (var item in items)
            {
                if (item.ProductId == 0) continue;   // ← custom item, no stock to check

                var product = _productRepo.GetById(item.ProductId);
                if (product == null)
                    return (false, $"Product '{item.ProductName}' not found.", null);
                if (product.StockQuantity < item.Quantity)
                    return (false, $"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}", null);
            }

            var sale = new Sale
            {
                SubTotal    = subTotal,
                Discount    = discount,
                FinalAmount = CalculationHelper.CalculateFinalAmount(subTotal, discount),
                PaymentMode = paymentMode,
                Notes       = notes,
                Items       = items
            };

            var saved = _salesRepo.AddSale(sale);

            foreach (var item in items)
            {
                if (item.ProductId == 0) continue;   // ← custom item, no stock to deduct
                _inventory.DeductStock(item.ProductId, item.Quantity);
            }

            return (true, $"Bill #{saved.SaleId} saved successfully!", saved);
        }
    }
}
