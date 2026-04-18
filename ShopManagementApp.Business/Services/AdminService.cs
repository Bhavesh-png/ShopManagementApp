using ShopManagementApp.Data.Excel;
using ShopManagementApp.Utils;

namespace ShopManagementApp.Business.Services
{
    /// <summary>
    /// Manages admin authentication, credential storage, and shop info.
    /// Reads/writes from the Settings sheet in Excel.
    /// Default credentials: admin / 1234
    /// </summary>
    public class AdminService
    {
        private readonly ExcelManager _excel = ExcelManager.Instance;

        // ════════════════════════════════════════════════════════════════════════
        //  Authentication
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>Returns true if the username and password match stored credentials.</summary>
        public bool ValidateLogin(string username, string password)
        {
            var (storedUser, storedPass) = GetCredentials();
            return username.Trim() == storedUser && password == storedPass;
        }

        /// <summary>Reads stored admin credentials from the Settings sheet.</summary>
        public (string Username, string Password) GetCredentials()
        {
            try
            {
                var ws   = _excel.GetSheet(Constants.SettingsSheet);
                string u = Constants.DefaultAdminUsername;
                string p = Constants.DefaultAdminPassword;
                int last = Data.Excel.ExcelHelper.GetLastRow(ws);

                for (int r = 2; r <= last; r++)
                {
                    string key = ws.Cell(r, 1).GetString();
                    if (key == "AdminUsername") u = ws.Cell(r, 2).GetString();
                    if (key == "AdminPassword") p = ws.Cell(r, 2).GetString();
                }
                return (u, p);
            }
            catch { return (Constants.DefaultAdminUsername, Constants.DefaultAdminPassword); }
        }

        /// <summary>Updates admin password in the Settings sheet.</summary>
        public (bool Success, string Message) ChangePassword(string currentPass, string newPass)
        {
            if (string.IsNullOrWhiteSpace(newPass))
                return (false, "New password cannot be empty.");
            if (newPass.Length < 4)
                return (false, "Password must be at least 4 characters.");

            var (_, storedPass) = GetCredentials();
            if (currentPass != storedPass)
                return (false, "Current password is incorrect.");

            try
            {
                var ws   = _excel.GetSheet(Constants.SettingsSheet);
                int last = Data.Excel.ExcelHelper.GetLastRow(ws);
                for (int r = 2; r <= last; r++)
                    if (ws.Cell(r, 1).GetString() == "AdminPassword")
                    { ws.Cell(r, 2).Value = newPass; break; }
                _excel.Save();
                return (true, "Password changed successfully.");
            }
            catch (Exception ex) { return (false, $"Error: {ex.Message}"); }
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Shop Info
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Reads ShopName/Address/Phone/GST from the Settings sheet and
        /// populates <see cref="Constants.ShopInfo"/>.
        /// Called once at startup (Program.cs) before MainForm is shown.
        /// </summary>
        public void LoadShopInfo()
        {
            try
            {
                var ws   = _excel.GetSheet(Constants.SettingsSheet);
                int last = Data.Excel.ExcelHelper.GetLastRow(ws);

                for (int r = 2; r <= last; r++)
                {
                    string key = ws.Cell(r, 1).GetString().Trim();
                    string val = ws.Cell(r, 2).GetString().Trim();

                    if (key == Constants.ShopNameKey    && !string.IsNullOrEmpty(val)) Constants.ShopInfo.Name    = val;
                    if (key == Constants.ShopAddressKey && !string.IsNullOrEmpty(val)) Constants.ShopInfo.Address = val;
                    if (key == Constants.ShopPhoneKey   && !string.IsNullOrEmpty(val)) Constants.ShopInfo.Phone   = val;
                    if (key == Constants.ShopGSTKey     && !string.IsNullOrEmpty(val)) Constants.ShopInfo.GST     = val;
                }
            }
            catch { /* If Settings sheet is unreadable, keep compile-time defaults */ }
        }

        /// <summary>
        /// Returns true if ShopName has been set in the Settings sheet
        /// (i.e. the shop has been configured on this installation).
        /// </summary>
        public bool IsShopInfoConfigured()
        {
            try
            {
                var ws   = _excel.GetSheet(Constants.SettingsSheet);
                int last = Data.Excel.ExcelHelper.GetLastRow(ws);
                for (int r = 2; r <= last; r++)
                    if (ws.Cell(r, 1).GetString().Trim() == Constants.ShopNameKey)
                        return !string.IsNullOrWhiteSpace(ws.Cell(r, 2).GetString());
                return false;
            }
            catch { return false; }
        }

        /// <summary>
        /// Writes shop info to the Settings sheet and updates
        /// <see cref="Constants.ShopInfo"/> immediately.
        /// Inserts missing key rows so the method works on upgraded installs
        /// where the old ShopData.xlsx did not have these rows.
        /// </summary>
        public (bool Success, string Message) SaveShopInfo(
            string name, string address, string phone, string gst)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (false, "Shop Name cannot be empty.");

            try
            {
                var ws   = _excel.GetSheet(Constants.SettingsSheet);
                int last = Data.Excel.ExcelHelper.GetLastRow(ws);

                // Build a map: key -> row number
                var rowMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int r = 2; r <= last; r++)
                {
                    string k = ws.Cell(r, 1).GetString().Trim();
                    if (!string.IsNullOrEmpty(k)) rowMap[k] = r;
                }

                // Helper — upsert a key/value pair
                void Upsert(string key, string value)
                {
                    if (rowMap.TryGetValue(key, out int row))
                    {
                        ws.Cell(row, 2).Value = value;
                    }
                    else
                    {
                        last++;
                        ws.Cell(last, 1).Value = key;
                        ws.Cell(last, 2).Value = value;
                        rowMap[key] = last;
                    }
                }

                Upsert(Constants.ShopNameKey,    name.Trim());
                Upsert(Constants.ShopAddressKey, address.Trim());
                Upsert(Constants.ShopPhoneKey,   phone.Trim());
                Upsert(Constants.ShopGSTKey,     gst.Trim());

                _excel.Save();

                // Update runtime values immediately
                Constants.ShopInfo.Name    = name.Trim();
                Constants.ShopInfo.Address = address.Trim();
                Constants.ShopInfo.Phone   = phone.Trim();
                Constants.ShopInfo.GST     = gst.Trim();

                return (true, "Shop settings saved successfully.");
            }
            catch (Exception ex) { return (false, $"Error saving: {ex.Message}"); }
        }
    }
}
