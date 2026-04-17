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
    ///   3. Launch MainForm
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
                    $"❌  Could not start {Constants.ShopName}.\n\n" +
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

            // ── Step 2: Launch the main window ──
            Application.Run(new MainForm());
        }
    }
}
