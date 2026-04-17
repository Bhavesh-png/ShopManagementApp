using ShopManagementApp.Data.Excel;
using ShopManagementApp.Utils;

namespace ShopManagementApp.Business.Services
{
    /// <summary>
    /// Manages admin authentication and credential storage.
    /// Reads/writes credentials from the Settings sheet in Excel.
    /// Default: admin / 1234
    /// </summary>
    public class AdminService
    {
        private readonly ExcelManager _excel = ExcelManager.Instance;

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
    }
}
