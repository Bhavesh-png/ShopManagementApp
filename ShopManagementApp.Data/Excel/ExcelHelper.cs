using ClosedXML.Excel;

namespace ShopManagementApp.Data.Excel
{
    /// <summary>
    /// Utility methods for reading/writing Excel rows.
    /// These helpers make repository code cleaner and shorter.
    /// </summary>
    public static class ExcelHelper
    {
        /// <summary>
        /// Returns the last used row number in a sheet.
        /// Row 1 is always headers, so data starts at row 2.
        /// Returns 1 if there is no data yet.
        /// </summary>
        public static int GetLastRow(IXLWorksheet ws)
        {
            var lastRow = ws.LastRowUsed();
            return lastRow?.RowNumber() ?? 1;
        }

        /// <summary>
        /// Returns the next available ID by finding the max ID in column 1
        /// and adding 1.  Returns 1 if the sheet is empty.
        /// </summary>
        public static int GetNextId(IXLWorksheet ws)
        {
            int lastRow = GetLastRow(ws);
            if (lastRow <= 1) return 1; // no data rows yet

            int maxId = 0;
            for (int r = 2; r <= lastRow; r++)
            {
                if (int.TryParse(ws.Cell(r, 1).GetString(), out int id))
                    maxId = Math.Max(maxId, id);
            }
            return maxId + 1;
        }

        /// <summary>Gets a string value from a cell (handles blank cells safely).</summary>
        public static string GetString(IXLWorksheet ws, int row, int col)
            => ws.Cell(row, col).GetString().Trim();

        /// <summary>Gets an integer from a cell, returns 0 if invalid.</summary>
        public static int GetInt(IXLWorksheet ws, int row, int col)
        {
            var cell = ws.Cell(row, col);
            // Numeric cells: read as double first to avoid GetString returning ""
            if (cell.DataType == XLDataType.Number)
                return (int)cell.GetDouble();
            int.TryParse(cell.GetString(), out int result);
            return result;
        }

        /// <summary>Gets a decimal from a cell, returns 0 if invalid.</summary>
        public static decimal GetDecimal(IXLWorksheet ws, int row, int col)
        {
            var cell = ws.Cell(row, col);
            // Numeric cells: read as double first to avoid GetString returning ""
            if (cell.DataType == XLDataType.Number)
                return (decimal)cell.GetDouble();
            decimal.TryParse(cell.GetString(), out decimal result);
            return result;
        }

        /// <summary>Gets a DateTime from a cell, returns DateTime.Now if invalid.</summary>
        public static DateTime GetDateTime(IXLWorksheet ws, int row, int col)
        {
            var cell = ws.Cell(row, col);
            if (cell.DataType == XLDataType.DateTime)
                return cell.GetDateTime();
            if (DateTime.TryParse(cell.GetString(), out DateTime dt))
                return dt;
            return DateTime.Now;
        }

        /// <summary>Gets a nullable DateTime from a cell. Returns null if empty.</summary>
        public static DateTime? GetNullableDateTime(IXLWorksheet ws, int row, int col)
        {
            string val = ws.Cell(row, col).GetString();
            if (string.IsNullOrWhiteSpace(val)) return null;
            var cell = ws.Cell(row, col);
            if (cell.DataType == XLDataType.DateTime)
                return cell.GetDateTime();
            if (DateTime.TryParse(val, out DateTime dt))
                return dt;
            return null;
        }

        /// <summary>
        /// Finds the row number that has a matching value in a given column.
        /// Returns -1 if not found.
        /// </summary>
        public static int FindRowById(IXLWorksheet ws, int id)
        {
            int lastRow = GetLastRow(ws);
            for (int r = 2; r <= lastRow; r++)
            {
                if (GetInt(ws, r, 1) == id) return r;
            }
            return -1;
        }

        /// <summary>Deletes a row and shifts everything up.</summary>
        public static void DeleteRow(IXLWorksheet ws, int rowNum)
        {
            ws.Row(rowNum).Delete();
        }
    }
}
