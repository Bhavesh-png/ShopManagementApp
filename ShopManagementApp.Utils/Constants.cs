namespace ShopManagementApp.Utils
{
    /// <summary>
    /// All constant values used across the application.
    /// Change ShopName, ShopAddress, ShopPhone to match your shop.
    /// </summary>
    public static class Constants
    {
        // ── Shop Information ──────────────────────────────────────────────────
        public const string ShopName    = "Gayatri Electronics & Hardware";
        public const string ShopAddress = "123 Main Market, Your City - 400001";
        public const string ShopPhone   = "+91 98765 43210";
        public const string ShopGST     = "GST No: 29XXXXX0000X1Z5";

        // ── Excel File Paths ──────────────────────────────────────────────────
        // Data folder sits next to the .exe file
        public static string DataFolderPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

        public static string ExcelFilePath =>
            Path.Combine(DataFolderPath, "ShopData.xlsx");

        // ── Excel Sheet Names ─────────────────────────────────────────────────
        public const string ProductsSheet  = "Products";
        public const string SalesSheet     = "Sales";
        public const string SaleItemsSheet = "SaleItems";
        public const string RepairsSheet   = "Repairs";
        public const string SettingsSheet  = "Settings";  // stores admin credentials

        // ── Admin Credentials (defaults — overridden by Settings sheet) ─────────
        public const string DefaultAdminUsername = "admin";
        public const string DefaultAdminPassword = "1234";

        // ── Repair Status Options ─────────────────────────────────────────────
        public static readonly string[] RepairStatuses =
            { "Pending", "In Progress", "Completed", "Delivered" };

        // ── Payment Modes ─────────────────────────────────────────────────────
        public static readonly string[] PaymentModes =
            { "Cash", "UPI", "Card", "Credit" };

        // ── Product Categories (Electronics / Electrical Hardware) ────────────
        public static readonly string[] ProductCategories =
        {
            "Electrical Wire", "Switch & Socket", "Fan", "Motor", "LED Light",
            "MCB & DB", "Conduit Pipe", "Welding Equipment", "Tools", "Other"
        };

        // ── Measurement Units ─────────────────────────────────────────────────
        public static readonly string[] Units =
            { "Pcs", "Meter", "Kg", "Box", "Pair", "Roll", "Set" };

        // ── Device Types for Repair ───────────────────────────────────────────
        public static readonly string[] DeviceTypes =
        {
            "Water Motor", "Ceiling Fan", "Table Fan", "Exhaust Fan",
            "Submersible Pump", "Welding Machine", "Grinder", "Drill Machine",
            "Electric Motor", "Generator", "Inverter", "Other"
        };

        // ── UI Colors ─────────────────────────────────────────────────────────
        public static class Colors
        {
            // Sidebar dark background
            public static System.Drawing.Color SidebarBg     = System.Drawing.Color.FromArgb(24, 24, 37);
            // Light grey content area
            public static System.Drawing.Color ContentBg     = System.Drawing.Color.FromArgb(245, 247, 250);
            // Primary accent (indigo/blue)
            public static System.Drawing.Color AccentBlue    = System.Drawing.Color.FromArgb(99, 102, 241);
            // Success green
            public static System.Drawing.Color AccentGreen   = System.Drawing.Color.FromArgb(34, 197, 94);
            // Warning orange
            public static System.Drawing.Color AccentOrange  = System.Drawing.Color.FromArgb(249, 115, 22);
            // Danger red
            public static System.Drawing.Color AccentRed     = System.Drawing.Color.FromArgb(239, 68, 68);
            // Card white
            public static System.Drawing.Color CardBg        = System.Drawing.Color.White;
            // Dark text
            public static System.Drawing.Color TextDark      = System.Drawing.Color.FromArgb(30, 30, 46);
            // Muted grey text
            public static System.Drawing.Color TextGray      = System.Drawing.Color.FromArgb(107, 114, 128);
            // Sidebar item text
            public static System.Drawing.Color SidebarText   = System.Drawing.Color.FromArgb(180, 180, 210);
            // Sidebar hover highlight
            public static System.Drawing.Color SidebarHover  = System.Drawing.Color.FromArgb(45, 45, 65);
            // Active sidebar item
            public static System.Drawing.Color SidebarActive = System.Drawing.Color.FromArgb(99, 102, 241);
        }

        // ── Low Stock Threshold ───────────────────────────────────────────────
        public const int LowStockThreshold = 10;
    }
}
