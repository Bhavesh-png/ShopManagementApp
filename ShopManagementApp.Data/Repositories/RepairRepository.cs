using ShopManagementApp.Data.Excel;
using ShopManagementApp.Models;
using ShopManagementApp.Utils;

namespace ShopManagementApp.Data.Repositories
{
    /// <summary>
    /// Handles CRUD operations for Repair jobs in the Excel sheet.
    ///
    /// Repairs sheet columns (updated — CustomerId removed):
    ///   Col 1 = RepairId
    ///   Col 2 = CustomerName
    ///   Col 3 = CustomerPhone
    ///   Col 4 = DeviceType
    ///   Col 5 = FaultDescription
    ///   Col 6 = Technician
    ///   Col 7 = EstimatedCost
    ///   Col 8 = FinalCost
    ///   Col 9 = Status
    ///   Col 10 = ReceivedDate
    ///   Col 11 = DeliveryDate
    ///   Col 12 = Notes
    /// </summary>
    public class RepairRepository
    {
        private readonly ExcelManager _excel = ExcelManager.Instance;

        public List<Repair> GetAll()
        {
            var list   = new List<Repair>();
            var ws     = _excel.GetSheet(Constants.RepairsSheet);
            int lastRow = ExcelHelper.GetLastRow(ws);

            for (int r = 2; r <= lastRow; r++)
            {
                // Skip blank rows — check CustomerName (col 2)
                if (string.IsNullOrWhiteSpace(ExcelHelper.GetString(ws, r, 2))) continue;
                list.Add(MapRow(ws, r));
            }
            return list;
        }

        public Repair? GetById(int id) => GetAll().FirstOrDefault(r => r.RepairId == id);

        public List<Repair> GetByStatus(string status)
            => GetAll().Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();

        /// <summary>Find repairs by customer name (partial, case-insensitive).</summary>
        public List<Repair> GetByCustomerName(string name)
            => GetAll().Where(r => r.CustomerName.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();

        /// <summary>Legacy overload — matches by customerId stored on model (always 0 now).</summary>
        public List<Repair> GetByCustomer(int customerId)
            => GetAll().Where(r => r.CustomerId == customerId).ToList();

        public List<Repair> GetActiveRepairs()
            => GetAll().Where(r => r.Status != "Delivered").ToList();

        public Repair Add(Repair repair)
        {
            var ws = _excel.GetSheet(Constants.RepairsSheet);
            int newRow = ExcelHelper.GetLastRow(ws) + 1;
            repair.RepairId     = ExcelHelper.GetNextId(ws);
            repair.ReceivedDate = DateTime.Now;
            repair.Status       = "Pending";
            repair.CustomerId   = 0;   // not tracked anymore

            WriteRow(ws, newRow, repair);
            _excel.Save();
            return repair;
        }

        public bool Update(Repair repair)
        {
            var ws = _excel.GetSheet(Constants.RepairsSheet);
            int rowNum = ExcelHelper.FindRowById(ws, repair.RepairId);
            if (rowNum < 0) return false;

            WriteRow(ws, rowNum, repair);
            _excel.Save();
            return true;
        }

        public bool Delete(int repairId)
        {
            var ws = _excel.GetSheet(Constants.RepairsSheet);
            int rowNum = ExcelHelper.FindRowById(ws, repairId);
            if (rowNum < 0) return false;

            ExcelHelper.DeleteRow(ws, rowNum);
            _excel.Save();
            return true;
        }

        // ── Private helpers ───────────────────────────────────────────────────

        /// <summary>Map a worksheet row to a Repair object (new schema, no CustomerId column).</summary>
        private static Repair MapRow(ClosedXML.Excel.IXLWorksheet ws, int r) => new Repair
        {
            RepairId         = ExcelHelper.GetInt(ws, r, 1),
            CustomerId       = 0,                                    // not stored in new schema
            CustomerName     = ExcelHelper.GetString(ws, r, 2),
            CustomerPhone    = ExcelHelper.GetString(ws, r, 3),
            DeviceType       = ExcelHelper.GetString(ws, r, 4),
            FaultDescription = ExcelHelper.GetString(ws, r, 5),
            Technician       = ExcelHelper.GetString(ws, r, 6),
            EstimatedCost    = ExcelHelper.GetDecimal(ws, r, 7),
            FinalCost        = ExcelHelper.GetDecimal(ws, r, 8),
            Status           = ExcelHelper.GetString(ws, r, 9),
            ReceivedDate     = ExcelHelper.GetDateTime(ws, r, 10),
            DeliveryDate     = ExcelHelper.GetNullableDateTime(ws, r, 11),
            Notes            = ExcelHelper.GetString(ws, r, 12)
        };

        /// <summary>Write a Repair to a worksheet row (new schema, no CustomerId column).</summary>
        private static void WriteRow(ClosedXML.Excel.IXLWorksheet ws, int row, Repair rep)
        {
            ws.Cell(row, 1).Value  = rep.RepairId;
            ws.Cell(row, 2).Value  = rep.CustomerName;
            ws.Cell(row, 3).Value  = rep.CustomerPhone;
            ws.Cell(row, 4).Value  = rep.DeviceType;
            ws.Cell(row, 5).Value  = rep.FaultDescription;
            ws.Cell(row, 6).Value  = rep.Technician;
            ws.Cell(row, 7).Value  = (double)rep.EstimatedCost;
            ws.Cell(row, 8).Value  = (double)rep.FinalCost;
            ws.Cell(row, 9).Value  = rep.Status;
            ws.Cell(row, 10).Value = rep.ReceivedDate.ToString("yyyy-MM-dd HH:mm");
            ws.Cell(row, 11).Value = rep.DeliveryDate.HasValue
                ? rep.DeliveryDate.Value.ToString("yyyy-MM-dd") : "";
            ws.Cell(row, 12).Value = rep.Notes;
        }
    }
}
