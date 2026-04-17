using ShopManagementApp.Models;

namespace ShopManagementApp.Business.Helpers
{
    /// <summary>
    /// Pure calculation logic — no UI, no Excel. Easy to test.
    /// </summary>
    public static class CalculationHelper
    {
        /// <summary>Calculates the subtotal from a list of sale items.</summary>
        public static decimal CalculateSubTotal(List<SaleItem> items)
            => items.Sum(i => i.TotalPrice);

        /// <summary>Calculates the final amount after applying discount.</summary>
        public static decimal CalculateFinalAmount(decimal subTotal, decimal discount)
            => Math.Max(0, subTotal - discount);

        /// <summary>Calculates profit on a single sale item.</summary>
        public static decimal CalculateItemProfit(SaleItem item, decimal purchasePrice)
            => (item.UnitPrice - purchasePrice) * item.Quantity;

        /// <summary>Calculates discount percentage from discount amount.</summary>
        public static decimal CalculateDiscountPercent(decimal subTotal, decimal discount)
            => subTotal == 0 ? 0 : Math.Round((discount / subTotal) * 100, 2);

        /// <summary>Validates that discount does not exceed the subtotal.</summary>
        public static bool IsDiscountValid(decimal discount, decimal subTotal)
            => discount >= 0 && discount <= subTotal;
    }
}
