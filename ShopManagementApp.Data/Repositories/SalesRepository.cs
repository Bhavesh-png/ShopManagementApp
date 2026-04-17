using ShopManagementApp.Data.Excel;
using ShopManagementApp.Models;
using ShopManagementApp.Utils;

namespace ShopManagementApp.Data.Repositories
{
    /// <summary>
    /// Handles Sales and SaleItems.
    /// New columns (customer removed, Notes added):
    ///   Sales     : SaleId | SaleDate | SubTotal | Discount | FinalAmount | PaymentMode | Notes
    ///   SaleItems : SaleItemId | SaleId | ProductId | ProductName | Quantity | UnitPrice
    /// </summary>
    public class SalesRepository
    {
        private readonly ExcelManager _excel = ExcelManager.Instance;

        public List<Sale> GetAll()
        {
            var sales = ReadSaleHeaders();
            var items = ReadAllSaleItems();
            foreach (var s in sales)
                s.Items = items.Where(i => i.SaleId == s.SaleId).ToList();
            return sales;
        }

        public Sale? GetById(int saleId) => GetAll().FirstOrDefault(s => s.SaleId == saleId);

        public List<Sale> GetByDateRange(DateTime from, DateTime to)
            => GetAll().Where(s => s.SaleDate.Date >= from.Date && s.SaleDate.Date <= to.Date).ToList();

        public Sale AddSale(Sale sale)
        {
            var salesWs = _excel.GetSheet(Constants.SalesSheet);
            var itemsWs = _excel.GetSheet(Constants.SaleItemsSheet);

            sale.SaleId   = ExcelHelper.GetNextId(salesWs);
            sale.SaleDate = DateTime.Now;

            int saleRow = ExcelHelper.GetLastRow(salesWs) + 1;
            WriteSaleRow(salesWs, saleRow, sale);

            int itemNextId = ExcelHelper.GetNextId(itemsWs);
            foreach (var item in sale.Items)
            {
                item.SaleItemId = itemNextId++;
                item.SaleId     = sale.SaleId;
                WriteSaleItemRow(itemsWs, ExcelHelper.GetLastRow(itemsWs) + 1, item);
            }
            _excel.Save();
            return sale;
        }

        // ── Readers ───────────────────────────────────────────────────────────

        private List<Sale> ReadSaleHeaders()
        {
            var list = new List<Sale>();
            var ws   = _excel.GetSheet(Constants.SalesSheet);
            int last = ExcelHelper.GetLastRow(ws);

            for (int r = 2; r <= last; r++)
            {
                // Skip if SaleId cell is blank (empty row)
                if (ws.Cell(r, 1).IsEmpty()) continue;
                list.Add(new Sale
                {
                    SaleId      = ExcelHelper.GetInt(ws, r, 1),
                    SaleDate    = ExcelHelper.GetDateTime(ws, r, 2),
                    SubTotal    = ExcelHelper.GetDecimal(ws, r, 3),
                    Discount    = ExcelHelper.GetDecimal(ws, r, 4),
                    FinalAmount = ExcelHelper.GetDecimal(ws, r, 5),
                    PaymentMode = ExcelHelper.GetString(ws, r, 6),
                    Notes       = ExcelHelper.GetString(ws, r, 7)
                });
            }
            return list;
        }

        private List<SaleItem> ReadAllSaleItems()
        {
            var list = new List<SaleItem>();
            var ws   = _excel.GetSheet(Constants.SaleItemsSheet);
            int last = ExcelHelper.GetLastRow(ws);

            for (int r = 2; r <= last; r++)
            {
                if (string.IsNullOrWhiteSpace(ExcelHelper.GetString(ws, r, 4))) continue; // ProductName empty = blank row
                list.Add(new SaleItem
                {
                    SaleItemId  = ExcelHelper.GetInt(ws, r, 1),
                    SaleId      = ExcelHelper.GetInt(ws, r, 2),
                    ProductId   = ExcelHelper.GetInt(ws, r, 3),
                    ProductName = ExcelHelper.GetString(ws, r, 4),
                    Quantity    = ExcelHelper.GetInt(ws, r, 5),
                    UnitPrice   = ExcelHelper.GetDecimal(ws, r, 6)
                });
            }
            return list;
        }

        // ── Writers ───────────────────────────────────────────────────────────

        private static void WriteSaleRow(ClosedXML.Excel.IXLWorksheet ws, int row, Sale s)
        {
            ws.Cell(row, 1).Value = s.SaleId;
            ws.Cell(row, 2).Value = s.SaleDate.ToString("yyyy-MM-dd HH:mm");
            ws.Cell(row, 3).Value = s.SubTotal;
            ws.Cell(row, 4).Value = s.Discount;
            ws.Cell(row, 5).Value = s.FinalAmount;
            ws.Cell(row, 6).Value = s.PaymentMode;
            ws.Cell(row, 7).Value = s.Notes;
        }

        private static void WriteSaleItemRow(ClosedXML.Excel.IXLWorksheet ws, int row, SaleItem i)
        {
            ws.Cell(row, 1).Value = i.SaleItemId;
            ws.Cell(row, 2).Value = i.SaleId;
            ws.Cell(row, 3).Value = i.ProductId;
            ws.Cell(row, 4).Value = i.ProductName;
            ws.Cell(row, 5).Value = i.Quantity;
            ws.Cell(row, 6).Value = i.UnitPrice;
        }
    }
}
