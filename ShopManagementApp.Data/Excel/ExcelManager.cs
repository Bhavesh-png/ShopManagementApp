using ClosedXML.Excel;
using ShopManagementApp.Utils;

namespace ShopManagementApp.Data.Excel
{
    /// <summary>
    /// Singleton that owns the single open XLWorkbook.
    ///
    /// Sheets managed:
    ///   Products  — stock catalog        (8 cols)
    ///   Sales     — bill headers         (7 cols)
    ///   SaleItems — per-line bill items  (6 cols)
    ///   Repairs   — repair jobs          (12 cols)
    ///   Settings  — admin creds + schema version
    ///
    /// SCHEMA VERSION:
    ///   A "SchemaVersion" key in Settings sheet lets us detect stale files.
    ///   If the version is old (or missing), the file is backed up and recreated.
    ///   This prevents "all 0s on dashboard" caused by column-layout mismatches.
    /// </summary>
    public class ExcelManager
    {
        // ── Singleton ─────────────────────────────────────────────────────────
        private static ExcelManager? _instance;
        public  static ExcelManager  Instance => _instance ??= new ExcelManager();
        private ExcelManager() { }

        private XLWorkbook? _workbook;

        /// <summary>
        /// Schema version — bump this when you change any sheet's column layout.
        /// On startup the app compares this against what's stored in Settings.
        /// A mismatch triggers a controlled file reset (old file backed up).
        /// </summary>
        private const string SchemaVersion = "3";

        // ════════════════════════════════════════════════════════════════════════
        //  Initialize — called ONCE from Program.cs
        // ════════════════════════════════════════════════════════════════════════

        public void Initialize()
        {
            Directory.CreateDirectory(Constants.DataFolderPath);

            const int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    if (File.Exists(Constants.ExcelFilePath))
                    {
                        _workbook = new XLWorkbook(Constants.ExcelFilePath);

                        // ── Schema version check ──
                        // If the file was created by an older version of the code,
                        // the column layout may differ. Back up and recreate.
                        if (!IsSchemaCurrentVersion())
                        {
                            _workbook.Dispose();
                            BackupOldFile();
                            _workbook = new XLWorkbook();
                            CreateAllSheets();
                            _workbook.SaveAs(Constants.ExcelFilePath);
                        }
                        else
                        {
                            EnsureAllSheetsExist();
                        }
                    }
                    else
                    {
                        // First run — create fresh workbook with sample data
                        _workbook = new XLWorkbook();
                        CreateAllSheets();
                        _workbook.SaveAs(Constants.ExcelFilePath);
                    }
                    return;
                }
                catch (IOException) when (attempt < maxRetries)
                {
                    Thread.Sleep(800 * attempt);
                }
                catch (IOException ioEx)
                {
                    throw new InvalidOperationException(
                        $"Could not open or create the data file after {maxRetries} attempts.\n" +
                        $"Please close ShopData.xlsx if it is open in Excel.\n\nDetails: {ioEx.Message}");
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Public API
        // ════════════════════════════════════════════════════════════════════════

        public IXLWorksheet GetSheet(string name)
        {
            if (_workbook == null)
                throw new InvalidOperationException(
                    "ExcelManager.Initialize() must be called before accessing data.");

            if (!_workbook.TryGetWorksheet(name, out var ws))
                throw new KeyNotFoundException($"Sheet '{name}' not found in workbook.");

            return ws;
        }

        public void Save()
        {
            if (_workbook == null) return;

            const int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _workbook.SaveAs(Constants.ExcelFilePath);
                    return;
                }
                catch (IOException) when (attempt < maxRetries)
                {
                    Thread.Sleep(500 * attempt);
                }
                catch (IOException ioEx)
                {
                    throw new InvalidOperationException(
                        $"Could not save data file.\n" +
                        $"Please close ShopData.xlsx if it is open in Excel.\n\nDetails: {ioEx.Message}");
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Schema version check
        // ════════════════════════════════════════════════════════════════════════

        private bool IsSchemaCurrentVersion()
        {
            try
            {
                if (!_workbook!.TryGetWorksheet(Constants.SettingsSheet, out var ws)) return false;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
                for (int r = 2; r <= lastRow; r++)
                {
                    if (ws.Cell(r, 1).GetString().Trim() == "SchemaVersion")
                        return ws.Cell(r, 2).GetString().Trim() == SchemaVersion;
                }
                return false;   // SchemaVersion row not found
            }
            catch
            {
                return false;
            }
        }

        private static void BackupOldFile()
        {
            try
            {
                string ts     = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backup = Path.Combine(Constants.DataFolderPath, $"ShopData_backup_{ts}.xlsx");
                File.Move(Constants.ExcelFilePath, backup);
            }
            catch
            {
                // Backup failed — delete instead so we can create fresh
                try { File.Delete(Constants.ExcelFilePath); } catch { /* ignored */ }
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Sheet creation
        // ════════════════════════════════════════════════════════════════════════

        private void CreateAllSheets()
        {
            CreateProductsSheet();
            CreateSalesSheet();
            CreateSaleItemsSheet();
            CreateRepairsSheet();
            CreateSettingsSheet();
        }

        private void EnsureAllSheetsExist()
        {
            bool dirty = false;
            if (!_workbook!.TryGetWorksheet(Constants.ProductsSheet,  out _)) { CreateProductsSheet();  dirty = true; }
            if (!_workbook.TryGetWorksheet(Constants.SalesSheet,       out _)) { CreateSalesSheet();     dirty = true; }
            if (!_workbook.TryGetWorksheet(Constants.SaleItemsSheet,   out _)) { CreateSaleItemsSheet(); dirty = true; }
            if (!_workbook.TryGetWorksheet(Constants.RepairsSheet,     out _)) { CreateRepairsSheet();   dirty = true; }
            if (!_workbook.TryGetWorksheet(Constants.SettingsSheet,    out _)) { CreateSettingsSheet();  dirty = true; }
            if (dirty) Save();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Products: ProductId|Name|Category|Brand|PurchasePrice|SellingPrice|StockQuantity|Unit
        // ─────────────────────────────────────────────────────────────────────
        private void CreateProductsSheet()
        {
            var ws = _workbook!.AddWorksheet(Constants.ProductsSheet);
            WriteHeaders(ws, new[] { "ProductId","Name","Category","Brand","PurchasePrice","SellingPrice","StockQuantity","Unit" });

            // 12 sample products — note LOW STOCK items (stock <= 10) for dashboard to show
            var rows = new object[][]
            {
                new object[] { 1,  "Copper Wire 1.5mm",       "Electrical Wire",   "Havells",    80,  110, 500, "Meter" },
                new object[] { 2,  "Copper Wire 2.5mm",       "Electrical Wire",   "Havells",   110,  150, 300, "Meter" },
                new object[] { 3,  "LED Bulb 9W",             "LED Light",         "Philips",    45,   75, 100, "Pcs"   },
                new object[] { 4,  "LED Bulb 18W",            "LED Light",         "Syska",      80,  130,  80, "Pcs"   },
                new object[] { 5,  "MCB 32A Single Pole",     "MCB & DB",          "Schneider", 180,  280,  50, "Pcs"   },
                new object[] { 6,  "Modular Switch 6A",       "Switch & Socket",   "Anchor",     35,   60, 200, "Pcs"   },
                new object[] { 7,  "3-Pin Socket 16A",        "Switch & Socket",   "Legrand",    75,  120, 150, "Pcs"   },
                new object[] { 8,  "Conduit Pipe 25mm",       "Conduit Pipe",      "Supreme",    40,   65, 200, "Meter" },
                new object[] { 9,  "Ceiling Fan 1200mm",      "Fan",               "Havells",   900, 1400,   8, "Pcs"   },  // ← LOW STOCK
                new object[] { 10, "Capacitor 2.5MFD",        "Motor",             "Generic",    50,   90,  50, "Pcs"   },
                new object[] { 11, "Welding Rod 3.15mm 5kg",  "Welding Equipment", "D&H",       350,  550,   5, "Box"   },  // ← LOW STOCK
                new object[] { 12, "Electric Tape Roll",      "Tools",             "3M",         25,   45,   3, "Roll"  }   // ← LOW STOCK
            };
            for (int i = 0; i < rows.Length; i++) WriteRow(ws, i + 2, rows[i]);
            AutoFit(ws, 8);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Sales: SaleId|SaleDate|SubTotal|Discount|FinalAmount|PaymentMode|Notes
        // ─────────────────────────────────────────────────────────────────────
        private void CreateSalesSheet()
        {
            var ws = _workbook!.AddWorksheet(Constants.SalesSheet);
            WriteHeaders(ws, new[] { "SaleId","SaleDate","SubTotal","Discount","FinalAmount","PaymentMode","Notes" });
            AutoFit(ws, 7);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  SaleItems: SaleItemId|SaleId|ProductId|ProductName|Quantity|UnitPrice
        // ─────────────────────────────────────────────────────────────────────
        private void CreateSaleItemsSheet()
        {
            var ws = _workbook!.AddWorksheet(Constants.SaleItemsSheet);
            WriteHeaders(ws, new[] { "SaleItemId","SaleId","ProductId","ProductName","Quantity","UnitPrice" });
            AutoFit(ws, 6);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Repairs: RepairId|CustomerName|CustomerPhone|DeviceType|FaultDescription
        //           |Technician|EstimatedCost|FinalCost|Status|ReceivedDate|DeliveryDate|Notes
        // ─────────────────────────────────────────────────────────────────────
        private void CreateRepairsSheet()
        {
            var ws = _workbook!.AddWorksheet(Constants.RepairsSheet);
            WriteHeaders(ws, new[]
            {
                "RepairId","CustomerName","CustomerPhone","DeviceType","FaultDescription",
                "Technician","EstimatedCost","FinalCost","Status","ReceivedDate","DeliveryDate","Notes"
            });
            AutoFit(ws, 12);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Settings: Key|Value  (admin creds + SchemaVersion)
        // ─────────────────────────────────────────────────────────────────────
        private void CreateSettingsSheet()
        {
            var ws = _workbook!.AddWorksheet(Constants.SettingsSheet);
            WriteHeaders(ws, new[] { "Key", "Value" });
            ws.Cell(2, 1).Value = "AdminUsername";
            ws.Cell(2, 2).Value = Constants.DefaultAdminUsername;
            ws.Cell(3, 1).Value = "AdminPassword";
            ws.Cell(3, 2).Value = Constants.DefaultAdminPassword;
            ws.Cell(4, 1).Value = "SchemaVersion";
            ws.Cell(4, 2).Value = SchemaVersion;   // ← marks this file as current schema
            // ── Shop info rows (blank on first install; filled by FirstRunSetupForm) ──
            ws.Cell(5, 1).Value = Constants.ShopNameKey;    ws.Cell(5, 2).Value = "";
            ws.Cell(6, 1).Value = Constants.ShopAddressKey; ws.Cell(6, 2).Value = "";
            ws.Cell(7, 1).Value = Constants.ShopPhoneKey;   ws.Cell(7, 2).Value = "";
            ws.Cell(8, 1).Value = Constants.ShopGSTKey;     ws.Cell(8, 2).Value = "";
            AutoFit(ws, 2);
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Low-level write helpers
        // ════════════════════════════════════════════════════════════════════════

        private static void WriteHeaders(IXLWorksheet ws, string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value                      = headers[i];
                cell.Style.Font.Bold            = true;
                cell.Style.Font.FontSize        = 10;
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(30, 30, 48);
                cell.Style.Font.FontColor       = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }
        }

        private static void WriteRow(IXLWorksheet ws, int row, object[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                var cell = ws.Cell(row, i + 1);
                var val  = values[i];
                if      (val is int     iv) cell.Value = iv;
                else if (val is double  dv) cell.Value = dv;
                else if (val is decimal mv) cell.Value = (double)mv;
                else if (val is bool    bv) cell.Value = bv;
                else if (val is DateTime dt)cell.Value = dt.ToString("yyyy-MM-dd HH:mm");
                else if (val is string  sv) cell.Value = sv;
                else                        cell.Value = val?.ToString() ?? "";
            }
        }

        private static void AutoFit(IXLWorksheet ws, int colCount)
        {
            for (int i = 1; i <= colCount; i++)
                ws.Column(i).AdjustToContents();
        }
    }
}
