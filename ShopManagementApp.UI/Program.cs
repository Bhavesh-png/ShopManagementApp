using ShopManagementApp.Business.Services;
using ShopManagementApp.Data.Excel;
using ShopManagementApp.UI.Forms;
using ShopManagementApp.Utils;

namespace ShopManagementApp.UI
{
    /// <summary>
    /// Application entry point.
    ///
    /// Startup sequence:
    ///   1. Enable visual styles (modern look)
    ///   2. Initialize ExcelManager — creates or opens ShopData.xlsx
    ///   3. Load shop info from Settings sheet into Constants.ShopInfo
    ///   4. If shop info not configured — show FirstRunSetupForm
    ///   5. Launch MainForm
    /// </summary>
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Enable modern Windows visual styles (rounded buttons, themes)
            ApplicationConfiguration.Initialize();

            // ── Step 1: Initialize the Excel data file ──
            // This creates Data/ShopData.xlsx if it doesn't exist,
            // or opens the existing one and adds any missing sheets.
            try
            {
                ExcelManager.Instance.Initialize();
            }
            catch (Exception ex)
            {
                // This is a fatal startup error — cannot run without data file
                MessageBox.Show(
                    $"❌  Could not start the application.\n\n" +
                    $"Reason: {ex.Message}\n\n" +
                    $"Possible fixes:\n" +
                    $"  • Close ShopData.xlsx if it is open in Excel\n" +
                    $"  • Make sure the app folder is not read-only\n" +
                    $"  • Run the app as Administrator once",
                    "Startup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;   // exit without opening MainForm
            }

            // ── Step 2: Load shop info from Settings sheet ──
            var adminSvc = new AdminService();
            adminSvc.LoadShopInfo();

            // ── Step 3: First-run setup if shop not yet configured ──
            if (!adminSvc.IsShopInfoConfigured())
            {
                var setup = new FirstRunSetupForm();
                if (setup.ShowDialog() != DialogResult.OK)
                {
                    // User closed the setup without saving — exit gracefully
                    return;
                }
            }

            // ── Step 4: Launch the main window ──
            Application.Run(new MainForm());
        }
    }
}
