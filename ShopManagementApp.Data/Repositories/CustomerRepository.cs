using ShopManagementApp.Data.Excel;
using ShopManagementApp.Models;
using ShopManagementApp.Utils;

namespace ShopManagementApp.Data.Repositories
{
    /// <summary>
    /// Handles all CRUD operations for Customers in the Excel sheet.
    /// Columns: CustomerId | Name | Phone | Email | Address | CreatedAt
    /// </summary>
    public class CustomerRepository
    {
        private readonly ExcelManager _excel = ExcelManager.Instance;

        public List<Customer> GetAll()
        {
            var list = new List<Customer>();
            var ws = _excel.GetSheet("Customers");
            int lastRow = ExcelHelper.GetLastRow(ws);

            for (int r = 2; r <= lastRow; r++)
            {
                // Skip rows where Name is blank (empty/deleted rows)
                if (string.IsNullOrWhiteSpace(ExcelHelper.GetString(ws, r, 2))) continue;
                list.Add(new Customer
                {
                    CustomerId = ExcelHelper.GetInt(ws, r, 1),
                    Name       = ExcelHelper.GetString(ws, r, 2),
                    Phone      = ExcelHelper.GetString(ws, r, 3),
                    Email      = ExcelHelper.GetString(ws, r, 4),
                    Address    = ExcelHelper.GetString(ws, r, 5),
                    CreatedAt  = ExcelHelper.GetDateTime(ws, r, 6)
                });
            }
            return list;
        }

        public Customer? GetById(int id) => GetAll().FirstOrDefault(c => c.CustomerId == id);

        /// <summary>Search customers by name OR phone number.</summary>
        public List<Customer> Search(string query)
        {
            query = query.ToLower().Trim();
            return GetAll().Where(c =>
                c.Name.ToLower().Contains(query) ||
                c.Phone.Contains(query)).ToList();
        }

        public Customer Add(Customer customer)
        {
            var ws = _excel.GetSheet("Customers");
            int newRow = ExcelHelper.GetLastRow(ws) + 1;
            customer.CustomerId = ExcelHelper.GetNextId(ws);
            customer.CreatedAt = DateTime.Now;

            WriteRow(ws, newRow, customer);
            _excel.Save();
            return customer;
        }

        public bool Update(Customer customer)
        {
            var ws = _excel.GetSheet("Customers");
            int rowNum = ExcelHelper.FindRowById(ws, customer.CustomerId);
            if (rowNum < 0) return false;

            WriteRow(ws, rowNum, customer);
            _excel.Save();
            return true;
        }

        public bool Delete(int customerId)
        {
            var ws = _excel.GetSheet("Customers");
            int rowNum = ExcelHelper.FindRowById(ws, customerId);
            if (rowNum < 0) return false;

            ExcelHelper.DeleteRow(ws, rowNum);
            _excel.Save();
            return true;
        }

        private static void WriteRow(ClosedXML.Excel.IXLWorksheet ws, int row, Customer c)
        {
            ws.Cell(row, 1).Value = c.CustomerId;
            ws.Cell(row, 2).Value = c.Name;
            ws.Cell(row, 3).Value = c.Phone;
            ws.Cell(row, 4).Value = c.Email;
            ws.Cell(row, 5).Value = c.Address;
            ws.Cell(row, 6).Value = c.CreatedAt.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
