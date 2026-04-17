using ShopManagementApp.Data.Excel;
using ShopManagementApp.Models;
using ShopManagementApp.Utils;

namespace ShopManagementApp.Data.Repositories
{
    /// <summary>
    /// Handles all CRUD operations for Products in the Excel sheet.
    /// Columns: ProductId | Name | Category | Brand | PurchasePrice | SellingPrice | StockQuantity | Unit
    /// </summary>
    public class ProductRepository
    {
        private readonly ExcelManager _excel = ExcelManager.Instance;

        // ── Read ──────────────────────────────────────────────────────────────

        /// <summary>Returns all products from the Products sheet.</summary>
        public List<Product> GetAll()
        {
            var products = new List<Product>();
            var ws = _excel.GetSheet(Constants.ProductsSheet);
            int lastRow = ExcelHelper.GetLastRow(ws);

            for (int r = 2; r <= lastRow; r++)
            {
                // Skip empty rows
                if (string.IsNullOrWhiteSpace(ExcelHelper.GetString(ws, r, 2))) continue;

                products.Add(new Product
                {
                    ProductId     = ExcelHelper.GetInt(ws, r, 1),
                    Name          = ExcelHelper.GetString(ws, r, 2),
                    Category      = ExcelHelper.GetString(ws, r, 3),
                    Brand         = ExcelHelper.GetString(ws, r, 4),
                    PurchasePrice = ExcelHelper.GetDecimal(ws, r, 5),
                    SellingPrice  = ExcelHelper.GetDecimal(ws, r, 6),
                    StockQuantity = ExcelHelper.GetInt(ws, r, 7),
                    Unit          = ExcelHelper.GetString(ws, r, 8)
                });
            }
            return products;
        }

        /// <summary>Returns a product by its ID. Returns null if not found.</summary>
        public Product? GetById(int id)
        {
            return GetAll().FirstOrDefault(p => p.ProductId == id);
        }

        // ── Create ────────────────────────────────────────────────────────────

        /// <summary>Adds a new product and saves the file.</summary>
        public void Add(Product product)
        {
            var ws = _excel.GetSheet(Constants.ProductsSheet);
            int newRow = ExcelHelper.GetLastRow(ws) + 1;
            product.ProductId = ExcelHelper.GetNextId(ws);

            WriteProductRow(ws, newRow, product);
            _excel.Save();
        }

        // ── Update ────────────────────────────────────────────────────────────

        /// <summary>Updates an existing product row in the sheet.</summary>
        public bool Update(Product product)
        {
            var ws = _excel.GetSheet(Constants.ProductsSheet);
            int rowNum = ExcelHelper.FindRowById(ws, product.ProductId);
            if (rowNum < 0) return false;

            WriteProductRow(ws, rowNum, product);
            _excel.Save();
            return true;
        }

        // ── Delete ────────────────────────────────────────────────────────────

        /// <summary>Deletes a product by ID.</summary>
        public bool Delete(int productId)
        {
            var ws = _excel.GetSheet(Constants.ProductsSheet);
            int rowNum = ExcelHelper.FindRowById(ws, productId);
            if (rowNum < 0) return false;

            ExcelHelper.DeleteRow(ws, rowNum);
            _excel.Save();
            return true;
        }

        // ── Stock Adjustment ──────────────────────────────────────────────────

        /// <summary>
        /// Adjusts stock quantity after a sale.
        /// Pass a negative delta to reduce stock (e.g., -5 means sold 5 units).
        /// </summary>
        public void AdjustStock(int productId, int delta)
        {
            var ws = _excel.GetSheet(Constants.ProductsSheet);
            int rowNum = ExcelHelper.FindRowById(ws, productId);
            if (rowNum < 0) return;

            int currentStock = ExcelHelper.GetInt(ws, rowNum, 7);
            int newStock = Math.Max(0, currentStock + delta); // never go below 0
            ws.Cell(rowNum, 7).Value = newStock;
            _excel.Save();
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private static void WriteProductRow(ClosedXML.Excel.IXLWorksheet ws, int row, Product p)
        {
            ws.Cell(row, 1).Value = p.ProductId;
            ws.Cell(row, 2).Value = p.Name;
            ws.Cell(row, 3).Value = p.Category;
            ws.Cell(row, 4).Value = p.Brand;
            ws.Cell(row, 5).Value = p.PurchasePrice;
            ws.Cell(row, 6).Value = p.SellingPrice;
            ws.Cell(row, 7).Value = p.StockQuantity;
            ws.Cell(row, 8).Value = p.Unit;
        }
    }
}
